import { apiFetch } from "./client";
import type { AssignmentResponse, CreateAssignmentRequest, UpdateAssignmentRequest } from "../types";

export function listAssignments(token: string): Promise<AssignmentResponse[]> {
  return apiFetch<AssignmentResponse[]>("/api/assignments", { token });
}

export function getAssignment(token: string, id: number): Promise<AssignmentResponse> {
  return apiFetch<AssignmentResponse>(`/api/assignments/${id}`, { token });
}

export function createAssignment(token: string, data: CreateAssignmentRequest): Promise<AssignmentResponse> {
  return apiFetch<AssignmentResponse>("/api/assignments", { method: "POST", body: data, token });
}

export function updateAssignment(
  token: string,
  id: number,
  data: UpdateAssignmentRequest,
): Promise<AssignmentResponse> {
  return apiFetch<AssignmentResponse>(`/api/assignments/${id}`, { method: "PUT", body: data, token });
}

export function updateAssignmentStatus(
  token: string,
  id: number,
  status: "Draft" | "Published",
): Promise<AssignmentResponse> {
  return apiFetch<AssignmentResponse>(`/api/assignments/${id}/status`, {
    method: "PATCH",
    body: { status },
    token,
  });
}

export function deleteAssignment(token: string, id: number): Promise<void> {
  return apiFetch<void>(`/api/assignments/${id}`, { method: "DELETE", token });
}
