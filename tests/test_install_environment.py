import importlib.util
import os
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location("install_environment", ROOT / "install_environment.py")
INSTALL = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(INSTALL)


class InstallEnvironmentTests(unittest.TestCase):
    def test_ensure_env_file_creates_dotenv_from_example(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir)
            example = root / ".env.example"
            example.write_text("MODEL_NAME=test-model\n", encoding="utf-8")

            created = INSTALL.ensure_env_file(root=root)

            self.assertTrue(created)
            self.assertTrue((root / ".env").exists())
            self.assertEqual((root / ".env").read_text(encoding="utf-8").strip(), "MODEL_NAME=test-model")

    def test_load_variables_into_runtime_environment(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir)
            env_file = root / ".env"
            env_file.write_text("MODEL_NAME=test-model\nOTHER_FLAG=abc\n", encoding="utf-8")

            original = os.environ.get("MODEL_NAME")
            original_other = os.environ.get("OTHER_FLAG")
            try:
                INSTALL.load_environment_variables(env_file)
                self.assertEqual(os.environ["MODEL_NAME"], "test-model")
                self.assertEqual(os.environ["OTHER_FLAG"], "abc")
            finally:
                if original is None:
                    os.environ.pop("MODEL_NAME", None)
                else:
                    os.environ["MODEL_NAME"] = original

                if original_other is None:
                    os.environ.pop("OTHER_FLAG", None)
                else:
                    os.environ["OTHER_FLAG"] = original_other

    def test_debug_mode_creates_dev_settings(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir)
            project_dir = root / "src" / "LocalAiFront"
            project_dir.mkdir(parents=True)
            (project_dir / "appsettings.example.json").write_text('{"Title": "example"}', encoding="utf-8")
            create = INSTALL.ensure_development_settings(root=root, debug=True)
            self.assertTrue(create)
            self.assertTrue((project_dir / "appsettings.Development.json").exists())


if __name__ == "__main__":
    unittest.main()
