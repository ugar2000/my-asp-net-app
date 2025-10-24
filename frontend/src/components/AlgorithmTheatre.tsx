"use client";

import { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { SIGNALR_ALGORITHM_HUB } from "@/lib/config";
import clsx from "clsx";
import { AlgorithmFamily, PathfindingAlgorithmType, SortingAlgorithmType } from "@/types/algorithms";

interface AlgorithmStepDto {
  stepIndex: number;
  stateSnapshot: string;
  cost: number;
  heuristic: number;
}

export function AlgorithmTheatre() {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [family, setFamily] = useState<AlgorithmFamily>(AlgorithmFamily.Pathfinding);
  const [pathfinding, setPathfinding] = useState<PathfindingAlgorithmType>(PathfindingAlgorithmType.AStar);
  const [sorting, setSorting] = useState<SortingAlgorithmType>(SortingAlgorithmType.MergeSort);
  const [seed, setSeed] = useState<number>(42);
  const [status, setStatus] = useState<string>("Очікування запуску…");
  const [metrics, setMetrics] = useState<{ cost: number; heuristic: number }>({ cost: 0, heuristic: 0 });

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;
    ctx.fillStyle = "#0f172a";
    ctx.fillRect(0, 0, canvas.width, canvas.height);
  }, []);

  const renderSnapshot = (snapshot: string) => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    try {
      const payload = JSON.parse(snapshot);
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      ctx.fillStyle = "#0f172a";
      ctx.fillRect(0, 0, canvas.width, canvas.height);

      if (payload.gridSize) {
        const gridSize = payload.gridSize ?? 12;
        const cellSize = canvas.width / gridSize;

        ctx.strokeStyle = "rgba(99,102,241,0.25)";
        for (let i = 0; i <= gridSize; i++) {
          ctx.beginPath();
          ctx.moveTo(i * cellSize, 0);
          ctx.lineTo(i * cellSize, canvas.height);
          ctx.stroke();
          ctx.beginPath();
          ctx.moveTo(0, i * cellSize);
          ctx.lineTo(canvas.width, i * cellSize);
          ctx.stroke();
        }

        const drawNodes = (nodes: any[], color: string) => {
          ctx.fillStyle = color;
          nodes?.forEach((node) => {
            ctx.fillRect(node.x * cellSize, node.y * cellSize, cellSize, cellSize);
          });
        };

        drawNodes(payload.walls ?? [], "rgba(148,163,184,0.35)");
        drawNodes(payload.closed ?? [], "rgba(99,102,241,0.35)");
        drawNodes(payload.open ?? [], "rgba(56,189,248,0.4)");

        if (payload.current) {
          ctx.fillStyle = "rgba(16,185,129,0.8)";
          ctx.fillRect(payload.current.x * cellSize, payload.current.y * cellSize, cellSize, cellSize);
        }

        if (payload.goal) {
          ctx.fillStyle = "rgba(239,68,68,0.8)";
          ctx.fillRect(payload.goal.x * cellSize, payload.goal.y * cellSize, cellSize, cellSize);
        }
      } else if (payload.bars) {
        const bars: number[] = payload.bars ?? [];
        const width = canvas.width / bars.length;
        ctx.fillStyle = "rgba(79, 70, 229, 0.7)";
        bars.forEach((value: number, index: number) => {
          const barHeight = (value / Math.max(...bars)) * canvas.height;
          ctx.fillRect(index * width, canvas.height - barHeight, width - 2, barHeight);
        });
        const [start, end] = payload.active ?? [];
        if (start !== undefined && end !== undefined) {
          ctx.fillStyle = "rgba(56,189,248,0.8)";
          ctx.fillRect(start * width, 0, (end - start + 1) * width, canvas.height);
        }
      }
    } catch (error) {
      console.error("Failed to parse algorithm snapshot", error);
    }
  };

  const stopConnection = async () => {
    if (connectionRef.current) {
      await connectionRef.current.stop();
      connectionRef.current = null;
    }
  };

  const runSimulation = async () => {
    await stopConnection();

    setStatus("Підключення до лабораторії…");

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(SIGNALR_ALGORITHM_HUB)
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    connection.onclose(() => setStatus("З'єднання завершено."));

    await connection.start();

    setStatus("Трансляція почалась…");

    const stream = connection.stream<AlgorithmStepDto>(
      "StreamAlgorithmAsync",
      family,
      family === AlgorithmFamily.Pathfinding ? pathfinding : null,
      family === AlgorithmFamily.Sorting ? sorting : null,
      seed,
    );

    stream.subscribe({
      next: (step) => {
        renderSnapshot(step.stateSnapshot);
        setMetrics({ cost: step.cost, heuristic: step.heuristic });
        setStatus(`Крок ${step.stepIndex}`);
      },
      error: (err) => {
        console.error(err);
        setStatus("Помилка під час симуляції");
      },
      complete: () => setStatus("Симуляцію завершено."),
    });
  };

  useEffect(() => {
    return () => {
      stopConnection();
    };
  }, []);

  return (
    <section className="card-panel p-10" id="theatre">
      <div className="flex flex-col gap-6 md:flex-row md:items-start md:justify-between">
        <div>
          <p className="text-xs uppercase tracking-[0.4em] text-indigo-300">Алгоритмічний театр</p>
          <h2 className="mt-3 text-3xl font-semibold text-indigo-100">Візуалізація алгоритмів у реальному часі</h2>
          <p className="mt-3 max-w-2xl text-sm text-slate-300">
            Оберіть сортування або пошук шляху, задайте зерно генератора та спостерігайте, як сервер передає кроки симуляції через SignalR.
          </p>
        </div>
        <div className="flex flex-wrap gap-4 text-sm">
          <button
            className={clsx("button-secondary", family === AlgorithmFamily.Pathfinding && "ring-2 ring-indigo-400")}
            onClick={() => setFamily(AlgorithmFamily.Pathfinding)}
          >
            Пошук шляху
          </button>
          <button
            className={clsx("button-secondary", family === AlgorithmFamily.Sorting && "ring-2 ring-indigo-400")}
            onClick={() => setFamily(AlgorithmFamily.Sorting)}
          >
            Сортування
          </button>
        </div>
      </div>

      <div className="mt-8 grid gap-6 md:grid-cols-[2fr,1fr]">
        <canvas ref={canvasRef} width={640} height={360} className="w-full rounded-2xl border border-indigo-500/30 bg-slate-950"></canvas>
        <div className="space-y-4 rounded-2xl border border-indigo-500/20 bg-slate-900/80 p-6 text-sm text-slate-200">
          <div className="space-y-2">
            <label className="block text-xs uppercase tracking-[0.4em] text-indigo-200">Зерно</label>
            <input
              type="number"
              value={seed}
              onChange={(e) => setSeed(Number(e.target.value) || 0)}
              className="input-shell"
            />
          </div>

          {family === AlgorithmFamily.Pathfinding ? (
            <div className="space-y-2">
              <label className="block text-xs uppercase tracking-[0.4em] text-indigo-200">Алгоритм пошуку</label>
              <select
                value={pathfinding}
                onChange={(e) => setPathfinding(Number(e.target.value) as PathfindingAlgorithmType)}
                className="input-shell"
              >
                <option value={PathfindingAlgorithmType.AStar}>A*</option>
                <option value={PathfindingAlgorithmType.Dijkstra}>Дейкстра</option>
              </select>
            </div>
          ) : (
            <div className="space-y-2">
              <label className="block text-xs uppercase tracking-[0.4em] text-indigo-200">Сортування</label>
              <select
                value={sorting}
                onChange={(e) => setSorting(Number(e.target.value) as SortingAlgorithmType)}
                className="input-shell"
              >
                <option value={SortingAlgorithmType.MergeSort}>Merge Sort</option>
                <option value={SortingAlgorithmType.QuickSort}>Quick Sort</option>
                <option value={SortingAlgorithmType.HeapSort}>Heap Sort</option>
              </select>
            </div>
          )}

          <button className="button-primary w-full" onClick={runSimulation}>
            Запустити симуляцію
          </button>

          <div className="pt-3 text-xs uppercase tracking-[0.35em] text-indigo-200">Стан: {status}</div>
          <div className="flex gap-6 text-sm text-slate-300">
            <span>Вартість: <strong className="text-indigo-200">{metrics.cost.toFixed(2)}</strong></span>
            <span>Евристика: <strong className="text-indigo-200">{metrics.heuristic.toFixed(2)}</strong></span>
          </div>
        </div>
      </div>
    </section>
  );
}
