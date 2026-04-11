import { cva, type VariantProps } from "class-variance-authority";
import * as React from "react";
import { cn } from "../../lib/utils";

const badgeVariants = cva(
  "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold",
  {
    variants: {
      variant: {
        default:  "bg-blue-100 text-blue-700 dark:bg-blue-900/50 dark:text-blue-300",
        receita:  "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/50 dark:text-emerald-300",
        despesa:  "bg-red-100 text-red-700 dark:bg-red-900/50 dark:text-red-300",
        ambas:    "bg-violet-100 text-violet-700 dark:bg-violet-900/50 dark:text-violet-300",
        outline:  "border border-gray-300 text-gray-600 dark:border-gray-600 dark:text-gray-300",
      },
    },
    defaultVariants: {
      variant: "default",
    },
  }
);

export interface BadgeProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> {}

function Badge({ className, variant, ...props }: BadgeProps) {
  return <div className={cn(badgeVariants({ variant }), className)} {...props} />;
}

export { Badge, badgeVariants };
