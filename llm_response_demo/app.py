import json
import os

import requests
from flask import Flask, Response, jsonify, render_template, request

app = Flask(__name__)

DMR_BASE_URL = os.getenv("DMR_BASE_URL", "http://host.docker.internal:12434")
MODEL_NAME = os.getenv("MODEL_NAME", "huggingface.co/unsloth/gemma-4-e4b-it-gguf:Q4_K_M")
CONTEXT_SIZE = int(os.getenv("CONTEXT_SIZE", "4096"))


@app.route("/")
def index():
    return render_template("index.html")


def extract_stream_text(payload):
    if not isinstance(payload, dict):
        return ""

    if isinstance(payload.get("message"), dict):
        content = payload["message"].get("content")
        if isinstance(content, list):
            return "".join(
                part.get("text", "")
                for part in content
                if isinstance(part, dict)
            )
        if isinstance(content, str):
            return content

    if isinstance(payload.get("content"), str):
        return payload["content"]

    choices = payload.get("choices") or []
    for choice in choices:
        if not isinstance(choice, dict):
            continue
        delta = choice.get("delta") or {}
        text = delta.get("content") or delta.get("text")
        if text:
            return str(text)

    return ""


@app.route("/api/chat", methods=["POST"])
def chat():
    payload = request.get_json(silent=True) or {}
    prompt = str(payload.get("prompt", "")).strip()

    if not prompt:
        return jsonify({"error": "Bitte einen Prompt eingeben."}), 400

    request_body = {
        "model": MODEL_NAME,
        "messages": [
            {
                "role": "system",
                "content": "Du bist ein hilfreicher Assistent. Antworte nur mit der finalen Antwort, ohne interne Denkprozesse, Chain-of-Thought oder Vorarbeiten.",
            },
            {"role": "user", "content": prompt},
        ],
        "stream": True,
        "options": {
            "num_ctx": CONTEXT_SIZE,
        },
    }

    def generate():
        try:
            with requests.post(
                f"{DMR_BASE_URL}/api/chat",
                json=request_body,
                stream=True,
                timeout=180,
            ) as upstream:
                upstream.raise_for_status()

                for line in upstream.iter_lines(decode_unicode=True):
                    if not line:
                        continue
                    raw = line.strip()
                    if not raw:
                        continue
                    if raw.startswith("data:"):
                        raw = raw[5:].strip()
                    if raw == "[DONE]":
                        break
                    try:
                        data = json.loads(raw)
                    except json.JSONDecodeError:
                        continue

                    text = extract_stream_text(data)
                    if text:
                        yield f"data: {json.dumps({'text': text})}\n\n"
        except Exception as exc:
            yield f"data: {json.dumps({'error': str(exc)})}\n\n"

    return Response(generate(), mimetype="text/event-stream")


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=8000, debug=False)
