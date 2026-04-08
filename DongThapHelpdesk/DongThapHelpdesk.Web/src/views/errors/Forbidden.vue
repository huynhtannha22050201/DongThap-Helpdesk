<template>
  <div class="min-h-screen flex items-center justify-center bg-slate-50">
    <div class="text-center">
      <p class="text-6xl font-bold text-[var(--color-brand-red)]">403</p>
      <h1 class="mt-4 text-2xl font-semibold text-slate-800">
        Không có quyền truy cập
      </h1>
      <p class="mt-2 text-slate-500">Bạn không có quyền truy cập trang này.</p>
      <RouterLink
        to="/"
        class="mt-6 inline-block px-6 py-2.5 bg-[var(--color-brand-red)] text-white text-sm font-medium rounded-md hover:bg-[var(--color-brand-red-hover)] transition-colors"
      >
        Về trang chủ
      </RouterLink>
    </div>
  </div>
</template>

<script setup>
import { RouterLink } from "vue-router";
</script>
