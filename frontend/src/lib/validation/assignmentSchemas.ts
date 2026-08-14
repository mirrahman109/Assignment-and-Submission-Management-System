import { z } from "zod";

// Number inputs are registered with react-hook-form's `valueAsNumber`, so the value
// reaching zod is already a number (NaN when the field is blank/non-numeric). Using
// z.number() rather than z.coerce.number() keeps the schema's input and output types
// identical, which is what zodResolver needs to line up with useForm's generic.
const numericField = (message: string) => z.number({ error: message });

export const createAssignmentSchema = z.object({
  title: z.string().min(1, "Title is required").max(200),
  description: z.string().min(1, "Description is required"),
  classSubjectId: z.string().min(1, "Select a class/subject"),
  deadline: z.string().min(1, "Deadline is required"),
  maxMarks: numericField("Max marks is required").positive("Max marks must be greater than zero"),
  allowLateSubmission: z.boolean(),
  publishImmediately: z.boolean(),
});
export type CreateAssignmentFormValues = z.infer<typeof createAssignmentSchema>;

export const updateAssignmentSchema = z.object({
  title: z.string().min(1, "Title is required").max(200),
  description: z.string().min(1, "Description is required"),
  deadline: z.string().min(1, "Deadline is required"),
  maxMarks: numericField("Max marks is required").positive("Max marks must be greater than zero"),
  allowLateSubmission: z.boolean(),
});
export type UpdateAssignmentFormValues = z.infer<typeof updateAssignmentSchema>;

export const gradeSubmissionSchema = z.object({
  marks: numericField("Marks are required").min(0, "Marks cannot be negative"),
  feedback: z.string().optional(),
});
export type GradeSubmissionFormValues = z.infer<typeof gradeSubmissionSchema>;
