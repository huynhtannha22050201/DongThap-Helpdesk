<!-- components/common/InfoRow.vue -->
<template>
  <div class="flex items-start gap-3 py-2.5">
    <div v-if="icon" class="text-slate-400 mt-0.5 shrink-0">
      <component :is="icon" :size="15" />
    </div>
    <div class="min-w-0">
      <p class="text-slate-400 mb-0.5 text-[12px] font-medium">{{ label }}</p>
      <div class="text-slate-700 text-[14px]">
        <!-- Slot để truyền nội dung phức tạp, fallback về prop value -->
        <slot>{{ value }}</slot>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  label: { type: String, required: true },
  value: { type: String, default: "" },
  icon: { type: [Object, Function], default: null },
});
</script>
