"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { getAssignment } from "@/lib/api/assignmentsApi";
import { createSubmission, listMySubmissions, updateSubmission } from "@/lib/api/submissionsApi";
import type { AssignmentResponse, SubmissionResponse } from "@/lib/types";
import { submissionSchema, type SubmissionFormValues } from "@/lib/validation/submissionSchemas";
import { Card } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Textarea } from "@/components/ui/Textarea";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";

export default function StudentAssignmentDetailPage() {
  const { token } = useAuth();
  const params = useParams<{ id: string }>();
  const assignmentId = Number(params.id);

  const [assignment, setAssignment] = useState<AssignmentResponse | null>(null);
  const [submission, setSubmission] = useState<SubmissionResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<SubmissionFormValues>({ resolver: zodResolver(submissionSchema) });

  async function load() {
    if (!token) return;
    setIsLoading(true);
    try {
      const [a, mySubs] = await Promise.all([getAssignment(token, assignmentId), listMySubmissions(token)]);
      setAssignment(a);
      const existing = mySubs.find((s) => s.assignmentId === assignmentId) ?? null;
      setSubmission(existing);
      reset({ answerText: existing?.answerText ?? "", attachmentUrl: existing?.attachmentUrl ?? "" });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load assignment.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, assignmentId]);

  async function onSubmit(values: SubmissionFormValues) {
    if (!token) return;
    setError(null);
    setSuccessMessage(null);
    const payload = { answerText: values.answerText, attachmentUrl: values.attachmentUrl || null };
    try {
      if (submission) {
        await updateSubmission(token, submission.id, payload);
        setSuccessMessage("Your submission was updated.");
      } else {
        await createSubmission(token, assignmentId, payload);
        setSuccessMessage("Your work was submitted.");
      }
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to submit.");
    }
  }

  if (isLoading) return <p className="text-sm text-zinc-500">Loading...</p>;
  if (!assignment) return <p className="text-sm text-red-600">{error ?? "Assignment not found."}</p>;

  const isGraded = submission?.status === "Graded";
  const isPastDeadline = new Date(assignment.deadline).getTime() < Date.now();
  const needsRevision = submission?.status === "NeedsRevision";

  // Mirrors SubmissionService's rules so the form isn't offered when the API would reject it.
  // The server remains the authority — this only avoids a pointless round trip.
  const deadlineBlocks = isPastDeadline && !assignment.allowLateSubmission && !needsRevision;
  const canWrite = !isGraded && !deadlineBlocks;

  const lockReason = isGraded
    ? "This submission has been graded and can no longer be edited. If your teacher reopens it for revision, you'll be able to edit it again."
    : "The deadline has passed and this assignment does not accept late submissions.";

  return (
    <div className="max-w-2xl space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">{assignment.title}</h1>
        <p className="mt-1 text-sm text-zinc-500">
          {assignment.subjectName} · Due {new Date(assignment.deadline).toLocaleString()}
          {isPastDeadline && " (past deadline)"} · Max marks {assignment.maxMarks}
        </p>
      </div>

      <Card>
        <h2 className="mb-2 text-sm font-medium text-zinc-500">Instructions</h2>
        <p className="whitespace-pre-wrap text-sm">{assignment.description}</p>
      </Card>

      {submission && (
        <Card>
          <div className="mb-3 flex items-center justify-between">
            <h2 className="text-lg font-medium">Your submission</h2>
            <Badge>{submission.status}</Badge>
          </div>
          {submission.status === "NeedsRevision" && (
            <p className="mb-3 text-sm text-amber-700 dark:text-amber-400">
              Your teacher asked you to revise this submission — edit and resubmit below.
            </p>
          )}
          {isGraded && (
            <div className="mb-3 space-y-1 text-sm">
              <p>
                <span className="font-medium">Marks:</span> {submission.marks} / {submission.maxMarks}
              </p>
              {submission.feedback && (
                <p>
                  <span className="font-medium">Feedback:</span> {submission.feedback}
                </p>
              )}
            </div>
          )}
        </Card>
      )}

      {!canWrite && (
        <Card>
          <p className="text-sm text-zinc-600 dark:text-zinc-400">{lockReason}</p>
        </Card>
      )}

      {canWrite && (
        <Card>
          <h2 className="mb-4 text-lg font-medium">{submission ? "Edit your submission" : "Submit your work"}</h2>
          {isPastDeadline && assignment.allowLateSubmission && !needsRevision && (
            <p className="mb-4 text-sm text-amber-700 dark:text-amber-400">
              The deadline has passed. Late submissions are allowed for this assignment, but yours will be marked
              late.
            </p>
          )}
          <form onSubmit={handleSubmit(onSubmit)}>
            <Textarea label="Your answer" rows={6} {...register("answerText")} error={errors.answerText?.message} />
            <Input
              label="Attachment URL (optional)"
              placeholder="https://..."
              {...register("attachmentUrl")}
              error={errors.attachmentUrl?.message}
            />
            {error && <p className="mb-3 text-sm text-red-600 dark:text-red-400">{error}</p>}
            {successMessage && <p className="mb-3 text-sm text-green-700 dark:text-green-400">{successMessage}</p>}
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Saving..." : submission ? "Save changes" : "Submit"}
            </Button>
          </form>
        </Card>
      )}
    </div>
  );
}
