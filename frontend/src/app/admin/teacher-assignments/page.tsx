"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import {
  createTeacherAssignment,
  deleteTeacherAssignment,
  listTeacherAssignments,
} from "@/lib/api/teacherAssignmentsApi";
import { listUsers } from "@/lib/api/usersApi";
import { listClassSubjects } from "@/lib/api/classSubjectsApi";
import type { ClassSubjectResponse, TeacherAssignmentResponse, UserResponse } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Select } from "@/components/ui/Select";
import { Button } from "@/components/ui/Button";
import { Table, type Column } from "@/components/ui/Table";

export default function AdminTeacherAssignmentsPage() {
  const { token } = useAuth();
  const [assignments, setAssignments] = useState<TeacherAssignmentResponse[]>([]);
  const [teachers, setTeachers] = useState<UserResponse[]>([]);
  const [classSubjects, setClassSubjects] = useState<ClassSubjectResponse[]>([]);
  const [teacherId, setTeacherId] = useState("");
  const [classSubjectId, setClassSubjectId] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function load() {
    if (!token) return;
    setIsLoading(true);
    try {
      const [assignmentData, teacherData, classSubjectData] = await Promise.all([
        listTeacherAssignments(token),
        listUsers(token, "Teacher"),
        listClassSubjects(token),
      ]);
      setAssignments(assignmentData);
      setTeachers(teacherData);
      setClassSubjects(classSubjectData);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load data.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  async function onCreate(e: React.FormEvent) {
    e.preventDefault();
    if (!token || !teacherId || !classSubjectId) return;
    setError(null);
    setIsSubmitting(true);
    try {
      await createTeacherAssignment(token, { teacherId: Number(teacherId), classSubjectId: Number(classSubjectId) });
      setTeacherId("");
      setClassSubjectId("");
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to assign teacher.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function onDelete(a: TeacherAssignmentResponse) {
    if (!token) return;
    if (!confirm(`Remove ${a.teacherName} from ${a.subjectName} (${a.classCourseName})?`)) return;
    try {
      await deleteTeacherAssignment(token, a.id);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to remove assignment.");
    }
  }

  const columns: Column<TeacherAssignmentResponse>[] = [
    { header: "Teacher", render: (a) => a.teacherName },
    { header: "Class", render: (a) => a.classCourseName },
    { header: "Subject", render: (a) => a.subjectName },
    {
      header: "Actions",
      render: (a) => (
        <Button variant="danger" className="px-2 py-1 text-xs" onClick={() => onDelete(a)}>
          Remove
        </Button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Teacher Assignments</h1>
      <p className="text-sm text-zinc-600 dark:text-zinc-400">
        A teacher can only create/manage assignments for a class-subject they&apos;re assigned to here.
      </p>

      <Card>
        <h2 className="mb-4 text-lg font-medium">Assign teacher</h2>
        <form onSubmit={onCreate} className="grid grid-cols-1 gap-x-4 sm:grid-cols-3 sm:items-end">
          <Select label="Teacher" value={teacherId} onChange={(e) => setTeacherId(e.target.value)}>
            <option value="">Select a teacher</option>
            {teachers.map((t) => (
              <option key={t.id} value={t.id}>
                {t.fullName}
              </option>
            ))}
          </Select>
          <Select label="Class / Subject" value={classSubjectId} onChange={(e) => setClassSubjectId(e.target.value)}>
            <option value="">Select a class/subject</option>
            {classSubjects.map((cs) => (
              <option key={cs.id} value={cs.id}>
                {cs.classCourseName} — {cs.subjectName}
              </option>
            ))}
          </Select>
          <div className="mb-4">
            <Button type="submit" disabled={isSubmitting || !teacherId || !classSubjectId}>
              {isSubmitting ? "Assigning..." : "Assign"}
            </Button>
          </div>
        </form>
        {error && <p className="mt-3 text-sm text-red-600 dark:text-red-400">{error}</p>}
      </Card>

      <Card>
        <h2 className="mb-4 text-lg font-medium">All assignments</h2>
        {isLoading ? (
          <p className="text-sm text-zinc-500">Loading...</p>
        ) : (
          <Table columns={columns} rows={assignments} keyFor={(a) => a.id} emptyMessage="No teacher assignments yet." />
        )}
      </Card>
    </div>
  );
}
