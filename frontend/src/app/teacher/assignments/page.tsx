"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { createAssignment, deleteAssignment, listAssignments, updateAssignmentStatus } from "@/lib/api/assignmentsApi";
import { listTeacherAssignments } from "@/lib/api/teacherAssignmentsApi";
import type { AssignmentResponse } from "@/lib/types";
import { createAssignmentSchema, type CreateAssignmentFormValues } from "@/lib/validation/assignmentSchemas";
import { Card } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Textarea } from "@/components/ui/Textarea";
import { Select } from "@/components/ui/Select";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { Table, type Column } from "@/components/ui/Table";

export default function TeacherAssignmentsPage() {
  const { token } = useAuth();
  const [assignments, setAssignments] = useState<AssignmentResponse[]>([]);
  const [classSubjectOptions, setClassSubjectOptions] = useState<{ id: number; label: string }[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CreateAssignmentFormValues>({
    resolver: zodResolver(createAssignmentSchema),
    defaultValues: { allowLateSubmission: false, publishImmediately: false },
  });

  async function load() {
    if (!token) return;
    setIsLoading(true);
    try {
      const [assignmentData, myTeachingAssignments] = await Promise.all([
        listAssignments(token),
        listTeacherAssignments(token),
      ]);
      setAssignments(assignmentData);
      const seen = new Set<number>();
      const options = myTeachingAssignments
        .filter((ta) => (seen.has(ta.classSubjectId) ? false : (seen.add(ta.classSubjectId), true)))
        .map((ta) => ({ id: ta.classSubjectId, label: `${ta.classCourseName} — ${ta.subjectName}` }));
      setClassSubjectOptions(options);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load assignments.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  async function onCreate(values: CreateAssignmentFormValues) {
    if (!token) return;
    setError(null);
    try {
      await createAssignment(token, {
        title: values.title,
        description: values.description,
        classSubjectId: Number(values.classSubjectId),
        deadline: new Date(values.deadline).toISOString(),
        maxMarks: values.maxMarks,
        allowLateSubmission: values.allowLateSubmission,
        publishImmediately: values.publishImmediately,
      });
      reset({ allowLateSubmission: false, publishImmediately: false, title: "", description: "", classSubjectId: "", deadline: "" });
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create assignment.");
    }
  }

  async function onToggleStatus(a: AssignmentResponse) {
    if (!token) return;
    try {
      await updateAssignmentStatus(token, a.id, a.status === "Published" ? "Draft" : "Published");
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update status.");
    }
  }

  async function onDelete(a: AssignmentResponse) {
    if (!token) return;
    if (!confirm(`Delete "${a.title}"? This only works if no one has submitted yet.`)) return;
    try {
      await deleteAssignment(token, a.id);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete assignment.");
    }
  }

  const columns: Column<AssignmentResponse>[] = [
    { header: "Title", render: (a) => a.title },
    { header: "Class / Subject", render: (a) => `${a.classCourseName} — ${a.subjectName}` },
    { header: "Deadline", render: (a) => new Date(a.deadline).toLocaleString() },
    { header: "Status", render: (a) => <Badge>{a.status}</Badge> },
    {
      header: "Actions",
      render: (a) => (
        <div className="flex flex-wrap gap-2">
          <Link href={`/teacher/assignments/${a.id}`} className="text-xs underline">
            Edit
          </Link>
          <Link href={`/teacher/assignments/${a.id}/submissions`} className="text-xs underline">
            Submissions
          </Link>
          <Button variant="secondary" className="px-2 py-1 text-xs" onClick={() => onToggleStatus(a)}>
            {a.status === "Published" ? "Unpublish" : "Publish"}
          </Button>
          <Button variant="danger" className="px-2 py-1 text-xs" onClick={() => onDelete(a)}>
            Delete
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Assignments</h1>

      <Card>
        <h2 className="mb-4 text-lg font-medium">New assignment</h2>
        {classSubjectOptions.length === 0 && !isLoading ? (
          <p className="text-sm text-zinc-500">
            You&apos;re not assigned to teach any class/subject yet — ask an Admin to assign you one first.
          </p>
        ) : (
          <form onSubmit={handleSubmit(onCreate)} className="grid grid-cols-1 gap-x-4 sm:grid-cols-2">
            <Input label="Title" {...register("title")} error={errors.title?.message} />
            <Select label="Class / Subject" {...register("classSubjectId")} error={errors.classSubjectId?.message}>
              <option value="">Select a class/subject</option>
              {classSubjectOptions.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.label}
                </option>
              ))}
            </Select>
            <div className="sm:col-span-2">
              <Textarea label="Description" {...register("description")} error={errors.description?.message} />
            </div>
            <Input label="Deadline" type="datetime-local" {...register("deadline")} error={errors.deadline?.message} />
            <Input
              label="Max marks"
              type="number"
              step="1"
              {...register("maxMarks", { valueAsNumber: true })}
              error={errors.maxMarks?.message}
            />
            <label className="mb-4 flex items-center gap-2 text-sm">
              <input type="checkbox" {...register("allowLateSubmission")} />
              Allow late submission
            </label>
            <label className="mb-4 flex items-center gap-2 text-sm">
              <input type="checkbox" {...register("publishImmediately")} />
              Publish immediately (otherwise saved as draft)
            </label>
            <div className="sm:col-span-2">
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? "Creating..." : "Create assignment"}
              </Button>
            </div>
          </form>
        )}
        {error && <p className="mt-3 text-sm text-red-600 dark:text-red-400">{error}</p>}
      </Card>

      <Card>
        <h2 className="mb-4 text-lg font-medium">Your assignments</h2>
        {isLoading ? (
          <p className="text-sm text-zinc-500">Loading...</p>
        ) : (
          <Table columns={columns} rows={assignments} keyFor={(a) => a.id} emptyMessage="No assignments yet." />
        )}
      </Card>
    </div>
  );
}
