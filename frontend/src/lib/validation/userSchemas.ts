import { z } from "zod";

export const createUserSchema = z
  .object({
    fullName: z.string().min(1, "Full name is required").max(200),
    email: z.string().min(1, "Email is required").email("Enter a valid email"),
    password: z.string().min(6, "Password must be at least 6 characters"),
    role: z.enum(["Admin", "Teacher", "Student"]),
    classCourseId: z.string().optional(),
  })
  .refine((data) => data.role !== "Student" || !!data.classCourseId, {
    message: "A student must be assigned to a class",
    path: ["classCourseId"],
  });

export type CreateUserFormValues = z.infer<typeof createUserSchema>;

export const updateUserSchema = z.object({
  fullName: z.string().min(1, "Full name is required").max(200),
  classCourseId: z.string().optional(),
});

export type UpdateUserFormValues = z.infer<typeof updateUserSchema>;
