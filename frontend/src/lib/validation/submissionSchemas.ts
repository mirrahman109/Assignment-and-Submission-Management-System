import { z } from "zod";

export const submissionSchema = z.object({
  answerText: z.string().min(1, "Answer is required"),
  attachmentUrl: z.string().url("Enter a valid URL").optional().or(z.literal("")),
});
export type SubmissionFormValues = z.infer<typeof submissionSchema>;
