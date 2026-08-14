# Assignment & Submission Management System

A role-based assignment and submission system for a school or college. Teachers create assignments
for a specific class + subject, students submit answers, and teachers grade them with marks and
feedback. Admins provision the users, classes, subjects, and teaching assignments that everything
else hangs off, and can see every assignment and submission in the system.

Built for the OnnoRokom Projukti Limited Assistant Software Engineer recruitment project.

- **Backend:** ASP.NET Core Web API (.NET 10), EF Core, PostgreSQL, JWT auth, Swagger, Serilog
- **Frontend:** Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS
- **Tests:** xUnit — 26 tests over business rules, authorization, and the submission workflow
- **Setup:** `docker compose up --build` — migrates and seeds the database automatically

---

## Table of contents

1. [Features by role](#features-by-role)
2. [Tech stack](#tech-stack)
3. [Quick start with Docker](#quick-start-with-docker)
4. [Demo credentials](#demo-credentials)
5. [Manual setup without Docker](#manual-setup-without-docker)
6. [Database setup](#database-setup)
7. [Running the tests](#running-the-tests)
8. [API reference](#api-reference)
9. [Project structure](#project-structure)
10. [Data model](#data-model)
11. [Key design decisions](#key-design-decisions)
12. [How role-based access is enforced](#how-role-based-access-is-enforced)
13. [Assumptions](#assumptions)
14. [Known limitations](#known-limitations)

---

## Features by role

### Admin
- Create, update, and deactivate users of all three roles.
- Manage classes/courses and subjects.
- Link subjects to classes (`Class ↔ Subject`), which is what an assignment is created against.
- Assign teachers to a specific class-subject pair — a teacher can only act on pairs they're assigned to.
- View every assignment in the system (drafts included) and every submission against it, filterable
  by class and status.

### Teacher
- Create, update, and delete assignments for the class-subjects they are assigned to.
- Set title, description, deadline, maximum marks, and whether late submissions are allowed.
- Keep an assignment as a **Draft** (invisible to students) or **Publish** it.
- View submissions for their own assignments only.
- Assign marks and write feedback.
- Change a submission's status — in particular set it to **NeedsRevision**, which reopens editing
  for the student even after the deadline has passed.

### Student
- See only assignments that are **Published** *and* belong to their own class.
- View assignment details, deadline, maximum marks, and whether late submission is allowed.
- Submit an answer (text plus an optional attachment URL).
- Edit their submission before the deadline, or after it if late submission is allowed, or any time
  the teacher has reopened it as *NeedsRevision*.
- See their submission status, marks, and teacher feedback.

---

## Tech stack

| Layer | Choice | Notes |
|---|---|---|
| Frontend | Next.js 16 (App Router), React 19, TypeScript | Client components + a `proxy.ts` edge redirect for UX-level route guarding |
| Styling | Tailwind CSS v4 | Responsive, light/dark aware |
| Forms | react-hook-form + zod | Client-side validation mirroring the server rules |
| Backend | ASP.NET Core Web API (.NET 10), C# | Controllers → Services → `AppDbContext` |
| ORM / DB | EF Core 10 + Npgsql, PostgreSQL 16 | Code-first migrations, idempotent seeder |
| Auth | JWT (HMAC-SHA256), `PasswordHasher<User>` | Plain `Users` table with a `Role` enum — no ASP.NET Identity schema |
| Validation | FluentValidation via a `ValidationActionFilter` | Plus server-side rule checks in the service layer |
| Docs | Swashbuckle / Swagger UI | Bearer auth wired into the UI |
| Logging | Serilog | Console + daily rolling file, request logging |
| Tests | xUnit + SQLite in-memory + `WebApplicationFactory` | Real constraint enforcement, not EF's InMemory provider |

---

## Quick start with Docker

Requires Docker Desktop (or Docker Engine + Compose v2). Nothing else — no .NET SDK, no Node.

```bash
git clone <repository-url>
cd "Assignment and Submission Management System"
docker compose up --build
```

That single command starts three containers:

| Service | URL | Notes |
|---|---|---|
| Frontend | http://localhost:3000 | Next.js production server |
| Backend API | http://localhost:5000 | ASP.NET Core |
| Swagger UI | http://localhost:5000/swagger | Click **Authorize** and paste a JWT to try secured endpoints |
| PostgreSQL | `localhost:5433` | Mapped off 5432 so it won't collide with a local Postgres |

On startup the API applies EF Core migrations and then seeds demo data **if the `Users` table is
empty** — so the evaluator never creates a table by hand, and restarting never duplicates data.

The compose file has working defaults for every variable, so it runs with no `.env` present. To
override them, copy `.env.example` to `.env` first:

```bash
cp .env.example .env
```

To stop, and to reset the database completely:

```bash
docker compose down          # stop the containers
docker compose down -v       # ...and drop the Postgres volume, so the next run re-seeds
```

---

## Demo credentials

All three roles are seeded and ready to log in at http://localhost:3000.

| Role | Email | Password |
|---|---|---|
| **Admin** | `admin@school.test` | `Admin@123` |
| **Teacher** | `teacher.math@school.test` | `Teacher@123` |
| **Student** | `student1@school.test` | `Student@123` |

Additional seeded accounts, useful for demonstrating that the access rules actually hold:

| Role | Email | Password | Why it's interesting |
|---|---|---|---|
| Teacher | `teacher.english@school.test` | `Teacher@123` | Teaches English in Grade 10-A only — cannot touch the Maths assignments |
| Student | `student2@school.test` | `Student@123` | Also Grade 10-A — cannot read student1's submission |
| Student | `student3@school.test` | `Student@123` | Grade 10-B — cannot see Grade 10-A assignments at all |

The seed also creates two classes, three subjects, four class-subject links, three teaching
assignments, and three assignments (one published, one draft, one past-deadline and already graded)
so every screen has something to show on first login.

> These are demo credentials for a seeded local database. They are not real secrets, and no real
> secret is committed anywhere in this repository.

---

## Manual setup without Docker

Prerequisites: **.NET 10 SDK**, **Node.js 20+**, and a running **PostgreSQL 16**.

### 1. Database

Create an empty database — the tables come from the migrations, not from you:

```bash
createdb -U postgres assignment_system
```

### 2. Backend

```bash
cd backend
cp .env.example .env          # then edit the values (see below)
```

Configuration comes from environment variables (ASP.NET Core maps `__` to a config section
separator) or from `appsettings.Development.json`. The variables that matter:

| Variable | Purpose |
|---|---|
| `ConnectionStrings__Default` | Postgres connection string |
| `Jwt__Key` | HMAC signing key — **must be at least 32 characters** |
| `Jwt__Issuer`, `Jwt__Audience` | Token issuer/audience, must match between issuing and validation |
| `Jwt__ExpiryMinutes` | Token lifetime (default 60) |
| `Cors__AllowedOrigins__0` | Frontend origin allowed to call the API |

Then run it:

```bash
dotnet run --project src/AssignmentSystem.Api
```

Migrations and seeding run automatically at startup. The API listens on **http://localhost:5000**
(the `http` profile in `Properties/launchSettings.json`) — the same port the Docker setup uses, so
the frontend's `NEXT_PUBLIC_API_URL` is identical either way. Swagger is at `/swagger`.

> If you already have the Docker stack running, stop it first (`docker compose down`) — otherwise
> the backend container is holding port 5000.

### 3. Frontend

```bash
cd frontend
cp .env.example .env.local    # NEXT_PUBLIC_API_URL=http://localhost:5000
npm install
npm run dev                   # http://localhost:3000
```

For a production build: `npm run build && npm start`.

---

## Database setup

There is no SQL script to run and no table to create by hand.

- **Schema** — EF Core code-first migrations in
  [`backend/src/AssignmentSystem.Api/Data/Migrations/`](backend/src/AssignmentSystem.Api/Data/Migrations/).
  `Program.cs` calls `Database.MigrateAsync()` on startup, so an empty database is brought fully up
  to date automatically the first time the API boots.
- **Sample data** — [`Data/Seed/DbSeeder.cs`](backend/src/AssignmentSystem.Api/Data/Seed/DbSeeder.cs)
  runs immediately after migration. It is idempotent: it returns early if any user already exists,
  so it seeds exactly once and restarts are safe.

To create a new migration after changing an entity:

```bash
cd backend
dotnet ef migrations add <Name> --project src/AssignmentSystem.Api
```

To start over from an empty database:

```bash
docker compose down -v && docker compose up --build
```

---

## Running the tests

```bash
cd backend
dotnet test
```

26 tests, all passing. They use **SQLite in-memory** rather than EF Core's InMemory provider,
because SQLite actually enforces unique and foreign-key constraints — which is what proves the
"one submission per student per assignment" rule holds at the database level and not just in C#.
A fake `IClock` makes deadline behaviour deterministic instead of dependent on wall-clock time.

What's covered:

**Business rules**
- Assignment creation is rejected for a teacher not assigned to that class-subject, and succeeds for one who is.
- Grading rejects negative marks and marks above the assignment's `MaxMarks`; a valid grade sets `Status = Graded`.
- A duplicate submission from the same student raises a conflict.
- Editing is blocked past the deadline, but allowed past the deadline once the teacher sets `NeedsRevision`.

**Authorization / information leaks**
- A student's assignment list and detail view never expose drafts or another class's assignments — and return **404, not 403**, so the response can't be used to prove such an assignment exists.
- A student cannot read or update another student's submission.
- A teacher cannot read or grade a submission on an assignment they don't own.

**Authentication**
- Wrong password and unknown email both fail, with identical wording so accounts can't be enumerated.
- A deactivated user cannot log in.
- The generated JWT carries the correct role and user-id claims, issuer, and audience.

**End-to-end authorization** (via `WebApplicationFactory<Program>`, exercising the real HTTP pipeline)
- An anonymous request to a secured endpoint is rejected.
- A student's token is rejected by an Admin-only endpoint; an admin's token is accepted.

---

## API reference

All endpoints require `Authorization: Bearer <token>` except `POST /api/auth/login`.
Full interactive documentation is at `/swagger`.

| Method | Endpoint | Who | Purpose |
|---|---|---|---|
| POST | `/api/auth/login` | Anonymous | Exchange credentials for a JWT |
| GET | `/api/auth/me` | Any | Current user from the token |
| GET/POST/PUT | `/api/users`, `/api/users/{id}` | Admin | Manage users |
| PATCH | `/api/users/{id}/deactivate` | Admin | Soft-delete a user |
| GET | `/api/classes`, `/api/subjects` | Any | Read catalogue |
| POST/PUT/DELETE | `/api/classes`, `/api/subjects` | Admin | Manage catalogue |
| GET | `/api/class-subjects` | Any | List class↔subject pairs |
| POST/DELETE | `/api/class-subjects` | Admin | Link/unlink a subject to a class |
| GET | `/api/teacher-assignments` | Admin, Teacher | List teaching assignments |
| POST/DELETE | `/api/teacher-assignments` | Admin | Assign/unassign a teacher to a class-subject |
| GET | `/api/assignments` | Any | **Role-scoped**: admin sees all, teacher sees own, student sees published-in-own-class |
| GET | `/api/assignments/{id}` | Any | Same scoping; 404 for a student who shouldn't see it |
| POST/PUT/DELETE | `/api/assignments`, `/api/assignments/{id}` | Teacher (owner) | Assignment CRUD |
| PATCH | `/api/assignments/{id}/status` | Teacher (owner) | Draft ↔ Published |
| POST | `/api/assignments/{id}/submissions` | Student | Submit an answer |
| GET | `/api/assignments/{id}/submissions` | Admin, owning Teacher | List submissions to grade |
| GET | `/api/submissions/mine` | Student | Own submissions with marks and feedback |
| GET/PUT | `/api/submissions/{id}` | Owner student, owning Teacher, Admin | Read / edit a submission |
| PUT | `/api/submissions/{id}/grade` | Owning Teacher, Admin | Set marks and feedback |
| PATCH | `/api/submissions/{id}/status` | Owning Teacher, Admin | e.g. reopen as `NeedsRevision` |

Errors come back as a consistent problem-shaped body — `{ "title", "status", "errors" }` — produced
by a single global exception middleware: `404` not found, `403` forbidden, `409` conflict, `400`
validation (with a per-field `errors` map), `401` unauthenticated. Unhandled exceptions are logged
in full and returned to the client as a bare `500` with no stack trace.

---

## Project structure

```
.
├── docker-compose.yml              # postgres + backend + frontend, one command
├── .env.example                    # variables used by docker-compose
├── backend/
│   ├── Dockerfile
│   ├── .env.example                # variables for running the API directly
│   ├── src/AssignmentSystem.Api/
│   │   ├── Program.cs              # DI, middleware order, migrate + seed on boot
│   │   ├── Controllers/            # thin — delegate to services
│   │   ├── Services/               # business rules AND authorization checks
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs     # relationships, unique indexes, check constraints
│   │   │   ├── Migrations/         # EF Core code-first migrations
│   │   │   └── Seed/DbSeeder.cs    # idempotent demo data
│   │   ├── Models/Entities/        # + Enums/
│   │   ├── DTOs/                   # request/response records per resource
│   │   ├── Validators/             # FluentValidation per request DTO
│   │   ├── Middleware/             # exception handling, validation filter
│   │   ├── Extensions/             # AddJwtAuth, AddSwaggerWithBearer, AddAppServices
│   │   └── Common/                 # NotFound/Forbidden/Conflict/Validation/Unauthorized exceptions
│   └── tests/AssignmentSystem.UnitTests/
│       ├── Services/               # business-rule and RBAC-leak tests
│       ├── Integration/            # WebApplicationFactory role-gating tests
│       └── Fixtures/               # SQLite factory, test data builder, fakes
└── frontend/
    ├── Dockerfile
    ├── .env.example                # NEXT_PUBLIC_API_URL
    └── src/
        ├── proxy.ts                # UX-only edge redirects by role
        ├── app/
        │   ├── (auth)/login/
        │   ├── admin/              # users, classes, subjects, class-subjects,
        │   │                       #   teacher-assignments, assignments (oversight)
        │   ├── teacher/            # assignments list/create, edit, submissions + grading
        │   └── student/            # assignments list/detail+submit, my submissions
        ├── components/ui/          # Button, Input, Select, Textarea, Card, Badge, Table
        ├── components/layout/      # Navbar, Sidebar, RoleGuard
        └── lib/
            ├── api/                # fetch wrapper + one module per resource
            ├── auth/               # AuthContext, token storage
            ├── types/              # shared entity types
            └── validation/         # zod schemas mirroring backend rules
```

---

## Data model

```
ClassCourse ──┐
              ├──< ClassSubject >──┬── Subject
Subject ──────┘                    │
                                   ├──< TeacherSubjectAssignment >── User (Teacher)
                                   │
                                   └──< Assignment ──< Submission >── User (Student)

User (Student) ──> ClassCourse        (one class per student)
```

| Entity | Key fields |
|---|---|
| `User` | `Id, FullName, Email (unique), PasswordHash, Role, ClassCourseId?, IsActive` |
| `ClassCourse` | `Id, Name, Description, IsActive` |
| `Subject` | `Id, Name, Code, IsActive` |
| `ClassSubject` | `Id, ClassCourseId, SubjectId` — unique on the pair |
| `TeacherSubjectAssignment` | `Id, TeacherId, ClassSubjectId` — unique on the pair |
| `Assignment` | `Id, Title, Description, ClassSubjectId, TeacherId, Deadline, MaxMarks, AllowLateSubmission, Status` |
| `Submission` | `Id, AssignmentId, StudentId, AnswerText, AttachmentUrl?, SubmittedAt, IsLate, Status, Marks?, Feedback?, GradedAt?, GradedByTeacherId?` — unique on `(AssignmentId, StudentId)` |

`TeacherSubjectAssignment` is the load-bearing table: every teacher-side permission check is a
lookup against it. Integrity is enforced in the database, not only in C# — unique indexes as above,
check constraints that an assignment's `MaxMarks` is positive and a submission's `Marks` is
non-negative, and `DeleteBehavior.Restrict` on every foreign key so nothing can cascade-delete a
student's grades.

---

## Key design decisions

**Assignments target a class-subject pair, not a class.** An assignment belongs to a
`ClassSubject`, which resolves "assign to a specific class/course and subject" in one foreign key
and makes the teacher permission check a single lookup on `(TeacherId, ClassSubjectId)`.

**One submission per student per assignment, edited in place.** Rather than a version-history
table, a student has exactly one `Submission` row per assignment, backed by a unique index. This
keeps "view submission status, marks and feedback" unambiguous — there is one authoritative row to
show. Editing rules:
1. Allowed only while the assignment is `Published` and belongs to the student's class.
2. Past the deadline, allowed only if `AllowLateSubmission` is true; the first late write sets `IsLate = true`.
3. Once `Graded`, the student can no longer edit — **unless** the teacher sets the status to
   `NeedsRevision`, which reopens editing regardless of the deadline. That is what the brief's
   "change the submission status when necessary" is for.

**Drafts return 404, not 403.** When a student requests an assignment they shouldn't see — a draft,
or another class's — the API answers `404 Not Found` rather than `403 Forbidden`. A 403 would
confirm that the assignment exists, which is itself a leak.

**Authorization lives in the service layer, not just on the controller.**
`[Authorize(Roles = "Teacher")]` proves the caller is *a* teacher; it says nothing about whether
they own the resource named in the URL. Every resource-scoped operation therefore re-checks
ownership against the database. The current user's identity is read from the validated JWT via
`ICurrentUserService` — never from an ID in the request body or path.

**Marks are validated against `MaxMarks` server-side.** `Marks >= 0` is a database check
constraint, but `Marks <= MaxMarks` spans two tables, so `SubmissionService.GradeAsync` enforces it
and a unit test pins the behaviour.

**No generic repository layer.** `DbContext` plus LINQ is already a data-access abstraction;
wrapping it in `IRepository<T>` at this scope would add indirection without adding a seam worth
having. Controllers stay thin, services own the rules.

**No ASP.NET Identity.** The requirement is login + JWT + roles, which a single `Users` table with
a `Role` enum satisfies. `PasswordHasher<User>` is still used for hashing, so password storage is
the same PBKDF2 implementation Identity would give you, without a dozen unused tables.

**Deadlines and lateness are computed server-side in UTC**, through an injectable `IClock`, so a
client's clock or timezone can never affect whether a submission counts as late — and tests can
control time directly.

---

## How role-based access is enforced

Access control is enforced by the **backend API**. The frontend's guards are for user experience
only — deleting a token in the browser or calling the API directly with `curl` gains nothing.

| Attack | What stops it |
|---|---|
| Student requests another student's submission by ID | `SubmissionService` requires `StudentId == currentUser.Id`, or that the caller is the owning teacher or an admin → `403` |
| Teacher grades a submission for a class-subject they don't teach | Every teacher operation joins through `TeacherSubjectAssignment` → `403` |
| Student lists assignments hoping for client-side-only filtering | List endpoints filter in SQL: `WHERE ClassCourseId = @studentClass AND Status = Published` |
| Student probes for drafts or other classes via status codes | Returns `404`, not `403`, so existence is never confirmed |
| Teacher creates an assignment for a class they don't teach by crafting the body | `AssignmentService.CreateAsync` verifies the `TeacherSubjectAssignment` row before inserting |
| Marks set above the maximum via a crafted grade request | `GradeAsync` validates `0 <= Marks <= MaxMarks` server-side |
| Duplicate submission through a race | Unique index on `(AssignmentId, StudentId)` backs the application check → `409` |
| Probing which emails have accounts via the login form | Unknown email and wrong password return an identical `401` message |

Verified end to end against the running stack: a student receives `404` for drafts and other
classes, `403` when touching another student's submission; the English teacher receives `403` on
every Maths resource; grading above `MaxMarks` returns `400`; a duplicate submission returns `409`;
a graded submission rejects student edits until it is reopened as `NeedsRevision`.

---

## Assumptions

The brief leaves these underspecified; here is what was chosen and why.

1. **A student belongs to exactly one class.** Modelled as a nullable `ClassCourseId` on `User`
   rather than an enrolment table. There is no enrolment history — moving a student to another
   class changes what they see going forward, and their past submissions stay attached to them.
2. **One submission per student per assignment, edited in place.** No version history; see the
   design decisions above for the full editing rules.
3. **A class and a subject are many-to-many**, joined by `ClassSubject`; an assignment binds exactly
   one such pair.
4. **No public self-registration.** Admins provision every account. This matches a school context
   and keeps role assignment out of an anonymous user's hands.
5. **Attachments are a URL, not an upload.** `AttachmentUrl` is a plain text field pointing at
   externally hosted work. Real file storage was out of scope; the field is there so the workflow is
   demonstrable.
6. **Deleting a user, class, or subject is a soft delete** (`IsActive = false`). Foreign keys use
   `Restrict`, so historical assignments and grades can never be destroyed by a delete elsewhere.
7. **JWTs last 60 minutes and there are no refresh tokens.** When a token expires the user logs in
   again; the client treats any `401` on a normal request as a session expiry and returns to login.
8. **A teacher may only be assigned to a class-subject by an admin**, and only then can they create
   assignments for it.
9. **`Marks` is a decimal**, so half marks (e.g. 8.5 / 10) are supported.
10. **All timestamps are stored and compared in UTC**; the UI renders them in the viewer's local
    timezone.

---

## Known limitations

Honest list of what this does not do, and what would change first with more time.

- **The JWT is stored in `localStorage`**, not an httpOnly cookie, which means it is reachable by
  JavaScript and therefore exposed to XSS. The mitigating factor is that all real access control is
  server-side, so a stolen token grants only that user's own permissions. A BFF pattern with
  httpOnly cookies plus refresh-token rotation would be the production choice.
- **`proxy.ts` decodes the JWT without verifying its signature.** It is a UX redirect only — a
  forged token would change nothing, because every API call is validated server-side.
- **No refresh tokens, no logout-everywhere, no token revocation list.** A token stays valid until
  it expires.
- **No pagination.** List endpoints return everything. Fine at demo scale, not at school scale;
  `GET /api/assignments` and `/api/users` would need paging and filtering first.
- **No real file uploads** — attachments are URLs, as noted above.
- **No frontend automated tests.** Testing effort went to the backend, where the business rules and
  authorization live and where the brief puts the emphasis. Component and E2E tests (Playwright)
  would be the next addition.
- **No email notifications** for new assignments, approaching deadlines, or returned grades.
- **Admin can grade.** `PUT /api/submissions/{id}/grade` accepts Admin as well as the owning
  teacher, as an administrative override. The admin UI deliberately does not expose it — the admin
  screens are read-only oversight — but the endpoint permits it.
- **No rate limiting or account lockout** on the login endpoint, so it is open to brute force.
- **Serilog writes to a file inside the container**, which is lost when the container is removed. A
  real deployment would ship logs to a collector (Seq, ELK, CloudWatch).
- **The default `Jwt__Key` in `appsettings.json` and `docker-compose.yml` is a documented
  placeholder** so the project runs with no setup. Any real deployment must override it with a
  generated secret via environment variables — never commit one.
