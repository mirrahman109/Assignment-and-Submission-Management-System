import { z } from "zod";

export const classSchema = z.object({
  name: z.string().min(1, "Name is required").max(150),
  description: z.string().max(1000).optional(),
});
export type ClassFormValues = z.infer<typeof classSchema>;

export const subjectSchema = z.object({
  name: z.string().min(1, "Name is required").max(150),
  code: z.string().min(1, "Code is required").max(30),
});
export type SubjectFormValues = z.infer<typeof subjectSchema>;
