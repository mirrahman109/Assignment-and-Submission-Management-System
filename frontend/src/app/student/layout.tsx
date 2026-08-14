import { RoleGuard } from "@/components/layout/RoleGuard";
import { Navbar } from "@/components/layout/Navbar";
import { Sidebar } from "@/components/layout/Sidebar";

const LINKS = [
  { href: "/student", label: "Dashboard" },
  { href: "/student/assignments", label: "Assignments" },
  { href: "/student/submissions", label: "My Submissions" },
];

export default function StudentLayout({ children }: { children: React.ReactNode }) {
  return (
    <RoleGuard allowedRoles={["Student"]}>
      <Navbar />
      <div className="flex flex-1">
        <Sidebar links={LINKS} />
        <main className="flex-1 p-6">{children}</main>
      </div>
    </RoleGuard>
  );
}
