<!-- components/common/SectionCard.vue -->
<template>
  <div
    class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden"
  >
    <div
      class="flex items-center justify-between px-5 py-3.5 border-b border-slate-100"
    >
      <div class="flex items-center gap-2">
        <component :is="icon" :size="18" class="text-[#DA251D]" />
        <h3 class="text-slate-800 text-[15px] font-semibold">{{ title }}</h3>
      </div>
      <!-- Slot cho action button ở góc phải header nếu cần -->
      <slot name="action" />
    </div>
    <div class="p-5">
      <slot />
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, required: true },
  icon: { type: [Object, Function], required: true }, // Lucide icon component
});
</script>
