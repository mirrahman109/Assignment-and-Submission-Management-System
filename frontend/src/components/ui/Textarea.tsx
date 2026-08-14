import { TextareaHTMLAttributes, forwardRef } from "react";

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  error?: string;
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ label, error, className = "", id, rows = 4, ...props }, ref) => {
    const areaId = id ?? props.name;
    return (
      <div className="mb-4">
        {label && (
          <label htmlFor={areaId} className="mb-1 block text-sm font-medium">
            {label}
          </label>
        )}
        <textarea
          ref={ref}
          id={areaId}
          rows={rows}
          className={`w-full rounded-md border px-3 py-2 text-sm dark:bg-zinc-900 ${
            error ? "border-red-500" : "border-zinc-300 dark:border-zinc-700"
          } ${className}`}
          {...props}
        />
        {error && <p className="mt-1 text-xs text-red-600 dark:text-red-400">{error}</p>}
      </div>
    );
  },
);
Textarea.displayName = "Textarea";
