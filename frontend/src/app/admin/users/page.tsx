"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuth } from "@/lib/auth/AuthContext";
import { ApiError } from "@/lib/api/client";
import { createUser, deactivateUser, listUsers, updateUser } from "@/lib/api/usersApi";
import { listClasses } from "@/lib/api/classesApi";
import type { ClassCourseResponse, UserResponse, UserRole } from "@/lib/types";
import { createUserSchema, type CreateUserFormValues } from "@/lib/validation/userSchemas";
import { Card } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Select } from "@/components/ui/Select";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { Table, type Column } from "@/components/ui/Table";

export default function AdminUsersPage() {
  const { token } = useAuth();
  const [users, setUsers] = useState<UserResponse[]>([]);
  const [classes, setClasses] = useState<ClassCourseResponse[]>([]);
  const [roleFilter, setRoleFilter] = useState<UserRole | "">("");
  const [editing, setEditing] = useState<UserResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const {
    register,
    handleSubmit,
    watch,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CreateUserFormValues>({
    resolver: zodResolver(createUserSchema),
    defaultValues: { role: "Student" },
  });
  const watchedRole = watch("role");

  async function loadUsers() {
    if (!token) return;
    setIsLoading(true);
    try {
      const data = await listUsers(token, roleFilter || undefined);
      setUsers(data);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load users.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    if (!token) return;
    loadUsers();
    listClasses(token).then(setClasses).catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, roleFilter]);

  async function onCreate(values: CreateUserFormValues) {
    if (!token) return;
    setError(null);
    try {
      await createUser(token, {
        fullName: values.fullName,
        email: values.email,
        password: values.password,
        role: values.role,
        classCourseId: values.role === "Student" && values.classCourseId ? Number(values.classCourseId) : null,
      });
      reset({ role: "Student", fullName: "", email: "", password: "", classCourseId: "" });
      await loadUsers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create user.");
    }
  }

  async function onSaveEdit(fullName: string, classCourseId: string) {
    if (!token || !editing) return;
    setError(null);
    try {
      await updateUser(token, editing.id, {
        fullName,
        classCourseId: editing.role === "Student" && classCourseId ? Number(classCourseId) : null,
      });
      setEditing(null);
      await loadUsers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update user.");
    }
  }

  async function onDeactivate(user: UserResponse) {
    if (!token) return;
    if (!confirm(`Deactivate ${user.fullName}?`)) return;
    try {
      await deactivateUser(token, user.id);
      await loadUsers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to deactivate user.");
    }
  }

  const columns: Column<UserResponse>[] = [
    { header: "Name", render: (u) => u.fullName },
    { header: "Email", render: (u) => u.email },
    { header: "Role", render: (u) => <Badge tone="info">{u.role}</Badge> },
    { header: "Class", render: (u) => u.classCourseName ?? "—" },
    {
      header: "Status",
      render: (u) => <Badge tone={u.isActive ? "success" : "danger"}>{u.isActive ? "Active" : "Inactive"}</Badge>,
    },
    {
      header: "Actions",
      render: (u) =>
        u.isActive ? (
          <div className="flex gap-2">
            <Button variant="secondary" className="px-2 py-1 text-xs" onClick={() => setEditing(u)}>
              Edit
            </Button>
            <Button variant="danger" className="px-2 py-1 text-xs" onClick={() => onDeactivate(u)}>
              Deactivate
            </Button>
          </div>
        ) : null,
    },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Users</h1>

      <Card>
        <h2 className="mb-4 text-lg font-medium">Add user</h2>
        <form onSubmit={handleSubmit(onCreate)} className="grid grid-cols-1 gap-x-4 sm:grid-cols-2">
          <Input label="Full name" {...register("fullName")} error={errors.fullName?.message} />
          <Input label="Email" type="email" {...register("email")} error={errors.email?.message} />
          <Input label="Password" type="password" {...register("password")} error={errors.password?.message} />
          <Select label="Role" {...register("role")} error={errors.role?.message}>
            <option value="Admin">Admin</option>
            <option value="Teacher">Teacher</option>
            <option value="Student">Student</option>
          </Select>
          {watchedRole === "Student" && (
            <Select label="Class" {...register("classCourseId")} error={errors.classCourseId?.message}>
              <option value="">Select a class</option>
              {classes.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </Select>
          )}
          <div className="sm:col-span-2">
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Creating..." : "Create user"}
            </Button>
          </div>
        </form>
        {error && <p className="mt-3 text-sm text-red-600 dark:text-red-400">{error}</p>}
      </Card>

      <Card>
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-lg font-medium">All users</h2>
          <Select
            value={roleFilter}
            onChange={(e) => setRoleFilter(e.target.value as UserRole | "")}
            className="w-40"
          >
            <option value="">All roles</option>
            <option value="Admin">Admin</option>
            <option value="Teacher">Teacher</option>
            <option value="Student">Student</option>
          </Select>
        </div>
        {isLoading ? (
          <p className="text-sm text-zinc-500">Loading...</p>
        ) : (
          <Table columns={columns} rows={users} keyFor={(u) => u.id} emptyMessage="No users found." />
        )}
      </Card>

      {editing && (
        <EditUserForm
          user={editing}
          classes={classes}
          onCancel={() => setEditing(null)}
          onSave={onSaveEdit}
        />
      )}
    </div>
  );
}

/** Email and role are deliberately not editable: the email is the login identifier, and changing a
 *  role would strand the class link and any assignments/submissions already tied to the account.
 *  Deactivate and re-create instead. */
function EditUserForm({
  user,
  classes,
  onCancel,
  onSave,
}: {
  user: UserResponse;
  classes: ClassCourseResponse[];
  onCancel: () => void;
  onSave: (fullName: string, classCourseId: string) => Promise<void>;
}) {
  const [fullName, setFullName] = useState(user.fullName);
  const [classCourseId, setClassCourseId] = useState(user.classCourseId ? String(user.classCourseId) : "");
  const [isSaving, setIsSaving] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setIsSaving(true);
    try {
      await onSave(fullName, classCourseId);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Card>
      <h2 className="mb-1 text-lg font-medium">Edit {user.email}</h2>
      <p className="mb-4 text-sm text-zinc-600 dark:text-zinc-400">
        Email and role can&apos;t be changed — deactivate and re-create the account instead.
      </p>
      <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-x-4 sm:grid-cols-2">
        <Input label="Full name" value={fullName} onChange={(e) => setFullName(e.target.value)} />
        {user.role === "Student" && (
          <Select label="Class" value={classCourseId} onChange={(e) => setClassCourseId(e.target.value)}>
            <option value="">Select a class</option>
            {classes.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </Select>
        )}
        <div className="flex gap-2 sm:col-span-2">
          <Button type="submit" disabled={isSaving || !fullName.trim()}>
            {isSaving ? "Saving..." : "Save changes"}
          </Button>
          <Button type="button" variant="secondary" onClick={onCancel}>
            Cancel
          </Button>
        </div>
      </form>
    </Card>
  );
}
