"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { getAssignment } from "@/lib/api/assignmentsApi";
import { listSubmissionsForAssignment } from "@/lib/api/submissionsApi";
import type { AssignmentResponse, SubmissionResponse } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Table, type Column } from "@/components/ui/Table";

export default function AdminAssignmentDetailPage() {
  const { token } = useAuth();
  const params = useParams<{ id: string }>();
  const assignmentId = Number(params.id);

  const [assignment, setAssignment] = useState<AssignmentResponse | null>(null);
  const [submissions, setSubmissions] = useState<SubmissionResponse[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!token) return;
    Promise.all([getAssignment(token, assignmentId), listSubmissionsForAssignment(token, assignmentId)])
      .then(([a, s]) => {
        setAssignment(a);
        setSubmissions(s);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load assignment."))
      .finally(() => setIsLoading(false));
  }, [token, assignmentId]);

  const columns: Column<SubmissionResponse>[] = [
    { header: "Student", render: (s) => s.studentName },
    { header: "Submitted", render: (s) => new Date(s.submittedAt).toLocaleString() },
    {
      header: "Late",
      render: (s) => (s.isLate ? <Badge tone="warning">Late</Badge> : <span className="text-zinc-400">—</span>),
    },
    { header: "Status", render: (s) => <Badge>{s.status}</Badge> },
    {
      header: "Marks",
      render: (s) => (s.marks !== null ? `${s.marks} / ${s.maxMarks}` : <span className="text-zinc-400">—</span>),
    },
    {
      header: "Feedback",
      render: (s) => s.feedback ?? <span className="text-zinc-400">—</span>,
    },
  ];

  if (isLoading) return <p className="text-sm text-zinc-500">Loading...</p>;
  if (error) return <p className="text-sm text-red-600 dark:text-red-400">{error}</p>;
  if (!assignment) return null;

  const graded = submissions.filter((s) => s.status === "Graded").length;

  return (
    <div className="space-y-6">
      <div>
        <Link href="/admin/assignments" className="text-sm text-zinc-500 underline underline-offset-2">
          ← All assignments
        </Link>
        <h1 className="mt-2 text-2xl font-semibold">{assignment.title}</h1>
      </div>

      <Card>
        <div className="grid grid-cols-1 gap-4 text-sm sm:grid-cols-2 lg:grid-cols-4">
          <Detail label="Class" value={assignment.classCourseName} />
          <Detail label="Subject" value={assignment.subjectName} />
          <Detail label="Teacher" value={assignment.teacherName} />
          <Detail label="Status" value={<Badge>{assignment.status}</Badge>} />
          <Detail label="Deadline" value={new Date(assignment.deadline).toLocaleString()} />
          <Detail label="Max marks" value={String(assignment.maxMarks)} />
          <Detail label="Late submissions" value={assignment.allowLateSubmission ? "Allowed" : "Not allowed"} />
          <Detail label="Created" value={new Date(assignment.createdAt).toLocaleDateString()} />
        </div>
        <div className="mt-4 border-t border-zinc-200 pt-4 dark:border-zinc-800">
          <p className="text-xs uppercase tracking-wide text-zinc-500">Description</p>
          <p className="mt-1 whitespace-pre-wrap text-sm">{assignment.description}</p>
        </div>
      </Card>

      <Card>
        <h2 className="mb-1 text-lg font-medium">Submissions</h2>
        <p className="mb-4 text-sm text-zinc-600 dark:text-zinc-400">
          {submissions.length} submitted · {graded} graded. Grading is the assigned teacher&apos;s job — this view is
          read-only.
        </p>
        <Table
          columns={columns}
          rows={submissions}
          keyFor={(s) => s.id}
          emptyMessage="No student has submitted this assignment yet."
        />
      </Card>
    </div>
  );
}

function Detail({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div>
      <p className="text-xs uppercase tracking-wide text-zinc-500">{label}</p>
      <p className="mt-1">{value}</p>
    </div>
  );
}
