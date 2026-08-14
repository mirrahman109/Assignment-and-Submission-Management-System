import Link from "next/link";
import { Card } from "@/components/ui/Card";

export default function StudentDashboardPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Student Dashboard</h1>
        <p className="mt-2 text-zinc-600 dark:text-zinc-400">Browse assignments for your class and track your submissions.</p>
      </div>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Link href="/student/assignments">
          <Card className="h-full transition-colors hover:border-zinc-400 dark:hover:border-zinc-600">
            <h2 className="font-medium">Assignments</h2>
            <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">See published assignments for your class and submit your work.</p>
          </Card>
        </Link>
        <Link href="/student/submissions">
          <Card className="h-full transition-colors hover:border-zinc-400 dark:hover:border-zinc-600">
            <h2 className="font-medium">My Submissions</h2>
            <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">Check status, marks, and feedback on what you&apos;ve turned in.</p>
          </Card>
        </Link>
      </div>
    </div>
  );
}
