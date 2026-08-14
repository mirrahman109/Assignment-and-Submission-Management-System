import { SelectHTMLAttributes, forwardRef } from "react";

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ label, error, className = "", id, children, ...props }, ref) => {
    const selectId = id ?? props.name;
    return (
      <div className="mb-4">
        {label && (
          <label htmlFor={selectId} className="mb-1 block text-sm font-medium">
            {label}
          </label>
        )}
        <select
          ref={ref}
          id={selectId}
          className={`w-full rounded-md border px-3 py-2 text-sm dark:bg-zinc-900 ${
            error ? "border-red-500" : "border-zinc-300 dark:border-zinc-700"
          } ${className}`}
          {...props}
        >
          {children}
        </select>
        {error && <p className="mt-1 text-xs text-red-600 dark:text-red-400">{error}</p>}
      </div>
    );
  },
);
Select.displayName = "Select";
