import { apiFetch } from "./client";
import type { CreateUserRequest, UpdateUserRequest, UserResponse, UserRole } from "../types";

export function listUsers(token: string, role?: UserRole): Promise<UserResponse[]> {
  const query = role ? `?role=${role}` : "";
  return apiFetch<UserResponse[]>(`/api/users${query}`, { token });
}

export function createUser(token: string, data: CreateUserRequest): Promise<UserResponse> {
  return apiFetch<UserResponse>("/api/users", { method: "POST", body: data, token });
}

export function updateUser(token: string, id: number, data: UpdateUserRequest): Promise<UserResponse> {
  return apiFetch<UserResponse>(`/api/users/${id}`, { method: "PUT", body: data, token });
}

export function deactivateUser(token: string, id: number): Promise<void> {
  return apiFetch<void>(`/api/users/${id}/deactivate`, { method: "PATCH", token });
}
