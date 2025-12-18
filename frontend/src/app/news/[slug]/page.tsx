"use client";

import { useEffect, useState, use } from "react";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { getJson } from "@/lib/api";
import { NewsArticleDto } from "@/types/news";
import { marked } from "marked";
import Link from "next/link";

export default function ArticlePage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = use(params);
  const [article, setArticle] = useState<NewsArticleDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    getJson<NewsArticleDto>(`/api/news/${slug}`)
      .then(setArticle)
      .catch((err) => {
        console.error(err);
        setError(true);
      })
      .finally(() => setLoading(false));
  }, [slug]);

  return (
    <div className="bg-lab-gradient text-slate-100 min-h-screen flex flex-col">
      <Header />
      <main className="mx-auto max-w-4xl px-6 py-16 flex-grow w-full">
        {loading && <p className="text-slate-400">Завантаження…</p>}
        {error && <p className="text-rose-300">Не вдалося завантажити статтю.</p>}
        {article && (
          <article className="prose prose-invert max-w-none">
            <div className="mb-8">
                <Link href="/news" className="text-indigo-400 hover:text-indigo-300 mb-4 inline-block">← Назад до новин</Link>
                <p className="text-xs uppercase tracking-[0.4em] text-indigo-300 mt-2">
                {article.authorName} • {new Date(article.publishedAtUtc).toLocaleString("uk-UA")}
                </p>
                <h1 className="text-4xl font-bold text-indigo-100 mt-2">{article.title}</h1>
            </div>
            
            <div
              className="mt-8 space-y-6"
              dangerouslySetInnerHTML={{ __html: marked.parse(article.content) }}
            />
          </article>
        )}
      </main>
      <Footer />
    </div>
  );
}
