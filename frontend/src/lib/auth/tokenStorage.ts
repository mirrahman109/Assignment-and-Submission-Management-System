import type { UserSummary } from "../types";

const TOKEN_KEY = "assignment_system_token";
const USER_KEY = "assignment_system_user";

// Mirrors the token into a plain (non-httpOnly) cookie purely so proxy.ts can see
// "is a token present" at the edge for UX redirects. The API itself never reads this
// cookie — every request explicitly attaches the Authorization header from localStorage.
const TOKEN_COOKIE = "auth_token";

export function saveSession(token: string, user: UserSummary) {
  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
  document.cookie = `${TOKEN_COOKIE}=${token}; path=/; SameSite=Lax`;
}

export function clearSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
  document.cookie = `${TOKEN_COOKIE}=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT`;
}

export function loadSession(): { token: string; user: UserSummary } | null {
  const token = localStorage.getItem(TOKEN_KEY);
  const userJson = localStorage.getItem(USER_KEY);
  if (!token || !userJson) {
    return null;
  }
  try {
    return { token, user: JSON.parse(userJson) as UserSummary };
  } catch {
    return null;
  }
}
