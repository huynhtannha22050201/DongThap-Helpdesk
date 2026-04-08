<template>
  <div class="min-h-screen flex items-center justify-center bg-slate-50">
    <div class="text-center">
      <p class="text-6xl font-bold text-slate-300">404</p>
      <h1 class="mt-4 text-2xl font-semibold text-slate-800">
        Không tìm thấy trang
      </h1>
      <p class="mt-2 text-slate-500">Trang bạn tìm kiếm không tồn tại.</p>
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
