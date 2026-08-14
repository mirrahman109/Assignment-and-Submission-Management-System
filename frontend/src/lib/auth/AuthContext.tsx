"use client";

import { createContext, useCallback, useContext, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import type { UserSummary } from "../types";
import { login as loginApi } from "../api/authApi";
import { onUnauthorized } from "../api/client";
import { clearSession, loadSession, saveSession } from "./tokenStorage";

interface AuthContextValue {
  user: UserSummary | null;
  token: string | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<UserSummary>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserSummary | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    const session = loadSession();
    if (session) {
      setUser(session.user);
      setToken(session.token);
    }
    setIsLoading(false);
  }, []);

  const logout = useCallback(() => {
    clearSession();
    setUser(null);
    setToken(null);
    router.push("/login");
  }, [router]);

  useEffect(() => {
    onUnauthorized(logout);
  }, [logout]);

  const login = useCallback(async (email: string, password: string) => {
    const result = await loginApi(email, password);
    saveSession(result.token, result.user);
    setUser(result.user);
    setToken(result.token);
    return result.user;
  }, []);

  return (
    <AuthContext.Provider value={{ user, token, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return ctx;
}
