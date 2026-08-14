import Link from "next/link";
import { Card } from "@/components/ui/Card";

export default function TeacherDashboardPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Teacher Dashboard</h1>
        <p className="mt-2 text-zinc-600 dark:text-zinc-400">Create, publish, and grade assignments for the classes you teach.</p>
      </div>
      <Link href="/teacher/assignments">
        <Card className="max-w-sm transition-colors hover:border-zinc-400 dark:hover:border-zinc-600">
          <h2 className="font-medium">Assignments</h2>
          <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">Create new assignments, publish drafts, and grade submissions.</p>
        </Card>
      </Link>
    </div>
  );
}
