"""Check current repository entry points without starting Unity or publishing."""
from pathlib import Path
import argparse
import hashlib
import json
import re
import sys
from urllib.parse import unquote


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument('--local-build', action='store_true', help='Also require one build/ZIP, launchers and matching DLL/ZIP hashes')
    args = parser.parse_args()
    root = args.root.resolve()
    errors = []
    required = ['README.md', 'CONTRIBUTING.md', 'PUBLICATION.md', '.gitignore', '.gitattributes',
                'TooLateToPanic/README.md', 'TooLateToPanic/LATEST-BUILD.json',
                'Docs/README.md', 'Docs/RELEASE-WORKFLOW.md', 'Docs/JAM-RULES-CHECKLIST.md',
                'Docs/TOO-LATE-TO-PANIC-JAM-TESLIM-BEYANI.md', 'Docs/JAM-3-DAKIKA-SUNUM.md',
                'TooLateToPanic/Assets', 'TooLateToPanic/Packages', 'TooLateToPanic/ProjectSettings']
    for rel in required:
        if not (root / rel).exists():
            errors.append('Missing entry point: ' + rel)
    if errors:
        print(json.dumps({'passed': False, 'errors': errors}, ensure_ascii=False, indent=2))
        return 1
    latest = json.loads((root / 'TooLateToPanic/LATEST-BUILD.json').read_text(encoding='utf-8-sig'))
    match = re.search(r'Home V([\d.]+).*?/r(\d+)', latest['version'])
    if not match:
        errors.append('Unrecognized LATEST version: ' + latest['version'])
    else:
        label = 'V' + match[1] + '-r' + match[2]
        for rel in ['README.md', 'TooLateToPanic/README.md']:
            if label not in (root / rel).read_text(encoding='utf-8-sig'):
                errors.append('README version differs from LATEST: ' + rel)
    checked_links = 0
    for rel in required:
        if not rel.endswith('.md'):
            continue
        text = (root / rel).read_text(encoding='utf-8-sig')
        for raw in re.findall(r'(?<!!)\[[^\]]+\]\(([^)]+)\)', text):
            target = raw.strip().strip('<>')
            if re.match(r'^[a-zA-Z]+:', target) or target.startswith('#'):
                continue
            target = unquote(target.split('#')[0])
            path = (root / rel).parent / target
            # Evidence and binaries are deliberately absent from a source checkout.
            try:
                relative_parts = path.resolve().relative_to(root).parts
            except ValueError:
                relative_parts = ()
            if not args.local_build and ('Evidence' in relative_parts or 'Builds' in relative_parts):
                continue
            checked_links += 1
            if not path.exists():
                errors.append('Broken current-document link: ' + rel + ' -> ' + target)
    ignore = (root / '.gitignore').read_text(encoding='utf-8-sig')
    for marker in ['/Evidence/', '/TooLateToPanicFPS/', '[Ll]ibrary', '[Bb]uilds', '[Uu]ser[Ss]ettings', '.env']:
        if marker not in ignore:
            errors.append('Missing publication ignore: ' + marker)
    if args.local_build:
        project = root / 'TooLateToPanic'
        exe, archive = project / latest['executable'], project / latest['portableZip']
        dll = exe.parent / 'TooLateToPanic_Data/Managed/Assembly-CSharp.dll'
        allowed = {exe.parent.resolve(), archive.resolve()}
        if {p.resolve() for p in (project / 'Builds').iterdir()} != allowed:
            errors.append('Builds must contain only the current build and portable ZIP')
        for path, expected in [(archive, latest['portableZipSha256']), (dll, latest['assemblyCSharpSha256'])]:
            if not path.is_file():
                errors.append('Missing current artifact: ' + str(path))
            else:
                with path.open('rb') as stream:
                    if hashlib.file_digest(stream, 'sha256').hexdigest().upper() != expected.upper():
                        errors.append('Current artifact SHA256 differs: ' + str(path))
        launcher = (project / 'Play-TooLateToPanic.cmd').read_text(encoding='utf-8-sig')
        if latest['executable'].replace('/', '\\') not in launcher:
            errors.append('Project launcher does not target LATEST executable')
        if 'TooLateToPanic\\Play-TooLateToPanic.cmd' not in (root / 'Play-TooLateToPanic.cmd').read_text(encoding='utf-8-sig'):
            errors.append('Root launcher does not delegate to the active project')
    print(json.dumps({'passed': not errors, 'version': latest['version'], 'localBuildChecked': args.local_build,
                      'currentDocumentLinks': checked_links, 'errors': errors,
                      'UnityOrPlayerStarted': False, 'remoteMutations': 0}, ensure_ascii=False, indent=2))
    return int(bool(errors))


if __name__ == '__main__':
    sys.exit(main())
