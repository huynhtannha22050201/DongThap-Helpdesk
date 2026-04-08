<template>
  <div class="space-y-6">
    <!-- Page Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-slate-800">Báo cáo tiến độ SLA</h1>
        <p class="text-sm text-slate-500 mt-1">
          Theo dõi hiệu suất xử lý phản ánh theo cam kết thời gian
        </p>
      </div>
      <div class="flex items-center gap-3">
        <!-- Bộ lọc thời gian -->
        <select
          v-model="selectedPeriod"
          class="px-3 py-2.5 border border-slate-200 rounded-xl text-sm bg-white shadow-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
        >
          <option value="week">7 ngày qua</option>
          <option value="month">Tháng này</option>
          <option value="quarter">Quý này</option>
          <option value="year">Năm nay</option>
        </select>
        <button
          @click="canExport && exportSlaReport()"
          :disabled="!canExport"
          class="flex items-center gap-2 px-4 py-2.5 border rounded-xl text-sm font-medium shadow-sm transition"
          :class="
            canExport
              ? 'bg-white border-slate-200 text-slate-700 hover:bg-slate-50 cursor-pointer'
              : 'bg-slate-50 border-slate-100 text-slate-400 cursor-not-allowed'
          "
        >
          <Download :size="16" />
          Xuất báo cáo
        </button>
      </div>
    </div>

    <!-- Tổng quan SLA — 4 chỉ số chính -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
      <div
        v-for="metric in slaMetrics"
        :key="metric.label"
        class="bg-white rounded-lg shadow-sm border border-slate-100 p-5"
      >
        <div class="flex items-center justify-between mb-3">
          <span class="text-sm text-slate-500">{{ metric.label }}</span>
          <component :is="metric.icon" :size="20" :class="metric.iconColor" />
        </div>
        <div class="flex items-end gap-2">
          <span class="text-3xl font-bold" :class="metric.valueColor">{{
            metric.value
          }}</span>
          <span class="text-sm text-slate-400 mb-1">{{ metric.unit }}</span>
        </div>
        <!-- Thanh tiến trình (cho tỷ lệ %) -->
        <div v-if="metric.showBar" class="mt-3">
          <div class="w-full h-2 bg-slate-100 rounded-full overflow-hidden">
            <div
              class="h-full rounded-full transition-all duration-500"
              :class="metric.barColor"
              :style="{ width: metric.barWidth }"
            ></div>
          </div>
        </div>
        <!-- Xu hướng so với kỳ trước -->
        <div class="flex items-center gap-1 mt-2">
          <TrendingUp
            v-if="metric.trend > 0"
            :size="14"
            class="text-green-500"
          />
          <TrendingDown
            v-else-if="metric.trend < 0"
            :size="14"
            class="text-red-500"
          />
          <Minus v-else :size="14" class="text-slate-400" />
          <span
            class="text-xs font-medium"
            :class="
              metric.trend > 0
                ? 'text-green-600'
                : metric.trend < 0
                  ? 'text-red-600'
                  : 'text-slate-400'
            "
          >
            {{ metric.trend > 0 ? "+" : "" }}{{ metric.trend }}% so với kỳ trước
          </span>
        </div>
      </div>
    </div>

    <!-- Biểu đồ SLA — dùng dữ liệu cho ApexCharts -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
      <!-- Biểu đồ xu hướng SLA theo ngày -->
      <div class="bg-white rounded-lg shadow-sm border border-slate-100 p-5">
        <h3 class="text-base font-semibold text-slate-800 mb-4">
          Xu hướng tuân thủ SLA
        </h3>
        <!-- Placeholder — thay bằng <apexchart> khi gắn vue3-apexcharts -->
        <div class="space-y-3">
          <div
            v-for="day in slaByDay"
            :key="day.date"
            class="flex items-center gap-3"
          >
            <span class="text-xs text-slate-500 w-16">{{ day.date }}</span>
            <div
              class="flex-1 h-6 bg-slate-100 rounded-full overflow-hidden flex"
            >
              <!-- Xanh = đúng hạn -->
              <div
                class="h-full bg-green-500 transition-all"
                :style="{ width: day.onTimePercent + '%' }"
              ></div>
              <!-- Vàng = cảnh báo -->
              <div
                class="h-full bg-amber-400 transition-all"
                :style="{ width: day.warningPercent + '%' }"
              ></div>
              <!-- Đỏ = quá hạn -->
              <div
                class="h-full bg-red-500 transition-all"
                :style="{ width: day.breachedPercent + '%' }"
              ></div>
            </div>
            <span class="text-xs font-medium text-slate-600 w-10 text-right">{{
              day.total
            }}</span>
          </div>
        </div>
        <!-- Chú thích -->
        <div
          class="flex items-center gap-4 mt-4 pt-3 border-t border-slate-100"
        >
          <div class="flex items-center gap-1.5">
            <div class="w-3 h-3 rounded-full bg-green-500"></div>
            <span class="text-xs text-slate-500">Đúng hạn</span>
          </div>
          <div class="flex items-center gap-1.5">
            <div class="w-3 h-3 rounded-full bg-amber-400"></div>
            <span class="text-xs text-slate-500">Sắp hết hạn</span>
          </div>
          <div class="flex items-center gap-1.5">
            <div class="w-3 h-3 rounded-full bg-red-500"></div>
            <span class="text-xs text-slate-500">Quá hạn</span>
          </div>
        </div>
      </div>

      <!-- Phân bố theo danh mục -->
      <div class="bg-white rounded-lg shadow-sm border border-slate-100 p-5">
        <h3 class="text-base font-semibold text-slate-800 mb-4">
          SLA theo danh mục sự cố
        </h3>
        <div class="space-y-4">
          <div v-for="cat in slaByCategory" :key="cat.name" class="space-y-1.5">
            <div class="flex items-center justify-between">
              <span class="text-sm text-slate-700">{{ cat.name }}</span>
              <span
                class="text-sm font-semibold"
                :class="
                  cat.rate >= 80
                    ? 'text-green-600'
                    : cat.rate >= 60
                      ? 'text-amber-600'
                      : 'text-red-600'
                "
              >
                {{ cat.rate }}%
              </span>
            </div>
            <div class="w-full h-2.5 bg-slate-100 rounded-full overflow-hidden">
              <div
                class="h-full rounded-full transition-all duration-500"
                :class="
                  cat.rate >= 80
                    ? 'bg-green-500'
                    : cat.rate >= 60
                      ? 'bg-amber-400'
                      : 'bg-red-500'
                "
                :style="{ width: cat.rate + '%' }"
              ></div>
            </div>
            <div class="flex items-center gap-3 text-xs text-slate-400">
              <span>{{ cat.onTime }} đúng hạn</span>
              <span>{{ cat.breached }} quá hạn</span>
              <span>{{ cat.total }} tổng</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Bảng SLA chi tiết theo đơn vị xử lý -->
    <div
      class="bg-white rounded-lg shadow-sm border border-slate-100 overflow-hidden"
    >
      <div
        class="flex items-center justify-between px-5 py-4 border-b border-slate-100"
      >
        <h3 class="text-base font-semibold text-slate-800">
          Hiệu suất theo đơn vị
        </h3>
        <div class="flex items-center gap-2 text-xs text-slate-400">
          <Info :size="14" />
          <span>Dựa trên dữ liệu {{ selectedPeriodLabel }}</span>
        </div>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full">
          <thead>
            <tr
              class="text-left text-xs font-medium text-slate-500 uppercase tracking-wider bg-slate-50/80"
            >
              <th class="px-5 py-3">Đơn vị</th>
              <th class="px-5 py-3 text-center">Tổng tiếp nhận</th>
              <th class="px-5 py-3 text-center">Đúng hạn</th>
              <th class="px-5 py-3 text-center">Quá hạn</th>
              <th class="px-5 py-3 text-center">Tỷ lệ SLA</th>
              <th class="px-5 py-3 text-center">TB thời gian xử lý</th>
              <th class="px-5 py-3 text-center">Đánh giá</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr
              v-for="dept in departmentSla"
              :key="dept.name"
              class="hover:bg-slate-50/50 transition"
            >
              <td class="px-5 py-3">
                <span class="text-sm font-medium text-slate-800">{{
                  dept.name
                }}</span>
              </td>
              <td class="px-5 py-3 text-center">
                <span class="text-sm text-slate-700 font-semibold">{{
                  dept.total
                }}</span>
              </td>
              <td class="px-5 py-3 text-center">
                <span class="text-sm text-green-600 font-medium">{{
                  dept.onTime
                }}</span>
              </td>
              <td class="px-5 py-3 text-center">
                <span class="text-sm text-red-600 font-medium">{{
                  dept.breached
                }}</span>
              </td>
              <td class="px-5 py-3 text-center">
                <div class="flex items-center justify-center gap-2">
                  <div
                    class="w-16 h-2 bg-slate-100 rounded-full overflow-hidden"
                  >
                    <div
                      class="h-full rounded-full"
                      :class="
                        dept.rate >= 80
                          ? 'bg-green-500'
                          : dept.rate >= 60
                            ? 'bg-amber-400'
                            : 'bg-red-500'
                      "
                      :style="{ width: dept.rate + '%' }"
                    ></div>
                  </div>
                  <span
                    class="text-sm font-semibold"
                    :class="
                      dept.rate >= 80
                        ? 'text-green-600'
                        : dept.rate >= 60
                          ? 'text-amber-600'
                          : 'text-red-600'
                    "
                  >
                    {{ dept.rate }}%
                  </span>
                </div>
              </td>
              <td class="px-5 py-3 text-center">
                <span class="text-sm text-slate-600">{{ dept.avgHours }}h</span>
              </td>
              <td class="px-5 py-3 text-center">
                <div class="flex items-center justify-center gap-1">
                  <Star :size="14" class="text-amber-400 fill-amber-400" />
                  <span class="text-sm font-medium text-slate-700">{{
                    dept.avgRating
                  }}</span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Danh sách phản ánh quá hạn SLA -->
    <div
      class="bg-white rounded-lg shadow-sm border border-slate-100 overflow-hidden"
    >
      <div
        class="flex items-center justify-between px-5 py-4 border-b border-slate-100"
      >
        <div class="flex items-center gap-2">
          <AlertTriangle :size="18" class="text-red-500" />
          <h3 class="text-base font-semibold text-slate-800">
            Phản ánh quá hạn SLA
          </h3>
          <span
            class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-red-50 text-red-600"
          >
            {{ breachedTickets.length }}
          </span>
        </div>
      </div>
      <div class="divide-y divide-slate-100">
        <div
          v-for="ticket in breachedTickets"
          :key="ticket.id"
          class="flex items-center gap-4 px-5 py-3 hover:bg-red-50/30 transition"
        >
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 mb-0.5">
              <span class="text-sm font-bold text-red-600">{{
                ticket.ticketCode
              }}</span>
              <span
                class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium"
                :class="getStatusBadge(ticket.status).class"
              >
                {{ getStatusBadge(ticket.status).label }}
              </span>
            </div>
            <p class="text-sm text-slate-700 truncate">{{ ticket.title }}</p>
          </div>
          <div class="text-right flex-shrink-0">
            <p class="text-sm font-medium text-slate-700">
              {{ ticket.department }}
            </p>
            <p class="text-xs text-red-500 font-medium">
              Quá hạn {{ ticket.overdueDays }} ngày
            </p>
          </div>
        </div>
        <div v-if="breachedTickets.length === 0" class="px-5 py-8 text-center">
          <CheckCircle :size="32" class="mx-auto text-green-400 mb-2" />
          <p class="text-sm text-slate-500">Không có phản ánh nào quá hạn</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { useAuthStore } from "@/stores/auth";
import {
  Download,
  TrendingUp,
  TrendingDown,
  Minus,
  Info,
  Star,
  AlertTriangle,
  CheckCircle,
  Clock,
  Target,
  Timer,
  BarChart3,
} from "lucide-vue-next";

const auth = useAuthStore();
const canExport = computed(() => auth.userRole !== "Admin");

const selectedPeriod = ref("month");

const selectedPeriodLabel = computed(() => {
  const map = {
    week: "7 ngày qua",
    month: "tháng này",
    quarter: "quý này",
    year: "năm nay",
  };
  return map[selectedPeriod.value];
});

// ── Chỉ số tổng quan — mock data từ GET /api/dashboard/sla ──
const slaMetrics = computed(() => [
  {
    label: "Tỷ lệ đúng hạn",
    value: "87.5",
    unit: "%",
    icon: Target,
    iconColor: "text-green-500",
    valueColor: "text-green-600",
    showBar: true,
    barColor: "bg-green-500",
    barWidth: "87.5%",
    trend: 3.2,
  },
  {
    label: "Tổng phản ánh",
    value: "156",
    unit: "phản ánh",
    icon: BarChart3,
    iconColor: "text-blue-500",
    valueColor: "text-slate-800",
    showBar: false,
    trend: 12,
  },
  {
    label: "TB thời gian xử lý",
    value: "38.2",
    unit: "giờ",
    icon: Timer,
    iconColor: "text-amber-500",
    valueColor: "text-slate-800",
    showBar: false,
    trend: -5.1,
  },
  {
    label: "Đang quá hạn",
    value: "7",
    unit: "phản ánh",
    icon: AlertTriangle,
    iconColor: "text-red-500",
    valueColor: "text-red-600",
    showBar: false,
    trend: -15,
  },
]);

// ── Dữ liệu xu hướng SLA theo ngày ─────────────────────
const slaByDay = ref([
  {
    date: "02/04",
    total: 18,
    onTimePercent: 78,
    warningPercent: 11,
    breachedPercent: 11,
  },
  {
    date: "03/04",
    total: 22,
    onTimePercent: 82,
    warningPercent: 9,
    breachedPercent: 9,
  },
  {
    date: "04/04",
    total: 15,
    onTimePercent: 87,
    warningPercent: 7,
    breachedPercent: 6,
  },
  {
    date: "05/04",
    total: 25,
    onTimePercent: 84,
    warningPercent: 8,
    breachedPercent: 8,
  },
  {
    date: "06/04",
    total: 20,
    onTimePercent: 90,
    warningPercent: 5,
    breachedPercent: 5,
  },
  {
    date: "07/04",
    total: 28,
    onTimePercent: 86,
    warningPercent: 7,
    breachedPercent: 7,
  },
  {
    date: "08/04",
    total: 12,
    onTimePercent: 92,
    warningPercent: 8,
    breachedPercent: 0,
  },
]);

// ── SLA theo danh mục ───────────────────────────────────
const slaByCategory = ref([
  { name: "Hạ tầng giao thông", rate: 82, onTime: 41, breached: 9, total: 50 },
  { name: "Môi trường", rate: 91, onTime: 32, breached: 3, total: 35 },
  { name: "An ninh trật tự", rate: 95, onTime: 38, breached: 2, total: 40 },
  { name: "Điện - Nước", rate: 72, onTime: 13, breached: 5, total: 18 },
  { name: "Hành chính công", rate: 85, onTime: 11, breached: 2, total: 13 },
]);

// ── SLA theo đơn vị ─────────────────────────────────────
const departmentSla = ref([
  {
    name: "Phòng Hạ tầng Kỹ thuật",
    total: 45,
    onTime: 37,
    breached: 8,
    rate: 82,
    avgHours: 42.5,
    avgRating: 4.1,
  },
  {
    name: "Phòng TN & Môi trường",
    total: 32,
    onTime: 29,
    breached: 3,
    rate: 91,
    avgHours: 35.2,
    avgRating: 4.3,
  },
  {
    name: "Công an TP. Cao Lãnh",
    total: 38,
    onTime: 36,
    breached: 2,
    rate: 95,
    avgHours: 18.7,
    avgRating: 4.6,
  },
  {
    name: "Cty Cấp nước Đồng Tháp",
    total: 18,
    onTime: 13,
    breached: 5,
    rate: 72,
    avgHours: 52.3,
    avgRating: 3.5,
  },
  {
    name: "UBND Phường 1",
    total: 23,
    onTime: 21,
    breached: 2,
    rate: 91,
    avgHours: 28.4,
    avgRating: 4.2,
  },
]);

// ── Danh sách phản ánh quá hạn ──────────────────────────
const breachedTickets = ref([
  {
    id: "1",
    ticketCode: "PA-20260401-001",
    title: "Ổ gà lớn trên đường Nguyễn Huệ",
    status: "InProgress",
    department: "Phòng Hạ tầng",
    overdueDays: 3,
  },
  {
    id: "2",
    ticketCode: "PA-20260330-005",
    title: "Cống thoát nước bị tắc tại Phường 4",
    status: "Assigned",
    department: "Phòng Hạ tầng",
    overdueDays: 5,
  },
  {
    id: "3",
    ticketCode: "PA-20260402-002",
    title: "Mất nước sinh hoạt khu phố 3",
    status: "InProgress",
    department: "Cty Cấp nước",
    overdueDays: 2,
  },
  {
    id: "4",
    ticketCode: "PA-20260329-008",
    title: "Rò rỉ ống nước chính trên đường Trần Hưng Đạo",
    status: "Assigned",
    department: "Cty Cấp nước",
    overdueDays: 7,
  },
]);

// ── Helpers ──────────────────────────────────────────────
function getStatusBadge(status) {
  const map = {
    New: { label: "Mới", class: "bg-blue-50 text-blue-700" },
    Assigned: { label: "Đã giao", class: "bg-indigo-50 text-indigo-700" },
    InProgress: { label: "Đang xử lý", class: "bg-amber-50 text-amber-700" },
    PendingVerification: {
      label: "Chờ xác minh",
      class: "bg-violet-50 text-violet-700",
    },
    Closed: { label: "Đã đóng", class: "bg-slate-100 text-slate-500" },
    Rejected: { label: "Từ chối", class: "bg-red-50 text-red-600" },
  };
  return map[status] || { label: status, class: "bg-slate-100 text-slate-600" };
}

function exportSlaReport() {
  // TODO: gọi GET /api/dashboard/export?type=sla
  alert("Chức năng xuất báo cáo SLA sẽ được gắn API sau.");
}
</script>
