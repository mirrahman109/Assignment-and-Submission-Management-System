import { RoleGuard } from "@/components/layout/RoleGuard";
import { Navbar } from "@/components/layout/Navbar";
import { Sidebar } from "@/components/layout/Sidebar";

const LINKS = [
  { href: "/teacher", label: "Dashboard" },
  { href: "/teacher/assignments", label: "Assignments" },
];

export default function TeacherLayout({ children }: { children: React.ReactNode }) {
  return (
    <RoleGuard allowedRoles={["Teacher"]}>
      <Navbar />
      <div className="flex flex-1">
        <Sidebar links={LINKS} />
        <main className="flex-1 p-6">{children}</main>
      </div>
    </RoleGuard>
  );
}
