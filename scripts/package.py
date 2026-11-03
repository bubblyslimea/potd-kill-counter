"""Package reviewed runtime files and their explicit stable or testing channel."""
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


def validate_release(version, release):
    if not re.fullmatch(r"\d+(?:\.\d+){3}", version or ""):
        raise ValueError("invalid assembly version")
    base = "v" + ".".join(version.split(".")[:3])
    if release["channel"] == "stable":
        valid = release["tag"] == base and version.endswith(".0")
    elif release["channel"] == "testing":
        valid = re.fullmatch(re.escape(base) + r"-alpha\.\d+", release["tag"]) is not None
    else:
        raise ValueError("release channel must be stable or testing")
    if not valid:
        raise ValueError("release tag and assembly version disagree")


def repository_entry(manifest, release, version):
    validate_release(version, release)
    link = REPO + "/releases/download/" + release["tag"] + "/PotdCounter.zip"
    entry = {key: manifest[key] for key in ["Author", "Name", "InternalName", "Description", "ApplicableVersion", "Tags", "DalamudApiLevel", "Punchline", "RepoUrl"]}
    testing = release["channel"] == "testing"
    entry.update({"AssemblyVersion": "0.0.0.0" if testing else version,
                  "IsTestingExclusive": testing, "IsHide": False, "AcceptsFeedback": False,
                  "DownloadLinkInstall": link, "DownloadLinkUpdate": link})
    if testing:
        entry.update({"TestingAssemblyVersion": version, "TestingDalamudApiLevel": 15,
                      "DownloadLinkTesting": link,
                      "TestingChangelog": "Boss victory\n\nShows the defeated boss name above DEFEATED with a green check\nAppears after a confirmed boss death and the end of combat\nReturns to OPEN after four seconds when passage activation is confirmed\nPreview the victory card in /potd preview"})
    else:
        entry["Description"] = "Tracks observed enemy kills, estimates remaining kills, and shows current and queued Flight in Palace of the Dead.\n\nOpen /potd for settings or /potd preview to inspect the HUD."
        entry["Changelog"] = "Stable release\n\nVersion 0.1.5 is now available without enabling testing builds.\nExisting users keep the same verified plugin build and settings."
    return entry


def package():
    release = json.loads((ROOT / "distribution/release.json").read_text())
    version = ET.parse(ROOT / "src/PotdCounter/PotdCounter.csproj").findtext(".//Version")
    validate_release(version, release)
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
    entry = repository_entry(manifest, release, version)
    (OUT / "repo.json").write_text(json.dumps([entry], indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    record = {"version": version, "tag": release["tag"], "channel": release["channel"],
              "archiveSha256": digest, "files": {name: sha(data) for name, data in sorted(files.items())}}
    (OUT / "release-record.json").write_text(json.dumps(record, indent=2) + "\n", encoding="utf-8")
    print("packaged", version, "for", release["channel"], "with", len(files), "reviewed files")
    print("archive SHA-256:", digest)


if __name__ == "__main__":
    package()
