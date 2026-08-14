"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { createSubject, deleteSubject, listSubjects } from "@/lib/api/subjectsApi";
import type { SubjectResponse } from "@/lib/types";
import { subjectSchema, type SubjectFormValues } from "@/lib/validation/catalogSchemas";
import { Card } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import { Table, type Column } from "@/components/ui/Table";

export default function AdminSubjectsPage() {
  const { token } = useAuth();
  const [subjects, setSubjects] = useState<SubjectResponse[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<SubjectFormValues>({ resolver: zodResolver(subjectSchema) });

  async function load() {
    if (!token) return;
    setIsLoading(true);
    try {
      setSubjects(await listSubjects(token));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load subjects.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  async function onCreate(values: SubjectFormValues) {
    if (!token) return;
    setError(null);
    try {
      await createSubject(token, values);
      reset();
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create subject.");
    }
  }

  async function onDeactivate(s: SubjectResponse) {
    if (!token) return;
    if (!confirm(`Deactivate "${s.name}"? It will no longer appear in this list once deactivated.`)) return;
    try {
      await deleteSubject(token, s.id);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to deactivate subject.");
    }
  }

  const columns: Column<SubjectResponse>[] = [
    { header: "Name", render: (s) => s.name },
    { header: "Code", render: (s) => s.code },
    {
      header: "Actions",
      render: (s) => (
        <Button variant="danger" className="px-2 py-1 text-xs" onClick={() => onDeactivate(s)}>
          Deactivate
        </Button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Subjects</h1>

      <Card>
        <h2 className="mb-4 text-lg font-medium">Add subject</h2>
        <form onSubmit={handleSubmit(onCreate)} className="grid grid-cols-1 gap-x-4 sm:grid-cols-2">
          <Input label="Name" {...register("name")} error={errors.name?.message} />
          <Input label="Code" {...register("code")} error={errors.code?.message} />
          <div className="sm:col-span-2">
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Creating..." : "Create subject"}
            </Button>
          </div>
        </form>
        {error && <p className="mt-3 text-sm text-red-600 dark:text-red-400">{error}</p>}
      </Card>

      <Card>
        <h2 className="mb-4 text-lg font-medium">All subjects</h2>
        {isLoading ? (
          <p className="text-sm text-zinc-500">Loading...</p>
        ) : (
          <Table columns={columns} rows={subjects} keyFor={(s) => s.id} emptyMessage="No subjects yet." />
        )}
      </Card>
    </div>
  );
}
