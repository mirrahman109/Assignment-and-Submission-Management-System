const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

export class ApiError extends Error {
  status: number;
  errors: Record<string, string[]> | null;

  constructor(status: number, message: string, errors: Record<string, string[]> | null) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.errors = errors;
  }
}

interface ProblemResponse {
  title?: string;
  errors?: Record<string, string[]>;
}

interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
  body?: unknown;
  token?: string | null;
  /** Set on the login request: a 401 there means "wrong credentials", not "session expired",
   *  so it must not trigger the global logout redirect. */
  ignoreUnauthorizedHandler?: boolean;
}

// Registered by AuthContext so a 401 from any request can trigger a logout without
// this module needing to import React/AuthContext and create a circular dependency.
type UnauthorizedListener = () => void;
let unauthorizedListener: UnauthorizedListener | null = null;

export function onUnauthorized(listener: UnauthorizedListener) {
  unauthorizedListener = listener;
}

export async function apiFetch<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = "GET", body, token, ignoreUnauthorizedHandler = false } = options;

  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (response.status === 204) {
    return undefined as T;
  }

  const data = (await response.json().catch(() => null)) as ProblemResponse | T | null;

  if (!response.ok) {
    if (response.status === 401 && !ignoreUnauthorizedHandler) {
      unauthorizedListener?.();
    }
    const problem = data as ProblemResponse | null;
    throw new ApiError(response.status, problem?.title ?? "Request failed", problem?.errors ?? null);
  }

  return data as T;
}
