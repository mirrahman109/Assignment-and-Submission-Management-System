"use client";

import { useAuth } from "@/lib/auth/AuthContext";

export function Navbar() {
  const { user, logout } = useAuth();

  return (
    <header className="flex items-center justify-between border-b border-zinc-200 px-6 py-3 dark:border-zinc-800">
      <span className="font-semibold">Assignment &amp; Submission Management System</span>
      {user && (
        <div className="flex items-center gap-4 text-sm">
          <span className="text-zinc-600 dark:text-zinc-400">
            {user.fullName} <span className="text-zinc-400 dark:text-zinc-500">({user.role})</span>
          </span>
          <button
            onClick={logout}
            className="rounded-md border border-zinc-300 px-3 py-1.5 hover:bg-zinc-100 dark:border-zinc-700 dark:hover:bg-zinc-800"
          >
            Log out
          </button>
        </div>
      )}
    </header>
  );
}
