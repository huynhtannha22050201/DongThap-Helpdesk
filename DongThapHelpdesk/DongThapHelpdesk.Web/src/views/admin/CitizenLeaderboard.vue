<template>
  <div class="space-y-5">
    <!-- Header -->
    <div
      class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3"
    >
      <div>
        <h1
          class="text-slate-800 font-semibold"
          style="font-size: clamp(22px, 3vw, 26px)"
        >
          Bảng xếp hạng Công dân tích cực
        </h1>
        <p class="text-slate-500 mt-0.5 text-[14px]">
          Thống kê người dân đóng góp nhiều nhất — Dữ liệu phục vụ khen thưởng
        </p>
      </div>
      <div class="flex items-center gap-2">
        <div
          class="flex items-center bg-white border border-slate-200 rounded-lg overflow-hidden"
        >
          <button
            v-for="p in periodOptions"
            :key="p.key"
            @click="activePeriod = p.key"
            :class="[
              'px-4 py-2.5 transition-colors cursor-pointer text-[13px] font-medium',
              activePeriod === p.key
                ? 'bg-[#DA251D] text-white'
                : 'text-slate-600 hover:bg-slate-50',
            ]"
          >
            {{ p.label }}
          </button>
        </div>
      </div>
    </div>

    <!-- Stats overview -->
    <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
      <div
        v-for="s in statsCards"
        :key="s.label"
        class="bg-white rounded-xl border border-slate-100 shadow-sm px-4 py-3.5 flex items-center gap-3"
      >
        <div
          :class="[
            'w-10 h-10 rounded-xl flex items-center justify-center shrink-0',
            s.color,
          ]"
        >
          <component :is="s.icon" :size="18" />
        </div>
        <div>
          <p class="text-slate-800 text-[20px] font-bold">{{ s.value }}</p>
          <p class="text-slate-400 text-[12px]">{{ s.label }}</p>
        </div>
      </div>
    </div>

    <!-- Top 3 Podium -->
    <div
      class="bg-gradient-to-br from-[#DA251D] via-[#DA251D] to-[#8B0000] rounded-2xl p-6 sm:p-8 lg:p-10 relative overflow-hidden"
    >
      <!-- Background decorations -->
      <div class="absolute inset-0 opacity-[0.06]">
        <div
          class="absolute top-0 right-0 w-80 h-80 bg-white rounded-full -translate-y-1/2 translate-x-1/3"
        />
        <div
          class="absolute bottom-0 left-0 w-52 h-52 bg-white rounded-full translate-y-1/2 -translate-x-1/4"
        />
        <div
          class="absolute top-1/2 left-1/3 w-32 h-32 bg-white rounded-full opacity-50"
        />
      </div>

      <div class="relative">
        <!-- Header -->
        <div class="text-center mb-14">
          <div
            class="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-[#FFC627]/20 backdrop-blur-sm mb-3"
          >
            <Trophy :size="16" class="text-[#FFC627]" />
            <span class="text-[#FFC627] text-[13px] font-semibold">{{
              activePeriod === "month" ? "Tháng 4/2026" : "Quý 2/2026"
            }}</span>
          </div>
          <h2 class="text-white text-[22px] sm:text-[26px] font-bold">
            Top 3 Công dân Tiêu biểu
          </h2>
          <p class="text-white/50 text-[13px] mt-1">
            Những người dân có đóng góp tích cực nhất trong kỳ
          </p>
        </div>

        <!-- Podium layout: 2nd - 1st - 3rd -->
        <div
          class="flex items-end justify-center gap-4 sm:gap-6 max-w-3xl mx-auto pb-2"
        >
          <!-- 2nd place -->
          <div class="flex-1 max-w-[210px] self-end">
            <div
              class="bg-gradient-to-b from-white/15 to-white/5 backdrop-blur-sm rounded-2xl border border-white/15 p-5 sm:p-6 text-center transition-all hover:from-white/20 hover:to-white/10 hover:shadow-lg hover:shadow-black/10"
            >
              <!-- Avatar -->
              <div class="relative inline-block mb-4">
                <div
                  class="w-18 h-18 sm:w-20 sm:h-20 rounded-full bg-gradient-to-br from-slate-200 via-slate-100 to-slate-300 flex items-center justify-center text-[22px] sm:text-[26px] font-bold text-slate-600 mx-auto ring-4 ring-white/20 shadow-xl"
                  style="width: 80px; height: 80px"
                >
                  {{ top3[1].initials }}
                </div>
                <div
                  class="absolute -bottom-2 left-1/2 -translate-x-1/2 w-7 h-7 rounded-full bg-gradient-to-br from-slate-300 to-slate-400 flex items-center justify-center text-[13px] font-bold text-white shadow-md ring-2 ring-white/30"
                >
                  2
                </div>
              </div>
              <h3 class="text-white text-[15px] font-semibold truncate">
                {{ top3[1].name }}
              </h3>
              <p class="text-white/40 text-[12px] mb-4 truncate">
                {{ top3[1].ward }}, {{ top3[1].district }}
              </p>
              <!-- Points -->
              <div class="flex items-center justify-center gap-1.5 mb-3">
                <Star :size="16" class="text-[#FFC627]" fill="#FFC627" />
                <span class="text-white text-[24px] font-extrabold">{{
                  top3[1].periodPoints
                }}</span>
              </div>
              <div
                class="flex items-center justify-center gap-2 text-white/35 text-[11px]"
              >
                <span>{{ top3[1].approved }} PA</span>
                <span class="w-1 h-1 rounded-full bg-white/20" />
                <span>{{ top3[1].rate }}%</span>
              </div>
            </div>
          </div>

          <!-- 1st place (tallest) -->
          <div class="flex-1 max-w-[230px] -mt-6">
            <div
              class="relative bg-gradient-to-b from-white/20 to-white/8 backdrop-blur-sm rounded-2xl border border-[#FFC627]/25 p-5 sm:p-7 text-center transition-all hover:from-white/25 hover:to-white/12 hover:shadow-xl hover:shadow-black/10"
            >
              <!-- Crown floating above -->
              <div class="absolute -top-5 left-1/2 -translate-x-1/2">
                <div
                  class="w-10 h-10 rounded-full bg-[#FFC627] flex items-center justify-center shadow-lg shadow-[#FFC627]/40 animate-bounce"
                  style="animation-duration: 2s"
                >
                  <Crown :size="20" class="text-[#8B4513]" />
                </div>
              </div>
              <!-- Avatar -->
              <div class="relative inline-block mb-4 mt-4">
                <div
                  class="rounded-full bg-gradient-to-br from-[#FFC627] via-[#F59E0B] to-[#D97706] flex items-center justify-center font-bold text-[#8B4513] mx-auto ring-4 ring-[#FFC627]/30 shadow-2xl text-[28px] sm:text-[32px]"
                  style="width: 96px; height: 96px"
                >
                  {{ top3[0].initials }}
                </div>
                <div
                  class="absolute -bottom-2 left-1/2 -translate-x-1/2 w-8 h-8 rounded-full bg-gradient-to-br from-[#FFC627] to-[#F59E0B] flex items-center justify-center text-[14px] font-bold text-[#8B4513] shadow-lg ring-2 ring-white/40"
                >
                  1
                </div>
              </div>
              <h3
                class="text-white text-[17px] sm:text-[19px] font-bold mt-2 truncate"
              >
                {{ top3[0].name }}
              </h3>
              <p class="text-white/40 text-[12px] sm:text-[13px] mb-4 truncate">
                {{ top3[0].ward }}, {{ top3[0].district }}
              </p>
              <!-- Points highlight -->
              <div
                class="inline-flex items-center gap-2 px-5 py-2.5 rounded-full bg-[#FFC627]/20 border border-[#FFC627]/25 mb-3"
              >
                <Star :size="18" class="text-[#FFC627]" fill="#FFC627" />
                <span
                  class="text-[#FFC627] text-[28px] sm:text-[32px] font-extrabold leading-none"
                  >{{ top3[0].periodPoints }}</span
                >
                <span class="text-[#FFC627]/50 text-[12px]">điểm</span>
              </div>
              <div
                class="flex items-center justify-center gap-3 text-white/40 text-[12px]"
              >
                <span class="flex items-center gap-1"
                  ><CheckCircle2 :size="12" class="text-emerald-400" />
                  {{ top3[0].approved }} phản ánh</span
                >
                <span class="flex items-center gap-1"
                  ><Target :size="12" class="text-blue-400" />
                  {{ top3[0].rate }}%</span
                >
              </div>
            </div>
          </div>

          <!-- 3rd place -->
          <div class="flex-1 max-w-[210px] self-end">
            <div
              class="bg-gradient-to-b from-white/12 to-white/4 backdrop-blur-sm rounded-2xl border border-white/10 p-5 sm:p-6 text-center transition-all hover:from-white/18 hover:to-white/8 hover:shadow-lg hover:shadow-black/10 mt-6"
            >
              <!-- Avatar -->
              <div class="relative inline-block mb-4">
                <div
                  class="rounded-full bg-gradient-to-br from-amber-500 via-amber-600 to-amber-700 flex items-center justify-center font-bold text-amber-100 mx-auto ring-4 ring-amber-500/20 shadow-xl text-[22px] sm:text-[26px]"
                  style="width: 80px; height: 80px"
                >
                  {{ top3[2].initials }}
                </div>
                <div
                  class="absolute -bottom-2 left-1/2 -translate-x-1/2 w-7 h-7 rounded-full bg-gradient-to-br from-amber-600 to-amber-700 flex items-center justify-center text-[13px] font-bold text-white shadow-md ring-2 ring-white/20"
                >
                  3
                </div>
              </div>
              <h3 class="text-white text-[15px] font-semibold truncate">
                {{ top3[2].name }}
              </h3>
              <p class="text-white/40 text-[12px] mb-4 truncate">
                {{ top3[2].ward }}, {{ top3[2].district }}
              </p>
              <!-- Points -->
              <div class="flex items-center justify-center gap-1.5 mb-3">
                <Star :size="16" class="text-[#FFC627]" fill="#FFC627" />
                <span class="text-white text-[24px] font-extrabold">{{
                  top3[2].periodPoints
                }}</span>
              </div>
              <div
                class="flex items-center justify-center gap-2 text-white/35 text-[11px]"
              >
                <span>{{ top3[2].approved }} PA</span>
                <span class="w-1 h-1 rounded-full bg-white/20" />
                <span>{{ top3[2].rate }}%</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Full leaderboard table -->
    <div
      class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden"
    >
      <div
        class="flex items-center justify-between px-5 py-4 border-b border-slate-100"
      >
        <h3 class="text-slate-800 text-[16px] font-semibold">
          Bảng xếp hạng đầy đủ — Top {{ leaderboard.length }}
        </h3>
        <div class="relative">
          <Search
            :size="16"
            class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
          />
          <input
            v-model="searchQuery"
            placeholder="Tìm người dân..."
            class="pl-9 pr-4 py-2 rounded-lg border border-slate-200 text-[13px] outline-none focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10"
          />
        </div>
      </div>

      <!-- Desktop table -->
      <div class="hidden md:block overflow-x-auto">
        <table class="w-full text-[13px]">
          <thead>
            <tr class="bg-slate-50/70">
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 w-16 font-medium text-slate-400"
              >
                Hạng
              </th>
              <th
                class="text-left px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Họ tên
              </th>
              <th
                class="text-left px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Địa chỉ
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Điểm {{ activePeriod === "month" ? "tháng" : "quý" }}
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Tổng điểm
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                PA được duyệt
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Tỷ lệ duyệt
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Đề xuất
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="c in filteredLeaderboard"
              :key="c.rank"
              class="border-b border-slate-50 hover:bg-slate-50/50 transition-colors"
            >
              <td class="px-4 py-3.5 text-center">
                <div
                  v-if="c.rank <= 3"
                  :class="[
                    'w-8 h-8 rounded-full flex items-center justify-center mx-auto text-[13px] font-bold',
                    c.rank === 1
                      ? 'bg-[#FFC627] text-[#8B4513]'
                      : c.rank === 2
                        ? 'bg-slate-200 text-slate-700'
                        : 'bg-amber-100 text-amber-700',
                  ]"
                >
                  {{ c.rank }}
                </div>
                <span v-else class="text-slate-400 font-medium">{{
                  c.rank
                }}</span>
              </td>
              <td class="px-4 py-3.5">
                <div class="flex items-center gap-3">
                  <div
                    :class="[
                      'w-9 h-9 rounded-full flex items-center justify-center shrink-0 text-[12px] font-semibold',
                      c.avatarColor,
                    ]"
                  >
                    {{ c.initials }}
                  </div>
                  <div>
                    <p class="text-slate-800 text-[14px] font-medium">
                      {{ c.name }}
                    </p>
                    <p class="text-slate-400 text-[12px]">{{ c.phone }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-3.5 text-slate-500">
                {{ c.ward }}, {{ c.district }}
              </td>
              <td class="px-4 py-3.5 text-center">
                <span
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-[#FFC627]/15 text-[#8B4513] text-[13px] font-bold"
                >
                  <Star :size="12" class="text-[#FFC627]" fill="#FFC627" />
                  {{ c.periodPoints }}
                </span>
              </td>
              <td class="px-4 py-3.5 text-center text-slate-700 font-medium">
                {{ c.totalPoints }}
              </td>
              <td class="px-4 py-3.5 text-center text-slate-600">
                {{ c.approved }}/{{ c.submitted }}
              </td>
              <td class="px-4 py-3.5 text-center">
                <div class="flex items-center justify-center gap-2">
                  <div
                    class="w-16 h-2 bg-slate-100 rounded-full overflow-hidden"
                  >
                    <div
                      :class="[
                        c.rate >= 80
                          ? 'bg-emerald-500'
                          : c.rate >= 50
                            ? 'bg-amber-500'
                            : 'bg-red-500',
                      ]"
                      class="h-full rounded-full"
                      :style="{ width: c.rate + '%' }"
                    />
                  </div>
                  <span class="text-slate-600 text-[12px] font-medium"
                    >{{ c.rate }}%</span
                  >
                </div>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span
                  v-if="c.rank <= 3"
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-emerald-50 text-emerald-700 text-[11px] font-semibold"
                >
                  <Award :size="12" /> Khen thưởng
                </span>
                <span
                  v-else-if="c.rank <= 5"
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-blue-50 text-blue-600 text-[11px] font-medium"
                >
                  Biểu dương
                </span>
                <span v-else class="text-slate-300 text-[12px]">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Mobile cards -->
      <div class="md:hidden divide-y divide-slate-50">
        <div v-for="c in filteredLeaderboard" :key="c.rank" class="px-4 py-4">
          <div class="flex items-center gap-3 mb-2">
            <div
              v-if="c.rank <= 3"
              :class="[
                'w-8 h-8 rounded-full flex items-center justify-center text-[13px] font-bold shrink-0',
                c.rank === 1
                  ? 'bg-[#FFC627] text-[#8B4513]'
                  : c.rank === 2
                    ? 'bg-slate-200 text-slate-700'
                    : 'bg-amber-100 text-amber-700',
              ]"
            >
              {{ c.rank }}
            </div>
            <span
              v-else
              class="w-8 h-8 flex items-center justify-center text-slate-400 font-medium text-[14px] shrink-0"
              >{{ c.rank }}</span
            >
            <div class="flex-1 min-w-0">
              <p class="text-slate-800 text-[14px] font-semibold truncate">
                {{ c.name }}
              </p>
              <p class="text-slate-400 text-[12px]">
                {{ c.ward }}, {{ c.district }}
              </p>
            </div>
            <div class="text-right shrink-0">
              <div class="flex items-center gap-1">
                <Star :size="14" class="text-[#FFC627]" fill="#FFC627" />
                <span class="text-slate-800 text-[18px] font-bold">{{
                  c.periodPoints
                }}</span>
              </div>
              <p class="text-slate-400 text-[11px]">
                {{ c.approved }} PA duyệt
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Point system info -->
    <div class="bg-white rounded-xl shadow-sm border border-slate-100 p-5">
      <h3
        class="text-slate-800 text-[15px] font-semibold mb-4 flex items-center gap-2"
      >
        <Info :size="16" class="text-blue-500" /> Quy chế tính điểm
      </h3>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
        <div
          v-for="rule in pointRules"
          :key="rule.action"
          class="flex items-center gap-3 p-3 rounded-lg bg-slate-50"
        >
          <div
            :class="[
              'w-10 h-10 rounded-lg flex items-center justify-center shrink-0',
              rule.points > 0
                ? 'bg-emerald-100 text-emerald-600'
                : 'bg-red-100 text-red-500',
            ]"
          >
            <span class="text-[14px] font-bold"
              >{{ rule.points > 0 ? "+" : "" }}{{ rule.points }}</span
            >
          </div>
          <div>
            <p class="text-slate-700 text-[13px] font-medium">
              {{ rule.action }}
            </p>
            <p class="text-slate-400 text-[12px]">{{ rule.desc }}</p>
          </div>
        </div>
      </div>
      <div class="mt-4 pt-4 border-t border-slate-100">
        <p class="text-slate-500 text-[13px] leading-[1.6]">
          Điểm tháng được reset vào ngày 01 hàng tháng. Điểm quý được reset vào
          đầu mỗi quý. Top 3 công dân có điểm cao nhất mỗi quý sẽ được UBND tỉnh
          xem xét khen thưởng.
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import {
  Trophy,
  Star,
  Award,
  Search,
  Download,
  Users,
  TrendingUp,
  Target,
  Info,
  Crown,
  CheckCircle2,
} from "lucide-vue-next";

const activePeriod = ref("month");
const searchQuery = ref("");
const periodOptions = [
  { key: "month", label: "Theo tháng" },
  { key: "quarter", label: "Theo quý" },
];

const AVATAR_COLORS = [
  "bg-blue-100 text-blue-700",
  "bg-emerald-100 text-emerald-700",
  "bg-violet-100 text-violet-700",
  "bg-amber-100 text-amber-700",
  "bg-rose-100 text-rose-700",
  "bg-cyan-100 text-cyan-700",
  "bg-pink-100 text-pink-700",
  "bg-indigo-100 text-indigo-700",
  "bg-lime-100 text-lime-700",
  "bg-orange-100 text-orange-700",
];

const leaderboard = [
  {
    rank: 1,
    name: "Võ Thị Phương",
    initials: "TP",
    phone: "0905555555",
    ward: "Phường 2",
    district: "TP. Cao Lãnh",
    periodPoints: 85,
    totalPoints: 240,
    submitted: 18,
    approved: 16,
    rate: 89,
    avatarColor: AVATAR_COLORS[0],
  },
  {
    rank: 2,
    name: "Nguyễn Văn An",
    initials: "VA",
    phone: "0904444444",
    ward: "Phường 1",
    district: "TP. Cao Lãnh",
    periodPoints: 72,
    totalPoints: 195,
    submitted: 15,
    approved: 12,
    rate: 80,
    avatarColor: AVATAR_COLORS[1],
  },
  {
    rank: 3,
    name: "Trần Minh Hoàng",
    initials: "MH",
    phone: "0906666666",
    ward: "Phường 3",
    district: "TP. Cao Lãnh",
    periodPoints: 65,
    totalPoints: 180,
    submitted: 14,
    approved: 13,
    rate: 93,
    avatarColor: AVATAR_COLORS[2],
  },
  {
    rank: 4,
    name: "Lê Thị Kim Ngân",
    initials: "KN",
    phone: "0907777777",
    ward: "Phường 6",
    district: "TP. Cao Lãnh",
    periodPoints: 55,
    totalPoints: 155,
    submitted: 12,
    approved: 10,
    rate: 83,
    avatarColor: AVATAR_COLORS[3],
  },
  {
    rank: 5,
    name: "Phạm Quốc Đạt",
    initials: "QĐ",
    phone: "0908888888",
    ward: "Xã Mỹ Tân",
    district: "H. Cao Lãnh",
    periodPoints: 50,
    totalPoints: 140,
    submitted: 11,
    approved: 9,
    rate: 82,
    avatarColor: AVATAR_COLORS[4],
  },
  {
    rank: 6,
    name: "Huỳnh Thanh Tâm",
    initials: "TT",
    phone: "0909111222",
    ward: "Phường 4",
    district: "TP. Sa Đéc",
    periodPoints: 45,
    totalPoints: 130,
    submitted: 10,
    approved: 8,
    rate: 80,
    avatarColor: AVATAR_COLORS[5],
  },
  {
    rank: 7,
    name: "Đặng Văn Phúc",
    initials: "VP",
    phone: "0909333444",
    ward: "Phường 1",
    district: "TP. Sa Đéc",
    periodPoints: 40,
    totalPoints: 115,
    submitted: 9,
    approved: 7,
    rate: 78,
    avatarColor: AVATAR_COLORS[6],
  },
  {
    rank: 8,
    name: "Bùi Thị Hồng",
    initials: "TH",
    phone: "0909555666",
    ward: "Xã An Bình",
    district: "H. Cao Lãnh",
    periodPoints: 35,
    totalPoints: 100,
    submitted: 8,
    approved: 6,
    rate: 75,
    avatarColor: AVATAR_COLORS[7],
  },
  {
    rank: 9,
    name: "Cao Minh Trí",
    initials: "MT",
    phone: "0909777888",
    ward: "Phường 2",
    district: "TP. Cao Lãnh",
    periodPoints: 30,
    totalPoints: 90,
    submitted: 7,
    approved: 5,
    rate: 71,
    avatarColor: AVATAR_COLORS[8],
  },
  {
    rank: 10,
    name: "Ngô Thị Mai Anh",
    initials: "MA",
    phone: "0909999000",
    ward: "Phường 3",
    district: "TX. Hồng Ngự",
    periodPoints: 25,
    totalPoints: 75,
    submitted: 6,
    approved: 5,
    rate: 83,
    avatarColor: AVATAR_COLORS[9],
  },
];

const top3 = computed(() => leaderboard.slice(0, 3));

const filteredLeaderboard = computed(() => {
  if (!searchQuery.value) return leaderboard;
  const q = searchQuery.value.toLowerCase();
  return leaderboard.filter(
    (c) =>
      c.name.toLowerCase().includes(q) ||
      c.ward.toLowerCase().includes(q) ||
      c.district.toLowerCase().includes(q),
  );
});

const statsCards = [
  {
    label: "Tổng công dân tham gia",
    value: "847",
    icon: Users,
    color: "bg-blue-50 text-blue-600",
  },
  {
    label: "Hoạt động trong tháng",
    value: "156",
    icon: TrendingUp,
    color: "bg-emerald-50 text-emerald-600",
  },
  {
    label: "Tỷ lệ duyệt TB",
    value: "82%",
    icon: Target,
    color: "bg-amber-50 text-amber-600",
  },
  {
    label: "Đề xuất khen thưởng",
    value: "3",
    icon: Award,
    color: "bg-violet-50 text-violet-600",
  },
];

const pointRules = [
  {
    action: "Phản ánh được duyệt",
    points: 10,
    desc: "Báo cáo hợp lệ được kiểm duyệt",
  },
  {
    action: "Phản ánh đã xử lý xong",
    points: 5,
    desc: "Báo cáo được đóng thành công",
  },
  {
    action: "Đánh giá kết quả",
    points: 2,
    desc: "Gửi đánh giá chất lượng xử lý",
  },
  {
    action: "PA đầu tiên trong tháng",
    points: 5,
    desc: "Thưởng khuyến khích hoạt động",
  },
  {
    action: "Hoạt động liên tiếp",
    points: 3,
    desc: "Gửi phản ánh nhiều ngày liên tiếp",
  },
  {
    action: "Phản ánh bị từ chối",
    points: -5,
    desc: "Báo cáo spam hoặc không hợp lệ",
  },
];
</script>
