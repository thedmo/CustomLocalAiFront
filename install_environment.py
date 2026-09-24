#!/usr/bin/env python3
"""Bereitet die lokale Entwicklungsumgebung fuer Docker Model Runner vor."""

import argparse
import os
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
ENV_FILE = ROOT / ".env"
ENV_EXAMPLE = ROOT / ".env.example"


def ensure_env_file(
    root: Path = ROOT,
    env_file: Path | None = None,
    env_example: Path | None = None,
) -> bool:
    env_file = env_file or root / ".env"
    env_example = env_example or root / ".env.example"

    print(f"[1/3] Pruefe Konfigurationsdateien: {env_file.name} und {env_example.name}")

    if env_file.exists():
        print(f"[1/3] {env_file.name} vorhanden, kein Copy erforderlich.")
        return False

    if not env_example.exists():
        raise FileNotFoundError(f"{env_example.name} wurde nicht gefunden.")

    with env_example.open("r", encoding="utf-8") as source:
        content = source.read()

    env_file.write_text(content, encoding="utf-8")
    print(
        f"[1/3] WARNUNG: {env_file.name} fehlte und wurde aus {env_example.name} erstellt. Bitte Werte pruefen."
    )
    return True


def load_environment_variables(env_file: Path = ENV_FILE) -> dict[str, str]:
    print(f"[2/3] Lade alle Werte aus {env_file.name} in die Laufzeitumgebung.")

    if not env_file.exists():
        raise FileNotFoundError(f"{env_file} wurde nicht gefunden.")

    loaded: dict[str, str] = {}
    for line in env_file.read_text(encoding="utf-8").splitlines():
        text = line.strip()
        if not text or text.startswith("#") or "=" not in text:
            continue

        key, _, value = line.partition("=")
        key = key.strip()
        value = value.strip()
        if key:
            os.environ[key] = value
            loaded[key] = value
            print(f"      - {key}={value}")

    print(f"[2/3] {len(loaded)} Umgebungsvariablen wurden gesetzt.")
    return loaded


def read_model_name(env_file: Path = ENV_FILE) -> str:
    print(f"[3/3] Lese MODEL_NAME aus {env_file.name}.")

    if not env_file.exists():
        raise SystemExit("Keine .env gefunden. Bitte zuerst .env.example nach .env kopieren.")

    for line in env_file.read_text(encoding="utf-8").splitlines():
        text = line.strip()
        if not text or text.startswith("#") or "=" not in text:
            continue

        key, _, value = line.partition("=")
        if key.strip() == "MODEL_NAME" and value.strip():
            print(f"[3/3] MODEL_NAME erkannt: {value.strip()}")
            return value.strip()

    raise SystemExit("MODEL_NAME fehlt in der .env-Datei.")


def pull_model(model_name: str) -> None:
    print(f"[3/3] Lade Modell {model_name} ueber Docker Model Runner...")
    completed = subprocess.run(["docker", "model", "pull", model_name])
    if completed.returncode != 0:
        raise SystemExit(f"Docker Model Runner konnte {model_name} nicht laden.")
    print(f"[3/3] Modell-Download abgeschlossen: {model_name}")


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Vorbereitung der lokalen Docker-Umgebung und Model-Download."
    )
    parser.add_argument(
        "-d",
        "--debug",
        action="store_true",
        help="Laedt alle .env-Variablen in die Laufzeitumgebung, ohne appsettings zu erzeugen.",
    )
    args = parser.parse_args()

    print("=== Setup: lokale Docker-Umgebung vorbereiten ===")
    ensure_env_file()

    if args.debug:
        load_environment_variables(ENV_FILE)

    model_name = read_model_name()
    pull_model(model_name)
    print("=== Setup abgeschlossen ===")


if __name__ == "__main__":
    try:
        main()
    except SystemExit:
        raise
    except FileNotFoundError as exc:
        print(f"Fehler: {exc}", file=sys.stderr)
        raise SystemExit(1)
