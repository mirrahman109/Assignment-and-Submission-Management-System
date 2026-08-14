import { apiFetch } from "./client";
import type {
  CreateSubmissionRequest,
  GradeSubmissionRequest,
  SubmissionResponse,
  SubmissionStatus,
  UpdateSubmissionRequest,
} from "../types";

export function createSubmission(
  token: string,
  assignmentId: number,
  data: CreateSubmissionRequest,
): Promise<SubmissionResponse> {
  return apiFetch<SubmissionResponse>(`/api/assignments/${assignmentId}/submissions`, {
    method: "POST",
    body: data,
    token,
  });
}

export function listSubmissionsForAssignment(token: string, assignmentId: number): Promise<SubmissionResponse[]> {
  return apiFetch<SubmissionResponse[]>(`/api/assignments/${assignmentId}/submissions`, { token });
}

export function listMySubmissions(token: string): Promise<SubmissionResponse[]> {
  return apiFetch<SubmissionResponse[]>("/api/submissions/mine", { token });
}

export function getSubmission(token: string, id: number): Promise<SubmissionResponse> {
  return apiFetch<SubmissionResponse>(`/api/submissions/${id}`, { token });
}

export function updateSubmission(
  token: string,
  id: number,
  data: UpdateSubmissionRequest,
): Promise<SubmissionResponse> {
  return apiFetch<SubmissionResponse>(`/api/submissions/${id}`, { method: "PUT", body: data, token });
}

export function gradeSubmission(
  token: string,
  id: number,
  data: GradeSubmissionRequest,
): Promise<SubmissionResponse> {
  return apiFetch<SubmissionResponse>(`/api/submissions/${id}/grade`, { method: "PUT", body: data, token });
}

export function updateSubmissionStatus(
  token: string,
  id: number,
  status: SubmissionStatus,
): Promise<SubmissionResponse> {
  return apiFetch<SubmissionResponse>(`/api/submissions/${id}/status`, {
    method: "PATCH",
    body: { status },
    token,
  });
}
