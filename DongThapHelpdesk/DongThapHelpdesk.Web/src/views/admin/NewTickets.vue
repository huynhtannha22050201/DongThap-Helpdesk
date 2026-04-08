<template>
  <div class="space-y-6">
    <!-- Page Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-slate-800">
          Tiếp nhận phản ánh mới
        </h1>
        <p class="text-sm text-slate-500 mt-1">
          Có <strong class="text-red-600">{{ newTickets.length }}</strong> phản
          ánh đang chờ kiểm duyệt
        </p>
      </div>
      <!-- Toggle chế độ xem -->
      <div class="flex items-center gap-1 bg-slate-100 rounded-lg p-1">
        <button
          @click="viewMode = 'card'"
          class="p-2 rounded-md transition"
          :class="
            viewMode === 'card'
              ? 'bg-white shadow-sm text-red-600'
              : 'text-slate-500 hover:text-slate-700'
          "
        >
          <LayoutGrid :size="16" />
        </button>
        <button
          @click="viewMode = 'list'"
          class="p-2 rounded-md transition"
          :class="
            viewMode === 'list'
              ? 'bg-white shadow-sm text-red-600'
              : 'text-slate-500 hover:text-slate-700'
          "
        >
          <List :size="16" />
        </button>
      </div>
    </div>

    <!-- Trạng thái rỗng -->
    <div
      v-if="newTickets.length === 0"
      class="bg-white rounded-lg shadow-sm border border-slate-100 p-16 text-center"
    >
      <CheckCircle :size="48" class="mx-auto text-green-400 mb-4" />
      <h3 class="text-lg font-semibold text-slate-700">
        Không có phản ánh mới
      </h3>
      <p class="text-sm text-slate-500 mt-1">
        Tất cả phản ánh đã được kiểm duyệt
      </p>
    </div>

    <!-- Chế độ xem Card (mặc định) -->
    <div
      v-if="viewMode === 'card' && newTickets.length > 0"
      class="grid grid-cols-1 lg:grid-cols-2 gap-4 items-stretch"
    >
      <div
        v-for="ticket in newTickets"
        :key="ticket.id"
        class="bg-white rounded-lg shadow-sm border border-slate-100 overflow-hidden hover:shadow-md transition flex flex-col"
      >
        <!-- Header card -->
        <div class="flex items-start justify-between p-4 pb-3">
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 mb-1">
              <span class="text-sm font-bold text-red-600">{{
                ticket.ticketCode
              }}</span>
              <span
                class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-blue-50 text-blue-700"
              >
                Mới
              </span>
            </div>
            <h3 class="text-base font-semibold text-slate-800 truncate">
              {{ ticket.title }}
            </h3>
          </div>
          <span class="text-xs text-slate-400 whitespace-nowrap ml-2">{{
            timeAgo(ticket.createdAt)
          }}</span>
        </div>

        <!-- Mô tả -->
        <div class="px-4 pb-3">
          <p class="text-sm text-slate-600 line-clamp-2">
            {{ ticket.description }}
          </p>
        </div>

        <!-- Thông tin chi tiết -->
        <div class="px-4 pb-3 grid grid-cols-2 gap-3">
          <div class="flex items-center gap-2 text-sm">
            <Tag :size="14" class="text-slate-400" />
            <span class="text-slate-600">{{ ticket.categoryName }}</span>
          </div>
          <div class="flex items-center gap-2 text-sm">
            <MapPin :size="14" class="text-slate-400" />
            <span class="text-slate-600 truncate">{{
              ticket.address || "Chưa có vị trí"
            }}</span>
          </div>
          <div class="flex items-center gap-2 text-sm">
            <User :size="14" class="text-slate-400" />
            <span class="text-slate-600">{{ ticket.reporterName }}</span>
          </div>
          <div class="flex items-center gap-2 text-sm">
            <Phone :size="14" class="text-slate-400" />
            <span class="text-slate-600">{{ ticket.reporterPhone }}</span>
          </div>
        </div>

        <!-- Ảnh đính kèm — chỉ hiện khi có ảnh -->
        <div
          v-if="ticket.attachments?.length > 0"
          class="px-4 pb-3 flex gap-2 overflow-x-auto"
        >
          <div
            v-for="(img, idx) in ticket.attachments.slice(0, 3)"
            :key="idx"
            class="w-20 h-20 rounded-lg bg-slate-100 border border-slate-200 flex-shrink-0 flex items-center justify-center overflow-hidden"
          >
            <ImageIcon :size="20" class="text-slate-400" />
          </div>
          <div
            v-if="ticket.attachments.length > 3"
            class="w-20 h-20 rounded-lg bg-slate-50 border border-slate-200 flex-shrink-0 flex items-center justify-center"
          >
            <span class="text-sm text-slate-500 font-medium"
              >+{{ ticket.attachments.length - 3 }}</span
            >
          </div>
        </div>

        <!-- Action buttons — chỉ Dispatcher được thao tác -->
        <div
          class="flex items-center gap-2 p-4 pt-3 border-t border-slate-100 bg-slate-50/50 mt-auto"
        >
          <template v-if="canAction">
            <button
              @click="openRejectModal(ticket)"
              class="flex-1 flex items-center justify-center gap-1.5 px-4 py-2.5 border border-slate-200 rounded-xl text-sm font-medium text-slate-700 bg-white hover:bg-slate-50 transition"
            >
              <XCircle :size="16" class="text-slate-500" />
              Từ chối
            </button>
            <button
              @click="openApproveModal(ticket)"
              class="flex-1 flex items-center justify-center gap-1.5 px-4 py-2.5 rounded-xl text-sm font-medium text-white bg-red-600 hover:bg-red-700 shadow-sm transition"
            >
              <CheckCircle :size="16" />
              Duyệt & Giao việc
            </button>
          </template>
          <!-- Admin/Manager chỉ xem -->
          <div v-else class="w-full text-center py-1">
            <span class="text-xs text-slate-400 italic"
              >Chỉ Dispatcher được duyệt phản ánh</span
            >
          </div>
        </div>
      </div>
    </div>

    <!-- Chế độ xem List -->
    <div
      v-if="viewMode === 'list' && newTickets.length > 0"
      class="bg-white rounded-lg shadow-sm border border-slate-100 overflow-hidden"
    >
      <div class="divide-y divide-slate-100">
        <div
          v-for="ticket in newTickets"
          :key="ticket.id"
          class="flex items-center gap-4 p-4 hover:bg-slate-50/50 transition"
        >
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 mb-1">
              <span class="text-sm font-bold text-red-600">{{
                ticket.ticketCode
              }}</span>
              <span class="text-xs text-slate-400">{{
                timeAgo(ticket.createdAt)
              }}</span>
            </div>
            <h3 class="text-sm font-semibold text-slate-800 truncate">
              {{ ticket.title }}
            </h3>
            <p class="text-xs text-slate-500 mt-0.5">
              {{ ticket.categoryName }} · {{ ticket.reporterName }} ·
              {{ ticket.reporterPhone }}
            </p>
          </div>
          <div class="flex items-center gap-2 flex-shrink-0">
            <template v-if="canAction">
              <button
                @click="openRejectModal(ticket)"
                class="px-3 py-2 border border-slate-200 rounded-xl text-sm font-medium text-slate-600 bg-white hover:bg-slate-50 transition"
              >
                Từ chối
              </button>
              <button
                @click="openApproveModal(ticket)"
                class="px-3 py-2 rounded-xl text-sm font-medium text-white bg-red-600 hover:bg-red-700 shadow-sm transition"
              >
                Duyệt
              </button>
            </template>
            <span v-else class="text-xs text-slate-400 italic">Chỉ xem</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Duyệt phản ánh -->
    <div
      v-if="approveModal.show"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="approveModal.show = false"
    >
      <div class="bg-white rounded-lg shadow-xl w-full max-w-md mx-4">
        <div
          class="flex items-center justify-between p-4 border-b border-slate-100"
        >
          <h3 class="text-lg font-semibold text-slate-800">Duyệt phản ánh</h3>
          <button
            @click="approveModal.show = false"
            class="p-1 text-slate-400 hover:text-slate-600"
          >
            <X :size="20" />
          </button>
        </div>
        <div class="p-4 space-y-4">
          <div>
            <p class="text-sm text-slate-500 mb-1">Phản ánh</p>
            <p class="text-sm font-semibold text-slate-800">
              {{ approveModal.ticket?.ticketCode }} —
              {{ approveModal.ticket?.title }}
            </p>
          </div>

          <!-- Chọn mức ưu tiên — PUT /api/tickets/{id}/approve -->
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-2"
              >Mức độ ưu tiên</label
            >
            <div class="grid grid-cols-4 gap-2">
              <button
                v-for="p in priorityOptions"
                :key="p.value"
                @click="approveModal.priority = p.value"
                class="px-3 py-2 rounded-lg text-sm font-medium border transition"
                :class="
                  approveModal.priority === p.value
                    ? 'border-red-300 bg-red-50 text-red-700'
                    : 'border-slate-200 text-slate-600 hover:border-slate-300'
                "
              >
                {{ p.label }}
              </button>
            </div>
          </div>

          <!-- Chọn đơn vị giao việc — PUT /api/tickets/{id}/assign -->
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1"
              >Giao cho đơn vị</label
            >
            <select
              v-model="approveModal.departmentId"
              class="w-full px-3 py-2.5 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 bg-white"
            >
              <option value="">— Chọn đơn vị —</option>
              <option
                v-for="dept in departments"
                :key="dept.id"
                :value="dept.id"
              >
                {{ dept.name }}
              </option>
            </select>
          </div>

          <!-- Ghi chú -->
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1"
              >Ghi chú (tuỳ chọn)</label
            >
            <textarea
              v-model="approveModal.note"
              rows="2"
              placeholder="Ghi chú khi giao việc..."
              class="w-full px-3 py-2 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 resize-none"
            ></textarea>
          </div>
        </div>
        <div
          class="flex items-center justify-end gap-2 p-4 border-t border-slate-100 bg-slate-50/50 rounded-b-lg"
        >
          <button
            @click="approveModal.show = false"
            class="px-4 py-2.5 text-sm font-medium text-slate-600 hover:text-slate-800 transition"
          >
            Huỷ
          </button>
          <button
            @click="confirmApprove"
            :disabled="!approveModal.departmentId"
            class="px-5 py-2.5 rounded-xl text-sm font-medium text-white bg-red-600 hover:bg-red-700 shadow-sm disabled:opacity-50 disabled:cursor-not-allowed transition"
          >
            Xác nhận duyệt
          </button>
        </div>
      </div>
    </div>

    <!-- Modal Từ chối phản ánh -->
    <div
      v-if="rejectModal.show"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="rejectModal.show = false"
    >
      <div class="bg-white rounded-lg shadow-xl w-full max-w-md mx-4">
        <div
          class="flex items-center justify-between p-4 border-b border-slate-100"
        >
          <h3 class="text-lg font-semibold text-slate-800">Từ chối phản ánh</h3>
          <button
            @click="rejectModal.show = false"
            class="p-1 text-slate-400 hover:text-slate-600"
          >
            <X :size="20" />
          </button>
        </div>
        <div class="p-4 space-y-4">
          <div>
            <p class="text-sm text-slate-500 mb-1">Phản ánh</p>
            <p class="text-sm font-semibold text-slate-800">
              {{ rejectModal.ticket?.ticketCode }} —
              {{ rejectModal.ticket?.title }}
            </p>
          </div>

          <!-- Lý do từ chối — PUT /api/tickets/{id}/reject -->
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1"
              >Lý do từ chối <span class="text-red-500">*</span></label
            >
            <textarea
              v-model="rejectModal.reason"
              rows="3"
              placeholder="Nhập lý do từ chối phản ánh..."
              class="w-full px-3 py-2 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 resize-none"
            ></textarea>
          </div>

          <!-- Lý do nhanh -->
          <div class="flex flex-wrap gap-2">
            <button
              v-for="reason in quickRejectReasons"
              :key="reason"
              @click="rejectModal.reason = reason"
              class="px-3 py-1.5 text-xs font-medium rounded-full border border-slate-200 text-slate-600 hover:border-red-200 hover:text-red-600 hover:bg-red-50 transition"
            >
              {{ reason }}
            </button>
          </div>
        </div>
        <div
          class="flex items-center justify-end gap-2 p-4 border-t border-slate-100 bg-slate-50/50 rounded-b-lg"
        >
          <button
            @click="rejectModal.show = false"
            class="px-4 py-2.5 text-sm font-medium text-slate-600 hover:text-slate-800 transition"
          >
            Huỷ
          </button>
          <button
            @click="confirmReject"
            :disabled="!rejectModal.reason.trim()"
            class="px-5 py-2.5 rounded-xl text-sm font-medium text-white bg-red-600 hover:bg-red-700 shadow-sm disabled:opacity-50 disabled:cursor-not-allowed transition"
          >
            Xác nhận từ chối
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { useAuthStore } from "@/stores/auth";
import {
  LayoutGrid,
  List,
  CheckCircle,
  XCircle,
  X,
  Tag,
  MapPin,
  User,
  Phone,
  ImageIcon,
} from "lucide-vue-next";

const auth = useAuthStore();
// Chỉ Dispatcher được duyệt/từ chối — Admin chỉ xem
const canAction = computed(() => ["Dispatcher"].includes(auth.userRole));

const viewMode = ref("card");

const priorityOptions = [
  { value: "Low", label: "Thấp" },
  { value: "Normal", label: "Bình thường" },
  { value: "High", label: "Cao" },
  { value: "Urgent", label: "Khẩn cấp" },
];

// Mock danh sách đơn vị — sẽ thay bằng GET /api/departments
const departments = ref([
  { id: "dept1", name: "Phòng Hạ tầng Kỹ thuật" },
  { id: "dept2", name: "Phòng Tài nguyên & Môi trường" },
  { id: "dept3", name: "Công an TP. Cao Lãnh" },
  { id: "dept4", name: "Công ty Cấp nước Đồng Tháp" },
  { id: "dept5", name: "UBND Phường 1" },
]);

// Lý do từ chối nhanh giúp Dispatcher thao tác nhanh hơn
const quickRejectReasons = [
  "Không có hình ảnh minh chứng",
  "Nội dung không rõ ràng",
  "Trùng lặp phản ánh đã có",
  "Không thuộc phạm vi tiếp nhận",
  "Thông tin người gửi không hợp lệ",
];

// ── Mock data — sẽ thay bằng GET /api/tickets?status=New ──
const allTickets = ref([
  {
    id: "10",
    ticketCode: "PA-20260407-001",
    title: "Ổ gà nguy hiểm tại ngã ba Trần Phú",
    description:
      "Ổ gà rộng khoảng 50cm, sâu 15cm ngay giữa đường. Nhiều xe máy đã bị ngã tại đây vào buổi tối do không có đèn chiếu sáng.",
    categoryName: "Hạ tầng giao thông",
    status: "New",
    reporterName: "Nguyễn Văn An",
    reporterPhone: "0901234567",
    address: "Ngã ba Trần Phú, Phường 1, TP. Cao Lãnh",
    attachments: [{ url: "" }, { url: "" }],
    createdAt: "2026-04-07T08:30:00",
  },
  {
    id: "11",
    ticketCode: "PA-20260407-002",
    title: "Rác thải sinh hoạt đổ trộm bên bờ kênh",
    description:
      "Có người đổ trộm rác thải sinh hoạt số lượng lớn bên bờ kênh Nguyễn Văn Tiếp, gây ô nhiễm nguồn nước và mùi hôi thối.",
    categoryName: "Môi trường",
    status: "New",
    reporterName: "Trần Thị Bình",
    reporterPhone: "0912345678",
    address: "Bờ kênh Nguyễn Văn Tiếp, Xã Mỹ Trà",
    attachments: [{ url: "" }, { url: "" }, { url: "" }, { url: "" }],
    createdAt: "2026-04-07T10:15:00",
  },
  {
    id: "12",
    ticketCode: "PA-20260407-003",
    title: "Quán nhậu mở nhạc lớn đến khuya",
    description:
      'Quán nhậu "Hai Lúa" hoạt động đến 1-2h sáng, mở nhạc loa kéo rất lớn ảnh hưởng đến giấc ngủ của cư dân xung quanh.',
    categoryName: "An ninh trật tự",
    status: "New",
    reporterName: "Lê Minh Cường",
    reporterPhone: "0923456789",
    address: "Đường 30/4, Phường 2, TP. Cao Lãnh",
    attachments: [],
    createdAt: "2026-04-07T22:45:00",
  },
  {
    id: "13",
    ticketCode: "PA-20260408-001",
    title: "Cống thoát nước bị vỡ gây ngập đường",
    description:
      "Cống thoát nước trên đường Lê Duẩn bị vỡ, nước thải tràn ra đường gây ngập khoảng 30cm mỗi khi mưa. Đường rất trơn trượt nguy hiểm.",
    categoryName: "Hạ tầng giao thông",
    status: "New",
    reporterName: "Phạm Thị Dung",
    reporterPhone: "0934567890",
    address: "Đường Lê Duẩn, Phường 4, TP. Cao Lãnh",
    attachments: [{ url: "" }],
    createdAt: "2026-04-08T06:00:00",
  },
]);

// Chỉ hiện phản ánh có trạng thái "New"
const newTickets = computed(() =>
  allTickets.value.filter((t) => t.status === "New"),
);

// ── Modal Duyệt ─────────────────────────────────────────
const approveModal = ref({
  show: false,
  ticket: null,
  priority: "Normal",
  departmentId: "",
  note: "",
});

function openApproveModal(ticket) {
  approveModal.value = {
    show: true,
    ticket,
    priority: "Normal",
    departmentId: "",
    note: "",
  };
}

function confirmApprove() {
  // TODO: gọi PUT /api/tickets/{id}/approve rồi PUT /api/tickets/{id}/assign
  const ticket = approveModal.value.ticket;
  allTickets.value = allTickets.value.filter((t) => t.id !== ticket.id);
  approveModal.value.show = false;
}

// ── Modal Từ chối ────────────────────────────────────────
const rejectModal = ref({
  show: false,
  ticket: null,
  reason: "",
});

function openRejectModal(ticket) {
  rejectModal.value = { show: true, ticket, reason: "" };
}

function confirmReject() {
  // TODO: gọi PUT /api/tickets/{id}/reject
  const ticket = rejectModal.value.ticket;
  allTickets.value = allTickets.value.filter((t) => t.id !== ticket.id);
  rejectModal.value.show = false;
}

// Helper: tính thời gian trước
function timeAgo(dateStr) {
  const now = new Date();
  const d = new Date(dateStr);
  const diffMs = now - d;
  const diffMins = Math.floor(diffMs / 60000);
  if (diffMins < 60) return `${diffMins} phút trước`;
  const diffHours = Math.floor(diffMins / 60);
  if (diffHours < 24) return `${diffHours} giờ trước`;
  const diffDays = Math.floor(diffHours / 24);
  return `${diffDays} ngày trước`;
}
</script>
