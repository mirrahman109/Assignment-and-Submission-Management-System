"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { listMySubmissions } from "@/lib/api/submissionsApi";
import type { SubmissionResponse } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Table, type Column } from "@/components/ui/Table";

export default function StudentSubmissionsPage() {
  const { token } = useAuth();
  const [submissions, setSubmissions] = useState<SubmissionResponse[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!token) return;
    listMySubmissions(token)
      .then(setSubmissions)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load submissions."))
      .finally(() => setIsLoading(false));
  }, [token]);

  const columns: Column<SubmissionResponse>[] = [
    { header: "Assignment", render: (s) => s.assignmentTitle },
    { header: "Submitted", render: (s) => new Date(s.submittedAt).toLocaleString() },
    { header: "Status", render: (s) => <Badge>{s.status}</Badge> },
    { header: "Marks", render: (s) => (s.marks !== null ? `${s.marks} / ${s.maxMarks}` : "—") },
    { header: "Feedback", render: (s) => s.feedback ?? "—" },
    {
      header: "",
      render: (s) => (
        <Link href={`/student/assignments/${s.assignmentId}`} className="text-xs underline">
          View
        </Link>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">My Submissions</h1>
      <Card>
        {error && <p className="mb-3 text-sm text-red-600 dark:text-red-400">{error}</p>}
        {isLoading ? (
          <p className="text-sm text-zinc-500">Loading...</p>
        ) : (
          <Table columns={columns} rows={submissions} keyFor={(s) => s.id} emptyMessage="You haven't submitted anything yet." />
        )}
      </Card>
    </div>
  );
}
