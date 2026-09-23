from pathlib import Path

ROOT = Path(__file__).resolve().parent
ENV_FILE = ROOT / ".env"


def read_model_name() -> str:
    if not ENV_FILE.exists():
        raise SystemExit(".env wurde nicht gefunden.")

    for line in ENV_FILE.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, _, value = line.partition("=")
        if key.strip() == "MODEL_NAME" and value.strip():
            return value.strip()

    raise SystemExit("MODEL_NAME fehlt in .env.")


if __name__ == "__main__":
    print(f"MODEL_NAME ist gesetzt: {read_model_name()}")
