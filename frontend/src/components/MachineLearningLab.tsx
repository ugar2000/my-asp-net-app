"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { postJson } from "@/lib/api";
import clsx from "clsx";

interface DigitPredictionResponseDto {
  sessionId: string;
  predictedDigit: number;
  confidence: number;
  clientPredictedDigit?: number | null;
  clientConfidence?: number | null;
  serverElapsedMilliseconds: number;
  clientElapsedMilliseconds?: number | null;
}

const CANVAS_SIZE = 280;
const TARGET_SIZE = 28;

export function MachineLearningLab() {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const [isDrawing, setIsDrawing] = useState(false);
  const [prediction, setPrediction] = useState<DigitPredictionResponseDto | null>(null);
  const [status, setStatus] = useState<string>("Намалюйте цифру та натисніть розпізнати");

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;
    ctx.fillStyle = "#010617";
    ctx.fillRect(0, 0, CANVAS_SIZE, CANVAS_SIZE);
  }, []);

  const handlePointer = useCallback((event: React.PointerEvent<HTMLCanvasElement>) => {
    const canvas = canvasRef.current;
    if (!canvas || !isDrawing) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    const rect = canvas.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;

    ctx.fillStyle = "rgba(255,255,255,0.95)";
    ctx.beginPath();
    ctx.arc(x, y, 12, 0, Math.PI * 2);
    ctx.fill();
  }, [isDrawing]);

  const capturePixels = () => {
    const canvas = canvasRef.current;
    if (!canvas) return [];

    const tmpCanvas = document.createElement("canvas");
    tmpCanvas.width = TARGET_SIZE;
    tmpCanvas.height = TARGET_SIZE;
    const tmpCtx = tmpCanvas.getContext("2d");
    if (!tmpCtx) return [];

    tmpCtx.drawImage(canvas, 0, 0, TARGET_SIZE, TARGET_SIZE);
    const { data } = tmpCtx.getImageData(0, 0, TARGET_SIZE, TARGET_SIZE);

    const pixels: number[] = [];
    for (let i = 0; i < data.length; i += 4) {
      // grayscale conversion
      const grayscale = data[i] * 0.299 + data[i + 1] * 0.587 + data[i + 2] * 0.114;
      pixels.push(grayscale / 255);
    }
    return pixels;
  };

  const clearCanvas = () => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;
    ctx.fillStyle = "#010617";
    ctx.fillRect(0, 0, CANVAS_SIZE, CANVAS_SIZE);
    setPrediction(null);
    setStatus("Намалюйте цифру та натисніть розпізнати");
  };

  const runInference = async () => {
    const pixels = capturePixels();
    if (!pixels.length) {
      return;
    }

    setStatus("Виконується розпізнавання…");
    try {
      const sessionId = crypto.randomUUID();
      const response = await postJson(
        "/api/ml/digit",
        {
          sessionId,
          pixels,
          width: TARGET_SIZE,
          height: TARGET_SIZE,
        },
      );
      const serverResult = response as DigitPredictionResponseDto;

      setPrediction(serverResult);
      setStatus("Готово! Порівняйте результати нижче.");
    } catch (error: any) {
      setStatus(error?.message ?? "Помилка під час розпізнавання");
    }
  };

  return (
    <section className="card-panel p-10" id="ml-lab">
      <div className="grid gap-6 md:grid-cols-[2fr,1fr]">
        <div>
          <p className="text-xs uppercase tracking-[0.4em] text-cyan-300">Лабораторія машинного навчання</p>
          <h2 className="mt-3 text-3xl font-semibold text-cyan-100">Порівняння ONNX у браузері та ML.NET на сервері</h2>
          <p className="mt-4 max-w-3xl text-sm text-slate-300">
            Намалюйте цифру на полотні. Браузер перетворить її у матрицю 28×28, а сервер виконає inference через ML.NET. Ви побачите прогноз та обчислену впевненість.
          </p>
          <div className="mt-6 flex flex-wrap gap-4 text-sm text-slate-300">
            <button className="button-primary" onClick={runInference}>Розпізнати</button>
            <button className="button-secondary" onClick={clearCanvas}>Очистити</button>
            <span className="ml-2 text-indigo-200">{status}</span>
          </div>
        </div>
        <div className="flex flex-col items-center gap-4">
          <canvas
            ref={canvasRef}
            width={CANVAS_SIZE}
            height={CANVAS_SIZE}
            className="w-full max-w-sm rounded-2xl border border-cyan-400/40 bg-slate-950"
            onPointerDown={(e) => {
              setIsDrawing(true);
              e.preventDefault();
              e.currentTarget.setPointerCapture(e.pointerId);
              handlePointer(e);
            }}
            onPointerMove={handlePointer}
            onPointerUp={(e) => {
              e.preventDefault();
              e.currentTarget.releasePointerCapture(e.pointerId);
              setIsDrawing(false);
            }}
            onPointerLeave={() => setIsDrawing(false)}
          />
          {prediction && (
            <div className="w-full rounded-2xl border border-cyan-500/30 bg-slate-900/70 p-6 text-sm text-slate-200">
              <p className="text-xs uppercase tracking-[0.4em] text-cyan-300">Результат ML.NET</p>
              <p className="mt-2 text-lg font-semibold text-cyan-100">
                {prediction.predictedDigit} ({(prediction.confidence * 100).toFixed(1)}%)
              </p>
              <p className="text-xs text-slate-400">Час виконання: {prediction.serverElapsedMilliseconds.toFixed(2)} мс</p>
            </div>
          )}
        </div>
      </div>
    </section>
  );
}
