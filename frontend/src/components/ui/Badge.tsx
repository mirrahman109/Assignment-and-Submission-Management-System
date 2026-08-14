type Tone = "neutral" | "success" | "warning" | "danger" | "info";

const TONE_CLASSES: Record<Tone, string> = {
  neutral: "bg-zinc-100 text-zinc-700 dark:bg-zinc-800 dark:text-zinc-300",
  success: "bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300",
  warning: "bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300",
  danger: "bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-300",
  info: "bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300",
};

const STATUS_TONE: Record<string, Tone> = {
  Draft: "neutral",
  Published: "success",
  Submitted: "info",
  NeedsRevision: "warning",
  Graded: "success",
};

export function Badge({ children, tone }: { children: string; tone?: Tone }) {
  const resolvedTone = tone ?? STATUS_TONE[children] ?? "neutral";
  return (
    <span className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-medium ${TONE_CLASSES[resolvedTone]}`}>
      {children}
    </span>
  );
}
