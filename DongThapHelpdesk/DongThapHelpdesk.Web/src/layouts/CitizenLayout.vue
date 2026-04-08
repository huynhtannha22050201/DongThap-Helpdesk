<template>
  <div class="min-h-screen flex flex-col bg-slate-50">
    <!-- Header -->
    <header class="bg-[var(--color-brand-red)] text-white shadow-md">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <!-- Logo -->
          <RouterLink to="/" class="flex items-center gap-3">
            <div
              class="w-10 h-10 bg-white/20 rounded-lg flex items-center justify-center font-bold text-lg"
            >
              ĐT
            </div>
            <div class="hidden sm:block">
              <p class="font-semibold text-sm leading-tight">
                Hệ thống Phản ánh
              </p>
              <p class="text-xs text-white/80">Tỉnh Đồng Tháp</p>
            </div>
          </RouterLink>

          <!-- Navigation -->
          <nav class="flex items-center gap-2 sm:gap-4">
            <RouterLink
              to="/gui-phan-anh"
              class="px-3 py-2 text-sm font-medium rounded-md hover:bg-white/10 transition-colors"
            >
              Gửi phản ánh
            </RouterLink>
            <RouterLink
              to="/tra-cuu"
              class="px-3 py-2 text-sm font-medium rounded-md hover:bg-white/10 transition-colors"
            >
              Tra cứu
            </RouterLink>
            <RouterLink
              v-if="auth.isCitizen"
              to="/tai-khoan"
              class="px-3 py-2 text-sm font-medium rounded-md hover:bg-white/10 transition-colors"
            >
              Tài khoản
            </RouterLink>
            <RouterLink
              v-if="!auth.isLoggedIn"
              to="/login"
              class="px-4 py-2 bg-white text-[var(--color-brand-red)] text-sm font-semibold rounded-md hover:bg-slate-100 transition-colors"
            >
              Đăng nhập
            </RouterLink>
          </nav>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="flex-1">
      <RouterView />
    </main>

    <!-- Footer -->
    <footer class="bg-slate-800 text-slate-300 py-8">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
          <div>
            <h3 class="text-white font-semibold mb-3">UBND Tỉnh Đồng Tháp</h3>
            <p class="text-sm">Số 11, Lý Thường Kiệt, Phường 1</p>
            <p class="text-sm">TP. Cao Lãnh, Đồng Tháp</p>
          </div>
          <div>
            <h3 class="text-white font-semibold mb-3">Liên hệ</h3>
            <p class="text-sm">Điện thoại: (0277) 385 1431</p>
            <p class="text-sm">Email: ubnd@dongthap.gov.vn</p>
          </div>
          <div>
            <h3 class="text-white font-semibold mb-3">Hỗ trợ</h3>
            <p class="text-sm">Hướng dẫn sử dụng</p>
            <p class="text-sm">Chính sách bảo mật</p>
          </div>
        </div>
        <div
          class="mt-8 pt-6 border-t border-slate-700 text-center text-sm text-slate-400"
        >
          © 2026 Cổng Phản ánh Kiến nghị — Tỉnh Đồng Tháp
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { RouterLink, RouterView } from "vue-router";
import { useAuthStore } from "@/stores/auth";

const auth = useAuthStore();
</script>
