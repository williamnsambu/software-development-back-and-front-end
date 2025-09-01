// src/components/ui/input.tsx
import * as React from "react";

export type InputProps = React.InputHTMLAttributes<HTMLInputElement>;

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className = "", ...props }, ref) => {
    return (
      <input
        ref={ref}
        className={`w-full rounded-xl border border-black/20 bg-transparent px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-black/20 ${className}`}
        {...props}
      />
    );
  }
);
Input.displayName = "Input";