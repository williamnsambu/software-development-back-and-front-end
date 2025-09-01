// src/components/ui/button.tsx
import * as React from "react";

export type ButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: "default" | "outline";
};

export function Button({
  className = "",
  variant = "default",
  ...props
}: ButtonProps) {
  const base =
    "inline-flex items-center justify-center rounded-xl px-4 py-2 text-sm font-medium transition";
  const variants = {
    default: "bg-black/80 text-white hover:bg-black",
    outline:
      "border border-black/20 hover:bg-black/5 text-black dark:text-white",
  };
  return (
    <button className={`${base} ${variants[variant]} ${className}`} {...props} />
  );
}