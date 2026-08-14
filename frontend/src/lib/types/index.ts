export type UserRole = "Admin" | "Teacher" | "Student";
export type AssignmentStatus = "Draft" | "Published";
export type SubmissionStatus = "Submitted" | "NeedsRevision" | "Graded";

export interface UserSummary {
  id: number;
  fullName: string;
  email: string;
  role: UserRole;
  classCourseId: number | null;
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
  user: UserSummary;
}

export interface UserResponse {
  id: number;
  fullName: string;
  email: string;
  role: UserRole;
  classCourseId: number | null;
  classCourseName: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateUserRequest {
  fullName: string;
  email: string;
  password: string;
  role: UserRole;
  classCourseId: number | null;
}

export interface UpdateUserRequest {
  fullName: string;
  classCourseId: number | null;
}

export interface ClassCourseResponse {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
}

export interface CreateClassCourseRequest {
  name: string;
  description: string | null;
}

export interface UpdateClassCourseRequest {
  name: string;
  description: string | null;
  isActive: boolean;
}

export interface SubjectResponse {
  id: number;
  name: string;
  code: string;
  isActive: boolean;
}

export interface CreateSubjectRequest {
  name: string;
  code: string;
}

export interface UpdateSubjectRequest {
  name: string;
  code: string;
  isActive: boolean;
}

export interface ClassSubjectResponse {
  id: number;
  classCourseId: number;
  classCourseName: string;
  subjectId: number;
  subjectName: string;
}

export interface CreateClassSubjectRequest {
  classCourseId: number;
  subjectId: number;
}

export interface TeacherAssignmentResponse {
  id: number;
  teacherId: number;
  teacherName: string;
  classSubjectId: number;
  classCourseName: string;
  subjectName: string;
}

export interface CreateTeacherAssignmentRequest {
  teacherId: number;
  classSubjectId: number;
}

export interface AssignmentResponse {
  id: number;
  title: string;
  description: string;
  classSubjectId: number;
  classCourseName: string;
  subjectName: string;
  teacherId: number;
  teacherName: string;
  deadline: string;
  maxMarks: number;
  allowLateSubmission: boolean;
  status: AssignmentStatus;
  createdAt: string;
}

export interface CreateAssignmentRequest {
  title: string;
  description: string;
  classSubjectId: number;
  deadline: string;
  maxMarks: number;
  allowLateSubmission: boolean;
  publishImmediately: boolean;
}

export interface UpdateAssignmentRequest {
  title: string;
  description: string;
  deadline: string;
  maxMarks: number;
  allowLateSubmission: boolean;
}

export interface SubmissionResponse {
  id: number;
  assignmentId: number;
  assignmentTitle: string;
  studentId: number;
  studentName: string;
  answerText: string;
  attachmentUrl: string | null;
  submittedAt: string;
  updatedAt: string | null;
  isLate: boolean;
  status: SubmissionStatus;
  marks: number | null;
  maxMarks: number;
  feedback: string | null;
  gradedAt: string | null;
}

export interface CreateSubmissionRequest {
  answerText: string;
  attachmentUrl: string | null;
}

export interface UpdateSubmissionRequest {
  answerText: string;
  attachmentUrl: string | null;
}

export interface GradeSubmissionRequest {
  marks: number;
  feedback: string | null;
}
