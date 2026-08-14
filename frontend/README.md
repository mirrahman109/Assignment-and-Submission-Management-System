# Frontend — Assignment & Submission Management System

Next.js 16 (App Router) + React 19 + TypeScript + Tailwind CSS.

Setup, environment variables, demo credentials, and the design decisions behind this app are all
documented in the **[root README](../README.md)** — start there.

## Running just the frontend

The API must already be running (see the root README), then:

```bash
cp .env.example .env.local    # NEXT_PUBLIC_API_URL=http://localhost:5000
npm install
npm run dev                   # http://localhost:3000
```

| Script | Purpose |
|---|---|
| `npm run dev` | Development server with hot reload |
| `npm run build` | Production build (also type-checks) |
| `npm start` | Serve the production build |
| `npm run lint` | ESLint |

## Layout

```
src/
  proxy.ts          UX-only redirects by role at the edge — real access control is the API's job
  app/
    (auth)/login/   Sign-in
    admin/          Users, classes, subjects, class↔subject links, teacher assignments,
                    and read-only oversight of every assignment and submission
    teacher/        Assignment CRUD, draft/publish, submissions and grading
    student/        Assignments for their class, submit/edit, marks and feedback
  components/ui/    Button, Input, Select, Textarea, Card, Badge, Table
  components/layout/ Navbar, Sidebar, RoleGuard
  lib/
    api/            fetch wrapper (Bearer header, 401 handling) + one module per resource
    auth/           AuthContext and token storage
    types/          Shared entity types mirroring the API's DTOs
    validation/     zod schemas mirroring the server-side rules
```

> The JWT lives in `localStorage` (and is mirrored into a plain cookie so `proxy.ts` can read it for
> redirects). This is a deliberate, documented trade-off — see *Known limitations* in the root README.
