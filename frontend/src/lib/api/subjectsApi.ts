import { apiFetch } from "./client";
import type { CreateSubjectRequest, SubjectResponse, UpdateSubjectRequest } from "../types";

export function listSubjects(token: string): Promise<SubjectResponse[]> {
  return apiFetch<SubjectResponse[]>("/api/subjects", { token });
}

export function createSubject(token: string, data: CreateSubjectRequest): Promise<SubjectResponse> {
  return apiFetch<SubjectResponse>("/api/subjects", { method: "POST", body: data, token });
}

export function updateSubject(token: string, id: number, data: UpdateSubjectRequest): Promise<SubjectResponse> {
  return apiFetch<SubjectResponse>(`/api/subjects/${id}`, { method: "PUT", body: data, token });
}

export function deleteSubject(token: string, id: number): Promise<void> {
  return apiFetch<void>(`/api/subjects/${id}`, { method: "DELETE", token });
}
