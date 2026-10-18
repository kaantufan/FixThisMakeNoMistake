"""Read-only source/index QA. No clone, fetch, stage, commit, push, tag or release.

The caller freezes sources, runs Prepare-GitHubSource.ps1 with -HashFiles,
copies reviewed changes into a separate clone and stages the intended paths.
This helper verifies that index. Output is one JSON receipt in --output.
"""
from pathlib import Path, PurePosixPath
import argparse
import hashlib
import json
import re
import subprocess
import sys


def read_json(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def sha(data):
    return hashlib.sha256(data).hexdigest().upper()


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("--repository", required=True)
    p.add_argument("--source-root", required=True)
    p.add_argument("--manifest", required=True)
    p.add_argument("--base", required=True)
    p.add_argument("--output", required=True)
    p.add_argument("--prior-state")
    p.add_argument("--prior-manifest")
    p.add_argument("--build-source-manifest", help="Optional exact accepted build's local source manifest (JSON)")
    p.add_argument("--extra-file", action="append", default=[])
    p.add_argument("--allow-adaptation", action="append", default=[])
    args = p.parse_args()
    repo, source = Path(args.repository).resolve(), Path(args.source_root).resolve()
    assert repo != source and (repo / ".git").exists(), "Separate Git checkout required"
    git = lambda *a: subprocess.check_output(["git", "-C", str(repo), *a])
    base = git("rev-parse", "HEAD").decode().strip()
    assert base == args.base == git("rev-parse", "origin/main").decode().strip()
    assert not git("diff", "--name-only"), "Stage all intended changes before verification"
    manifest = read_json(args.manifest)
    assert not manifest["missingGuids"]
    selected = {x["path"]: x for x in manifest["files"]}
    root_extras = [x for x in (".gitignore", ".gitattributes", "CONTRIBUTING.md", "PUBLICATION.md") if (source / x).is_file()]
    for rel in root_extras + args.extra_file:
        path = source / rel
        selected[rel] = {"path": rel, "sha256": sha(path.read_bytes()), "bytes": path.stat().st_size}
    for rel, row in selected.items():
        assert not PurePosixPath(rel).is_absolute() and ".." not in PurePosixPath(rel).parts
        data = (source / rel).read_bytes()
        assert sha(data) == row["sha256"].upper() and len(data) == row["bytes"], ("Frozen source drift", rel)
    source_build = None
    if args.build_source_manifest:
        value = read_json(args.build_source_manifest)
        rows = value["files"] if isinstance(value, dict) else value
        code = [x for x in rows if x["path"].endswith((".cs", ".shader"))]
        compiled = [x for x in code if x["path"].endswith(".cs") and x["path"].startswith(("Assets/Game/Runtime/", "Assets/Game/Art/", "Assets/Game/Editor/"))]
        code_drift = [x["path"] for x in code if selected.get("TooLateToPanic/" + x["path"], {}).get("sha256", "").upper() != x["sha256"].upper()]
        asset_drift = [x["path"] for x in rows if x["path"].startswith("Assets/") and "TooLateToPanic/" + x["path"] in selected and selected["TooLateToPanic/" + x["path"]]["sha256"].upper() != x["sha256"].upper()]
        assert not code_drift and not asset_drift, {"codeDrift": code_drift, "selectedAssetDrift": asset_drift}
        source_build = {"compiledCSharpFiles": len(compiled), "codeShaderFiles": len(code), "codeDrift": code_drift, "selectedAssetDrift": asset_drift, "newBuildOrNativeRun": False}
    raw = git("ls-files", "--stage", "-z")
    index = {}
    for item in raw.split(b"\0"):
        if not item:
            continue
        meta, rel = item.split(b"\t", 1)
        mode, oid, stage = meta.decode().split()
        assert stage == "0" and mode in {"100644", "100755"}
        index[rel.decode()] = oid
    previous = {}
    for item in git("ls-tree", "-rz", args.base).split(b"\0"):
        if item:
            meta, rel = item.split(b"\t", 1)
            previous[rel.decode()] = meta.decode().split()[2]
    assert not set(previous) - set(index), "This source update must preserve all previous paths"
    assert set(selected) <= set(index), "Every selected source path must be staged"
    assert not set(index) - set(previous) - set(selected), "New public paths must be in the reviewed manifest"
    preserved_adaptations = set()
    if args.prior_state or args.prior_manifest:
        assert args.prior_state and args.prior_manifest
        prior = read_json(args.prior_state)
        assert prior["commit"] == base
        old = {x["path"]: x for x in read_json(args.prior_manifest)["files"]}
        for rel in prior.get("documentationAdaptations", []):
            if rel in selected and old.get(rel, {}).get("sha256") == selected[rel]["sha256"]:
                if index.get(rel) == previous.get(rel):
                    preserved_adaptations.add(rel)
    adapted = set(args.allow_adaptation)
    assert adapted <= set(selected)
    forbidden = {"library", "temp", "logs", "build", "builds", "obj", "usersettings", "evidence", "toolatetopanicfps", ".codex", ".claude", "saves", "__pycache__", ".pytest_cache"}
    secret = re.compile(rb"(?:gh[pousr]_[A-Za-z0-9]{30,}|github_pat_[A-Za-z0-9_]{50,}|sk-[A-Za-z0-9_-]{32,}|AKIA[0-9A-Z]{16}|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----)")
    yaml_suffixes = {".meta", ".prefab", ".unity", ".asset", ".mat", ".controller", ".anim", ".shader", ".overrideController", ".asmdef", ".asmref"}
    guids, refs, expected = {}, set(), []
    batch = subprocess.Popen(["git", "-C", str(repo), "cat-file", "--batch"], stdin=subprocess.PIPE, stdout=subprocess.PIPE)
    try:
        for rel, oid in sorted(index.items()):
            path = PurePosixPath(rel)
            assert not {x.lower() for x in path.parts} & forbidden, rel
            assert not path.name.startswith(".env") and path.suffix.lower() not in {".pem", ".key", ".pfx", ".exe", ".dll", ".zip", ".7z", ".pyc", ".pyo"}, rel
            assert not (path.name.lower().startswith(("highscore", "score-profile", "audio-profile")) and path.suffix.lower() == ".json"), rel
            batch.stdin.write((oid + "\n").encode())
            batch.stdin.flush()
            header = batch.stdout.readline().decode().split()
            assert header[0] == oid and header[1] == "blob"
            size = int(header[2])
            data = batch.stdout.read(size)
            assert len(data) == size and batch.stdout.read(1) == b"\n"
            assert size < 100 * 1024 * 1024 and not secret.search(data), rel
            assert not data.startswith(b"version https://git-lfs.github.com/spec/v1"), rel
            assert (repo / rel).read_bytes() == data, ("Working tree/index byte mismatch", rel)
            digest = sha(data)
            if rel in selected and rel not in adapted | preserved_adaptations:
                assert digest == selected[rel]["sha256"].upper(), ("Source/index byte mismatch", rel)
            elif rel not in selected:
                assert oid == previous[rel], ("Unselected historical path changed", rel)
            if rel.startswith("TooLateToPanic/Assets/"):
                if path.suffix == ".meta":
                    match = re.search(rb"^guid: ([0-9a-f]{32})", data, re.M)
                    if match:
                        guid = match[1].decode()
                        assert guid not in guids, ("Duplicate GUID", guid, rel, guids.get(guid))
                        guids[guid] = rel
                if path.suffix in yaml_suffixes:
                    refs.update(x.decode() for x in re.findall(rb"guid: ([0-9a-f]{32})", data))
            expected.append({"path": rel, "sha256": digest, "bytes": size})
    finally:
        failing = sys.exc_info()[0] is not None
        batch.stdin.close()
        if failing:
            batch.terminate()
            batch.wait()
        else:
            assert batch.wait() == 0
    missing = sorted(g for g in refs if not g.startswith("0" * 16) and g not in guids)
    keepers = [r for r in selected if re.search(r"ResidenceGenerated/bake-[^/]+/shader-\d+\.mat$", r)]
    assert not missing and len(keepers) == 6, {"missingGuids": missing, "selectedShaderKeepers": keepers}
    result = {"status": "SOURCE_INDEX_VERIFIED_NO_PUBLICATION_PERFORMED", "baseCommit": base, "selectedSourceFiles": len(selected), "rootExtraFiles": root_extras, "sourceVsRecordedBuild": source_build, "publicIndexFiles": len(index), "indexByteMismatches": 0, "missingGuids": missing, "selectedShaderKeepers": keepers, "preservedPriorAdaptations": sorted(preserved_adaptations), "explicitAdaptations": sorted(adapted), "newBuildOrNativeRun": False, "remoteMutations": 0, "files": expected}
    out = Path(args.output).resolve()
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "files"}, indent=2))


if __name__ == "__main__":
    main()
