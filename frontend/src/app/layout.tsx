import type { Metadata } from "next";
import { AuthProvider } from "@/lib/auth/AuthContext";
import "./globals.css";

export const metadata: Metadata = {
  title: "Assignment & Submission Management System",
  description: "Role-based assignment and submission management for Admins, Teachers, and Students",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className="h-full antialiased">
      <body className="min-h-full flex flex-col">
        <AuthProvider>{children}</AuthProvider>
      </body>
    </html>
  );
}
