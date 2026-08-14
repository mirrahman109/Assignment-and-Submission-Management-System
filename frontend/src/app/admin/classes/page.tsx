"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { createClass, deleteClass, listClasses } from "@/lib/api/classesApi";
import type { ClassCourseResponse } from "@/lib/types";
import { classSchema, type ClassFormValues } from "@/lib/validation/catalogSchemas";
import { Card } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import { Table, type Column } from "@/components/ui/Table";

export default function AdminClassesPage() {
  const { token } = useAuth();
  const [classes, setClasses] = useState<ClassCourseResponse[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ClassFormValues>({ resolver: zodResolver(classSchema) });

  async function load() {
    if (!token) return;
    setIsLoading(true);
    try {
      setClasses(await listClasses(token));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load classes.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  async function onCreate(values: ClassFormValues) {
    if (!token) return;
    setError(null);
    try {
      await createClass(token, { name: values.name, description: values.description || null });
      reset();
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create class.");
    }
  }

  async function onDeactivate(c: ClassCourseResponse) {
    if (!token) return;
    if (!confirm(`Deactivate "${c.name}"? It will no longer appear in this list once deactivated.`)) return;
    try {
      await deleteClass(token, c.id);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to deactivate class.");
    }
  }

  const columns: Column<ClassCourseResponse>[] = [
    { header: "Name", render: (c) => c.name },
    { header: "Description", render: (c) => c.description ?? "—" },
    {
      header: "Actions",
      render: (c) => (
        <Button variant="danger" className="px-2 py-1 text-xs" onClick={() => onDeactivate(c)}>
          Deactivate
        </Button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Classes</h1>

      <Card>
        <h2 className="mb-4 text-lg font-medium">Add class</h2>
        <form onSubmit={handleSubmit(onCreate)} className="grid grid-cols-1 gap-x-4 sm:grid-cols-2">
          <Input label="Name" {...register("name")} error={errors.name?.message} />
          <Input label="Description (optional)" {...register("description")} error={errors.description?.message} />
          <div className="sm:col-span-2">
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Creating..." : "Create class"}
            </Button>
          </div>
        </form>
        {error && <p className="mt-3 text-sm text-red-600 dark:text-red-400">{error}</p>}
      </Card>

      <Card>
        <h2 className="mb-4 text-lg font-medium">All classes</h2>
        {isLoading ? (
          <p className="text-sm text-zinc-500">Loading...</p>
        ) : (
          <Table columns={columns} rows={classes} keyFor={(c) => c.id} emptyMessage="No classes yet." />
        )}
      </Card>
    </div>
  );
}
