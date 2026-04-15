<!-- components/ticket/StatusBadge.vue -->
<template>
  <span
    :class="[
      'inline-flex items-center gap-1.5 rounded-full font-medium',
      cfg.bg,
      sizeClass,
    ]"
  >
    <span :class="['rounded-full', cfg.dot, dotSize]" />
    {{ cfg.label }}
  </span>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  status: { type: String, required: true },
  size: { type: String, default: "md" }, // "sm" | "md"
});

// Màu khớp design tokens trong rules.md
const statusMap = {
  New: { bg: "bg-blue-50 text-blue-700", dot: "bg-blue-500", label: "Mới" },
  UnderReview: {
    bg: "bg-orange-50 text-orange-700",
    dot: "bg-orange-500",
    label: "Đang duyệt",
  },
  Assigned: {
    bg: "bg-indigo-50 text-indigo-700",
    dot: "bg-indigo-500",
    label: "Đã giao",
  },
  InProgress: {
    bg: "bg-amber-50 text-amber-700",
    dot: "bg-amber-500",
    label: "Đang xử lý",
  },
  PendingVerification: {
    bg: "bg-violet-50 text-violet-700",
    dot: "bg-violet-500",
    label: "Chờ xác minh",
  },
  Closed: {
    bg: "bg-slate-100 text-slate-500",
    dot: "bg-slate-400",
    label: "Đã đóng",
  },
  Rejected: {
    bg: "bg-red-50 text-red-600",
    dot: "bg-red-500",
    label: "Từ chối",
  },
};

const cfg = computed(() => statusMap[props.status] || statusMap.New);

// Responsive size cho badge
const sizeClass = computed(() =>
  props.size === "sm" ? "px-2 py-0.5 text-[11px]" : "px-2.5 py-1 text-[13px]",
);
const dotSize = computed(() =>
  props.size === "sm" ? "w-1 h-1" : "w-1.5 h-1.5",
);
</script>
