import { apiFetch } from "./client";
import type { ClassSubjectResponse, CreateClassSubjectRequest } from "../types";

export function listClassSubjects(token: string): Promise<ClassSubjectResponse[]> {
  return apiFetch<ClassSubjectResponse[]>("/api/class-subjects", { token });
}

export function createClassSubject(token: string, data: CreateClassSubjectRequest): Promise<ClassSubjectResponse> {
  return apiFetch<ClassSubjectResponse>("/api/class-subjects", { method: "POST", body: data, token });
}

export function deleteClassSubject(token: string, id: number): Promise<void> {
  return apiFetch<void>(`/api/class-subjects/${id}`, { method: "DELETE", token });
}
