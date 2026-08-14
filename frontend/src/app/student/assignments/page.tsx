"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { listAssignments } from "@/lib/api/assignmentsApi";
import { listMySubmissions } from "@/lib/api/submissionsApi";
import type { AssignmentResponse, SubmissionResponse } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Table, type Column } from "@/components/ui/Table";

export default function StudentAssignmentsPage() {
  const { token } = useAuth();
  const [assignments, setAssignments] = useState<AssignmentResponse[]>([]);
  const [submissions, setSubmissions] = useState<SubmissionResponse[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!token) return;
    Promise.all([listAssignments(token), listMySubmissions(token)])
      .then(([a, s]) => {
        setAssignments(a);
        setSubmissions(s);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load assignments."))
      .finally(() => setIsLoading(false));
  }, [token]);

  const submissionByAssignment = new Map(submissions.map((s) => [s.assignmentId, s]));

  const columns: Column<AssignmentResponse>[] = [
    { header: "Title", render: (a) => a.title },
    { header: "Subject", render: (a) => a.subjectName },
    { header: "Deadline", render: (a) => new Date(a.deadline).toLocaleString() },
    { header: "Max marks", render: (a) => a.maxMarks },
    {
      header: "Your status",
      render: (a) => {
        const submission = submissionByAssignment.get(a.id);
        if (!submission) return <Badge tone="neutral">Not submitted</Badge>;
        return <Badge>{submission.status}</Badge>;
      },
    },
    {
      header: "",
      render: (a) => (
        <Link href={`/student/assignments/${a.id}`} className="text-xs underline">
          View
        </Link>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Assignments</h1>
      <Card>
        {error && <p className="mb-3 text-sm text-red-600 dark:text-red-400">{error}</p>}
        {isLoading ? (
          <p className="text-sm text-zinc-500">Loading...</p>
        ) : (
          <Table columns={columns} rows={assignments} keyFor={(a) => a.id} emptyMessage="No assignments published for your class yet." />
        )}
      </Card>
    </div>
  );
}
