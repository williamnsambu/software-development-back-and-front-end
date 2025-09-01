// src/components/ui/card.tsx
import * as React from "react";

export type CardProps = React.HTMLAttributes<HTMLDivElement>;

export function Card({ className = "", ...props }: CardProps) {
  return (
    <div
      className={`rounded-2xl border p-4 shadow-sm bg-white/5 ${className}`}
      {...props}
    />
  );
}