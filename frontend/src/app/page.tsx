"use client";

import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { AlgorithmTheatre } from "@/components/AlgorithmTheatre";
import { MachineLearningLab } from "@/components/MachineLearningLab";
import { GpuSimulator } from "@/components/GpuSimulator";
import { CompilerPlayground } from "@/components/CompilerPlayground";
import { ClubMode } from "@/components/ClubMode";
import { NewsFeed } from "@/components/NewsFeed";
import Link from "next/link";

export default function Home() {
  return (
    <div className="bg-lab-gradient text-slate-100 min-h-screen">
      <Header />
      <main className="mx-auto max-w-6xl px-6 py-16 space-y-16">
        <section className="grid gap-6 rounded-3xl border border-indigo-500/40 bg-slate-900/60 p-12 shadow-xl shadow-indigo-500/10">
          <p className="text-sm uppercase tracking-[0.6em] text-indigo-300">Codex Programming Collective</p>
          <h1 className="text-5xl font-semibold leading-tight text-indigo-100 glow-text">Там, де код стає наукою.</h1>
          <p className="max-w-3xl text-lg text-slate-300">
            Ласкаво просимо до дослідницької лабораторії клубу Codex. Тут алгоритми транслюються в реальному часі, ML.NET порівнюється з ONNX прямо в браузері, а учасники працюють у спільному режимі клубу.
          </p>
          <div className="flex flex-wrap gap-4 pt-2">
            <a className="button-primary" href="#theatre">Дослідити театр</a>
            <a className="button-secondary" href="#club-mode">Запустити режим клубу</a>
          </div>
        </section>

        <AlgorithmTheatre />
        <MachineLearningLab />
        <GpuSimulator />
        <CompilerPlayground />
        <ClubMode />

        <section className="card-panel p-10" id="news-preview">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs uppercase tracking-[0.4em] text-indigo-300">Останні статті</p>
              <h2 className="mt-3 text-3xl font-semibold text-indigo-100">Codex Dispatch</h2>
            </div>
            <Link href="/news" className="button-secondary">Всі публікації</Link>
          </div>
          <div className="mt-6">
            <NewsFeed compact />
          </div>
        </section>
      </main>
      <Footer />
    </div>
  );
}
