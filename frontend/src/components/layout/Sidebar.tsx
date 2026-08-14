"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

export interface SidebarLink {
  href: string;
  label: string;
}

export function Sidebar({ links }: { links: SidebarLink[] }) {
  const pathname = usePathname();

  return (
    <nav className="w-48 shrink-0 border-r border-zinc-200 p-4 dark:border-zinc-800">
      <ul className="space-y-1">
        {links.map((link) => {
          const isActive = pathname === link.href;
          return (
            <li key={link.href}>
              <Link
                href={link.href}
                className={`block rounded-md px-3 py-2 text-sm ${
                  isActive
                    ? "bg-zinc-900 text-white dark:bg-zinc-100 dark:text-zinc-900"
                    : "text-zinc-700 hover:bg-zinc-100 dark:text-zinc-300 dark:hover:bg-zinc-800"
                }`}
              >
                {link.label}
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}
