#!/usr/bin/env python3
"""Bereitet die lokale Entwicklungsumgebung vor: appsettings.Development.json und Modell-Download."""

import shutil
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
PROJECT_DIR = ROOT / "src" / "LocalAiFront"
ENV_FILE = ROOT / ".env"


def ensure_development_settings() -> None:
    target = PROJECT_DIR / "appsettings.Development.json"
    if target.exists():
        print(
            "appsettings.Development.json existiert bereits, wird nicht ueberschrieben."
        )
        return
    source = PROJECT_DIR / "appsettings.example.json"
    shutil.copy(source, target)
    print("appsettings.Development.json wurde aus appsettings.example.json erzeugt.")


def read_model_name() -> str:
    if not ENV_FILE.exists():
        print(
            "Keine .env gefunden. Bitte .env.example nach .env kopieren und MODEL_NAME setzen."
        )
        sys.exit(1)

    for line in ENV_FILE.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, _, value = line.partition("=")
        if key.strip() == "MODEL_NAME":
            return value.strip()

    print("MODEL_NAME ist in .env nicht gesetzt.")
    sys.exit(1)


def pull_model(model_name: str) -> None:
    print(f"Lade Modell {model_name} ueber Docker Model Runner...")
    subprocess.run(["docker", "model", "pull", model_name], check=True)


if __name__ == "__main__":
    ensure_development_settings()
    pull_model(read_model_name())
