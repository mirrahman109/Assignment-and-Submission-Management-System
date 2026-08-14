"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { listAssignments } from "@/lib/api/assignmentsApi";
import type { AssignmentResponse } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Select } from "@/components/ui/Select";
import { Badge } from "@/components/ui/Badge";
import { Table, type Column } from "@/components/ui/Table";

export default function AdminAssignmentsPage() {
  const { token } = useAuth();
  const [assignments, setAssignments] = useState<AssignmentResponse[]>([]);
  const [classFilter, setClassFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!token) return;
    // Admin is unfiltered server-side, so this is genuinely every assignment,
    // drafts from every teacher included.
    listAssignments(token)
      .then(setAssignments)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load assignments."))
      .finally(() => setIsLoading(false));
  }, [token]);

  const classNames = useMemo(
    () => [...new Set(assignments.map((a) => a.classCourseName))].sort(),
    [assignments],
  );

  const visible = assignments.filter(
    (a) =>
      (!classFilter || a.classCourseName === classFilter) &&
      (!statusFilter || a.status === statusFilter),
  );

  const columns: Column<AssignmentResponse>[] = [
    {
      header: "Title",
      render: (a) => (
        <Link href={`/admin/assignments/${a.id}`} className="font-medium underline underline-offset-2">
          {a.title}
        </Link>
      ),
    },
    { header: "Class", render: (a) => a.classCourseName },
    { header: "Subject", render: (a) => a.subjectName },
    { header: "Teacher", render: (a) => a.teacherName },
    { header: "Deadline", render: (a) => new Date(a.deadline).toLocaleString() },
    { header: "Max marks", render: (a) => a.maxMarks },
    { header: "Status", render: (a) => <Badge>{a.status}</Badge> },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">All Assignments</h1>
        <p className="mt-2 text-sm text-zinc-600 dark:text-zinc-400">
          Read-only oversight across every class, subject, and teacher — drafts included. Open one to see its
          submissions.
        </p>
      </div>

      <Card>
        <div className="grid grid-cols-1 gap-x-4 sm:grid-cols-3">
          <Select label="Class" value={classFilter} onChange={(e) => setClassFilter(e.target.value)}>
            <option value="">All classes</option>
            {classNames.map((name) => (
              <option key={name} value={name}>
                {name}
              </option>
            ))}
          </Select>
          <Select label="Status" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
            <option value="">All statuses</option>
            <option value="Published">Published</option>
            <option value="Draft">Draft</option>
          </Select>
          <div className="mb-4 flex items-end text-sm text-zinc-500 dark:text-zinc-400">
            Showing {visible.length} of {assignments.length}
          </div>
        </div>
      </Card>

      <Card>
        {error && <p className="mb-3 text-sm text-red-600 dark:text-red-400">{error}</p>}
        {isLoading ? (
          <p className="text-sm text-zinc-500">Loading...</p>
        ) : (
          <Table
            columns={columns}
            rows={visible}
            keyFor={(a) => a.id}
            emptyMessage="No assignments match these filters."
          />
        )}
      </Card>
    </div>
  );
}
