"use client";

import Link from "next/link";
import { useAuthStore } from "@/store/auth-store";
import { useRouter } from "next/navigation";
import { useState } from "react";

const navItems = [
  { href: "/#theatre", label: "Алгоритмічний театр" },
  { href: "/#ml-lab", label: "ML Лабораторія" },
  { href: "/#gpu-simulator", label: "GPU Симулятор" },
  { href: "/#compiler-playground", label: "Пісочниця компілятора" },
  { href: "/#club-mode", label: "Режим клубу" },
  { href: "/news", label: "Хроніки" },
];

export function Header() {
  const { token, displayName, roles, clear } = useAuthStore();
  const router = useRouter();
  const [menuOpen, setMenuOpen] = useState(false);

  const logout = () => {
    clear();
    router.push("/");
  };

  const isAdmin = roles.includes("Admin");

  return (
    <header className="sticky top-0 z-50 backdrop-blur bg-slate-950/70 border-b border-indigo-500/30">
      <nav className="mx-auto flex max-w-6xl items-center justify-between px-6 py-5">
        <Link href="/" className="flex items-center space-x-3">
          <span className="text-2xl font-semibold tracking-widest text-indigo-300 glow-text">CODEX CLUB</span>
        </Link>
        <div className="hidden items-center gap-8 text-sm uppercase tracking-[0.35em] md:flex">
          {navItems.map((item) => (
            <Link key={item.href} href={item.href} className="nav-link">
              {item.label}
            </Link>
          ))}
        </div>
        <div className="flex items-center gap-3">
          {token ? (
            <div className="flex items-center gap-3">
              <span className="text-sm text-slate-300">{displayName}</span>
              {isAdmin && (
                <Link href="/admin/news" className="button-secondary hidden md:inline-flex">
                  Публікації
                </Link>
              )}
              <button onClick={logout} className="button-secondary">
                Вийти
              </button>
            </div>
          ) : (
            <Link href="/login" className="button-secondary">
              Увійти
            </Link>
          )}
          <button
            className="md:hidden text-slate-200"
            onClick={() => setMenuOpen((prev) => !prev)}
            aria-label="Toggle navigation"
          >
            ☰
          </button>
        </div>
      </nav>
      {menuOpen && (
        <div className="md:hidden border-t border-indigo-500/20 bg-slate-950/90 px-6 py-4 space-y-4">
          {navItems.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className="block nav-link"
              onClick={() => setMenuOpen(false)}
            >
              {item.label}
            </Link>
          ))}
        </div>
      )}
    </header>
  );
}
