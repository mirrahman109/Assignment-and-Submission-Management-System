"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { getAssignment } from "@/lib/api/assignmentsApi";
import { gradeSubmission, listSubmissionsForAssignment, updateSubmissionStatus } from "@/lib/api/submissionsApi";
import type { AssignmentResponse, SubmissionResponse } from "@/lib/types";
import { gradeSubmissionSchema, type GradeSubmissionFormValues } from "@/lib/validation/assignmentSchemas";
import { Card } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Textarea } from "@/components/ui/Textarea";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";

function GradeForm({
  submission,
  token,
  onDone,
}: {
  submission: SubmissionResponse;
  token: string;
  onDone: () => void;
}) {
  const [error, setError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<GradeSubmissionFormValues>({
    resolver: zodResolver(gradeSubmissionSchema),
    defaultValues: { marks: submission.marks ?? 0, feedback: submission.feedback ?? "" },
  });

  async function onSubmit(values: GradeSubmissionFormValues) {
    setError(null);
    try {
      await gradeSubmission(token, submission.id, { marks: values.marks, feedback: values.feedback || null });
      onDone();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save grade.");
    }
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="mt-3 rounded-md border border-zinc-200 p-4 dark:border-zinc-800">
      <Input
        label={`Marks (out of ${submission.maxMarks})`}
        type="number"
        step="0.5"
        {...register("marks", { valueAsNumber: true })}
        error={errors.marks?.message}
      />
      <Textarea label="Feedback" {...register("feedback")} error={errors.feedback?.message} />
      {error && <p className="mb-3 text-sm text-red-600 dark:text-red-400">{error}</p>}
      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? "Saving..." : "Save grade"}
      </Button>
    </form>
  );
}

export default function AssignmentSubmissionsPage() {
  const { token } = useAuth();
  const params = useParams<{ id: string }>();
  const assignmentId = Number(params.id);

  const [assignment, setAssignment] = useState<AssignmentResponse | null>(null);
  const [submissions, setSubmissions] = useState<SubmissionResponse[]>([]);
  const [gradingId, setGradingId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  async function load() {
    if (!token) return;
    setIsLoading(true);
    try {
      const [a, subs] = await Promise.all([
        getAssignment(token, assignmentId),
        listSubmissionsForAssignment(token, assignmentId),
      ]);
      setAssignment(a);
      setSubmissions(subs);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load submissions.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, assignmentId]);

  async function onRequestRevision(submissionId: number) {
    if (!token) return;
    try {
      await updateSubmissionStatus(token, submissionId, "NeedsRevision");
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to request revision.");
    }
  }

  if (isLoading) return <p className="text-sm text-zinc-500">Loading...</p>;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Submissions{assignment ? `: ${assignment.title}` : ""}</h1>
      {error && <p className="text-sm text-red-600 dark:text-red-400">{error}</p>}

      {submissions.length === 0 ? (
        <p className="text-sm text-zinc-500">No submissions yet.</p>
      ) : (
        <div className="space-y-4">
          {submissions.map((s) => (
            <Card key={s.id}>
              <div className="flex items-start justify-between gap-4">
                <div>
                  <p className="font-medium">{s.studentName}</p>
                  <p className="mt-1 whitespace-pre-wrap text-sm text-zinc-600 dark:text-zinc-400">{s.answerText}</p>
                  <p className="mt-2 text-xs text-zinc-500">
                    Submitted {new Date(s.submittedAt).toLocaleString()}
                    {s.isLate && " (late)"}
                  </p>
                </div>
                <div className="flex shrink-0 flex-col items-end gap-2">
                  <Badge>{s.status}</Badge>
                  {s.marks !== null && (
                    <span className="text-sm font-medium">
                      {s.marks} / {s.maxMarks}
                    </span>
                  )}
                  <div className="flex gap-2">
                    <Button
                      variant="secondary"
                      className="px-2 py-1 text-xs"
                      onClick={() => setGradingId(gradingId === s.id ? null : s.id)}
                    >
                      {gradingId === s.id ? "Cancel" : s.status === "Graded" ? "Re-grade" : "Grade"}
                    </Button>
                    {s.status === "Graded" && (
                      <Button
                        variant="secondary"
                        className="px-2 py-1 text-xs"
                        onClick={() => onRequestRevision(s.id)}
                      >
                        Request revision
                      </Button>
                    )}
                  </div>
                </div>
              </div>
              {gradingId === s.id && token && (
                <GradeForm
                  submission={s}
                  token={token}
                  onDone={() => {
                    setGradingId(null);
                    load();
                  }}
                />
              )}
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
