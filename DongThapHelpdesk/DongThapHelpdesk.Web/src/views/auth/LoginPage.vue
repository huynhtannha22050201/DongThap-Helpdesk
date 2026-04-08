<template>
  <div class="min-h-screen flex">
    <!-- LEFT: BRANDING (desktop only) -->
    <div class="hidden lg:flex lg:w-[55%] xl:w-[55%] relative overflow-hidden">
      <img
        :src="BG_IMAGE"
        alt="Đồng Tháp landscape"
        class="absolute inset-0 w-full h-full object-cover"
      />
      <div class="absolute inset-0 bg-gradient-to-br from-[#DA251D]/92 via-[#DA251D]/88 to-[#8B0000]/92" />

      <!-- Pattern -->
      <svg class="absolute inset-0 w-full h-full opacity-[0.06]" xmlns="http://www.w3.org/2000/svg">
        <defs>
          <pattern id="grid" width="40" height="40" patternUnits="userSpaceOnUse">
            <circle cx="20" cy="20" r="1.5" fill="white" />
          </pattern>
        </defs>
        <rect width="100%" height="100%" fill="url(#grid)" />
      </svg>

      <!-- Decorative circles -->
      <div class="absolute -top-20 -left-20 w-80 h-80 rounded-full border border-white/10" />
      <div class="absolute -bottom-32 -right-32 w-[500px] h-[500px] rounded-full border border-white/8" />
      <div class="absolute top-1/4 right-10 w-40 h-40 rounded-full bg-white/5" />

      <!-- Content -->
      <div class="relative z-10 flex flex-col justify-between p-12 xl:p-16 w-full">
        <div class="flex items-center gap-4">
          <EmblemIcon :size="64" />
          <div>
            <p class="text-white/70 uppercase tracking-wider text-[11px] font-semibold">UBND Tỉnh Đồng Tháp</p>
            <p class="text-white/50 text-[12px]">Cổng dịch vụ công trực tuyến</p>
          </div>
        </div>

        <div class="my-auto">
          <div class="w-16 h-1 bg-[#FFC627] rounded-full mb-8" />
          <h1 class="text-white mb-4 font-bold leading-[1.2]" style="font-size: clamp(28px, 3.5vw, 42px)">
            HỆ THỐNG<br />TIẾP NHẬN<br />
            <span class="text-[#FFC627]">PHẢN ÁNH, KIẾN NGHỊ</span>
          </h1>
          <p class="text-white/70 max-w-md text-[16px] leading-[1.7]">
            Tiếp nhận và xử lý phản ánh, kiến nghị của người dân trên địa bàn tỉnh Đồng Tháp một cách nhanh chóng, minh bạch và hiệu quả.
          </p>

          <div class="flex gap-8 mt-10">
            <div v-for="s in stats" :key="s.label">
              <p class="text-white text-[24px] font-bold">{{ s.value }}</p>
              <p class="text-white/50 text-[12px]">{{ s.label }}</p>
            </div>
          </div>
        </div>

        <p class="text-white/30 text-[12px]">© 2026 UBND Tỉnh Đồng Tháp — Phát triển bởi Sở Thông tin và Truyền thông</p>
      </div>
    </div>

    <!-- RIGHT: LOGIN FORM -->
    <div class="flex-1 flex flex-col min-h-screen bg-slate-50 relative">
      <!-- Mobile header -->
      <div class="lg:hidden bg-[#DA251D] px-6 py-5">
        <div class="flex items-center gap-3">
          <EmblemIcon :size="44" />
          <div>
            <p class="text-white text-[14px] font-semibold">Hệ thống Phản ánh, Kiến nghị</p>
            <p class="text-white/60 text-[12px]">Tỉnh Đồng Tháp</p>
          </div>
        </div>
      </div>

      <!-- Form area -->
      <div class="flex-1 flex items-center justify-center px-6 py-10">
        <div class="w-full max-w-[420px]">
          <!-- Heading -->
          <div class="text-center mb-8">
            <div class="w-14 h-14 rounded-2xl bg-[#DA251D]/10 flex items-center justify-center mx-auto mb-4">
              <Shield :size="26" class="text-[#DA251D]" />
            </div>
            <h2 class="text-slate-800 mb-1.5 text-[24px] font-bold">Đăng nhập hệ thống</h2>
            <p class="text-slate-500 text-[14px] leading-[1.6]">Sử dụng số điện thoại và mật khẩu đã đăng ký</p>
          </div>

          <!-- Error alert -->
          <div
            v-if="error"
            class="flex items-start gap-3 px-4 py-3.5 rounded-xl bg-red-50 border border-red-200 mb-5"
          >
            <AlertCircle :size="18" class="text-red-500 shrink-0 mt-0.5" />
            <div>
              <p class="text-red-700 text-[14px] font-semibold">Đăng nhập không thành công</p>
              <p class="text-red-600 mt-0.5 text-[13px]">Số điện thoại hoặc mật khẩu không đúng. Vui lòng thử lại.</p>
            </div>
          </div>

          <!-- Form -->
          <form @submit.prevent="handleSubmit" class="space-y-4">
            <!-- Phone -->
            <div>
              <label class="text-slate-700 mb-1.5 block text-[13px] font-medium">Số điện thoại</label>
              <div class="relative">
                <Phone :size="18" :class="['absolute left-4 top-1/2 -translate-y-1/2 transition-colors', phoneFocused ? 'text-[#DA251D]' : 'text-slate-400']" />
                <input
                  type="tel"
                  v-model="phone"
                  @focus="phoneFocused = true"
                  @blur="phoneFocused = false"
                  @input="error = false"
                  placeholder="Nhập số điện thoại"
                  :class="[inputBase, 'pl-12 pr-4', error ? inputError : phoneFocused ? inputFocus : inputNormal]"
                  class="text-[15px]"
                  autocomplete="tel"
                />
              </div>
            </div>

            <!-- Password -->
            <div>
              <label class="text-slate-700 mb-1.5 block text-[13px] font-medium">Mật khẩu</label>
              <div class="relative">
                <svg
                  :class="['absolute left-4 top-1/2 -translate-y-1/2 transition-colors', pwFocused ? 'text-[#DA251D]' : 'text-slate-400']"
                  width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                  stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                >
                  <rect width="18" height="11" x="3" y="11" rx="2" ry="2" />
                  <path d="M7 11V7a5 5 0 0 1 10 0v4" />
                </svg>
                <input
                  :type="showPassword ? 'text' : 'password'"
                  v-model="password"
                  @focus="pwFocused = true"
                  @blur="pwFocused = false"
                  @input="error = false"
                  placeholder="Nhập mật khẩu"
                  :class="[inputBase, 'pl-12 pr-12', error ? inputError : pwFocused ? inputFocus : inputNormal]"
                  class="text-[15px]"
                  autocomplete="current-password"
                />
                <button
                  type="button"
                  @click="showPassword = !showPassword"
                  class="absolute right-3 top-1/2 -translate-y-1/2 p-1.5 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors cursor-pointer"
                >
                  <EyeOff v-if="showPassword" :size="18" />
                  <Eye v-else :size="18" />
                </button>
              </div>
            </div>

            <!-- Remember + Forgot -->
            <div class="flex items-center justify-between pt-1">
              <label class="flex items-center gap-2.5 cursor-pointer select-none group">
                <div class="relative">
                  <input type="checkbox" v-model="remember" class="sr-only peer" />
                  <div class="w-[18px] h-[18px] rounded-md border-2 border-slate-300 peer-checked:border-[#DA251D] peer-checked:bg-[#DA251D] transition-all flex items-center justify-center group-hover:border-slate-400">
                    <svg v-if="remember" width="11" height="11" viewBox="0 0 12 12" fill="none">
                      <path d="M2.5 6L5 8.5L9.5 3.5" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                    </svg>
                  </div>
                </div>
                <span class="text-slate-600 text-[13px]">Ghi nhớ đăng nhập</span>
              </label>
              <a href="#" class="text-[#DA251D] hover:text-[#b81f18] transition-colors text-[13px] font-medium">Quên mật khẩu?</a>
            </div>

            <!-- Submit -->
            <button
              type="submit"
              :disabled="loading || !phone.trim() || !password.trim()"
              class="w-full flex items-center justify-center gap-2.5 py-3.5 rounded-xl bg-[#DA251D] text-white hover:bg-[#b81f18] active:scale-[0.99] disabled:opacity-50 disabled:cursor-not-allowed transition-all cursor-pointer shadow-lg shadow-[#DA251D]/20 text-[15px] font-semibold"
            >
              <template v-if="loading">
                <Loader2 :size="18" class="animate-spin" /> Đang xác thực...
              </template>
              <template v-else>
                <LogIn :size="18" /> Đăng nhập
              </template>
            </button>
          </form>

          <!-- Divider -->
          <div class="flex items-center gap-4 my-6">
            <div class="flex-1 h-px bg-slate-200" />
            <span class="text-slate-400 text-[13px]">hoặc</span>
            <div class="flex-1 h-px bg-slate-200" />
          </div>

          <!-- VNeID -->
          <button class="w-full flex items-center justify-center gap-2.5 py-3 rounded-xl border-2 border-slate-200 bg-white text-slate-700 hover:border-slate-300 hover:bg-slate-50 transition-all cursor-pointer text-[14px] font-medium">
            <div class="w-5 h-5 rounded bg-[#1E3A8A] flex items-center justify-center shrink-0">
              <span class="text-white text-[8px] font-extrabold" style="letter-spacing: -0.02em">VN</span>
            </div>
            Đăng nhập bằng VNeID
            <ExternalLink :size="14" class="text-slate-400" />
          </button>

          <!-- Citizen link -->
          <div class="mt-8 p-4 rounded-xl bg-[#FFC627]/10 border border-[#FFC627]/30">
            <div class="flex items-start gap-3">
              <div class="w-9 h-9 rounded-lg bg-[#FFC627]/20 flex items-center justify-center shrink-0 mt-0.5">
                <Landmark :size="18" class="text-[#b8900a]" />
              </div>
              <div>
                <p class="text-slate-700 mb-1 text-[14px] font-semibold">Bạn là người dân?</p>
                <p class="text-slate-500 mb-2 text-[13px] leading-[1.6]">Gửi phản ánh, kiến nghị không cần tài khoản đăng nhập.</p>
                <RouterLink to="/gui-phan-anh" class="inline-flex items-center gap-1 text-[#DA251D] hover:text-[#b81f18] transition-colors text-[13px] font-semibold">
                  Gửi phản ánh ngay <ChevronRight :size="14" />
                </RouterLink>
              </div>
            </div>
          </div>

          <p class="lg:hidden text-center text-slate-400 mt-8 text-[12px]">© 2026 UBND Tỉnh Đồng Tháp</p>
        </div>
      </div>

      <!-- Desktop bottom -->
      <div class="hidden lg:flex items-center justify-center pb-6 text-slate-400 gap-4 text-[12px]">
        <span>© 2026 UBND Tỉnh Đồng Tháp</span>
        <span>•</span>
        <a href="#" class="hover:text-slate-600 transition-colors">Hướng dẫn sử dụng</a>
        <span>•</span>
        <a href="#" class="hover:text-slate-600 transition-colors">Liên hệ hỗ trợ</a>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { RouterLink, useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  Phone, Eye, EyeOff, LogIn, AlertCircle, Shield,
  ChevronRight, Landmark, ExternalLink, Loader2, Star,
} from 'lucide-vue-next'

// ── Emblem sub-component ──
const EmblemIcon = {
  props: { size: { type: Number, default: 80 } },
  template: `
    <div
      class="rounded-full border-[3px] border-white/40 bg-white/10 backdrop-blur-sm flex items-center justify-center shrink-0"
      :style="{ width: size + 'px', height: size + 'px' }"
    >
      <div class="flex flex-col items-center">
        <svg :width="size * 0.3" :height="size * 0.3" viewBox="0 0 24 24" fill="#FFC627" stroke="#FFC627" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>
        </svg>
        <span class="text-white mt-0.5 font-bold" :style="{ fontSize: (size * 0.12) + 'px', letterSpacing: '0.05em' }">ĐỒNG THÁP</span>
      </div>
    </div>
  `
}

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const phone = ref('')
const password = ref('')
const showPassword = ref(false)
const remember = ref(false)
const error = ref(false)
const loading = ref(false)
const phoneFocused = ref(false)
const pwFocused = ref(false)

const BG_IMAGE = 'https://images.unsplash.com/photo-1743485754183-ab56725376fd?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxNZWtvbmclMjBkZWx0YSUyMFZpZXRuYW0lMjBzdW5zZXQlMjBsYW5kc2NhcGV8ZW58MXx8fHwxNzc1MTIzMDU4fDA&ixlib=rb-4.1.0&q=80&w=1080'

const stats = [
  { value: '12,847', label: 'Phản ánh tiếp nhận' },
  { value: '94.2%', label: 'Tỷ lệ xử lý đúng hạn' },
  { value: '11', label: 'Huyện/Thị/TP' },
]

const inputBase = 'w-full py-3 rounded-xl border bg-white outline-none transition-all duration-200'
const inputNormal = 'border-slate-200 hover:border-slate-300'
const inputFocus = 'border-[#DA251D]/50 ring-3 ring-[#DA251D]/10'
const inputError = 'border-red-300 ring-3 ring-red-100'

async function handleSubmit() {
  loading.value = true
  error.value = false

  const result = await auth.login(phone.value, password.value)

  loading.value = false

  if (result.success) {
    const redirect = route.query.redirect || (auth.isStaff ? '/admin' : '/')
    router.push(redirect)
  } else {
    error.value = true
  }
}
</script>