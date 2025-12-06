"use client";

import useSWR from "swr";
import { getJson, postJson } from "@/lib/api";
import { NewsArticleDto, NewsArticleSummaryDto, NewsArticleCreateDto } from "@/types/news";
import { useAuthStore } from "@/store/auth-store";
import { useEffect, useMemo, useState } from "react";
import { marked } from "marked";

const fetcher = (url: string, token?: string) => getJson<NewsArticleSummaryDto[]>(url, token);

export function NewsFeed({ compact = false }: { compact?: boolean } = {}) {
  const { token, roles } = useAuthStore();
  const { data, mutate, isLoading, error } = useSWR(["/api/news", token], ([url, auth]) => fetcher(url, auth ?? undefined));
  const [selected, setSelected] = useState<NewsArticleDto | null>(null);
  const isAdmin = useMemo(() => roles.includes("Admin"), [roles]);

  const openArticle = async (slug: string) => {
    try {
      const article = await getJson<NewsArticleDto>(`/api/news/${slug}`);
      setSelected(article);
      if (typeof window !== "undefined") {
        window.history.replaceState(null, "", `/news#${slug}`);
      }
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    if (!compact && typeof window !== "undefined" && window.location.hash) {
      const slug = window.location.hash.replace("#", "");
      if (slug) {
        openArticle(slug);
      }
    }
  }, [compact]);

  const articles = compact && data ? data.slice(0, 3) : data;

  return (
    <section
      className={compact ? "rounded-3xl border border-indigo-500/30 bg-slate-900/60 p-8" : "card-panel p-10"}
      id="news"
    >
      {!compact && (
        <div className="flex flex-col gap-6 md:flex-row md:items-start md:justify-between">
          <div>
            <p className="text-xs uppercase tracking-[0.4em] text-indigo-300">Хроніки лабораторії</p>
            <h2 className="mt-3 text-3xl font-semibold text-indigo-100">Оновлення Codex Dispatch</h2>
            <p className="mt-3 max-w-3xl text-sm text-slate-300">
              Всі події дослідницької лабораторії: релізи, алгоритмічні експерименти, колаборації зі студентами. Публікації доступні через REST API, тому фронт легко будувати на будь-якому стеку.
            </p>
          </div>
          <div className="text-sm text-slate-300">
            {isAdmin && token ? <AdminNewsForm onPublished={() => mutate()} /> : <p>Авторизація необхідна для публікацій.</p>}
          </div>
        </div>
      )}

      <div className={compact ? "mt-8 space-y-4" : "mt-8 grid gap-6 md:grid-cols-[1.4fr,1fr]"}>
        <div className="space-y-4">
          {isLoading && <p className="text-slate-400">Завантаження…</p>}
          {error && <p className="text-rose-300">Не вдалося завантажити статті.</p>}
          {articles?.map((article) => (
            <article key={article.id} className="rounded-2xl border border-indigo-500/30 bg-slate-900/65 p-6 transition hover:border-indigo-400/60">
              <div className="flex flex-col gap-2">
                <p className="text-xs uppercase tracking-[0.4em] text-indigo-300">
                  {article.authorName} • {new Date(article.publishedAtUtc).toLocaleDateString("uk-UA")}
                </p>
                <h3 className="text-2xl font-semibold text-slate-100">{article.title}</h3>
                <p className="text-sm text-slate-300">{article.excerpt}</p>
                {compact ? (
                  <a className="button-secondary w-fit" href={`/news#${article.slug}`}>
                    Перейти до статті
                  </a>
                ) : (
                  <button className="button-secondary w-fit" onClick={() => openArticle(article.slug)}>
                    Читати статтю
                  </button>
                )}
              </div>
            </article>
          ))}
        </div>
        {!compact && (
          <div className="rounded-2xl border border-indigo-500/30 bg-slate-900/70 p-6 text-sm text-slate-200">
            {selected ? (
              <div className="prose prose-invert max-w-none">
                <h3 className="text-2xl font-semibold text-indigo-100">{selected.title}</h3>
                <p className="text-xs uppercase tracking-[0.4em] text-indigo-300">
                  {selected.authorName} • {new Date(selected.publishedAtUtc).toLocaleString("uk-UA")}
                </p>
                <div
                  className="mt-4 space-y-4"
                  dangerouslySetInnerHTML={{ __html: marked.parse(selected.content) }}
                />
              </div>
            ) : (
              <p className="text-slate-400">Оберіть статтю, щоб переглянути зміст.</p>
            )}
          </div>
        )}
      </div>
    </section>
  );
}

function AdminNewsForm({ onPublished }: { onPublished: () => void }) {
  const [form, setForm] = useState<NewsArticleCreateDto>({
    title: "",
    slug: "",
    excerpt: "",
    content: "",
    coverImageUrl: "",
  });
  const [submitting, setSubmitting] = useState(false);
  const { token } = useAuthStore();

  const submit = async () => {
    if (!token) return;
    setSubmitting(true);
    try {
      await postJson("/api/news", form, token);
      setForm({ title: "", slug: "", excerpt: "", content: "", coverImageUrl: "" });
      onPublished();
    } catch (err) {
      console.error(err);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="rounded-2xl border border-indigo-500/30 bg-slate-900/70 p-4 text-sm text-slate-200">
      <p className="text-xs uppercase tracking-[0.4em] text-indigo-300">Публікувати</p>
      <input
        className="input-shell mt-3"
        placeholder="Заголовок"
        value={form.title}
        onChange={(e) => setForm((prev) => ({ ...prev, title: e.target.value }))}
      />
      <input
        className="input-shell mt-3"
        placeholder="Slug"
        value={form.slug}
        onChange={(e) => setForm((prev) => ({ ...prev, slug: e.target.value }))}
      />
      <textarea
        className="input-shell mt-3"
        placeholder="Короткий опис"
        value={form.excerpt}
        onChange={(e) => setForm((prev) => ({ ...prev, excerpt: e.target.value }))}
      />
      <textarea
        className="input-shell mt-3 h-32"
        placeholder="Markdown контент"
        value={form.content}
        onChange={(e) => setForm((prev) => ({ ...prev, content: e.target.value }))}
      />
      <input
        className="input-shell mt-3"
        placeholder="URL обкладинки (опційно)"
        value={form.coverImageUrl}
        onChange={(e) => setForm((prev) => ({ ...prev, coverImageUrl: e.target.value }))}
      />
      <button className="button-primary mt-4 w-full" onClick={submit} disabled={submitting}>
        {submitting ? "Публікуємо…" : "Додати статтю"}
      </button>
    </div>
  );
}

//dotnet restore
// dotnet watch run --project src/Server/NetAppForVika.Server.csproj --urls http://localhost:5050
