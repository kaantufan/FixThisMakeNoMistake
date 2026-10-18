param(
    [string]$SourceRoot = '',
    [string]$AuditDirectory = '',
    [string]$Destination = '',
    [switch]$HashFiles
)

$ErrorActionPreference = 'Stop'
if (!$SourceRoot) { $SourceRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..')) }
if (!$AuditDirectory) { $AuditDirectory = Join-Path $SourceRoot 'Evidence\RepositoryAudit' }
$source = (Resolve-Path -LiteralPath $SourceRoot).Path.TrimEnd('\')
$assetRoot = Join-Path $source 'TooLateToPanic\Assets'
$generatedPattern = '[\\/]Game[\\/]Resources[\\/](Generated|HomeGenerated|ResidenceGenerated)[\\/]'
$yamlExtensions = @('.meta', '.prefab', '.unity', '.asset', '.mat', '.controller', '.anim', '.shader', '.overrideController', '.asmdef', '.asmref')
$assetFiles = @(Get-ChildItem -LiteralPath $assetRoot -Recurse -File -Force)
$guidMap = @{}
foreach ($meta in @($assetFiles | Where-Object Extension -eq '.meta')) {
    $match = [regex]::Match([IO.File]::ReadAllText($meta.FullName), '(?m)^guid: ([0-9a-f]{32})')
    if ($match.Success) {
        $guid = $match.Groups[1].Value
        if ($guidMap.ContainsKey($guid)) { throw "Duplicate asset GUID: $guid" }
        $guidMap[$guid] = $meta.FullName.Substring(0, $meta.FullName.Length - 5)
    }
}

# Preserve every authored/source/vendor asset. Historical generated meshes are
# selected only through references from these assets; nothing is deleted locally.
$selected = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$neededGenerated = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$visited = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$missing = [Collections.Generic.HashSet[string]]::new()
$queue = [Collections.Generic.Queue[string]]::new()
$excludedRadioPattern = '[\\/]Game[\\/]Deferred[\\/]Radio-legacy([\\/]|\.meta$)'
foreach ($file in $assetFiles) {
    if ($file.FullName -match $generatedPattern -or $file.FullName -match $excludedRadioPattern) { continue }
    [void]$selected.Add($file.FullName)
    if ($file.Extension -in $yamlExtensions) { $queue.Enqueue($file.FullName) }
}
# Runtime Shader.Find dependencies are not represented by prefab GUID edges.
# Keep the six explicit shader materials from the latest full residence bake.
# Cached-art builds create empty later folders, so choose only a bake with keepers.
$shaderKeepers = @($assetFiles | Where-Object {
    $_.FullName -match '[\\/]Resources[\\/]ResidenceGenerated[\\/]bake-[^\\/]+[\\/]shader-\d+\.mat$'
})
if ($shaderKeepers.Count -gt 0) {
    $shaderBake = @($shaderKeepers.DirectoryName | Sort-Object -Unique -Descending)[0]
    foreach ($keeper in @($shaderKeepers | Where-Object DirectoryName -eq $shaderBake)) {
        [void]$selected.Add($keeper.FullName)
        [void]$neededGenerated.Add($keeper.FullName)
        $queue.Enqueue($keeper.FullName)
        if ([IO.File]::Exists($keeper.FullName + '.meta')) {
            [void]$selected.Add($keeper.FullName + '.meta')
            $queue.Enqueue($keeper.FullName + '.meta')
        }
    }
}
while ($queue.Count -gt 0) {
    $path = $queue.Dequeue()
    if (-not $visited.Add($path)) { continue }
    foreach ($match in [regex]::Matches([IO.File]::ReadAllText($path), 'guid: ([0-9a-f]{32})')) {
        $guid = $match.Groups[1].Value
        if ($guid -match '^0{16}') { continue }
        if (-not $guidMap.ContainsKey($guid)) { [void]$missing.Add($guid); continue }
        $dependency = $guidMap[$guid]
        if ($dependency -match $excludedRadioPattern) { throw 'A selected asset references excluded legacy radio audio.' }
        if ($dependency -match $generatedPattern -and [IO.File]::Exists($dependency) -and $neededGenerated.Add($dependency)) {
            [void]$selected.Add($dependency)
            $queue.Enqueue($dependency)
            if ([IO.File]::Exists($dependency + '.meta')) {
                [void]$selected.Add($dependency + '.meta')
                $queue.Enqueue($dependency + '.meta')
            }
        }
    }
}
if ($missing.Count -gt 0) { throw "Unresolved non-built-in asset GUIDs: $($missing -join ', ')" }

# Keep the stable GUIDs of every containing Unity asset directory.
foreach ($path in @($selected)) {
    $directory = [IO.Path]::GetDirectoryName($path)
    while ($directory.StartsWith($assetRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
        if ([IO.File]::Exists($directory + '.meta')) { [void]$selected.Add($directory + '.meta') }
        $directory = [IO.Path]::GetDirectoryName($directory)
    }
}

$includeDirectories = @('TooLateToPanic\Packages', 'TooLateToPanic\ProjectSettings', 'TooLateToPanic\Documentation', 'Docs', 'Tools')
foreach ($relative in $includeDirectories) {
    foreach ($file in @(Get-ChildItem -LiteralPath (Join-Path $source $relative) -Recurse -File -Force)) {
        [void]$selected.Add($file.FullName)
    }
}
$includeFiles = @('.gitignore', '.gitattributes', 'CONTRIBUTING.md', 'PUBLICATION.md', 'AGENTS.md', 'README.md', 'Play-TooLateToPanic.cmd', 'TooLateToPanic\AGENTS.md', 'TooLateToPanic\README.md', 'TooLateToPanic\LATEST-BUILD.json', 'TooLateToPanic\Play-TooLateToPanic.cmd')
foreach ($relative in $includeFiles) {
    $path = Join-Path $source $relative
    if ([IO.File]::Exists($path)) { [void]$selected.Add($path) }
}
$entries = @($selected | Sort-Object | ForEach-Object {
    $file = Get-Item -LiteralPath $_
    $entry = [ordered]@{ path = $file.FullName.Substring($source.Length + 1).Replace('\', '/'); bytes = $file.Length }
    if ($HashFiles) { $entry.sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash }
    [pscustomobject]$entry
})
$oversized = @($entries | Where-Object bytes -ge 100MB)
if ($oversized.Count -gt 0) { throw "GitHub oversized source blobs: $($oversized.path -join ', ')" }
$generatedGroups = @($neededGenerated | Group-Object {
    $relative = $_.Substring($assetRoot.Length + 1)
    if ($relative -match '^Game\\Resources\\([^\\]+)(\\bake-[^\\]+)?') { $matches[0].Replace('\', '/') }
} | Sort-Object Name | Select-Object @{N='directory';E={$_.Name}}, @{N='assetFiles';E={$_.Count}})
$manifest = [ordered]@{
    schema = 1
    generatedUtc = [DateTime]::UtcNow.ToString('o')
    sourceRoot = $source
    scope = 'Unified isometric Unity source with bag-only first-person stage, documentation, prompts and tools. Separate legacy FPS checkout is preserved locally and excluded.'
    fileCount = $entries.Count
    bytes = ($entries | Measure-Object bytes -Sum).Sum
    largest = @($entries | Sort-Object bytes -Descending | Select-Object -First 10)
    generatedDependencyAssets = $neededGenerated.Count
    generatedGroups = $generatedGroups
    missingGuids = @($missing)
    excluded = @('TooLateToPanicFPS/', '.claude/', 'Evidence/', '**/Library/', '**/Temp/', '**/Logs/', '**/Builds/', '**/UserSettings/', 'persistent saves and scores', 'Assets/Game/Deferred/Radio-legacy/ (unverified synthesized-audio redistribution terms)', 'unreferenced historical generated meshes/materials')
    notes = @('No local source or build files are removed.', 'No Git or remote mutations are performed.', 'Local metadata paths in historical docs/LATEST-BUILD remain original; evidence directories are not published.', 'Quaternius vendor files and both CC0 notices are preserved.', 'This static GUID closure is not a fresh Unity import/build test. Root must run after source freeze and verify current build separately.')
    files = $entries
}
New-Item -ItemType Directory -Path $AuditDirectory -Force | Out-Null
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $AuditDirectory 'source-publish-manifest.json') -Encoding utf8
$entries.path | Set-Content -LiteralPath (Join-Path $AuditDirectory 'source-publish-files.txt') -Encoding utf8

if ($Destination) {
    $target = [IO.Path]::GetFullPath($Destination).TrimEnd('\')
    if ($target -eq $source -or $source.StartsWith($target + '\', [StringComparison]::OrdinalIgnoreCase) -or $target.StartsWith($assetRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Destination must be a separate publication checkout.'
    }
    foreach ($entry in $entries) {
        $from = Join-Path $source $entry.path
        $to = Join-Path $target $entry.path
        New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($to)) -Force | Out-Null
        Copy-Item -LiteralPath $from -Destination $to -Force
    }
    $radioNote = Join-Path $target 'TooLateToPanic\Assets\Game\Deferred\README-PUBLIC-SOURCE.md'
    New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($radioNote)) -Force | Out-Null
    @'
# Deferred audio

Historical Microsoft Tolga radio WAV files are retained in the local development archive but are excluded from this public source snapshot because their redistribution terms have not been independently verified. They are outside Resources, are not used by the current campaign/short-preparation audio policy, and are not part of the current Windows player package.

See `../../../Documentation/ASSET-CREDITS.md` for provenance and the current audio policy. No Microsoft voice engine or voice data is included.
'@ | Set-Content -LiteralPath $radioNote -Encoding utf8
}
[pscustomobject]@{Files=$entries.Count;Bytes=$manifest.bytes;GeneratedAssets=$neededGenerated.Count;MissingGuids=$missing.Count;Destination=$Destination} | ConvertTo-Json
