import { InputHTMLAttributes, forwardRef } from "react";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(({ label, error, className = "", id, ...props }, ref) => {
  const inputId = id ?? props.name;
  return (
    <div className="mb-4">
      {label && (
        <label htmlFor={inputId} className="mb-1 block text-sm font-medium">
          {label}
        </label>
      )}
      <input
        ref={ref}
        id={inputId}
        className={`w-full rounded-md border px-3 py-2 text-sm dark:bg-zinc-900 ${
          error ? "border-red-500" : "border-zinc-300 dark:border-zinc-700"
        } ${className}`}
        {...props}
      />
      {error && <p className="mt-1 text-xs text-red-600 dark:text-red-400">{error}</p>}
    </div>
  );
});
Input.displayName = "Input";
