import { ReactNode } from "react";

export interface Column<T> {
  header: string;
  render: (row: T) => ReactNode;
  className?: string;
}

interface TableProps<T> {
  columns: Column<T>[];
  rows: T[];
  keyFor: (row: T) => string | number;
  emptyMessage?: string;
}

export function Table<T>({ columns, rows, keyFor, emptyMessage = "Nothing here yet." }: TableProps<T>) {
  if (rows.length === 0) {
    return <p className="py-6 text-sm text-zinc-500 dark:text-zinc-400">{emptyMessage}</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-left text-sm">
        <thead>
          <tr className="border-b border-zinc-200 text-zinc-500 dark:border-zinc-800 dark:text-zinc-400">
            {columns.map((col) => (
              <th key={col.header} className="px-3 py-2 font-medium">
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr
              key={keyFor(row)}
              className="border-b border-zinc-100 last:border-0 dark:border-zinc-900"
            >
              {columns.map((col) => (
                <td key={col.header} className={`px-3 py-2 ${col.className ?? ""}`}>
                  {col.render(row)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
