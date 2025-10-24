"use client";

import { useState } from "react";
import { postJson } from "@/lib/api";

interface CompilerAnalysisResponseDto {
  syntaxTree: string;
  diagnostics: string[];
  executionOutput: string;
}

const DEFAULT_SCRIPT = `using System;\nusing System.Linq;\n\nvar numbers = Enumerable.Range(1, 5);\nforeach (var n in numbers)\n{\n    Console.WriteLine($"square({n}) = {n * n}");\n}`;

export function CompilerPlayground() {
  const [code, setCode] = useState(DEFAULT_SCRIPT);
  const [result, setResult] = useState<CompilerAnalysisResponseDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const analyze = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await postJson("/api/compiler/analyze", { code, runScript: true });
      setResult(response as CompilerAnalysisResponseDto);
    } catch (err: any) {
      setError(err?.message ?? "Не вдалося виконати аналіз");
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="card-panel p-10" id="compiler-playground">
      <div className="grid gap-6 md:grid-cols-[1.5fr,1fr]">
        <div>
          <p className="text-xs uppercase tracking-[0.4em] text-fuchsia-300">Пісочниця компілятора</p>
          <h2 className="mt-3 text-3xl font-semibold text-fuchsia-100">Roslyn scripting + AST</h2>
          <p className="mt-4 max-w-2xl text-sm text-slate-300">
            Вставте C#-код, і сервер побудує синтаксичне дерево разом із результатом виконання. Зручно для візуальних міні-лекцій про компіляцію.
          </p>
          <textarea
            className="input-shell mt-6 h-64 font-mono text-sm"
            value={code}
            onChange={(e) => setCode(e.target.value)}
          />
          <button className="button-primary mt-4" disabled={loading} onClick={analyze}>
            {loading ? "Аналізуємо…" : "Запустити Roslyn"}
          </button>
        </div>
        <div className="space-y-6 rounded-2xl border border-fuchsia-400/30 bg-slate-900/70 p-6 text-sm text-slate-200">
          {error && <p className="text-rose-300">{error}</p>}
          {result && (
            <>
              <div>
                <p className="text-xs uppercase tracking-[0.4em] text-fuchsia-300">Синтаксичне дерево</p>
                <pre className="mt-3 max-h-40 overflow-auto whitespace-pre-wrap rounded-xl bg-slate-950/80 p-3 text-xs text-slate-300">
                  {result.syntaxTree}
                </pre>
              </div>
              <div>
                <p className="text-xs uppercase tracking-[0.4em] text-fuchsia-300">Діагностика</p>
                {result.diagnostics.length === 0 ? (
                  <p className="mt-2 text-slate-300">Діагностика відсутня.</p>
                ) : (
                  <ul className="mt-2 space-y-2 text-xs text-amber-200">
                    {result.diagnostics.map((diag, index) => (
                      <li key={index}>{diag}</li>
                    ))}
                  </ul>
                )}
              </div>
              <div>
                <p className="text-xs uppercase tracking-[0.4em] text-fuchsia-300">Вивід скрипту</p>
                <pre className="mt-3 whitespace-pre-wrap rounded-xl bg-slate-950/80 p-3 text-xs text-emerald-200">
                  {result.executionOutput || "(немає виводу)"}
                </pre>
              </div>
            </>
          )}
        </div>
      </div>
    </section>
  );
}
