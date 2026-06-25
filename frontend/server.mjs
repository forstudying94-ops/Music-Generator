import http from "node:http";
import https from "node:https";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const distDir = path.join(path.dirname(fileURLToPath(import.meta.url)), "dist");
const port = Number(process.env.PORT || 3000);
const backend = resolveBackend();
const backendClient = backend.protocol === "https:" ? https : http;

function resolveBackend() {
  if (process.env.BACKEND_URL) {
    return new URL(process.env.BACKEND_URL);
  }

  const host = (process.env.BACKEND_HOST || "127.0.0.1:8080").trim();
  if (host.startsWith("http://") || host.startsWith("https://")) {
    return new URL(host);
  }

  if (host.includes(":") && !host.includes(".")) {
    return new URL(`http://${host}`);
  }

  return new URL(`https://${host}`);
}

const mimeTypes = {
  ".html": "text/html; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".ico": "image/x-icon",
  ".png": "image/png",
  ".svg": "image/svg+xml",
  ".json": "application/json",
  ".woff2": "font/woff2",
};

function sendFile(filePath, res) {
  fs.readFile(filePath, (err, data) => {
    if (err) {
      res.writeHead(404);
      res.end("Not found");
      return;
    }

    const ext = path.extname(filePath);
    res.writeHead(200, {
      "Content-Type": mimeTypes[ext] || "application/octet-stream",
    });
    res.end(data);
  });
}

function proxyApi(req, res) {
  const options = {
    hostname: backend.hostname,
    port: backend.port || (backend.protocol === "https:" ? 443 : 80),
    path: req.url,
    method: req.method,
    headers: {
      ...req.headers,
      host: backend.host,
    },
    timeout: 120_000,
  };

  const proxyReq = backendClient.request(options, (proxyRes) => {
    res.writeHead(proxyRes.statusCode || 502, proxyRes.headers);
    proxyRes.pipe(res);
  });

  proxyReq.on("timeout", () => {
    proxyReq.destroy();
    res.writeHead(504);
    res.end("Gateway timeout");
  });

  proxyReq.on("error", (err) => {
    console.error("API proxy error:", err.message);
    res.writeHead(502);
    res.end("Bad gateway");
  });

  if (req.method === "GET" || req.method === "HEAD") {
    proxyReq.end();
    return;
  }

  req.pipe(proxyReq);
}

http
  .createServer((req, res) => {
    const url = req.url || "/";

    if (url.startsWith("/api/")) {
      proxyApi(req, res);
      return;
    }

    const safePath = path.normalize(url).replace(/^(\.\.[/\\])+/, "");
    const filePath = path.join(distDir, safePath === "/" ? "index.html" : safePath);

    if (!filePath.startsWith(distDir)) {
      res.writeHead(403);
      res.end("Forbidden");
      return;
    }

    fs.stat(filePath, (err, stats) => {
      if (!err && stats.isFile()) {
        sendFile(filePath, res);
        return;
      }

      sendFile(path.join(distDir, "index.html"), res);
    });
  })
  .listen(port, "0.0.0.0", () => {
    console.log(`Frontend on :${port}, API via ${backend.origin}`);
  });
