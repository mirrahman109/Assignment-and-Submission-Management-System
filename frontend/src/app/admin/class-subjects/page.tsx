"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { createClassSubject, deleteClassSubject, listClassSubjects } from "@/lib/api/classSubjectsApi";
import { listClasses } from "@/lib/api/classesApi";
import { listSubjects } from "@/lib/api/subjectsApi";
import type { ClassCourseResponse, ClassSubjectResponse, SubjectResponse } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Select } from "@/components/ui/Select";
import { Button } from "@/components/ui/Button";
import { Table, type Column } from "@/components/ui/Table";

export default function AdminClassSubjectsPage() {
  const { token } = useAuth();
  const [links, setLinks] = useState<ClassSubjectResponse[]>([]);
  const [classes, setClasses] = useState<ClassCourseResponse[]>([]);
  const [subjects, setSubjects] = useState<SubjectResponse[]>([]);
  const [classId, setClassId] = useState("");
  const [subjectId, setSubjectId] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function load() {
    if (!token) return;
    setIsLoading(true);
    try {
      const [linkData, classData, subjectData] = await Promise.all([
        listClassSubjects(token),
        listClasses(token),
        listSubjects(token),
      ]);
      setLinks(linkData);
      setClasses(classData);
      setSubjects(subjectData);
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
    if (!token || !classId || !subjectId) return;
    setError(null);
    setIsSubmitting(true);
    try {
      await createClassSubject(token, { classCourseId: Number(classId), subjectId: Number(subjectId) });
      setClassId("");
      setSubjectId("");
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to link class and subject.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function onDelete(link: ClassSubjectResponse) {
    if (!token) return;
    if (!confirm(`Remove ${link.subjectName} from ${link.classCourseName}?`)) return;
    try {
      await deleteClassSubject(token, link.id);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to remove link.");
    }
  }

  const columns: Column<ClassSubjectResponse>[] = [
    { header: "Class", render: (l) => l.classCourseName },
    { header: "Subject", render: (l) => l.subjectName },
    {
      header: "Actions",
      render: (l) => (
        <Button variant="danger" className="px-2 py-1 text-xs" onClick={() => onDelete(l)}>
          Remove
        </Button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Class ↔ Subject Links</h1>
      <p className="text-sm text-zinc-600 dark:text-zinc-400">
        An assignment is created against one of these pairs — link a class to a subject before assigning a teacher to teach it.
      </p>

      <Card>
        <h2 className="mb-4 text-lg font-medium">Add link</h2>
        <form onSubmit={onCreate} className="grid grid-cols-1 gap-x-4 sm:grid-cols-3 sm:items-end">
          <Select label="Class" value={classId} onChange={(e) => setClassId(e.target.value)}>
            <option value="">Select a class</option>
            {classes.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </Select>
          <Select label="Subject" value={subjectId} onChange={(e) => setSubjectId(e.target.value)}>
            <option value="">Select a subject</option>
            {subjects.map((s) => (
              <option key={s.id} value={s.id}>
                {s.name}
              </option>
            ))}
          </Select>
          <div className="mb-4">
            <Button type="submit" disabled={isSubmitting || !classId || !subjectId}>
              {isSubmitting ? "Linking..." : "Add link"}
            </Button>
          </div>
        </form>
        {error && <p className="mt-3 text-sm text-red-600 dark:text-red-400">{error}</p>}
      </Card>

      <Card>
        <h2 className="mb-4 text-lg font-medium">All links</h2>
        {isLoading ? (
          <p className="text-sm text-zinc-500">Loading...</p>
        ) : (
          <Table columns={columns} rows={links} keyFor={(l) => l.id} emptyMessage="No links yet." />
        )}
      </Card>
    </div>
  );
}
