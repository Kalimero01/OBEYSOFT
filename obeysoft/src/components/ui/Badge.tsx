import * as React from "react";

import { cn } from "../../lib/utils";

const badgeVariants = {
  default: "border-transparent bg-secondary/60 text-secondary-foreground",
  outline: "border border-border text-muted-foreground",
  success: "border-transparent bg-emerald-500/15 text-emerald-300",
  warning: "border-transparent bg-amber-500/15 text-amber-200",
  danger: "border-transparent bg-destructive/20 text-destructive-foreground"
} as const;

export type BadgeVariant = keyof typeof badgeVariants;

export interface BadgeProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: BadgeVariant;
}

export function Badge({ className, variant = "default", ...props }: BadgeProps) {
  return (
    <div
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium",
        badgeVariants[variant],
        className
      )}
      {...props}
    />
  );
}


