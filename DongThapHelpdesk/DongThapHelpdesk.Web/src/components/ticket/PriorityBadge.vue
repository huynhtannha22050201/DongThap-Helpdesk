<!-- components/ticket/PriorityBadge.vue -->
<template>
  <span
    :class="[
      'inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full border text-[13px] font-medium',
      cfg.bg,
    ]"
  >
    <CircleDot :size="12" />
    {{ cfg.label }}
  </span>
</template>

<script setup>
import { computed } from "vue";
import { CircleDot } from "lucide-vue-next";

const props = defineProps({
  priority: { type: String, required: true },
});

const priorityMap = {
  Low: {
    bg: "bg-slate-50 text-slate-500 border-slate-200",
    label: "Ưu tiên thấp",
  },
  Normal: {
    bg: "bg-blue-50 text-blue-600 border-blue-100",
    label: "Ưu tiên TB",
  },
  High: { bg: "bg-red-50 text-red-600 border-red-100", label: "Ưu tiên cao" },
  Urgent: {
    bg: "bg-orange-50 text-orange-600 border-orange-100",
    label: "Khẩn cấp",
  },
};

const cfg = computed(() => priorityMap[props.priority] || priorityMap.Normal);
</script>
