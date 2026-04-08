<template>
  <div class="space-y-6">
    <!-- Page Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-slate-800">Danh sách phản ánh</h1>
        <p class="text-sm text-slate-500 mt-1">
          Quản lý và theo dõi tất cả phản ánh trong hệ thống
        </p>
      </div>
      <!-- Nút xuất CSV — gọi GET /api/dashboard/export -->
      <button
        @click="canViewDetail && exportCsv()"
        :disabled="!canViewDetail"
        class="flex items-center gap-2 px-4 py-2 border rounded-xl text-sm font-medium shadow-sm transition"
        :class="
          canViewDetail
            ? 'bg-white border-slate-200 text-slate-700 hover:bg-slate-50 cursor-pointer'
            : 'bg-slate-50 border-slate-100 text-slate-400 cursor-not-allowed'
        "
      >
        <Download :size="16" />
        Xuất báo cáo
      </button>
    </div>

    <!-- Bộ lọc -->
    <div class="bg-white rounded-lg shadow-sm border border-slate-100 p-4">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
        <!-- Tìm kiếm theo mã hoặc tiêu đề -->
        <div class="lg:col-span-2 relative">
          <Search
            :size="16"
            class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
          />
          <input
            v-model="filters.search"
            type="text"
            placeholder="Tìm theo mã, tiêu đề, SĐT..."
            class="w-full pl-10 pr-4 py-2.5 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
          />
        </div>

        <!-- Lọc theo trạng thái -->
        <select
          v-model="filters.status"
          class="w-full px-3 py-2.5 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 bg-white"
        >
          <option value="">Tất cả trạng thái</option>
          <option v-for="s in statusOptions" :key="s.value" :value="s.value">
            {{ s.label }}
          </option>
        </select>

        <!-- Lọc theo mức độ ưu tiên -->
        <select
          v-model="filters.priority"
          class="w-full px-3 py-2.5 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 bg-white"
        >
          <option value="">Tất cả ưu tiên</option>
          <option v-for="p in priorityOptions" :key="p.value" :value="p.value">
            {{ p.label }}
          </option>
        </select>

        <!-- Lọc theo danh mục -->
        <select
          v-model="filters.category"
          class="w-full px-3 py-2.5 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 bg-white"
        >
          <option value="">Tất cả danh mục</option>
          <option v-for="c in categories" :key="c.id" :value="c.id">
            {{ c.name }}
          </option>
        </select>
      </div>

      <!-- Hàng lọc phụ: khoảng thời gian + nút reset -->
      <div
        class="flex flex-wrap items-center gap-4 mt-4 pt-4 border-t border-slate-100"
      >
        <div class="flex items-center gap-2 text-sm text-slate-600">
          <Calendar :size="16" class="text-slate-400" />
          <span>Từ</span>
          <input
            v-model="filters.dateFrom"
            type="date"
            class="px-3 py-1.5 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
          />
          <span>đến</span>
          <input
            v-model="filters.dateTo"
            type="date"
            class="px-3 py-1.5 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
          />
        </div>

        <button
          @click="resetFilters"
          class="ml-auto flex items-center gap-1 px-3 py-1.5 text-sm text-slate-500 hover:text-red-600 transition"
        >
          <RotateCcw :size="14" />
          Đặt lại bộ lọc
        </button>
      </div>
    </div>

    <!-- Thống kê nhanh theo trạng thái — mỗi card mang màu nền semantic -->
    <div class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-7 gap-3">
      <button
        v-for="stat in statusStats"
        :key="stat.status"
        @click="
          filters.status = filters.status === stat.status ? '' : stat.status
        "
        class="flex flex-col items-center p-3 rounded-lg border transition cursor-pointer"
        :class="[
          stat.bg,
          filters.status === stat.status
            ? stat.activeBorder
            : stat.defaultBorder,
        ]"
      >
        <span class="text-lg font-bold" :class="stat.color">{{
          stat.count
        }}</span>
        <span class="text-xs text-slate-500 mt-0.5">{{ stat.label }}</span>
      </button>
    </div>

    <!-- Bảng dữ liệu -->
    <div
      class="bg-white rounded-lg shadow-sm border border-slate-100 overflow-hidden"
    >
      <!-- Thanh info -->
      <div
        class="flex items-center justify-between px-4 py-3 border-b border-slate-100 bg-slate-50/50"
      >
        <span class="text-sm text-slate-600">
          Hiển thị <strong>{{ filteredTickets.length }}</strong> /
          {{ tickets.length }} phản ánh
        </span>
        <select
          v-model="pageSize"
          class="px-2 py-1 border border-slate-200 rounded text-sm bg-white"
        >
          <option :value="10">10 / trang</option>
          <option :value="20">20 / trang</option>
          <option :value="50">50 / trang</option>
        </select>
      </div>

      <!-- Table -->
      <div class="overflow-x-auto">
        <table class="w-full">
          <thead>
            <tr
              class="text-left text-xs font-medium text-slate-500 uppercase tracking-wider"
            >
              <th class="px-4 py-3">Mã</th>
              <th class="px-4 py-3">Tiêu đề</th>
              <th class="px-4 py-3">Danh mục</th>
              <th class="px-4 py-3">Trạng thái</th>
              <th class="px-4 py-3">Ưu tiên</th>
              <th class="px-4 py-3">Người gửi</th>
              <th class="px-4 py-3">Đơn vị xử lý</th>
              <th class="px-4 py-3">SLA</th>
              <th class="px-4 py-3">Ngày tạo</th>
              <th class="px-4 py-3 text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr
              v-for="ticket in paginatedTickets"
              :key="ticket.id"
              class="hover:bg-slate-50/50 transition"
            >
              <!-- Mã phản ánh -->
              <td class="px-4 py-3">
                <span class="text-sm font-semibold text-red-600">{{
                  ticket.ticketCode
                }}</span>
              </td>

              <!-- Tiêu đề — giới hạn 1 dòng -->
              <td class="px-4 py-3 max-w-[220px]">
                <p class="text-sm text-slate-800 font-medium truncate">
                  {{ ticket.title }}
                </p>
              </td>

              <!-- Danh mục -->
              <td class="px-4 py-3">
                <span class="text-sm text-slate-600">{{
                  ticket.categoryName || "—"
                }}</span>
              </td>

              <!-- Status badge — màu semantic theo rules.md -->
              <td class="px-4 py-3">
                <span
                  class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                  :class="getStatusBadge(ticket.status).class"
                >
                  {{ getStatusBadge(ticket.status).label }}
                </span>
              </td>

              <!-- Priority badge -->
              <td class="px-4 py-3">
                <span
                  class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                  :class="getPriorityBadge(ticket.priority).class"
                >
                  {{ getPriorityBadge(ticket.priority).label }}
                </span>
              </td>

              <!-- Người gửi -->
              <td class="px-4 py-3">
                <div class="text-sm text-slate-800">
                  {{ ticket.reporterName }}
                </div>
                <div class="text-xs text-slate-400">
                  {{ ticket.reporterPhone }}
                </div>
              </td>

              <!-- Đơn vị xử lý -->
              <td class="px-4 py-3">
                <span class="text-sm text-slate-600">{{
                  ticket.assignedDepartmentName || "—"
                }}</span>
              </td>

              <!-- SLA — hiện icon cảnh báo nếu quá hạn -->
              <td class="px-4 py-3">
                <div class="flex items-center gap-1">
                  <AlertTriangle
                    v-if="ticket.isSlaBreached"
                    :size="14"
                    class="text-red-500"
                  />
                  <Clock v-else :size="14" class="text-slate-400" />
                  <span
                    class="text-xs font-medium"
                    :class="
                      ticket.isSlaBreached ? 'text-red-600' : 'text-slate-500'
                    "
                  >
                    {{ ticket.slaHours }}h
                  </span>
                </div>
              </td>

              <!-- Ngày tạo -->
              <td class="px-4 py-3">
                <span class="text-sm text-slate-500">{{
                  formatDate(ticket.createdAt)
                }}</span>
              </td>

              <!-- Thao tác -->
              <td class="px-4 py-3 text-center">
                <button
                  @click="canViewDetail && goToDetail(ticket.id)"
                  :disabled="!canViewDetail"
                  class="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium rounded-lg transition"
                  :class="
                    canViewDetail
                      ? 'text-red-600 bg-red-50 hover:bg-red-100 cursor-pointer'
                      : 'text-slate-400 bg-slate-50 cursor-not-allowed'
                  "
                >
                  <Eye :size="14" />
                  Xem
                </button>
              </td>
            </tr>

            <!-- Trạng thái rỗng -->
            <tr v-if="filteredTickets.length === 0">
              <td colspan="10" class="px-4 py-12 text-center">
                <Inbox :size="40" class="mx-auto text-slate-300 mb-3" />
                <p class="text-sm text-slate-500">
                  Không tìm thấy phản ánh nào phù hợp
                </p>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Phân trang -->
      <div
        v-if="totalPages > 1"
        class="flex items-center justify-between px-4 py-3 border-t border-slate-100 bg-slate-50/50"
      >
        <span class="text-sm text-slate-500">
          Trang {{ currentPage }} / {{ totalPages }}
        </span>
        <div class="flex items-center gap-1">
          <button
            @click="currentPage = Math.max(1, currentPage - 1)"
            :disabled="currentPage === 1"
            class="p-2 rounded-lg text-slate-500 hover:bg-white hover:shadow-sm disabled:opacity-40 disabled:cursor-not-allowed transition"
          >
            <ChevronLeft :size="16" />
          </button>
          <button
            v-for="page in visiblePages"
            :key="page"
            @click="currentPage = page"
            class="px-3 py-1.5 rounded-lg text-sm font-medium transition"
            :class="
              currentPage === page
                ? 'bg-red-600 text-white shadow-sm'
                : 'text-slate-600 hover:bg-white hover:shadow-sm'
            "
          >
            {{ page }}
          </button>
          <button
            @click="currentPage = Math.min(totalPages, currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="p-2 rounded-lg text-slate-500 hover:bg-white hover:shadow-sm disabled:opacity-40 disabled:cursor-not-allowed transition"
          >
            <ChevronRight :size="16" />
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "@/stores/auth";
import {
  Search,
  Download,
  Calendar,
  RotateCcw,
  Eye,
  Clock,
  AlertTriangle,
  Inbox,
  ChevronLeft,
  ChevronRight,
} from "lucide-vue-next";

const router = useRouter();

const auth = useAuthStore();
const canViewDetail = computed(() => auth.userRole !== "Admin");

// ── Bộ lọc ──────────────────────────────────────────────
const filters = ref({
  search: "",
  status: "",
  priority: "",
  category: "",
  dateFrom: "",
  dateTo: "",
});

const currentPage = ref(1);
const pageSize = ref(10);

// Reset về trang 1 khi filter thay đổi
watch(
  filters,
  () => {
    currentPage.value = 1;
  },
  { deep: true },
);

const statusOptions = [
  { value: "New", label: "Mới" },
  { value: "UnderReview", label: "Đang kiểm duyệt" },
  { value: "Assigned", label: "Đã giao" },
  { value: "InProgress", label: "Đang xử lý" },
  { value: "PendingVerification", label: "Chờ xác minh" },
  { value: "Closed", label: "Đã đóng" },
  { value: "Rejected", label: "Từ chối" },
];

const priorityOptions = [
  { value: "Low", label: "Thấp" },
  { value: "Normal", label: "Bình thường" },
  { value: "High", label: "Cao" },
  { value: "Urgent", label: "Khẩn cấp" },
];

const categories = [
  { id: "cat1", name: "Hạ tầng giao thông" },
  { id: "cat2", name: "Môi trường" },
  { id: "cat3", name: "An ninh trật tự" },
  { id: "cat4", name: "Điện - Nước" },
  { id: "cat5", name: "Hành chính công" },
];

// ── Mock data — sẽ thay bằng GET /api/tickets ───────────
const tickets = ref([
  {
    id: "1",
    ticketCode: "PA-20260401-001",
    title: "Ổ gà lớn trên đường Nguyễn Huệ",
    categoryName: "Hạ tầng giao thông",
    categoryId: "cat1",
    status: "InProgress",
    priority: "High",
    reporterName: "Nguyễn Văn An",
    reporterPhone: "0901234567",
    assignedDepartmentName: "Phòng Hạ tầng",
    slaHours: 48,
    isSlaBreached: false,
    createdAt: "2026-04-01T08:30:00",
  },
  {
    id: "2",
    ticketCode: "PA-20260401-002",
    title: "Rác thải chất đống tại ngã tư Lê Lợi",
    categoryName: "Môi trường",
    categoryId: "cat2",
    status: "Assigned",
    priority: "Normal",
    reporterName: "Trần Thị Bình",
    reporterPhone: "0912345678",
    assignedDepartmentName: "Phòng TN&MT",
    slaHours: 72,
    isSlaBreached: false,
    createdAt: "2026-04-01T10:15:00",
  },
  {
    id: "3",
    ticketCode: "PA-20260402-001",
    title: "Đèn đường hư tại khu dân cư Phường 2",
    categoryName: "Điện - Nước",
    categoryId: "cat4",
    status: "New",
    priority: "Normal",
    reporterName: "Lê Minh Cường",
    reporterPhone: "0923456789",
    assignedDepartmentName: null,
    slaHours: 72,
    isSlaBreached: false,
    createdAt: "2026-04-02T07:00:00",
  },
  {
    id: "4",
    ticketCode: "PA-20260402-002",
    title: "Mất nước sinh hoạt khu phố 3",
    categoryName: "Điện - Nước",
    categoryId: "cat4",
    status: "PendingVerification",
    priority: "Urgent",
    reporterName: "Phạm Thị Dung",
    reporterPhone: "0934567890",
    assignedDepartmentName: "Cty Cấp nước",
    slaHours: 24,
    isSlaBreached: true,
    createdAt: "2026-04-02T09:45:00",
  },
  {
    id: "5",
    ticketCode: "PA-20260403-001",
    title: "Tiếng ồn từ quán karaoke sau 22h",
    categoryName: "An ninh trật tự",
    categoryId: "cat3",
    status: "Closed",
    priority: "Low",
    reporterName: "Hoàng Văn Em",
    reporterPhone: "0945678901",
    assignedDepartmentName: "Công an Phường 1",
    slaHours: 72,
    isSlaBreached: false,
    createdAt: "2026-04-03T22:30:00",
  },
  {
    id: "6",
    ticketCode: "PA-20260403-002",
    title: "Cống thoát nước bị tắc nghẽn",
    categoryName: "Hạ tầng giao thông",
    categoryId: "cat1",
    status: "Rejected",
    priority: "Normal",
    reporterName: "Võ Thị Phương",
    reporterPhone: "0956789012",
    assignedDepartmentName: null,
    slaHours: 72,
    isSlaBreached: false,
    createdAt: "2026-04-03T14:20:00",
  },
  {
    id: "7",
    ticketCode: "PA-20260404-001",
    title: "Cây xanh ngã đổ chắn lối đi",
    categoryName: "Môi trường",
    categoryId: "cat2",
    status: "InProgress",
    priority: "High",
    reporterName: "Đặng Quốc Huy",
    reporterPhone: "0967890123",
    assignedDepartmentName: "Phòng TN&MT",
    slaHours: 48,
    isSlaBreached: true,
    createdAt: "2026-04-04T06:00:00",
  },
  {
    id: "8",
    ticketCode: "PA-20260404-002",
    title: "Vỉa hè bị lấn chiếm kinh doanh",
    categoryName: "An ninh trật tự",
    categoryId: "cat3",
    status: "UnderReview",
    priority: "Normal",
    reporterName: "Nguyễn Thị Lan",
    reporterPhone: "0978901234",
    assignedDepartmentName: null,
    slaHours: 72,
    isSlaBreached: false,
    createdAt: "2026-04-04T11:30:00",
  },
]);

// ── Thống kê nhanh theo trạng thái ──────────────────────
// bg: nền mặc định | activeBorder: viền khi đang lọc | defaultBorder: viền bình thường
const statusStats = computed(() => {
  const countByStatus = (status) =>
    tickets.value.filter((t) => t.status === status).length;
  return [
    {
      status: "",
      label: "Tổng",
      count: tickets.value.length,
      color: "text-slate-800",
      bg: "bg-white",
      activeBorder: "border-red-300 ring-1 ring-red-200",
      defaultBorder: "border-slate-100 hover:border-slate-200",
    },
    {
      status: "New",
      label: "Mới",
      count: countByStatus("New"),
      color: "text-blue-600",
      bg: "bg-blue-50/40",
      activeBorder: "border-blue-300 ring-1 ring-blue-200",
      defaultBorder: "border-blue-100 hover:border-blue-200",
    },
    {
      status: "UnderReview",
      label: "Kiểm duyệt",
      count: countByStatus("UnderReview"),
      color: "text-orange-600",
      bg: "bg-orange-50/40",
      activeBorder: "border-orange-300 ring-1 ring-orange-200",
      defaultBorder: "border-orange-100 hover:border-orange-200",
    },
    {
      status: "Assigned",
      label: "Đã giao",
      count: countByStatus("Assigned"),
      color: "text-indigo-600",
      bg: "bg-indigo-50/40",
      activeBorder: "border-indigo-300 ring-1 ring-indigo-200",
      defaultBorder: "border-indigo-100 hover:border-indigo-200",
    },
    {
      status: "InProgress",
      label: "Đang xử lý",
      count: countByStatus("InProgress"),
      color: "text-amber-600",
      bg: "bg-amber-50/40",
      activeBorder: "border-amber-300 ring-1 ring-amber-200",
      defaultBorder: "border-amber-100 hover:border-amber-200",
    },
    {
      status: "PendingVerification",
      label: "Chờ xác minh",
      count: countByStatus("PendingVerification"),
      color: "text-violet-600",
      bg: "bg-violet-50/40",
      activeBorder: "border-violet-300 ring-1 ring-violet-200",
      defaultBorder: "border-violet-100 hover:border-violet-200",
    },
    {
      status: "Closed",
      label: "Đã đóng",
      count: countByStatus("Closed"),
      color: "text-slate-500",
      bg: "bg-slate-50/40",
      activeBorder: "border-slate-300 ring-1 ring-slate-200",
      defaultBorder: "border-slate-100 hover:border-slate-200",
    },
  ];
});

// ── Lọc dữ liệu ────────────────────────────────────────
const filteredTickets = computed(() => {
  return tickets.value.filter((t) => {
    // Tìm kiếm full-text
    if (filters.value.search) {
      const q = filters.value.search.toLowerCase();
      const match =
        t.ticketCode.toLowerCase().includes(q) ||
        t.title.toLowerCase().includes(q) ||
        t.reporterName.toLowerCase().includes(q) ||
        t.reporterPhone.includes(q);
      if (!match) return false;
    }
    if (filters.value.status && t.status !== filters.value.status) return false;
    if (filters.value.priority && t.priority !== filters.value.priority)
      return false;
    if (filters.value.category && t.categoryId !== filters.value.category)
      return false;
    if (filters.value.dateFrom && t.createdAt < filters.value.dateFrom)
      return false;
    if (
      filters.value.dateTo &&
      t.createdAt > filters.value.dateTo + "T23:59:59"
    )
      return false;
    return true;
  });
});

// ── Phân trang ──────────────────────────────────────────
const totalPages = computed(
  () => Math.ceil(filteredTickets.value.length / pageSize.value) || 1,
);

const paginatedTickets = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredTickets.value.slice(start, start + pageSize.value);
});

// Hiển thị tối đa 5 nút trang
const visiblePages = computed(() => {
  const total = totalPages.value;
  const current = currentPage.value;
  if (total <= 5) return Array.from({ length: total }, (_, i) => i + 1);
  const start = Math.max(1, Math.min(current - 2, total - 4));
  return Array.from({ length: 5 }, (_, i) => start + i);
});

// ── Helpers ─────────────────────────────────────────────
// Màu badge trạng thái — khớp design tokens trong rules.md
function getStatusBadge(status) {
  const map = {
    New: { label: "Mới", class: "bg-blue-50 text-blue-700" },
    UnderReview: {
      label: "Đang kiểm duyệt",
      class: "bg-orange-50 text-orange-700",
    },
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

function getPriorityBadge(priority) {
  const map = {
    Low: { label: "Thấp", class: "bg-slate-100 text-slate-600" },
    Normal: { label: "Bình thường", class: "bg-blue-50 text-blue-600" },
    High: { label: "Cao", class: "bg-orange-50 text-orange-600" },
    Urgent: { label: "Khẩn cấp", class: "bg-red-50 text-red-600" },
  };
  return (
    map[priority] || { label: priority, class: "bg-slate-100 text-slate-600" }
  );
}

function formatDate(dateStr) {
  const d = new Date(dateStr);
  return d.toLocaleDateString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

function resetFilters() {
  filters.value = {
    search: "",
    status: "",
    priority: "",
    category: "",
    dateFrom: "",
    dateTo: "",
  };
}

function goToDetail(id) {
  router.push(`/admin/tickets/${id}`);
}

function exportCsv() {
  // TODO: gọi GET /api/dashboard/export
  alert("Chức năng xuất CSV sẽ được gắn API sau.");
}
</script>
