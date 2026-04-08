<template>
  <div class="space-y-4">
    <!-- Page Header + Bộ lọc trên cùng -->
    <div class="flex flex-wrap items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-800">Bản đồ phản ánh</h1>
        <p class="text-sm text-slate-500 mt-1">Phân bố phản ánh theo vị trí địa lý — Tỉnh Đồng Tháp</p>
      </div>
      <div class="flex items-center gap-3">
        <!-- Lọc trạng thái -->
        <select
          v-model="filters.status"
          class="px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white shadow-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
        >
          <option value="">Tất cả trạng thái</option>
          <option v-for="s in statusOptions" :key="s.value" :value="s.value">{{ s.label }}</option>
        </select>

        <!-- Lọc danh mục -->
        <select
          v-model="filters.category"
          class="px-3 py-2 border border-slate-200 rounded-xl text-sm bg-white shadow-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
        >
          <option value="">Tất cả danh mục</option>
          <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
        </select>

        <!-- Toggle heatmap -->
        <button
          @click="showHeatmap = !showHeatmap"
          class="flex items-center gap-1.5 px-3 py-2 rounded-xl text-sm font-medium border shadow-sm transition"
          :class="showHeatmap
            ? 'bg-red-50 border-red-200 text-red-700'
            : 'bg-white border-slate-200 text-slate-600 hover:bg-slate-50'"
        >
          <Flame :size="16" />
          Heatmap
        </button>
      </div>
    </div>

    <!-- Container chính: Bản đồ + Sidebar -->
    <div class="flex gap-4" style="height: calc(100vh - 200px);">
      <!-- Bản đồ Leaflet -->
      <div class="flex-1 bg-white rounded-lg shadow-sm border border-slate-100 overflow-hidden relative">
        <!-- Map container — Leaflet sẽ mount vào đây -->
        <div ref="mapContainer" class="w-full h-full">
          <!-- Placeholder khi chưa có Leaflet -->
          <div class="w-full h-full flex flex-col items-center justify-center bg-slate-50">
            <MapIcon :size="64" class="text-slate-300 mb-4" />
            <h3 class="text-lg font-semibold text-slate-500">Bản đồ Đồng Tháp</h3>
            <p class="text-sm text-slate-400 mt-1">Tích hợp Leaflet.js khi gắn API thật</p>
            <p class="text-xs text-slate-300 mt-3">Tọa độ trung tâm: 10.4538°N, 105.6324°E</p>

            <!-- Mock bản đồ bằng grid điểm phản ánh -->
            <div class="mt-6 w-full max-w-lg px-8">
              <div class="relative bg-green-50 border-2 border-dashed border-green-200 rounded-xl p-6" style="aspect-ratio: 4/3;">
                <!-- Các marker giả lập -->
                <div
                  v-for="point in filteredPoints"
                  :key="point.id"
                  class="absolute w-4 h-4 rounded-full border-2 border-white shadow-md cursor-pointer hover:scale-150 transition-transform"
                  :class="getMarkerColor(point.status)"
                  :style="{ left: point.x + '%', top: point.y + '%' }"
                  :title="point.title"
                  @click="selectPoint(point)"
                ></div>
              </div>
            </div>
          </div>
        </div>

        <!-- Chú thích màu marker -->
        <div class="absolute bottom-4 left-4 bg-white/95 backdrop-blur-sm rounded-lg shadow-md border border-slate-100 p-3">
          <p class="text-xs font-medium text-slate-600 mb-2">Chú thích</p>
          <div class="space-y-1.5">
            <div class="flex items-center gap-2">
              <div class="w-3 h-3 rounded-full bg-blue-500"></div>
              <span class="text-xs text-slate-500">Mới</span>
            </div>
            <div class="flex items-center gap-2">
              <div class="w-3 h-3 rounded-full bg-amber-500"></div>
              <span class="text-xs text-slate-500">Đang xử lý</span>
            </div>
            <div class="flex items-center gap-2">
              <div class="w-3 h-3 rounded-full bg-green-500"></div>
              <span class="text-xs text-slate-500">Đã đóng</span>
            </div>
            <div class="flex items-center gap-2">
              <div class="w-3 h-3 rounded-full bg-red-500"></div>
              <span class="text-xs text-slate-500">Quá hạn SLA</span>
            </div>
          </div>
        </div>

        <!-- Thống kê nhanh overlay -->
        <div class="absolute top-4 right-4 bg-white/95 backdrop-blur-sm rounded-lg shadow-md border border-slate-100 p-3 min-w-[160px]">
          <p class="text-xs font-medium text-slate-600 mb-2">Tổng quan</p>
          <div class="space-y-1.5">
            <div class="flex items-center justify-between">
              <span class="text-xs text-slate-500">Tổng điểm</span>
              <span class="text-sm font-bold text-slate-800">{{ filteredPoints.length }}</span>
            </div>
            <div class="flex items-center justify-between">
              <span class="text-xs text-slate-500">Quá hạn</span>
              <span class="text-sm font-bold text-red-600">{{ breachedCount }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Sidebar: Danh sách phản ánh -->
      <div class="w-80 bg-white rounded-lg shadow-sm border border-slate-100 flex flex-col overflow-hidden flex-shrink-0">
        <!-- Search trong sidebar -->
        <div class="p-3 border-b border-slate-100">
          <div class="relative">
            <Search :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
            <input
              v-model="sidebarSearch"
              type="text"
              placeholder="Tìm phản ánh..."
              class="w-full pl-9 pr-3 py-2 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
            />
          </div>
        </div>

        <!-- Danh sách -->
        <div class="flex-1 overflow-y-auto divide-y divide-slate-100">
          <div
            v-for="point in sidebarPoints"
            :key="point.id"
            @click="selectPoint(point)"
            class="p-3 cursor-pointer transition"
            :class="selectedPoint?.id === point.id ? 'bg-red-50' : 'hover:bg-slate-50'"
          >
            <div class="flex items-center gap-2 mb-1">
              <div class="w-2.5 h-2.5 rounded-full flex-shrink-0" :class="getMarkerColor(point.status)"></div>
              <span class="text-xs font-bold text-red-600">{{ point.ticketCode }}</span>
              <span
                v-if="point.isSlaBreached"
                class="ml-auto text-xs font-medium text-red-500 flex items-center gap-0.5"
              >
                <AlertTriangle :size="10" />
                Quá hạn
              </span>
            </div>
            <p class="text-sm text-slate-800 font-medium truncate">{{ point.title }}</p>
            <p class="text-xs text-slate-400 mt-0.5 truncate">{{ point.address }}</p>
          </div>

          <div v-if="sidebarPoints.length === 0" class="p-6 text-center">
            <p class="text-sm text-slate-400">Không tìm thấy phản ánh</p>
          </div>
        </div>

        <!-- Chi tiết phản ánh được chọn -->
        <div
          v-if="selectedPoint"
          class="border-t border-slate-100 bg-slate-50/50 p-4 space-y-3"
        >
          <div class="flex items-center justify-between">
            <span class="text-sm font-bold text-red-600">{{ selectedPoint.ticketCode }}</span>
            <span
              class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium"
              :class="getStatusBadge(selectedPoint.status).class"
            >
              {{ getStatusBadge(selectedPoint.status).label }}
            </span>
          </div>
          <h4 class="text-sm font-semibold text-slate-800">{{ selectedPoint.title }}</h4>
          <div class="space-y-1.5 text-xs text-slate-500">
            <div class="flex items-center gap-1.5">
              <MapPin :size="12" />
              <span>{{ selectedPoint.address }}</span>
            </div>
            <div class="flex items-center gap-1.5">
              <Tag :size="12" />
              <span>{{ selectedPoint.categoryName }}</span>
            </div>
            <div class="flex items-center gap-1.5">
              <User :size="12" />
              <span>{{ selectedPoint.reporterName }}</span>
            </div>
            <div class="flex items-center gap-1.5">
              <Clock :size="12" />
              <span>{{ formatDate(selectedPoint.createdAt) }}</span>
            </div>
          </div>
          <button
            @click="goToDetail(selectedPoint.id)"
            class="w-full flex items-center justify-center gap-1 px-3 py-2 rounded-xl text-sm font-medium text-white bg-red-600 hover:bg-red-700 shadow-sm transition"
          >
            <Eye :size="14" />
            Xem chi tiết
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import {
  Search, MapPin, Tag, User, Clock, Eye, Flame,
  AlertTriangle, Map as MapIcon
} from 'lucide-vue-next'

const router = useRouter()

const showHeatmap = ref(false)
const sidebarSearch = ref('')
const selectedPoint = ref(null)

const filters = ref({ status: '', category: '' })

const statusOptions = [
  { value: 'New', label: 'Mới' },
  { value: 'InProgress', label: 'Đang xử lý' },
  { value: 'Closed', label: 'Đã đóng' },
  { value: 'Rejected', label: 'Từ chối' }
]

const categories = [
  { id: 'cat1', name: 'Hạ tầng giao thông' },
  { id: 'cat2', name: 'Môi trường' },
  { id: 'cat3', name: 'An ninh trật tự' },
  { id: 'cat4', name: 'Điện - Nước' }
]

// ── Mock data — sẽ thay bằng GET /api/dashboard/heatmap ──
// x, y là tọa độ tương đối trên bản đồ mock (%)
const allPoints = ref([
  { id: '1', ticketCode: 'PA-20260401-001', title: 'Ổ gà lớn trên đường Nguyễn Huệ',
    status: 'InProgress', categoryId: 'cat1', categoryName: 'Hạ tầng giao thông',
    address: 'Đường Nguyễn Huệ, TP. Cao Lãnh', reporterName: 'Nguyễn Văn An',
    lat: 10.4538, lng: 105.6324, x: 45, y: 35, isSlaBreached: true,
    createdAt: '2026-04-01T08:30:00' },
  { id: '2', ticketCode: 'PA-20260401-002', title: 'Rác thải đổ trộm bờ kênh',
    status: 'New', categoryId: 'cat2', categoryName: 'Môi trường',
    address: 'Kênh Nguyễn Văn Tiếp, Xã Mỹ Trà', reporterName: 'Trần Thị Bình',
    lat: 10.4612, lng: 105.6501, x: 62, y: 28, isSlaBreached: false,
    createdAt: '2026-04-01T10:15:00' },
  { id: '3', ticketCode: 'PA-20260402-001', title: 'Đèn đường hư khu dân cư Phường 2',
    status: 'InProgress', categoryId: 'cat4', categoryName: 'Điện - Nước',
    address: 'Khu dân cư Phường 2, TP. Cao Lãnh', reporterName: 'Lê Minh Cường',
    lat: 10.4480, lng: 105.6250, x: 38, y: 42, isSlaBreached: false,
    createdAt: '2026-04-02T07:00:00' },
  { id: '4', ticketCode: 'PA-20260402-002', title: 'Mất nước sinh hoạt khu phố 3',
    status: 'InProgress', categoryId: 'cat4', categoryName: 'Điện - Nước',
    address: 'Khu phố 3, Phường Mỹ Phú', reporterName: 'Phạm Thị Dung',
    lat: 10.4355, lng: 105.6180, x: 30, y: 55, isSlaBreached: true,
    createdAt: '2026-04-02T09:45:00' },
  { id: '5', ticketCode: 'PA-20260403-001', title: 'Tiếng ồn quán karaoke sau 22h',
    status: 'Closed', categoryId: 'cat3', categoryName: 'An ninh trật tự',
    address: 'Đường 30/4, Phường 1', reporterName: 'Hoàng Văn Em',
    lat: 10.4590, lng: 105.6400, x: 52, y: 30, isSlaBreached: false,
    createdAt: '2026-04-03T22:30:00' },
  { id: '6', ticketCode: 'PA-20260404-001', title: 'Cây xanh ngã đổ chắn lối đi',
    status: 'InProgress', categoryId: 'cat2', categoryName: 'Môi trường',
    address: 'Đường Lý Thường Kiệt, Phường 3', reporterName: 'Đặng Quốc Huy',
    lat: 10.4500, lng: 105.6350, x: 48, y: 40, isSlaBreached: true,
    createdAt: '2026-04-04T06:00:00' },
  { id: '7', ticketCode: 'PA-20260405-001', title: 'Ngập nước do cống tắc tại ngã tư',
    status: 'New', categoryId: 'cat1', categoryName: 'Hạ tầng giao thông',
    address: 'Ngã tư Lê Lợi - Trần Phú', reporterName: 'Bùi Thanh Tâm',
    lat: 10.4560, lng: 105.6280, x: 42, y: 38, isSlaBreached: false,
    createdAt: '2026-04-05T15:00:00' },
  { id: '8', ticketCode: 'PA-20260406-001', title: 'Vỉa hè bị chiếm dụng bán hàng rong',
    status: 'New', categoryId: 'cat3', categoryName: 'An ninh trật tự',
    address: 'Đường Nguyễn Sinh Sắc, Phường 4', reporterName: 'Ngô Minh Tuấn',
    lat: 10.4420, lng: 105.6450, x: 55, y: 50, isSlaBreached: false,
    createdAt: '2026-04-06T09:20:00' },
  { id: '9', ticketCode: 'PA-20260407-001', title: 'Rò rỉ ống nước chính đường lớn',
    status: 'InProgress', categoryId: 'cat4', categoryName: 'Điện - Nước',
    address: 'Đường Trần Hưng Đạo, TP. Cao Lãnh', reporterName: 'Vũ Thị Mai',
    lat: 10.4510, lng: 105.6220, x: 35, y: 45, isSlaBreached: false,
    createdAt: '2026-04-07T11:30:00' },
  { id: '10', ticketCode: 'PA-20260408-001', title: 'Hố ga mất nắp gây nguy hiểm',
    status: 'New', categoryId: 'cat1', categoryName: 'Hạ tầng giao thông',
    address: 'Đường Phạm Hữu Lầu, Phường 6', reporterName: 'Trương Văn Đạt',
    lat: 10.4380, lng: 105.6380, x: 50, y: 58, isSlaBreached: false,
    createdAt: '2026-04-08T07:00:00' }
])

// ── Lọc dữ liệu ────────────────────────────────────────
const filteredPoints = computed(() => {
  return allPoints.value.filter(p => {
    if (filters.value.status && p.status !== filters.value.status) return false
    if (filters.value.category && p.categoryId !== filters.value.category) return false
    return true
  })
})

// Sidebar lọc thêm theo search text
const sidebarPoints = computed(() => {
  if (!sidebarSearch.value) return filteredPoints.value
  const q = sidebarSearch.value.toLowerCase()
  return filteredPoints.value.filter(p =>
    p.ticketCode.toLowerCase().includes(q)
    || p.title.toLowerCase().includes(q)
    || p.address.toLowerCase().includes(q)
  )
})

const breachedCount = computed(() => filteredPoints.value.filter(p => p.isSlaBreached).length)

// ── Helpers ─────────────────────────────────────────────
// Màu marker theo trạng thái — ưu tiên đỏ nếu quá hạn SLA
function getMarkerColor(status) {
  if (status === 'InProgress' || status === 'Assigned') return 'bg-amber-500'
  if (status === 'New') return 'bg-blue-500'
  if (status === 'Closed') return 'bg-green-500'
  return 'bg-slate-400'
}

function getStatusBadge(status) {
  const map = {
    New: { label: 'Mới', class: 'bg-blue-50 text-blue-700' },
    Assigned: { label: 'Đã giao', class: 'bg-indigo-50 text-indigo-700' },
    InProgress: { label: 'Đang xử lý', class: 'bg-amber-50 text-amber-700' },
    PendingVerification: { label: 'Chờ xác minh', class: 'bg-violet-50 text-violet-700' },
    Closed: { label: 'Đã đóng', class: 'bg-slate-100 text-slate-500' },
    Rejected: { label: 'Từ chối', class: 'bg-red-50 text-red-600' }
  }
  return map[status] || { label: status, class: 'bg-slate-100 text-slate-600' }
}

function formatDate(dateStr) {
  return new Date(dateStr).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

function selectPoint(point) {
  selectedPoint.value = point
}

function goToDetail(id) {
  router.push(`/admin/tickets/${id}`)
}
</script>