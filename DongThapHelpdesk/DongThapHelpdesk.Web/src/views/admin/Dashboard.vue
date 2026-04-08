<template>
  <div class="space-y-5">
    <!-- Page header -->
    <div
      class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3"
    >
      <div>
        <h1
          class="text-slate-800 font-semibold"
          style="font-size: clamp(22px, 3vw, 26px)"
        >
          {{ pageTitle }}
        </h1>
        <p class="text-slate-500 mt-0.5 text-[14px]">{{ pageSubtitle }}</p>
      </div>
      <div class="flex items-center gap-2">
        <div
          class="flex items-center bg-white border border-slate-200 rounded-lg overflow-hidden"
        >
          <button
            v-for="p in periods"
            :key="p.key"
            @click="period = p.key"
            :class="[
              'px-3.5 py-2 transition-colors cursor-pointer text-[13px] font-medium',
              period === p.key
                ? 'bg-[#DA251D] text-white'
                : 'text-slate-600 hover:bg-slate-50',
            ]"
          >
            {{ p.label }}
          </button>
        </div>
        <button
          class="p-2.5 rounded-lg border border-slate-200 bg-white text-slate-500 hover:bg-slate-50 hover:text-slate-700 transition-colors cursor-pointer"
        >
          <RefreshCw :size="16" />
        </button>
      </div>
    </div>

    <!-- ═══ ADMIN / MANAGER DASHBOARD ═══ -->
    <template v-if="role === 'Admin' || role === 'Manager'">
      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-5 gap-4">
        <KpiCard v-for="kpi in adminKpis" :key="kpi.title" v-bind="kpi" />
        <SlaRing :percentage="92" />
      </div>

      <!-- Charts -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div
          class="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-100 p-5"
        >
          <div class="flex items-center justify-between mb-5">
            <div>
              <h3 class="text-slate-800 text-[16px] font-semibold">
                Phản ánh theo tháng
              </h3>
              <p class="text-slate-400 mt-0.5 text-[13px]">6 tháng gần nhất</p>
            </div>
            <div class="flex items-center gap-4">
              <div class="flex items-center gap-1.5">
                <div class="w-3 h-3 rounded-sm bg-[#DA251D]" />
                <span class="text-slate-500 text-[12px]">Tiếp nhận</span>
              </div>
              <div class="flex items-center gap-1.5">
                <div class="w-3 h-3 rounded-sm bg-emerald-500" />
                <span class="text-slate-500 text-[12px]">Đã xử lý</span>
              </div>
            </div>
          </div>
          <apexchart
            type="bar"
            height="280"
            :options="barOptions"
            :series="barSeries"
          />
        </div>
        <div class="bg-white rounded-xl shadow-sm border border-slate-100 p-5">
          <div class="mb-4">
            <h3 class="text-slate-800 text-[16px] font-semibold">
              Phân bố theo lĩnh vực
            </h3>
            <p class="text-slate-400 mt-0.5 text-[13px]">Tổng: 945 phản ánh</p>
          </div>
          <apexchart
            type="donut"
            height="220"
            :options="donutOptions"
            :series="donutSeries"
          />
          <div class="grid grid-cols-2 gap-x-4 gap-y-2 mt-2">
            <div
              v-for="c in categoryData"
              :key="c.name"
              class="flex items-center gap-2"
            >
              <div
                class="w-2.5 h-2.5 rounded-full shrink-0"
                :style="{ backgroundColor: c.color }"
              />
              <span class="text-slate-600 truncate text-[12px]">{{
                c.name
              }}</span>
              <span class="text-slate-400 ml-auto text-[12px]">{{
                c.value
              }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Department Performance (Manager-specific) -->
      <div
        v-if="role === 'Manager'"
        class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden"
      >
        <div class="px-5 py-4 border-b border-slate-100">
          <h3 class="text-slate-800 text-[16px] font-semibold">
            Hiệu suất phòng ban của tôi
          </h3>
          <p class="text-slate-400 mt-0.5 text-[13px]">
            Phòng Giao thông Vận tải — Tháng này
          </p>
        </div>
        <div class="p-5 grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div class="text-center p-4 bg-blue-50 rounded-xl">
            <p class="text-blue-600 text-[28px] font-bold">45</p>
            <p class="text-slate-500 text-[13px]">Đang xử lý</p>
          </div>
          <div class="text-center p-4 bg-emerald-50 rounded-xl">
            <p class="text-emerald-600 text-[28px] font-bold">189</p>
            <p class="text-slate-500 text-[13px]">Đã hoàn thành</p>
          </div>
          <div class="text-center p-4 bg-amber-50 rounded-xl">
            <p class="text-amber-600 text-[28px] font-bold">80.8%</p>
            <p class="text-slate-500 text-[13px]">Tỷ lệ đúng hạn</p>
          </div>
        </div>
      </div>

      <!-- Map -->
      <MapSection />
    </template>

    <!-- ═══ DISPATCHER DASHBOARD ═══ -->
    <template v-else-if="role === 'Dispatcher'">
      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        <KpiCard v-for="kpi in dispatcherKpis" :key="kpi.title" v-bind="kpi" />
      </div>

      <!-- Pending Tickets -->
      <div
        class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden"
      >
        <div
          class="flex items-center justify-between px-5 py-4 border-b border-slate-100"
        >
          <div class="flex items-center gap-2">
            <div
              class="w-8 h-8 rounded-lg bg-blue-50 flex items-center justify-center"
            >
              <Inbox :size="16" class="text-blue-600" />
            </div>
            <div>
              <h3 class="text-slate-800 text-[16px] font-semibold">
                Phản ánh chờ duyệt
              </h3>
              <p class="text-slate-400 text-[13px]">
                Cần kiểm duyệt và phân công xử lý
              </p>
            </div>
          </div>
          <RouterLink
            to="/admin/tiep-nhan"
            class="flex items-center gap-1.5 text-[#DA251D] text-[14px] font-medium hover:underline"
          >
            Xem tất cả <ArrowRight :size="15" />
          </RouterLink>
        </div>
        <div class="divide-y divide-slate-50">
          <div
            v-for="t in pendingTickets"
            :key="t.code"
            class="px-5 py-4 hover:bg-slate-50/60 transition-colors cursor-pointer flex items-start gap-4"
          >
            <div
              class="w-10 h-10 rounded-lg bg-blue-50 flex items-center justify-center shrink-0"
            >
              <Inbox :size="18" class="text-blue-600" />
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 mb-1">
                <span
                  class="font-mono text-[#DA251D] text-[13px] font-medium"
                  >{{ t.code }}</span
                >
                <span
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-red-50 text-red-600 text-[11px] font-medium"
                  v-if="t.priority === 'High'"
                  >Ưu tiên cao</span
                >
              </div>
              <p class="text-slate-800 text-[14px] font-medium truncate">
                {{ t.title }}
              </p>
              <div
                class="flex items-center gap-3 mt-1 text-slate-400 text-[12px]"
              >
                <span class="flex items-center gap-1"
                  ><Tag :size="11" /> {{ t.category }}</span
                >
                <span class="flex items-center gap-1"
                  ><Clock :size="11" /> {{ t.time }}</span
                >
                <span class="flex items-center gap-1"
                  ><User :size="11" /> {{ t.reporter }}</span
                >
              </div>
            </div>
            <div class="flex items-center gap-2 shrink-0">
              <button
                class="px-3 py-1.5 rounded-lg bg-[#DA251D] text-white text-[12px] font-medium hover:bg-[#b81f18] transition-colors cursor-pointer"
              >
                Duyệt
              </button>
              <button
                class="px-3 py-1.5 rounded-lg border border-slate-200 text-slate-500 text-[12px] font-medium hover:bg-slate-50 transition-colors cursor-pointer"
              >
                Từ chối
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Today stats -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <div class="bg-white rounded-xl shadow-sm border border-slate-100 p-5">
          <h3 class="text-slate-800 text-[16px] font-semibold mb-4">
            Hoạt động hôm nay
          </h3>
          <div class="space-y-3">
            <div
              v-for="a in todayActivities"
              :key="a.text"
              class="flex items-center gap-3"
            >
              <div
                :class="[
                  'w-8 h-8 rounded-full flex items-center justify-center shrink-0',
                  a.bg,
                ]"
              >
                <component :is="a.icon" :size="14" :class="a.iconColor" />
              </div>
              <div class="flex-1">
                <p class="text-slate-700 text-[13px]">{{ a.text }}</p>
                <p class="text-slate-400 text-[12px]">{{ a.time }}</p>
              </div>
            </div>
          </div>
        </div>
        <div class="bg-white rounded-xl shadow-sm border border-slate-100 p-5">
          <h3 class="text-slate-800 text-[16px] font-semibold mb-4">
            Phân bố theo phòng ban
          </h3>
          <apexchart
            type="donut"
            height="220"
            :options="donutOptions"
            :series="donutSeries"
          />
        </div>
      </div>
    </template>

    <!-- ═══ ASSIGNEE DASHBOARD ═══ -->
    <template v-else-if="role === 'Assignee'">
      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        <KpiCard v-for="kpi in assigneeKpis" :key="kpi.title" v-bind="kpi" />
      </div>

      <!-- My assigned tickets -->
      <div
        class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden"
      >
        <div
          class="flex items-center justify-between px-5 py-4 border-b border-slate-100"
        >
          <div class="flex items-center gap-2">
            <div
              class="w-8 h-8 rounded-lg bg-amber-50 flex items-center justify-center"
            >
              <ClipboardList :size="16" class="text-amber-600" />
            </div>
            <div>
              <h3 class="text-slate-800 text-[16px] font-semibold">
                Công việc của tôi
              </h3>
              <p class="text-slate-400 text-[13px]">
                Phản ánh được giao cho bạn xử lý
              </p>
            </div>
          </div>
          <RouterLink
            to="/admin/kanban"
            class="flex items-center gap-1.5 text-[#DA251D] text-[14px] font-medium hover:underline"
          >
            Bảng Kanban <ArrowRight :size="15" />
          </RouterLink>
        </div>
        <div class="divide-y divide-slate-50">
          <div
            v-for="t in myTickets"
            :key="t.code"
            class="px-5 py-4 hover:bg-slate-50/60 transition-colors cursor-pointer"
          >
            <div class="flex items-start justify-between gap-3 mb-2">
              <div>
                <div class="flex items-center gap-2 mb-1">
                  <span
                    class="font-mono text-[#DA251D] text-[13px] font-medium"
                    >{{ t.code }}</span
                  >
                  <span
                    :class="[
                      'inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-medium',
                      statusCfg[t.status].bg,
                      statusCfg[t.status].text,
                    ]"
                  >
                    <span
                      :class="[
                        'w-1.5 h-1.5 rounded-full',
                        statusCfg[t.status].dot,
                      ]"
                    />
                    {{ statusCfg[t.status].label }}
                  </span>
                </div>
                <p class="text-slate-800 text-[14px] font-medium">
                  {{ t.title }}
                </p>
              </div>
              <div class="text-right shrink-0">
                <span
                  v-if="t.slaBreached"
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded bg-red-100 text-red-600 text-[11px] font-semibold"
                >
                  <AlertTriangle :size="11" /> QUÁ HẠN
                </span>
                <span
                  v-else
                  class="text-slate-500 text-[12px] flex items-center gap-1"
                  ><Timer :size="12" /> {{ t.slaRemaining }}</span
                >
              </div>
            </div>
            <div class="flex items-center gap-3 text-slate-400 text-[12px]">
              <span class="flex items-center gap-1"
                ><Tag :size="11" /> {{ t.category }}</span
              >
              <span class="flex items-center gap-1"
                ><MapPin :size="11" /> {{ t.location }}</span
              >
            </div>
            <!-- Action buttons -->
            <div class="flex items-center gap-2 mt-3">
              <button
                v-if="t.status === 'Assigned'"
                class="px-3 py-1.5 rounded-lg bg-[#DA251D] text-white text-[12px] font-medium hover:bg-[#b81f18] transition-colors cursor-pointer"
              >
                <Play :size="12" class="inline mr-1" /> Bắt đầu xử lý
              </button>
              <button
                v-if="t.status === 'InProgress'"
                class="px-3 py-1.5 rounded-lg bg-emerald-600 text-white text-[12px] font-medium hover:bg-emerald-700 transition-colors cursor-pointer"
              >
                <Send :size="12" class="inline mr-1" /> Báo cáo kết quả
              </button>
              <button
                class="px-3 py-1.5 rounded-lg border border-slate-200 text-slate-500 text-[12px] font-medium hover:bg-slate-50 transition-colors cursor-pointer"
              >
                Xem chi tiết
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- SLA Warning -->
      <div
        v-if="slaBreachedCount > 0"
        class="bg-red-50 border border-red-200 rounded-xl p-5 flex items-start gap-4"
      >
        <div
          class="w-10 h-10 rounded-full bg-red-100 flex items-center justify-center shrink-0"
        >
          <AlertTriangle :size="20" class="text-red-600" />
        </div>
        <div>
          <h3 class="text-red-800 text-[15px] font-semibold">
            {{ slaBreachedCount }} phản ánh đã quá hạn SLA
          </h3>
          <p class="text-red-600 text-[13px] mt-1 leading-[1.6]">
            Các phản ánh này cần được xử lý ngay lập tức. Vui lòng ưu tiên hoàn
            thành hoặc liên hệ Dispatcher để chuyển phân công.
          </p>
        </div>
      </div>
    </template>

    <!-- ═══ RECENT TABLE (hiện cho tất cả role) ═══ -->
    <div
      class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden"
    >
      <div
        class="flex items-center justify-between px-5 py-4 border-b border-slate-100"
      >
        <div>
          <h3 class="text-slate-800 text-[16px] font-semibold">
            Phản ánh gần đây
          </h3>
          <p class="text-slate-400 mt-0.5 text-[13px]">Cập nhật mới nhất</p>
        </div>
        <RouterLink
          to="/admin/danh-sach"
          class="flex items-center gap-1.5 text-[#DA251D] hover:underline text-[14px] font-medium"
        >
          Xem tất cả <ArrowRight :size="15" />
        </RouterLink>
      </div>
      <div class="hidden lg:block overflow-x-auto">
        <table class="w-full">
          <thead>
            <tr class="bg-slate-50/80">
              <th
                v-for="h in tableHeaders"
                :key="h"
                class="text-left px-4 py-3 text-slate-400 border-b border-slate-100 uppercase text-[12px] font-semibold tracking-[0.03em]"
              >
                {{ h }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="t in recentTickets"
              :key="t.code"
              class="hover:bg-slate-50/60 transition-colors cursor-pointer group border-b border-slate-50 last:border-0"
            >
              <td class="px-4 py-3.5">
                <span
                  class="text-[#DA251D] font-mono group-hover:underline text-[13px] font-medium"
                  >{{ t.code }}</span
                >
              </td>
              <td class="px-4 py-3.5 max-w-[260px]">
                <span class="text-slate-700 truncate block text-[13px]">{{
                  t.title
                }}</span>
              </td>
              <td class="px-4 py-3.5">
                <span class="text-slate-500 text-[13px]">{{ t.category }}</span>
              </td>
              <td class="px-4 py-3.5">
                <span
                  :class="[
                    'inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-[12px] font-medium',
                    statusCfg[t.status].bg,
                    statusCfg[t.status].text,
                  ]"
                >
                  <span
                    :class="[
                      'w-1.5 h-1.5 rounded-full',
                      statusCfg[t.status].dot,
                    ]"
                  />
                  {{ statusCfg[t.status].label }}
                </span>
              </td>
              <td class="px-4 py-3.5">
                <span class="text-slate-400 text-[13px]">{{
                  t.createdAt
                }}</span>
              </td>
              <td class="px-4 py-3.5">
                <span
                  v-if="t.slaBreached"
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded bg-red-100 text-red-700 text-[11px] font-semibold"
                  ><AlertTriangle :size="11" /> QUÁ HẠN</span
                >
                <span
                  v-else-if="t.slaRemaining === '—'"
                  class="text-slate-300 text-[13px]"
                  >—</span
                >
                <span
                  v-else
                  class="text-slate-500 flex items-center gap-1 text-[13px]"
                  ><Timer :size="13" class="text-slate-400" />
                  {{ t.slaRemaining }}</span
                >
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { RouterLink } from "vue-router";
import { useAuthStore } from "@/stores/auth";
import {
  Inbox,
  Clock,
  CheckCircle2,
  AlertTriangle,
  TrendingUp,
  TrendingDown,
  ArrowRight,
  MapPin,
  Filter,
  Download,
  RefreshCw,
  Timer,
  Tag,
  User,
  ClipboardList,
  Play,
  Send,
  UserPlus,
  Eye,
} from "lucide-vue-next";

const auth = useAuthStore();
// Lấy role từ auth store, fallback 'Admin' khi chưa đăng nhập (dev mode)
const role = computed(() => auth.userRole || "Manager");

// ── Page title theo role ──
const pageTitle = computed(() => {
  const titles = {
    Admin: "Bảng điều khiển",
    Manager: "Bảng điều khiển — Quản lý",
    Dispatcher: "Trung tâm Điều phối",
    Assignee: "Công việc của tôi",
  };
  return titles[role.value] || "Bảng điều khiển";
});
const pageSubtitle = computed(() => {
  const subs = {
    Admin: "Tổng quan tình hình phản ánh toàn hệ thống",
    Manager: "Hiệu suất phòng ban và thống kê phản ánh",
    Dispatcher: "Tiếp nhận, kiểm duyệt và phân công phản ánh",
    Assignee: "Danh sách phản ánh được giao và tiến trình xử lý",
  };
  return subs[role.value] || "";
});

const period = ref("month");
const periods = [
  { key: "week", label: "Tuần" },
  { key: "month", label: "Tháng" },
  { key: "quarter", label: "Quý" },
];

// ── KPI Card sub-component ──
const KpiCard = {
  props: [
    "title",
    "value",
    "trend",
    "trendLabel",
    "icon",
    "iconBg",
    "iconColor",
    "accent",
  ],
  template: `
    <div class="bg-white rounded-xl shadow-sm border border-slate-100 p-5 hover:shadow-md hover:border-slate-200 transition-all">
      <div class="flex items-start justify-between mb-4">
        <span class="text-slate-500 text-[13px] font-medium">{{ title }}</span>
        <div :class="['w-10 h-10 rounded-xl flex items-center justify-center', iconBg, iconColor]"><component :is="icon" :size="20" /></div>
      </div>
      <p :class="['mb-1', accent ? 'text-red-600' : 'text-slate-900']" style="font-size: 32px; font-weight: 700; line-height: 1">{{ value }}</p>
      <div v-if="trendLabel" class="flex items-center gap-1 mt-2">
        <component :is="trend === 'up' ? TrendingUp : TrendingDown" :size="14" :class="trend === 'up' ? 'text-emerald-500' : 'text-red-500'" />
        <span :class="[trend === 'up' ? 'text-emerald-600' : 'text-red-600']" class="text-[12px] font-medium">{{ trendLabel }}</span>
        <span class="text-slate-400 text-[12px]">so với tháng trước</span>
      </div>
    </div>
  `,
  components: { TrendingUp, TrendingDown },
};

// ── SLA Ring ──
const SlaRing = {
  props: ["percentage"],
  setup(props) {
    const circ = 2 * Math.PI * 36;
    const offset = circ - (props.percentage / 100) * circ;
    const color =
      props.percentage >= 90
        ? "#10b981"
        : props.percentage >= 70
          ? "#f59e0b"
          : "#ef4444";
    return { circ, offset, color };
  },
  components: { TrendingUp },
  template: `
    <div class="sm:col-span-2 xl:col-span-1 bg-white rounded-xl shadow-sm border border-slate-100 p-5 hover:shadow-md hover:border-slate-200 transition-all flex items-center gap-5">
      <div class="relative w-[88px] h-[88px] shrink-0">
        <svg width="88" height="88" class="transform -rotate-90">
          <circle cx="44" cy="44" r="36" fill="none" stroke="#f1f5f9" stroke-width="8" />
          <circle cx="44" cy="44" r="36" fill="none" :stroke="color" stroke-width="8" stroke-linecap="round" :stroke-dasharray="circ" :stroke-dashoffset="offset" class="transition-all duration-700" />
        </svg>
        <div class="absolute inset-0 flex items-center justify-center"><span class="text-slate-800 text-[20px] font-bold">{{ percentage }}%</span></div>
      </div>
      <div>
        <p class="text-slate-500 mb-0.5 text-[13px] font-medium">Tỷ lệ đúng hạn</p>
        <p class="text-slate-800 text-[14px] font-semibold">SLA Compliance</p>
        <div class="flex items-center gap-1 mt-1.5"><TrendingUp :size="14" class="text-emerald-500" /><span class="text-emerald-600 text-[12px] font-medium">+2.3%</span></div>
      </div>
    </div>
  `,
};

// ── Map Section ──
const MapSection = {
  components: { MapPin, Filter },
  template: `
    <div class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden">
      <div class="flex items-center justify-between px-5 py-4 border-b border-slate-100">
        <div class="flex items-center gap-2"><MapPin :size="18" class="text-[#DA251D]" /><h3 class="text-slate-800 text-[16px] font-semibold">Bản đồ phản ánh</h3></div>
      </div>
      <div class="relative" style="height: 300px">
        <img src="https://images.unsplash.com/photo-1722082840106-c6508ee966ea?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=1080" alt="Bản đồ" class="w-full h-full object-cover" />
        <div class="absolute inset-0 bg-slate-900/10" />
        <div class="absolute bottom-3 left-3 bg-white/90 backdrop-blur-sm px-3 py-1.5 rounded-md shadow-sm text-[12px]">
          <span class="text-slate-600 font-medium">TP. Cao Lãnh, Đồng Tháp</span>
        </div>
      </div>
    </div>
  `,
};

// ── Status config ──
const statusCfg = {
  New: {
    bg: "bg-blue-50",
    text: "text-blue-700",
    dot: "bg-blue-500",
    label: "Mới",
  },
  Assigned: {
    bg: "bg-indigo-50",
    text: "text-indigo-700",
    dot: "bg-indigo-500",
    label: "Đã giao",
  },
  InProgress: {
    bg: "bg-amber-50",
    text: "text-amber-700",
    dot: "bg-amber-500",
    label: "Đang xử lý",
  },
  Resolved: {
    bg: "bg-emerald-50",
    text: "text-emerald-700",
    dot: "bg-emerald-500",
    label: "Hoàn thành",
  },
  Closed: {
    bg: "bg-slate-100",
    text: "text-slate-500",
    dot: "bg-slate-400",
    label: "Đã đóng",
  },
  Rejected: {
    bg: "bg-red-50",
    text: "text-red-600",
    dot: "bg-red-500",
    label: "Từ chối",
  },
};

// ── KPI data per role ──
const adminKpis = [
  {
    title: "Phản ánh mới",
    value: "47",
    trend: "up",
    trendLabel: "+12.5%",
    icon: Inbox,
    iconBg: "bg-blue-50",
    iconColor: "text-blue-600",
  },
  {
    title: "Đang xử lý",
    value: "156",
    trend: "down",
    trendLabel: "-3.2%",
    icon: Clock,
    iconBg: "bg-amber-50",
    iconColor: "text-amber-600",
  },
  {
    title: "Đã hoàn thành",
    value: "189",
    trend: "up",
    trendLabel: "+8.1%",
    icon: CheckCircle2,
    iconBg: "bg-emerald-50",
    iconColor: "text-emerald-600",
  },
  {
    title: "Quá hạn SLA",
    value: "23",
    trend: "up",
    trendLabel: "+5 vụ",
    icon: AlertTriangle,
    iconBg: "bg-red-50",
    iconColor: "text-red-500",
    accent: true,
  },
];

const dispatcherKpis = [
  {
    title: "Chờ duyệt",
    value: "12",
    trend: "up",
    trendLabel: "+3 hôm nay",
    icon: Inbox,
    iconBg: "bg-blue-50",
    iconColor: "text-blue-600",
  },
  {
    title: "Đã duyệt hôm nay",
    value: "8",
    icon: CheckCircle2,
    iconBg: "bg-emerald-50",
    iconColor: "text-emerald-600",
  },
  {
    title: "Đã phân công",
    value: "6",
    icon: UserPlus,
    iconBg: "bg-indigo-50",
    iconColor: "text-indigo-600",
  },
  {
    title: "Từ chối hôm nay",
    value: "2",
    icon: AlertTriangle,
    iconBg: "bg-red-50",
    iconColor: "text-red-500",
    accent: true,
  },
];

const assigneeKpis = [
  {
    title: "Được giao cho tôi",
    value: "7",
    trend: "up",
    trendLabel: "+2 mới",
    icon: ClipboardList,
    iconBg: "bg-blue-50",
    iconColor: "text-blue-600",
  },
  {
    title: "Đang xử lý",
    value: "3",
    icon: Clock,
    iconBg: "bg-amber-50",
    iconColor: "text-amber-600",
  },
  {
    title: "Hoàn thành tháng này",
    value: "12",
    trend: "up",
    trendLabel: "+4",
    icon: CheckCircle2,
    iconBg: "bg-emerald-50",
    iconColor: "text-emerald-600",
  },
  {
    title: "Quá hạn SLA",
    value: "2",
    icon: AlertTriangle,
    iconBg: "bg-red-50",
    iconColor: "text-red-500",
    accent: true,
  },
];

// ── Dispatcher: pending tickets ──
const pendingTickets = [
  {
    code: "PA-042026-005",
    title: "Ổ gà lớn đường Nguyễn Huệ gây nguy hiểm cho người đi đường",
    category: "Giao thông",
    priority: "High",
    time: "15 phút trước",
    reporter: "Nguyễn Văn A",
  },
  {
    code: "PA-042026-004",
    title: "Rác thải sinh hoạt tràn ngập khu vực chợ Phường 2",
    category: "Môi trường",
    priority: "Normal",
    time: "1 giờ trước",
    reporter: "Trần Thị B",
  },
  {
    code: "PA-042026-003",
    title: "Đèn đường hư hỏng đoạn Trần Phú dài 200m",
    category: "Hạ tầng",
    priority: "Normal",
    time: "2 giờ trước",
    reporter: "Lê Minh C",
  },
];

const todayActivities = [
  {
    text: "Duyệt PA-042026-001 — Giao Phòng Giao thông",
    time: "09:15",
    icon: CheckCircle2,
    bg: "bg-emerald-50",
    iconColor: "text-emerald-600",
  },
  {
    text: "Từ chối PA-042026-002 — Trùng lặp",
    time: "09:30",
    icon: AlertTriangle,
    bg: "bg-red-50",
    iconColor: "text-red-500",
  },
  {
    text: "Duyệt PA-032026-015 — Giao Phòng TN-MT",
    time: "10:00",
    icon: CheckCircle2,
    bg: "bg-emerald-50",
    iconColor: "text-emerald-600",
  },
  {
    text: "Duyệt PA-032026-014 — Giao Công an TP",
    time: "10:45",
    icon: CheckCircle2,
    bg: "bg-emerald-50",
    iconColor: "text-emerald-600",
  },
];

// ── Assignee: my tickets ──
const myTickets = [
  {
    code: "PA-032026-010",
    title: "Mất nước sinh hoạt KDC Mỹ Trà 3 ngày",
    status: "InProgress",
    category: "Hạ tầng",
    location: "KDC Mỹ Trà",
    slaRemaining: "Quá hạn 2 ngày",
    slaBreached: true,
  },
  {
    code: "PA-032026-009",
    title: "Rác thải công nghiệp ven sông Tiền",
    status: "InProgress",
    category: "Môi trường",
    location: "Xã An Bình",
    slaRemaining: "Còn 2 ngày",
    slaBreached: false,
  },
  {
    code: "PA-042026-006",
    title: "Cây xanh ngã đổ chắn đường Tôn Đức Thắng",
    status: "Assigned",
    category: "Hạ tầng",
    location: "Phường 2",
    slaRemaining: "Còn 5 ngày",
    slaBreached: false,
  },
  {
    code: "PA-042026-007",
    title: "Vỉa hè bị lấn chiếm buôn bán trên đường Lê Duẩn",
    status: "Assigned",
    category: "Giao thông",
    location: "Phường 1",
    slaRemaining: "Còn 6 ngày",
    slaBreached: false,
  },
];
const slaBreachedCount = computed(
  () => myTickets.filter((t) => t.slaBreached).length,
);

// ── Charts ──
const categoryData = [
  { name: "Giao thông", value: 312, color: "#3b82f6" },
  { name: "Môi trường", value: 234, color: "#10b981" },
  { name: "Hạ tầng", value: 189, color: "#f59e0b" },
  { name: "An ninh", value: 98, color: "#8b5cf6" },
  { name: "Y tế", value: 67, color: "#ec4899" },
  { name: "Khác", value: 45, color: "#94a3b8" },
];
const barOptions = {
  chart: {
    type: "bar",
    toolbar: { show: false },
    fontFamily: "Inter, sans-serif",
  },
  plotOptions: { bar: { borderRadius: 4, columnWidth: "60%" } },
  colors: ["#DA251D", "#10b981"],
  dataLabels: { enabled: false },
  xaxis: {
    categories: ["T10", "T11", "T12", "T01", "T02", "T03"],
    axisBorder: { show: false },
    axisTicks: { show: false },
    labels: { style: { colors: "#94a3b8", fontSize: "13px" } },
  },
  yaxis: { labels: { style: { colors: "#94a3b8", fontSize: "12px" } } },
  grid: {
    borderColor: "#f1f5f9",
    strokeDashArray: 3,
    yaxis: { lines: { show: true } },
    xaxis: { lines: { show: false } },
  },
  tooltip: { theme: "light" },
  legend: { show: false },
};
const barSeries = [
  { name: "Tiếp nhận", data: [145, 178, 156, 198, 167, 212] },
  { name: "Đã xử lý", data: [132, 165, 148, 180, 155, 189] },
];
const donutSeries = categoryData.map((c) => c.value);
const donutOptions = {
  chart: { type: "donut", fontFamily: "Inter, sans-serif" },
  labels: categoryData.map((c) => c.name),
  colors: categoryData.map((c) => c.color),
  plotOptions: { pie: { donut: { size: "65%" } } },
  dataLabels: { enabled: false },
  legend: { show: false },
  stroke: { show: false },
};

// ── Recent table ──
const tableHeaders = [
  "Mã PA",
  "Tiêu đề",
  "Lĩnh vực",
  "Trạng thái",
  "Ngày tạo",
  "SLA",
];
const recentTickets = [
  {
    code: "PA-032026-012",
    title: "Ổ gà lớn trên đường Nguyễn Huệ",
    category: "Giao thông",
    status: "New",
    createdAt: "02/04",
    slaRemaining: "6 ngày",
    slaBreached: false,
  },
  {
    code: "PA-032026-011",
    title: "Rác thải công nghiệp ven sông Tiền",
    category: "Môi trường",
    status: "InProgress",
    createdAt: "01/04",
    slaRemaining: "3 ngày",
    slaBreached: false,
  },
  {
    code: "PA-032026-010",
    title: "Đèn đường hư hỏng Phường 2",
    category: "Hạ tầng",
    status: "Assigned",
    createdAt: "01/04",
    slaRemaining: "5 ngày",
    slaBreached: false,
  },
  {
    code: "PA-032026-009",
    title: "Karaoke quá giờ Phường 3",
    category: "An ninh",
    status: "InProgress",
    createdAt: "31/03",
    slaRemaining: "2 ngày",
    slaBreached: false,
  },
  {
    code: "PA-032026-008",
    title: "Mất nước KDC Mỹ Trà",
    category: "Hạ tầng",
    status: "InProgress",
    createdAt: "30/03",
    slaRemaining: "Quá hạn",
    slaBreached: true,
  },
];
</script>
