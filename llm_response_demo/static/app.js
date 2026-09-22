const promptEl = document.getElementById("prompt");
const statusEl = document.getElementById("status");
const responseEl = document.getElementById("response");
const sendBtn = document.getElementById("sendBtn");

if (sendBtn) {
  sendBtn.addEventListener("click", async () => {
    const prompt = (promptEl?.value || "").trim();

    if (!prompt) {
      statusEl.textContent = "Bitte einen Prompt eingeben.";
      return;
    }

    statusEl.textContent = "Wird verarbeitet...";
    responseEl.textContent = "";

    try {
      const res = await fetch("/api/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ prompt }),
      });

      if (!res.ok || !res.body) {
        const data = await res.json().catch(() => ({}));
        throw new Error(data.error || "Unbekannter Fehler");
      }

      const reader = res.body.getReader();
      const decoder = new TextDecoder();
      let buffer = "";

      while (true) {
        const { value, done } = await reader.read();
        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        const parts = buffer.split("\n\n");
        buffer = parts.pop() || "";

        for (const part of parts) {
          const line = part.trim();
          if (!line.startsWith("data:")) continue;
          const payload = line.slice(5).trim();
          if (!payload || payload === "[DONE]") continue;

          try {
            const data = JSON.parse(payload);
            const text = data?.text || data?.content || data?.answer || "";
            if (text) responseEl.textContent += text;
          } catch {
            responseEl.textContent += payload;
          }
        }
      }

      if (buffer.trim()) {
        const payload = buffer.trim();
        if (payload.startsWith("data:")) {
          const text = payload.slice(5).trim();
          try {
            const data = JSON.parse(text);
            const msg = data?.text || data?.content || data?.answer || "";
            if (msg) responseEl.textContent += msg;
          } catch {
            responseEl.textContent += text;
          }
        }
      }

      statusEl.textContent = "Fertig.";
    } catch (err) {
      statusEl.textContent = "Fehler: " + err.message;
      responseEl.textContent = "Es konnte keine Antwort generiert werden.";
    }
  });
}
