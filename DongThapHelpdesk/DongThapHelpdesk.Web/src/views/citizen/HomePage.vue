<template>
  <div>
    <!-- HERO SECTION -->
    <section
      class="relative bg-gradient-to-br from-[#DA251D] via-[#DA251D] to-[#8B0000] overflow-hidden"
    >
      <div class="absolute inset-0 opacity-10">
        <div
          class="absolute top-0 right-0 w-96 h-96 bg-white rounded-full -translate-y-1/2 translate-x-1/3"
        />
        <div
          class="absolute bottom-0 left-0 w-72 h-72 bg-white rounded-full translate-y-1/2 -translate-x-1/4"
        />
      </div>
      <div
        class="absolute inset-0 opacity-[0.12]"
        :style="{
          backgroundImage: `url(${lotusPattern})`,
          backgroundSize: '300px',
          backgroundRepeat: 'repeat',
        }"
      />
      <svg
        class="absolute inset-0 w-full h-full opacity-[0.04]"
        xmlns="http://www.w3.org/2000/svg"
      >
        <defs>
          <pattern
            id="dots"
            width="40"
            height="40"
            patternUnits="userSpaceOnUse"
          >
            <circle cx="20" cy="20" r="1.5" fill="white" />
          </pattern>
        </defs>
        <rect width="100%" height="100%" fill="url(#dots)" />
      </svg>

      <div
        class="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16 sm:py-24 text-center"
      >
        <div
          class="w-16 h-16 rounded-2xl bg-white/15 backdrop-blur-sm flex items-center justify-center mx-auto mb-6"
        >
          <svg
            width="32"
            height="32"
            viewBox="0 0 24 24"
            fill="#FFC627"
            stroke="#FFC627"
            stroke-width="2"
          >
            <polygon
              points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"
            />
          </svg>
        </div>
        <h1
          class="text-white font-bold mb-3"
          style="font-size: clamp(28px, 5vw, 44px); line-height: 1.15"
        >
          CỔNG TIẾP NHẬN<br />
          <span class="text-[#FFC627]">PHẢN ÁNH, KIẾN NGHỊ</span>
        </h1>
        <p
          class="text-white/80 text-lg sm:text-xl mb-8 max-w-2xl mx-auto leading-relaxed"
        >
          Tỉnh Đồng Tháp — Tiếp nhận và xử lý phản ánh của người dân một cách
          nhanh chóng, minh bạch và hiệu quả
        </p>
        <div
          class="flex flex-col sm:flex-row items-center justify-center gap-4"
        >
          <RouterLink
            to="/gui-phan-anh"
            class="flex items-center gap-2.5 px-8 py-4 bg-white text-[#DA251D] rounded-xl font-semibold text-[16px] hover:bg-slate-50 transition-all shadow-lg shadow-black/10 cursor-pointer"
          >
            <PenSquare :size="20" /> Gửi Phản Ánh
          </RouterLink>
          <RouterLink
            to="/tra-cuu"
            class="flex items-center gap-2.5 px-8 py-4 border-2 border-white/40 text-white rounded-xl font-semibold text-[16px] hover:bg-white/10 transition-all cursor-pointer"
          >
            <Search :size="20" /> Tra Cứu Kết Quả
          </RouterLink>
        </div>
      </div>
    </section>

    <!-- HOW IT WORKS -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <div class="text-center mb-12">
        <h2 class="text-slate-800 text-[24px] sm:text-[28px] font-bold mb-2">
          Quy trình xử lý phản ánh
        </h2>
        <p class="text-slate-500 text-[15px]">
          4 bước đơn giản từ khi gửi đến khi nhận kết quả
        </p>
      </div>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <div
          v-for="(step, i) in steps"
          :key="i"
          class="relative bg-white rounded-xl border border-slate-100 shadow-sm p-6 text-center hover:shadow-md hover:border-slate-200 transition-all"
        >
          <div
            class="absolute -top-3 left-1/2 -translate-x-1/2 w-7 h-7 rounded-full bg-[#DA251D] text-white flex items-center justify-center text-[12px] font-bold"
          >
            {{ i + 1 }}
          </div>
          <div
            :class="[
              'w-14 h-14 rounded-2xl flex items-center justify-center mx-auto mb-4',
              step.iconBg,
            ]"
          >
            <component :is="step.icon" :size="24" :class="step.iconColor" />
          </div>
          <h3 class="text-slate-800 text-[15px] font-semibold mb-2">
            {{ step.title }}
          </h3>
          <p class="text-slate-500 text-[13px] leading-[1.6]">
            {{ step.desc }}
          </p>
        </div>
      </div>
    </section>

    <!-- STATISTICS -->
    <section class="bg-white border-y border-slate-100">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div class="grid grid-cols-2 lg:grid-cols-4 gap-6">
          <div v-for="stat in stats" :key="stat.label" class="text-center">
            <p
              :class="stat.valueColor"
              class="font-bold mb-1"
              style="font-size: clamp(28px, 4vw, 40px)"
            >
              {{ stat.value }}
            </p>
            <p class="text-slate-500 text-[14px]">{{ stat.label }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- RECENT REPORTS -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <div class="flex items-center justify-between mb-8">
        <div>
          <h2 class="text-slate-800 text-[24px] font-bold mb-1">
            Phản ánh gần đây
          </h2>
          <p class="text-slate-500 text-[14px]">
            Những phản ánh mới nhất từ người dân
          </p>
        </div>
        <RouterLink
          to="/tra-cuu"
          class="hidden sm:flex items-center gap-1.5 text-[#DA251D] text-[14px] font-medium hover:underline"
        >
          Xem tất cả <ArrowRight :size="16" />
        </RouterLink>
      </div>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
        <div
          v-for="t in recentTickets"
          :key="t.code"
          class="bg-white rounded-xl border border-slate-100 shadow-sm p-5 hover:shadow-md hover:border-slate-200 transition-all"
        >
          <div class="flex items-center justify-between mb-3">
            <span class="font-mono text-slate-400 text-[12px] font-medium">{{
              t.code
            }}</span>
            <span
              :class="[
                'inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-medium',
                STATUS_CFG[t.status].bg,
                STATUS_CFG[t.status].text,
              ]"
            >
              <span
                :class="['w-1.5 h-1.5 rounded-full', STATUS_CFG[t.status].dot]"
              />
              {{ STATUS_CFG[t.status].label }}
            </span>
          </div>
          <h3
            class="text-slate-800 text-[14px] font-medium mb-2 line-clamp-2 leading-[1.5]"
          >
            {{ t.title }}
          </h3>
          <div class="flex items-center gap-3 flex-wrap">
            <span
              class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-slate-100 text-slate-600 text-[11px] font-medium"
            >
              <Tag :size="10" /> {{ t.category }}
            </span>
            <span class="text-slate-400 text-[12px] flex items-center gap-1"
              ><Calendar :size="11" /> {{ t.date }}</span
            >
          </div>
        </div>
      </div>
      <div class="sm:hidden text-center mt-6">
        <RouterLink
          to="/tra-cuu"
          class="inline-flex items-center gap-1.5 text-[#DA251D] text-[14px] font-medium"
        >
          Xem tất cả phản ánh <ArrowRight :size="16" />
        </RouterLink>
      </div>
    </section>
  </div>
</template>

<script setup>
import { RouterLink } from "vue-router";
import {
  PenSquare,
  Search,
  CheckCircle,
  Wrench,
  MessageCircle,
  ArrowRight,
  Tag,
  Calendar,
} from "lucide-vue-next";
import lotusPattern from "@/assets/images/lotus-pattern.jpg";

const steps = [
  {
    title: "Gửi phản ánh",
    desc: "Người dân mô tả vấn đề, đính kèm hình ảnh và vị trí GPS",
    icon: PenSquare,
    iconBg: "bg-blue-50",
    iconColor: "text-blue-600",
  },
  {
    title: "Tiếp nhận & Phân công",
    desc: "Điều phối viên kiểm duyệt và phân công cho phòng ban xử lý",
    icon: CheckCircle,
    iconBg: "bg-indigo-50",
    iconColor: "text-indigo-600",
  },
  {
    title: "Xử lý",
    desc: "Cán bộ tiến hành xử lý và báo cáo kết quả qua hệ thống",
    icon: Wrench,
    iconBg: "bg-amber-50",
    iconColor: "text-amber-600",
  },
  {
    title: "Phản hồi kết quả",
    desc: "Người dân nhận thông báo kết quả và đánh giá chất lượng",
    icon: MessageCircle,
    iconBg: "bg-emerald-50",
    iconColor: "text-emerald-600",
  },
];

const stats = [
  {
    value: "12,847",
    label: "Tổng phản ánh tiếp nhận",
    valueColor: "text-slate-800",
  },
  {
    value: "11,902",
    label: "Đã xử lý hoàn thành",
    valueColor: "text-emerald-600",
  },
  { value: "94.2%", label: "Tỷ lệ đúng hạn SLA", valueColor: "text-[#FFC627]" },
  { value: "156", label: "Đang xử lý", valueColor: "text-amber-600" },
];

const STATUS_CFG = {
  New: {
    label: "Mới",
    bg: "bg-blue-50",
    text: "text-blue-700",
    dot: "bg-blue-500",
  },
  InProgress: {
    label: "Đang xử lý",
    bg: "bg-amber-50",
    text: "text-amber-700",
    dot: "bg-amber-500",
  },
  Resolved: {
    label: "Đã xử lý",
    bg: "bg-emerald-50",
    text: "text-emerald-700",
    dot: "bg-emerald-500",
  },
};

const recentTickets = [
  {
    code: "PA-042026-001",
    title:
      "Ổ gà lớn trên đường Nguyễn Huệ, đoạn gần ngã tư Lê Lợi gây nguy hiểm",
    category: "Giao thông",
    status: "New",
    date: "02/04/2026",
  },
  {
    code: "PA-032026-012",
    title: "Rác thải công nghiệp đổ trộm ven sông Tiền khu vực cầu Mỹ Thuận",
    category: "Môi trường",
    status: "InProgress",
    date: "01/04/2026",
  },
  {
    code: "PA-032026-010",
    title:
      "Đèn đường hư hỏng khu vực Phường 2 đoạn dài 200m không có chiếu sáng",
    category: "Hạ tầng",
    status: "InProgress",
    date: "01/04/2026",
  },
  {
    code: "PA-032026-007",
    title: "Cầu gỗ xuống cấp nguy hiểm tại xã Mỹ Tân, huyện Cao Lãnh",
    category: "Giao thông",
    status: "Resolved",
    date: "29/03/2026",
  },
  {
    code: "PA-032026-006",
    title: "Xả rác bừa bãi tại chợ Cao Lãnh gây mất vệ sinh môi trường",
    category: "Môi trường",
    status: "Resolved",
    date: "28/03/2026",
  },
  {
    code: "PA-032026-005",
    title: "Tiếng ồn từ quán karaoke hoạt động sau 22h tại khu phố 3 Phường 1",
    category: "An ninh",
    status: "InProgress",
    date: "27/03/2026",
  },
];
</script>
