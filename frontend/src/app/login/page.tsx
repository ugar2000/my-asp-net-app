"use client";

import { useState } from "react";
import { postJson } from "@/lib/api";
import { useAuthStore } from "@/store/auth-store";
import { useRouter } from "next/navigation";

interface LoginResponseDto {
  token: string;
  displayName: string;
  email: string;
  roles: string[];
  expiresAtUtc: string;
}

export default function LoginPage() {
  const router = useRouter();
  const setSession = useAuthStore((state) => state.setSession);
  const [tab, setTab] = useState<"login" | "register">("login");
  const [loginModel, setLoginModel] = useState({ email: "", password: "" });
  const [registerModel, setRegisterModel] = useState({ email: "", password: "", displayName: "", bio: "" });
  const [message, setMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const login = async () => {
    setLoading(true);
    setMessage(null);
    try {
      const response = await postJson("/api/auth/login", loginModel);
      const data = response as LoginResponseDto;
      setSession({
        token: data.token,
        displayName: data.displayName,
        email: data.email,
        roles: data.roles,
      });
      router.push("/");
    } catch (err: any) {
      setMessage(err?.message ?? "Не вдалося увійти");
    } finally {
      setLoading(false);
    }
  };

  const register = async () => {
    setLoading(true);
    setMessage(null);
    try {
      await postJson("/api/auth/register", registerModel);
      setMessage("Акаунт створено! Увійдіть за своїми даними.");
      setTab("login");
    } catch (err: any) {
      setMessage(err?.message ?? "Не вдалося зареєструватися");
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="mx-auto flex min-h-screen max-w-4xl flex-col items-center justify-center px-6 py-16">
      <section className="w-full rounded-3xl border border-indigo-500/40 bg-slate-900/70 p-10 shadow-2xl shadow-indigo-900/40">
        <h1 className="text-3xl font-semibold text-indigo-100">Лабораторний доступ</h1>
        <p className="mt-2 text-sm text-slate-400">Авторизація надає доступ до режиму клубу, публікацій та живих симуляцій.</p>

        <div className="mt-8 flex gap-4">
          <button className={`button-secondary ${tab === "login" ? "ring-2 ring-indigo-400" : "opacity-70"}`} onClick={() => setTab("login")}>Вхід</button>
          <button className={`button-secondary ${tab === "register" ? "ring-2 ring-indigo-400" : "opacity-70"}`} onClick={() => setTab("register")}>Реєстрація</button>
        </div>

        {tab === "login" ? (
          <div className="mt-6 space-y-4">
            <div>
              <label className="text-sm text-slate-300">Email</label>
              <input className="input-shell mt-1" value={loginModel.email} onChange={(e) => setLoginModel((prev) => ({ ...prev, email: e.target.value }))} />
            </div>
            <div>
              <label className="text-sm text-slate-300">Пароль</label>
              <input type="password" className="input-shell mt-1" value={loginModel.password} onChange={(e) => setLoginModel((prev) => ({ ...prev, password: e.target.value }))} />
            </div>
            <button className="button-primary" disabled={loading} onClick={login}>
              {loading ? "Входимо…" : "Увійти"}
            </button>
          </div>
        ) : (
          <div className="mt-6 space-y-4">
            <div>
              <label className="text-sm text-slate-300">Email</label>
              <input className="input-shell mt-1" value={registerModel.email} onChange={(e) => setRegisterModel((prev) => ({ ...prev, email: e.target.value }))} />
            </div>
            <div>
              <label className="text-sm text-slate-300">Пароль</label>
              <input type="password" className="input-shell mt-1" value={registerModel.password} onChange={(e) => setRegisterModel((prev) => ({ ...prev, password: e.target.value }))} />
            </div>
            <div>
              <label className="text-sm text-slate-300">Ім'я</label>
              <input className="input-shell mt-1" value={registerModel.displayName} onChange={(e) => setRegisterModel((prev) => ({ ...prev, displayName: e.target.value }))} />
            </div>
            <div>
              <label className="text-sm text-slate-300">Біографія</label>
              <textarea className="input-shell mt-1" value={registerModel.bio} onChange={(e) => setRegisterModel((prev) => ({ ...prev, bio: e.target.value }))} />
            </div>
            <button className="button-primary" disabled={loading} onClick={register}>
              {loading ? "Реєструємо…" : "Створити акаунт"}
            </button>
          </div>
        )}

        {message && <p className="mt-4 text-sm text-indigo-200">{message}</p>}
      </section>
    </main>
  );
}
