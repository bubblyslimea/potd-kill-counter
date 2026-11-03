import importlib.util
from pathlib import Path
import unittest

root = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("counter_package", root / "scripts/package.py")
package = importlib.util.module_from_spec(spec)
spec.loader.exec_module(package)


class ReleaseChannelTests(unittest.TestCase):
    def setUp(self):
        self.manifest = {"Author": "Riri Fujiwara", "Name": "Riri's PotD Counter", "InternalName": "PotdCounter",
                         "Description": "testing description", "ApplicableVersion": "any", "Tags": ["potd"],
                         "DalamudApiLevel": 15, "Punchline": "counter", "RepoUrl": package.REPO}

    def test_stable_visible_without_testing(self):
        entry = package.repository_entry(self.manifest, {"channel": "stable", "tag": "v0.1.5"}, "0.1.5.0")
        self.assertFalse(entry["IsTestingExclusive"])
        self.assertFalse(entry["IsHide"])
        self.assertEqual(entry["AssemblyVersion"], "0.1.5.0")
        self.assertFalse(any(k.startswith("Testing") or k == "DownloadLinkTesting" for k in entry))
        self.assertEqual(entry["DownloadLinkInstall"], package.REPO + "/releases/download/v0.1.5/PotdCounter.zip")
        self.assertEqual(entry["DownloadLinkUpdate"], entry["DownloadLinkInstall"])
        self.assertIn("Changelog", entry)

    def test_testing_remains_opt_in(self):
        entry = package.repository_entry(self.manifest, {"channel": "testing", "tag": "v0.1.6-alpha.1"}, "0.1.6.0")
        self.assertTrue(entry["IsTestingExclusive"])
        self.assertEqual(entry["AssemblyVersion"], "0.0.0.0")
        self.assertEqual(entry["TestingAssemblyVersion"], "0.1.6.0")
        self.assertEqual(entry["DownloadLinkTesting"], entry["DownloadLinkInstall"])

    def test_channel_and_version_must_match(self):
        for channel, tag, version in [("unknown", "v0.1.5", "0.1.5.0"),
                                      ("stable", "v0.1.5-alpha.1", "0.1.5.0"),
                                      ("testing", "v0.1.5", "0.1.5.0"),
                                      ("stable", "v0.1.6", "0.1.5.0"),
                                      ("testing", "v0.1.6-alpha.1", "0.1.5.0"),
                                      ("stable", "v0.1.5", "0.1.5.1")]:
            with self.subTest(channel=channel, tag=tag, version=version), self.assertRaises(ValueError):
                package.validate_release(version, {"channel": channel, "tag": tag})

    def test_manifest_and_identity_are_preserved(self):
        before = dict(self.manifest)
        entry = package.repository_entry(self.manifest, {"channel": "stable", "tag": "v0.1.5"}, "0.1.5.0")
        self.assertEqual(self.manifest, before)
        for key in ["Author", "Name", "InternalName", "DalamudApiLevel", "RepoUrl"]:
            self.assertEqual(entry[key], self.manifest[key])
