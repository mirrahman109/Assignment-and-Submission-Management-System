"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { getAssignment, updateAssignment } from "@/lib/api/assignmentsApi";
import { updateAssignmentSchema, type UpdateAssignmentFormValues } from "@/lib/validation/assignmentSchemas";
import { Card } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Textarea } from "@/components/ui/Textarea";
import { Button } from "@/components/ui/Button";

function toDatetimeLocalValue(iso: string) {
  const date = new Date(iso);
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
  return local.toISOString().slice(0, 16);
}

export default function EditAssignmentPage() {
  const { token } = useAuth();
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const assignmentId = Number(params.id);

  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [title, setTitle] = useState("");

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<UpdateAssignmentFormValues>({ resolver: zodResolver(updateAssignmentSchema) });

  useEffect(() => {
    if (!token) return;
    getAssignment(token, assignmentId)
      .then((a) => {
        setTitle(a.title);
        reset({
          title: a.title,
          description: a.description,
          deadline: toDatetimeLocalValue(a.deadline),
          maxMarks: a.maxMarks,
          allowLateSubmission: a.allowLateSubmission,
        });
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load assignment."))
      .finally(() => setIsLoading(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, assignmentId]);

  async function onSubmit(values: UpdateAssignmentFormValues) {
    if (!token) return;
    setError(null);
    try {
      await updateAssignment(token, assignmentId, {
        title: values.title,
        description: values.description,
        deadline: new Date(values.deadline).toISOString(),
        maxMarks: values.maxMarks,
        allowLateSubmission: values.allowLateSubmission,
      });
      router.push("/teacher/assignments");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save changes.");
    }
  }

  if (isLoading) return <p className="text-sm text-zinc-500">Loading...</p>;

  return (
    <div className="max-w-xl space-y-6">
      <h1 className="text-2xl font-semibold">Edit: {title}</h1>
      <Card>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Input label="Title" {...register("title")} error={errors.title?.message} />
          <Textarea label="Description" {...register("description")} error={errors.description?.message} />
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
          {error && <p className="mb-4 text-sm text-red-600 dark:text-red-400">{error}</p>}
          <div className="flex gap-2">
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Saving..." : "Save changes"}
            </Button>
            <Button type="button" variant="secondary" onClick={() => router.push("/teacher/assignments")}>
              Cancel
            </Button>
          </div>
        </form>
      </Card>
    </div>
  );
}
