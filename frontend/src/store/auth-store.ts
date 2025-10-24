"use client";

import { create } from "zustand";
import { persist } from "zustand/middleware";

interface AuthState {
  token?: string;
  displayName?: string;
  email?: string;
  roles: string[];
  setSession: (payload: { token: string; displayName: string; email: string; roles: string[] }) => void;
  clear: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      roles: [],
      setSession: (payload) =>
        set({
          token: payload.token,
          displayName: payload.displayName,
          email: payload.email,
          roles: payload.roles,
        }),
      clear: () => set({ token: undefined, displayName: undefined, email: undefined, roles: [] }),
    }),
    {
      name: "codexclub-auth",
      partialize: (state) => ({ token: state.token, displayName: state.displayName, email: state.email, roles: state.roles }),
    },
  ),
);
