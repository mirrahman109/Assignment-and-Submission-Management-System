import { apiFetch } from "./client";
import type { CreateTeacherAssignmentRequest, TeacherAssignmentResponse } from "../types";

export function listTeacherAssignments(token: string): Promise<TeacherAssignmentResponse[]> {
  return apiFetch<TeacherAssignmentResponse[]>("/api/teacher-assignments", { token });
}

export function createTeacherAssignment(
  token: string,
  data: CreateTeacherAssignmentRequest,
): Promise<TeacherAssignmentResponse> {
  return apiFetch<TeacherAssignmentResponse>("/api/teacher-assignments", { method: "POST", body: data, token });
}

export function deleteTeacherAssignment(token: string, id: number): Promise<void> {
  return apiFetch<void>(`/api/teacher-assignments/${id}`, { method: "DELETE", token });
}
