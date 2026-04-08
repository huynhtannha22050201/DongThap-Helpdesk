<template>
  <div class="max-w-3xl mx-auto px-4 py-8">
    <!-- Header -->
    <div class="text-center mb-8">
      <div
        class="w-14 h-14 rounded-2xl bg-[#DA251D]/10 flex items-center justify-center mx-auto mb-4"
      >
        <Search :size="26" class="text-[#DA251D]" />
      </div>
      <h1 class="text-slate-800 text-[24px] sm:text-[28px] font-bold mb-2">
        Tra Cứu Kết Quả Phản Ánh
      </h1>
      <p class="text-slate-500 text-[15px]">
        Nhập mã phản ánh hoặc số điện thoại để tra cứu tiến trình xử lý
      </p>
    </div>

    <!-- Search bar -->
    <div
      class="bg-white rounded-2xl shadow-sm border border-slate-100 p-5 mb-6"
    >
      <form
        @submit.prevent="handleSearch"
        class="flex flex-col sm:flex-row gap-3"
      >
        <div class="relative flex-1">
          <Search
            :size="18"
            class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400"
          />
          <input
            v-model="searchQuery"
            placeholder="Nhập mã phản ánh (VD: PA-032026-001) hoặc số điện thoại"
            class="w-full py-3.5 pl-12 pr-4 rounded-xl border border-slate-200 bg-white hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10 outline-none transition-all text-[15px]"
          />
        </div>
        <button
          type="submit"
          :disabled="!searchQuery.trim() || searching"
          class="flex items-center justify-center gap-2 px-6 py-3.5 bg-[#DA251D] text-white rounded-xl font-semibold hover:bg-[#b81f18] disabled:opacity-50 disabled:cursor-not-allowed transition-all cursor-pointer text-[15px] shrink-0"
        >
          <Loader2 v-if="searching" :size="18" class="animate-spin" />
          <Search v-else :size="18" />
          Tra cứu
        </button>
      </form>
    </div>

    <!-- Empty state (initial) -->
    <div
      v-if="!hasSearched"
      class="bg-white rounded-2xl shadow-sm border border-slate-100 p-12 text-center"
    >
      <div
        class="w-20 h-20 rounded-full bg-slate-100 flex items-center justify-center mx-auto mb-4"
      >
        <FileSearch :size="36" class="text-slate-300" />
      </div>
      <h3 class="text-slate-600 text-[16px] font-semibold mb-2">
        Nhập thông tin để bắt đầu tra cứu
      </h3>
      <p class="text-slate-400 text-[14px] max-w-md mx-auto leading-[1.6]">
        Bạn có thể tra cứu bằng mã phản ánh (ví dụ: PA-032026-001) hoặc số điện
        thoại đã đăng ký khi gửi phản ánh.
      </p>
    </div>

    <!-- Not found -->
    <div
      v-else-if="results.length === 0"
      class="bg-white rounded-2xl shadow-sm border border-slate-100 p-12 text-center"
    >
      <div
        class="w-20 h-20 rounded-full bg-red-50 flex items-center justify-center mx-auto mb-4"
      >
        <SearchX :size="36" class="text-red-300" />
      </div>
      <h3 class="text-slate-600 text-[16px] font-semibold mb-2">
        Không tìm thấy phản ánh nào
      </h3>
      <p class="text-slate-400 text-[14px] max-w-md mx-auto leading-[1.6]">
        Vui lòng kiểm tra lại mã phản ánh hoặc số điện thoại. Nếu bạn vừa gửi,
        hệ thống có thể cần vài phút để cập nhật.
      </p>
    </div>

    <!-- Results -->
    <div v-else class="space-y-4">
      <p class="text-slate-500 text-[14px]">
        Tìm thấy
        <span class="text-slate-800 font-semibold">{{ results.length }}</span>
        phản ánh
      </p>

      <div
        v-for="t in results"
        :key="t.code"
        class="bg-white rounded-2xl shadow-sm border border-slate-100 overflow-hidden hover:shadow-md transition-all"
      >
        <!-- Card header -->
        <div
          class="p-5 cursor-pointer"
          @click="expanded === t.code ? (expanded = null) : (expanded = t.code)"
        >
          <div class="flex items-start justify-between gap-3 mb-3">
            <div>
              <div class="flex items-center gap-2.5 flex-wrap mb-1.5">
                <span class="font-mono text-[#DA251D] text-[15px] font-bold">{{
                  t.code
                }}</span>
                <span
                  :class="[
                    'inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[12px] font-medium',
                    STATUS_CFG[t.status].bg,
                    STATUS_CFG[t.status].text,
                  ]"
                >
                  <span
                    :class="[
                      'w-1.5 h-1.5 rounded-full',
                      STATUS_CFG[t.status].dot,
                    ]"
                  />
                  {{ STATUS_CFG[t.status].label }}
                </span>
                <span
                  :class="[
                    'inline-flex items-center gap-1 px-2 py-0.5 rounded-full border text-[11px] font-medium',
                    PRIO_CFG[t.priority].cls,
                  ]"
                >
                  {{ PRIO_CFG[t.priority].label }}
                </span>
              </div>
              <h3 class="text-slate-800 text-[15px] font-medium leading-[1.5]">
                {{ t.title }}
              </h3>
            </div>
            <div class="text-slate-300 shrink-0 mt-1">
              <ChevronUp v-if="expanded === t.code" :size="20" />
              <ChevronDown v-else :size="20" />
            </div>
          </div>
          <div
            class="flex items-center gap-4 flex-wrap text-slate-400 text-[13px]"
          >
            <span class="flex items-center gap-1"
              ><Tag :size="13" /> {{ t.category }}</span
            >
            <span class="flex items-center gap-1"
              ><Calendar :size="13" /> {{ t.createdAt }}</span
            >
            <span v-if="t.department" class="flex items-center gap-1"
              ><Building2 :size="13" /> {{ t.department }}</span
            >
          </div>
        </div>

        <!-- Expanded detail -->
        <div
          v-if="expanded === t.code"
          class="border-t border-slate-100 p-5 space-y-5"
        >
          <!-- Info grid -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div
              v-for="info in [
                { icon: MapPin, label: 'Vị trí', value: t.location },
                {
                  icon: Building2,
                  label: 'Phòng ban xử lý',
                  value: t.department || 'Chưa phân công',
                },
                {
                  icon: User,
                  label: 'Cán bộ xử lý',
                  value: t.assignee || 'Chưa phân công',
                },
                { icon: Clock, label: 'Hạn xử lý SLA', value: t.slaDeadline },
              ]"
              :key="info.label"
              class="flex items-start gap-3"
            >
              <component
                :is="info.icon"
                :size="16"
                class="text-slate-400 mt-0.5 shrink-0"
              />
              <div>
                <p class="text-slate-400 text-[12px] font-medium">
                  {{ info.label }}
                </p>
                <p class="text-slate-700 text-[14px]">{{ info.value }}</p>
              </div>
            </div>
          </div>

          <!-- SLA Progress -->
          <div
            v-if="t.status !== 'Closed' && t.status !== 'Rejected'"
            class="bg-slate-50 rounded-xl p-4"
          >
            <div class="flex items-center justify-between mb-2">
              <span class="text-slate-500 text-[13px] font-medium"
                >Tiến trình SLA</span
              >
              <span
                :class="[t.slaBreached ? 'text-red-600' : 'text-emerald-600']"
                class="text-[13px] font-semibold"
              >
                {{ t.slaBreached ? "QUÁ HẠN" : t.slaRemaining }}
              </span>
            </div>
            <div class="w-full h-2.5 bg-slate-200 rounded-full overflow-hidden">
              <div
                :class="[
                  t.slaBreached
                    ? 'bg-red-500'
                    : t.slaPercent > 80
                      ? 'bg-amber-500'
                      : 'bg-emerald-500',
                ]"
                class="h-full rounded-full transition-all"
                :style="{ width: Math.min(t.slaPercent, 100) + '%' }"
              />
            </div>
          </div>

          <!-- Rejection reason -->
          <div
            v-if="t.status === 'Rejected' && t.rejectionReason"
            class="flex items-start gap-3 p-4 bg-red-50 border border-red-200 rounded-xl"
          >
            <AlertTriangle :size="18" class="text-red-500 shrink-0 mt-0.5" />
            <div>
              <p class="text-red-700 text-[14px] font-semibold mb-0.5">
                Lý do từ chối
              </p>
              <p class="text-red-600 text-[13px] leading-[1.6]">
                {{ t.rejectionReason }}
              </p>
            </div>
          </div>

          <!-- Description -->
          <div>
            <p class="text-slate-400 text-[12px] font-medium mb-1">
              Nội dung phản ánh
            </p>
            <p class="text-slate-700 text-[14px] leading-[1.7]">
              {{ t.description }}
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import {
  Search,
  FileSearch,
  SearchX,
  ChevronDown,
  ChevronUp,
  Tag,
  Calendar,
  Building2,
  MapPin,
  User,
  Clock,
  AlertTriangle,
  Loader2,
} from "lucide-vue-next";

const searchQuery = ref("");
const searching = ref(false);
const hasSearched = ref(false);
const results = ref([]);
const expanded = ref(null);

const STATUS_CFG = {
  New: {
    label: "Mới",
    bg: "bg-blue-50",
    text: "text-blue-700",
    dot: "bg-blue-500",
  },
  Assigned: {
    label: "Đã giao",
    bg: "bg-indigo-50",
    text: "text-indigo-700",
    dot: "bg-indigo-500",
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
  Closed: {
    label: "Đã đóng",
    bg: "bg-slate-100",
    text: "text-slate-600",
    dot: "bg-slate-400",
  },
  Rejected: {
    label: "Từ chối",
    bg: "bg-red-50",
    text: "text-red-600",
    dot: "bg-red-500",
  },
};
const PRIO_CFG = {
  High: { label: "Ưu tiên cao", cls: "bg-red-50 text-red-600 border-red-100" },
  Normal: {
    label: "Ưu tiên TB",
    cls: "bg-blue-50 text-blue-600 border-blue-100",
  },
  Low: {
    label: "Ưu tiên thấp",
    cls: "bg-slate-50 text-slate-500 border-slate-200",
  },
};

const MOCK_DATA = [
  {
    code: "PA-032026-001",
    title:
      "Ổ gà lớn trên đường Nguyễn Huệ, đoạn gần ngã tư Lê Lợi gây nguy hiểm cho người tham gia giao thông",
    status: "InProgress",
    priority: "High",
    category: "Giao thông",
    createdAt: "28/03/2026",
    location: "Đường Nguyễn Huệ, Phường 2, TP. Cao Lãnh",
    department: "Phòng Hạ tầng GT",
    assignee: "Lê Văn Bình",
    slaDeadline: "04/04/2026",
    slaRemaining: "Còn 2 ngày 4 giờ",
    slaPercent: 69,
    slaBreached: false,
    description:
      "Trên đường Nguyễn Huệ xuất hiện nhiều ổ gà lớn với đường kính khoảng 50-80cm, sâu 10-15cm. Đã có ít nhất 3 vụ tai nạn nhỏ trong tuần qua.",
    rejectionReason: null,
  },
  {
    code: "PA-032026-005",
    title: "Rác thải tập kết tại bãi đất trống trên đường Lê Duẩn gây mùi hôi",
    status: "Assigned",
    priority: "Normal",
    category: "Môi trường",
    createdAt: "25/03/2026",
    location: "Đường Lê Duẩn, Phường 2, TP. Cao Lãnh",
    department: "Phòng TN-MT",
    assignee: "Phạm Văn Hùng",
    slaDeadline: "01/04/2026",
    slaRemaining: "Còn 5 ngày",
    slaPercent: 35,
    slaBreached: false,
    description:
      "Bãi rác tự phát trên bãi đất trống, gây mùi hôi thối cho khu dân cư xung quanh.",
    rejectionReason: null,
  },
  {
    code: "PA-022026-012",
    title: "Đèn chiếu sáng hư hỏng đoạn đường Trần Phú dài 200m",
    status: "Resolved",
    priority: "Normal",
    category: "Hạ tầng đô thị",
    createdAt: "10/02/2026",
    location: "Đường Trần Phú, Phường 3, TP. Cao Lãnh",
    department: "Phòng Hạ tầng KT",
    assignee: "Nguyễn Hữu Toàn",
    slaDeadline: "17/02/2026",
    slaRemaining: "—",
    slaPercent: 100,
    slaBreached: false,
    description:
      "5 bóng đèn chiếu sáng trên đoạn đường dài 200m không hoạt động.",
    rejectionReason: null,
  },
  {
    code: "PA-012026-015",
    title: "Phản ánh về chất lượng nước sinh hoạt có mùi lạ",
    status: "Rejected",
    priority: "Normal",
    category: "Môi trường",
    createdAt: "10/01/2026",
    location: "Phường 4, TP. Cao Lãnh",
    department: "Phòng TN-MT",
    assignee: null,
    slaDeadline: "—",
    slaRemaining: "—",
    slaPercent: 0,
    slaBreached: false,
    description: "Nước máy có mùi clo nồng và đôi khi có màu vàng.",
    rejectionReason:
      "Qua kiểm tra, chỉ số chất lượng nước đạt chuẩn QCVN. Mùi clo nhẹ là bình thường trong quá trình xử lý nước. Khuyến nghị xả nước đầu nguồn trước khi sử dụng.",
  },
];

function handleSearch() {
  searching.value = true;
  hasSearched.value = true;
  expanded.value = null;

  setTimeout(() => {
    const q = searchQuery.value.trim().toLowerCase();
    if (q.startsWith("pa-")) {
      results.value = MOCK_DATA.filter((t) => t.code.toLowerCase().includes(q));
    } else if (/^0\d+$/.test(q)) {
      results.value = MOCK_DATA.slice(0, 3);
    } else {
      results.value = MOCK_DATA.filter(
        (t) =>
          t.title.toLowerCase().includes(q) || t.code.toLowerCase().includes(q),
      );
    }
    if (results.value.length === 1) expanded.value = results.value[0].code;
    searching.value = false;
  }, 800);
}
</script>
