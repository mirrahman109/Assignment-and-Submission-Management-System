import Link from "next/link";
import { Card } from "@/components/ui/Card";

const SECTIONS = [
  { href: "/admin/users", title: "Users", description: "Create and manage Admin, Teacher, and Student accounts." },
  { href: "/admin/classes", title: "Classes", description: "Manage class/course records." },
  { href: "/admin/subjects", title: "Subjects", description: "Manage subject records." },
  { href: "/admin/class-subjects", title: "Class ↔ Subject", description: "Link subjects to classes." },
  {
    href: "/admin/teacher-assignments",
    title: "Teacher Assignments",
    description: "Assign teachers to a class-subject they're allowed to manage.",
  },
];

export default function AdminDashboardPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Admin Dashboard</h1>
        <p className="mt-2 text-zinc-600 dark:text-zinc-400">Set up the school structure before teachers and students can use the system.</p>
      </div>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {SECTIONS.map((s) => (
          <Link key={s.href} href={s.href}>
            <Card className="h-full transition-colors hover:border-zinc-400 dark:hover:border-zinc-600">
              <h2 className="font-medium">{s.title}</h2>
              <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">{s.description}</p>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  );
}
