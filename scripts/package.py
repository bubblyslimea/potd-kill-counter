"""Package reviewed runtime files and the matching testing repository entry."""
from pathlib import Path
import hashlib
import json
import re
import xml.etree.ElementTree as ET
import zipfile

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "artifacts"
BUILD = ROOT / "src/PotdCounter/bin/Release"
REPO = "https://github.com/bubblyslimea/potd-kill-counter"


def sha(data):
    return hashlib.sha256(data).hexdigest()


def package():
    release = json.loads((ROOT / "distribution/release.json").read_text())
    version = ET.parse(ROOT / "src/PotdCounter/PotdCounter.csproj").findtext(".//Version")
    if not re.fullmatch(r"\d+(?:\.\d+){3}", version or ""):
        raise ValueError("invalid assembly version")
    if release["channel"] != "testing" or not re.fullmatch(r"v\d+\.\d+\.\d+-alpha\.\d+", release["tag"]):
        raise ValueError("this publisher requires an explicit alpha release")
    if not release["tag"].startswith("v" + ".".join(version.split(".")[:3]) + "-alpha."):
        raise ValueError("release tag and assembly version disagree")
    names = ["PotdCounter.dll", "PotdCounter.json", "PotdCounter.deps.json"]
    files = {name: (BUILD / name).read_bytes() for name in names}
    manifest = json.loads(files["PotdCounter.json"])
    expected = {"InternalName": "PotdCounter", "Author": "Riri Fujiwara", "AssemblyVersion": version,
                "DalamudApiLevel": 15, "RepoUrl": REPO}
    for key, value in expected.items():
        if manifest.get(key) != value:
            raise ValueError("runtime manifest does not match the reviewed release: " + key)
    if files["PotdCounter.dll"][:2] != b"MZ":
        raise ValueError("plugin assembly is missing")
    for value in files.values():
        for encoding in ("utf-8", "utf-16-le"):
            text = value.decode(encoding, errors="ignore")
            if re.search(r"(?i)([a-z]:[\\/]Users[\\/]|/Users/|/home/|/workspace/|\.pdb\b)", text):
                raise ValueError("runtime package contains a build path or debug metadata")
    files.update({name: (ROOT / name).read_bytes() for name in ["LICENSE", "THIRD_PARTY_NOTICES.md"]})
    OUT.mkdir(exist_ok=True)
    archive = OUT / "PotdCounter.zip"
    with zipfile.ZipFile(archive, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=9) as z:
        for name, data in sorted(files.items()):
            info = zipfile.ZipInfo(name, date_time=(2026, 1, 1, 0, 0, 0))
            info.compress_type = zipfile.ZIP_DEFLATED
            info.create_system = 3
            info.external_attr = 0o100644 << 16
            z.writestr(info, data, compresslevel=9)
    digest = sha(archive.read_bytes())
    (OUT / "PotdCounter.zip.sha256").write_text(digest + "  PotdCounter.zip\n", encoding="utf-8")
    link = REPO + "/releases/download/" + release["tag"] + "/PotdCounter.zip"
    entry = {key: manifest[key] for key in ["Author", "Name", "InternalName", "Description", "ApplicableVersion", "Tags", "DalamudApiLevel", "Punchline", "RepoUrl"]}
    entry.update({"AssemblyVersion": "0.0.0.0", "TestingAssemblyVersion": version,
                  "TestingDalamudApiLevel": 15, "IsTestingExclusive": True, "IsHide": False,
                  "AcceptsFeedback": False, "DownloadLinkInstall": link, "DownloadLinkUpdate": link,
                  "DownloadLinkTesting": link, "TestingChangelog": "a little counter for the climb\n\ncounts observed kills, remembers Flight and gives a rough passage estimate\nlive PotD checks are still pending"})
    (OUT / "repo.json").write_text(json.dumps([entry], indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    record = {"version": version, "tag": release["tag"], "channel": release["channel"],
              "archiveSha256": digest, "files": {name: sha(data) for name, data in sorted(files.items())}}
    (OUT / "release-record.json").write_text(json.dumps(record, indent=2) + "\n", encoding="utf-8")
    print("packaged", version, "with", len(files), "reviewed files")
    print("archive SHA-256:", digest)


if __name__ == "__main__":
    package()
