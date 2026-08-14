import { apiFetch } from "./client";
import type { ClassCourseResponse, CreateClassCourseRequest, UpdateClassCourseRequest } from "../types";

export function listClasses(token: string): Promise<ClassCourseResponse[]> {
  return apiFetch<ClassCourseResponse[]>("/api/classes", { token });
}

export function createClass(token: string, data: CreateClassCourseRequest): Promise<ClassCourseResponse> {
  return apiFetch<ClassCourseResponse>("/api/classes", { method: "POST", body: data, token });
}

export function updateClass(token: string, id: number, data: UpdateClassCourseRequest): Promise<ClassCourseResponse> {
  return apiFetch<ClassCourseResponse>(`/api/classes/${id}`, { method: "PUT", body: data, token });
}

export function deleteClass(token: string, id: number): Promise<void> {
  return apiFetch<void>(`/api/classes/${id}`, { method: "DELETE", token });
}
