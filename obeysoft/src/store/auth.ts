import { create } from "zustand";

import {
  loginRequest,
  meRequest,
  registerRequest,
  tokenStorage,
  type Me
} from "../lib/api";

type AuthStatus = "idle" | "loading" | "authenticated" | "error";

type AuthState = {
  user: Me | null;
  status: AuthStatus;
  error?: string | null;
  register: (email: string, displayName: string, password: string) => Promise<void>;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  check: () => Promise<void>;
  setUser: (user: Me | null) => void;
};

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  status: "idle",
  error: null,
  async register(email, displayName, password) {
    set({ status: "loading", error: null });
    try {
      const { token } = await registerRequest(email, displayName, password);
      tokenStorage.set(token);
      const me = await meRequest();
      set({ user: me, status: "authenticated", error: null });
    } catch (error: any) {
      tokenStorage.clear();
      set({ status: "error", error: error?.message ?? "Kayıt başarısız" });
      throw error;
    }
  },
  async login(email, password) {
    set({ status: "loading", error: null });
    try {
      const { token } = await loginRequest(email, password);
      tokenStorage.set(token);
      const me = await meRequest();
      set({ user: me, status: "authenticated", error: null });
    } catch (error: any) {
      tokenStorage.clear();
      set({ status: "error", error: error?.message ?? "Giriş başarısız" });
      throw error;
    }
  },
  logout() {
    tokenStorage.clear();
    set({ user: null, status: "idle", error: null });
  },
  async check() {
    const { user, status } = get();
    const token = tokenStorage.get();
    if (!token) {
      set({ user: null, status: "idle" });
      return;
    }
    if (user && status === "authenticated") return;

    set({ status: "loading" });
    try {
      const me = await meRequest();
      set({ user: me, status: "authenticated", error: null });
    } catch (error) {
      tokenStorage.clear();
      set({ user: null, status: "idle" });
      throw error;
    }
  },
  setUser(next) {
    set({ user: next });
  }
}));


