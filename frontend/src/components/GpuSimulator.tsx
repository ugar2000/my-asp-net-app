"use client";

import { useEffect, useRef, useState } from "react";

export function GpuSimulator() {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const [feed, setFeed] = useState(0.055);
  const [kill, setKill] = useState(0.062);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    const { width, height } = canvas;
    const gradient = ctx.createLinearGradient(0, 0, width, height);
    gradient.addColorStop(0, "rgba(79, 70, 229, 0.6)");
    gradient.addColorStop(1, "rgba(14, 116, 144, 0.6)");
    ctx.fillStyle = gradient;
    ctx.fillRect(0, 0, width, height);

    ctx.fillStyle = "rgba(148, 163, 184, 0.35)";
    ctx.font = "16px var(--font-mono)";
    ctx.fillText(`feed: ${feed.toFixed(3)}`, 20, 40);
    ctx.fillText(`kill: ${kill.toFixed(3)}`, 20, 70);
  }, [feed, kill]);

  return (
    <section className="card-panel p-10" id="gpu-simulator">
      <div className="grid gap-6 md:grid-cols-[2fr,1fr]">
        <div>
          <p className="text-xs uppercase tracking-[0.4em] text-emerald-300">GPU Симулятор</p>
          <h2 className="mt-3 text-3xl font-semibold text-emerald-100">Реакційно-дифузна система у WebGPU</h2>
          <p className="mt-4 max-w-2xl text-sm text-slate-300">
            У продакшн-версії цей модуль запускає реакційно-дифузне моделювання через WebGPU. Панель нижче дозволяє змінювати параметри фунгіцид-поживної моделі.
          </p>
        </div>
        <div className="rounded-2xl border border-emerald-400/30 bg-slate-900/70 p-6 text-sm text-slate-200">
          <label className="block text-xs uppercase tracking-[0.4em] text-emerald-200">Feed rate</label>
          <input
            className="input-shell mt-2"
            type="range"
            min="0.01"
            max="0.09"
            step="0.001"
            value={feed}
            onChange={(e) => setFeed(Number(e.target.value))}
          />
          <label className="mt-4 block text-xs uppercase tracking-[0.4em] text-emerald-200">Kill rate</label>
          <input
            className="input-shell mt-2"
            type="range"
            min="0.01"
            max="0.09"
            step="0.001"
            value={kill}
            onChange={(e) => setKill(Number(e.target.value))}
          />
        </div>
      </div>
      <canvas ref={canvasRef} width={640} height={360} className="mt-8 w-full rounded-2xl border border-emerald-400/30 bg-slate-950"></canvas>
    </section>
  );
}
