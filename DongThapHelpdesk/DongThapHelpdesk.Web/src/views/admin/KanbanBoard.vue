<template>
  <div class="flex flex-col h-[calc(100vh-56px)]">
    <!-- PAGE HEADER -->
    <div class="shrink-0 px-1 pb-4">
      <div
        class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3 mb-4"
      >
        <div>
          <h1
            class="text-slate-800 font-semibold"
            style="font-size: clamp(22px, 3vw, 26px)"
          >
            Bảng Kanban
          </h1>
          <p class="text-slate-500 mt-0.5 text-[14px]">
            Quản lý luồng xử lý phản ánh — {{ totalCount }} phản ánh
            <span v-if="breachedCount > 0" class="text-red-500 ml-2 font-medium"
              >• {{ breachedCount }} quá hạn</span
            >
          </p>
        </div>
        <div class="flex items-center gap-2">
          <button
            class="p-2.5 rounded-lg border border-slate-200 bg-white text-slate-500 hover:bg-slate-50 hover:text-slate-700 transition-colors cursor-pointer"
          >
            <RefreshCw :size="16" />
          </button>
        </div>
      </div>

      <!-- TOOLBAR -->
      <div
        class="bg-white rounded-xl shadow-sm border border-slate-100 px-4 py-3"
      >
        <div
          class="flex flex-col lg:flex-row gap-3 items-start lg:items-center"
        >
          <div class="relative flex-1 min-w-0 w-full lg:max-w-[300px]">
            <Search
              :size="16"
              class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
            />
            <input
              v-model="search"
              type="text"
              placeholder="Tìm theo mã PA hoặc tiêu đề..."
              class="w-full pl-9 pr-4 py-2 rounded-lg border border-slate-200 bg-white hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10 outline-none transition-all text-[13px]"
            />
          </div>
          <div class="flex flex-wrap items-center gap-2">
            <FilterDropdown
              label="Lĩnh vực"
              :icon="Tag"
              :options="categoryOptions"
              v-model="catFilter"
            />
            <FilterDropdown
              label="Phòng ban"
              :icon="Building2"
              :options="deptOptions"
              v-model="deptFilter"
            />
            <FilterDropdown
              label="Mức độ"
              :icon="CircleDot"
              :options="prioOptions"
              v-model="prioFilter"
            />
            <button
              @click="slaOnly = !slaOnly"
              :class="[
                'flex items-center gap-1.5 px-3 py-2 rounded-lg border transition-all cursor-pointer text-[13px] font-medium',
                slaOnly
                  ? 'bg-red-50 border-red-200 text-red-600'
                  : 'bg-white border-slate-200 text-slate-500 hover:border-slate-300',
              ]"
            >
              <AlertTriangle :size="14" /> Quá hạn SLA
            </button>
            <button
              v-if="hasFilters"
              @click="clearFilters"
              class="flex items-center gap-1 px-2.5 py-2 text-slate-400 hover:text-slate-600 transition-colors cursor-pointer text-[13px]"
            >
              <X :size="14" /> Xóa lọc
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- KANBAN BOARD -->
    <div class="flex-1 overflow-x-auto overflow-y-hidden pb-4 px-1">
      <div class="flex gap-4 h-full min-w-max lg:min-w-0">
        <div
          v-for="col in columns"
          :key="col.key"
          class="flex flex-col min-w-[280px] w-[280px] lg:w-auto lg:flex-1 shrink-0"
        >
          <!-- Column header -->
          <div class="flex items-center justify-between mb-3">
            <div class="flex items-center gap-2">
              <div
                :class="[
                  'w-7 h-7 rounded-lg text-white flex items-center justify-center',
                  col.accentBg,
                ]"
              >
                <component :is="col.icon" :size="15" />
              </div>
              <h3 :class="[col.accent]" class="text-[14px] font-semibold">
                {{ col.label }}
              </h3>
              <span
                class="px-1.5 py-0.5 rounded-full bg-slate-100 text-slate-500 text-[12px] font-semibold"
              >
                {{ getFilteredTickets(col.key).length }}
              </span>
            </div>
            <button
              class="text-slate-300 hover:text-slate-500 transition-colors cursor-pointer p-1"
            >
              <MoreHorizontal :size="16" />
            </button>
          </div>

          <!-- Drop zone -->
          <draggable
            :list="tickets[col.key]"
            group="kanban"
            item-key="id"
            :class="[
              'flex-1 rounded-xl p-2 space-y-2.5 transition-all min-h-[200px] bg-slate-50/50 border border-transparent',
            ]"
            ghost-class="opacity-40"
            drag-class="rotate-[-2deg] shadow-xl scale-[1.03] ring-2 ring-[#DA251D]/20"
            @change="onDragChange($event, col.key)"
          >
            <template #item="{ element }">
              <TicketCard
                v-show="isTicketVisible(element)"
                :ticket="element"
                @click="
                  $router.push({
                    name: 'TicketDetail',
                    params: { id: element.id },
                  })
                "
              />
            </template>
            <template #footer>
              <div
                v-if="getFilteredTickets(col.key).length === 0"
                class="flex flex-col items-center justify-center py-10 text-center"
              >
                <div
                  class="w-10 h-10 rounded-full bg-slate-100 flex items-center justify-center mb-2"
                >
                  <Archive :size="18" class="text-slate-300" />
                </div>
                <p class="text-slate-400 text-[13px]">Trống</p>
              </div>
            </template>
          </draggable>
        </div>
      </div>
    </div>

    <!-- REJECTED SECTION -->
    <div class="shrink-0 px-1 pb-2">
      <div
        class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden mt-5"
      >
        <button
          @click="rejectedExpanded = !rejectedExpanded"
          class="w-full flex items-center justify-between px-5 py-3.5 hover:bg-slate-50/50 transition-colors cursor-pointer"
        >
          <div class="flex items-center gap-2.5">
            <div
              class="w-7 h-7 rounded-lg bg-red-500 text-white flex items-center justify-center"
            >
              <Ban :size="15" />
            </div>
            <h3 class="text-red-600 text-[14px] font-semibold">Đã từ chối</h3>
            <span
              class="px-1.5 py-0.5 rounded-full bg-red-100 text-red-600 text-[12px] font-semibold"
            >
              {{ rejectedTickets.length }}
            </span>
          </div>
          <ChevronDown
            v-if="rejectedExpanded"
            :size="18"
            class="text-slate-400"
          />
          <ChevronRight v-else :size="18" class="text-slate-400" />
        </button>
        <div
          v-if="rejectedExpanded"
          class="px-5 pb-4 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3"
        >
          <div
            v-for="t in rejectedTickets"
            :key="t.id"
            class="bg-red-50/60 rounded-lg border border-red-100 p-3.5"
          >
            <div class="flex items-center justify-between mb-2">
              <span class="text-slate-400 font-mono text-[12px] font-medium">{{
                t.code
              }}</span>
              <span
                class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-red-100 text-red-600 text-[11px] font-medium"
              >
                <XCircle :size="11" /> Từ chối
              </span>
            </div>
            <p
              class="text-slate-700 mb-2 line-clamp-2 text-[13px] font-medium leading-[1.5]"
            >
              {{ t.title }}
            </p>
            <div class="flex items-start gap-2 p-2 bg-red-100/60 rounded-md">
              <AlertTriangle :size="13" class="text-red-400 mt-0.5 shrink-0" />
              <p class="text-red-600 text-[12px] leading-[1.5]">
                {{ t.rejectionReason }}
              </p>
            </div>
            <div
              class="flex items-center justify-between mt-2.5 pt-2 border-t border-red-100/60"
            >
              <div class="flex items-center gap-1.5">
                <div
                  class="w-5 h-5 rounded-full bg-red-100 flex items-center justify-center text-red-500 text-[9px] font-semibold"
                >
                  {{ t.reporterInitials }}
                </div>
                <span class="text-slate-500 text-[11px]">{{ t.reporter }}</span>
              </div>
              <span class="text-slate-400 text-[11px]">{{ t.createdAt }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from "vue";
import { useRouter } from "vue-router";
import draggable from "vuedraggable";
import {
  Search,
  Tag,
  Building2,
  CircleDot,
  AlertTriangle,
  X,
  RefreshCw,
  MoreHorizontal,
  Archive,
  Ban,
  ChevronDown,
  ChevronRight,
  XCircle,
  Clock,
  UserPlus,
  Play,
  CheckCircle2,
} from "lucide-vue-next";

const router = useRouter();

// ── Filter Dropdown sub-component ──
const FilterDropdown = {
  props: ["label", "icon", "options", "modelValue"],
  emits: ["update:modelValue"],
  template: `
    <div class="relative">
      <select
        :value="modelValue"
        @change="$emit('update:modelValue', $event.target.value)"
        class="appearance-none pl-8 pr-8 py-2 rounded-lg border border-slate-200 bg-white text-slate-600 hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10 outline-none transition-all cursor-pointer text-[13px]"
      >
        <option value="">{{ label }}</option>
        <option v-for="o in options" :key="o.value" :value="o.value">{{ o.label }}</option>
      </select>
      <component :is="icon" :size="14" class="absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
    </div>
  `,
};

// ── Ticket Card sub-component ──
const TicketCard = {
  props: ["ticket"],
  emits: ["click"],
  setup(props, { emit }) {
    return { Clock, Tag, Building2, AlertTriangle, MoreHorizontal };
  },
  template: `
    <div
      @click="$emit('click')"
      :class="[
        'bg-white rounded-lg border shadow-sm transition-all cursor-grab active:cursor-grabbing group hover:shadow-md hover:border-slate-200',
        ticket.slaBreached ? 'border-l-[4px] border-l-red-500 border-t-slate-100 border-r-slate-100 border-b-slate-100' : 'border-slate-100'
      ]"
    >
      <div class="p-3.5">
        <div class="flex items-center justify-between mb-2">
          <span class="text-slate-400 font-mono text-[12px] font-medium">{{ ticket.code }}</span>
          <div class="flex items-center gap-2">
            <div class="flex items-center gap-1.5">
              <div :class="['w-2 h-2 rounded-full ring-2', priorityCfg.color, priorityCfg.ring]" />
              <span class="text-slate-400 text-[11px]">{{ priorityCfg.label }}</span>
            </div>
            <button class="text-slate-300 hover:text-slate-500 opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer p-0.5">
              <MoreHorizontal :size="14" />
            </button>
          </div>
        </div>
        <p class="text-slate-800 mb-2.5 line-clamp-2 text-[13px] font-medium leading-[1.5]">{{ ticket.title }}</p>
        <div class="flex flex-wrap gap-1.5 mb-3">
          <span :class="['inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-medium', ticket.categoryColor]">
            <Tag :size="10" /> {{ ticket.category }}
          </span>
          <span v-if="ticket.department" class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-slate-100 text-slate-600 text-[11px] font-medium">
            <Building2 :size="10" /> {{ ticket.department }}
          </span>
        </div>
        <div class="flex items-center justify-between pt-2.5 border-t border-slate-50">
          <div class="flex items-center gap-2">
            <div class="w-6 h-6 rounded-full bg-slate-100 flex items-center justify-center text-slate-500 shrink-0 text-[10px] font-semibold">
              {{ ticket.reporterInitials }}
            </div>
            <span class="text-slate-500 truncate max-w-[90px] text-[12px]">{{ ticket.reporter }}</span>
          </div>
          <span v-if="ticket.slaBreached" class="inline-flex items-center gap-1 px-1.5 py-0.5 rounded bg-red-100 text-red-600 text-[10px] font-semibold">
            <AlertTriangle :size="10" /> QUÁ HẠN
          </span>
          <span v-else-if="ticket.slaRemaining !== '—'" class="inline-flex items-center gap-1 text-slate-400 text-[11px]">
            <Clock :size="11" /> {{ ticket.slaRemaining }}
          </span>
          <span v-else class="text-slate-300 text-[11px]">—</span>
        </div>
      </div>
    </div>
  `,
  computed: {
    priorityCfg() {
      const cfg = {
        High: { color: "bg-red-500", ring: "ring-red-200", label: "Cao" },
        Normal: { color: "bg-blue-500", ring: "ring-blue-200", label: "TB" },
        Low: { color: "bg-slate-400", ring: "ring-slate-200", label: "Thấp" },
      };
      return cfg[this.ticket.priority] || cfg.Normal;
    },
  },
};

// ── Column config ──
const columns = [
  {
    key: "New",
    label: "Mới",
    accent: "text-blue-600",
    accentBg: "bg-blue-500",
    icon: CircleDot,
  },
  {
    key: "Assigned",
    label: "Đã giao",
    accent: "text-indigo-600",
    accentBg: "bg-indigo-500",
    icon: UserPlus,
  },
  {
    key: "InProgress",
    label: "Đang xử lý",
    accent: "text-amber-600",
    accentBg: "bg-amber-500",
    icon: Play,
  },
  {
    key: "Resolved",
    label: "Đã xử lý",
    accent: "text-emerald-600",
    accentBg: "bg-emerald-500",
    icon: CheckCircle2,
  },
  {
    key: "Closed",
    label: "Đã đóng",
    accent: "text-slate-500",
    accentBg: "bg-slate-400",
    icon: Archive,
  },
];

// ── Ticket data ──
const tickets = reactive({
  New: [
    {
      id: "t1",
      code: "PA-042026-001",
      title:
        "Ổ gà lớn trên đường Nguyễn Huệ, đoạn gần ngã tư Lê Lợi gây nguy hiểm",
      category: "Giao thông",
      categoryColor: "bg-blue-100 text-blue-700",
      priority: "High",
      reporter: "Nguyễn Văn A",
      reporterInitials: "NA",
      slaRemaining: "6 ngày",
      slaBreached: false,
      createdAt: "02/04",
      status: "New",
    },
    {
      id: "t2",
      code: "PA-042026-002",
      title: "Rác thải sinh hoạt tràn ngập khu vực chợ Cao Lãnh",
      category: "Môi trường",
      categoryColor: "bg-emerald-100 text-emerald-700",
      priority: "Normal",
      reporter: "Trần Thị B",
      reporterInitials: "TB",
      slaRemaining: "5 ngày",
      slaBreached: false,
      createdAt: "02/04",
      status: "New",
    },
    {
      id: "t3",
      code: "PA-042026-003",
      title: "Đèn tín hiệu giao thông hư hỏng tại ngã tư Trần Phú",
      category: "Giao thông",
      categoryColor: "bg-blue-100 text-blue-700",
      priority: "High",
      reporter: "Lê Minh C",
      reporterInitials: "MC",
      slaRemaining: "4 ngày",
      slaBreached: false,
      createdAt: "01/04",
      status: "New",
    },
    {
      id: "t4",
      code: "PA-042026-004",
      title: "Tiếng ồn từ công trình xây dựng ngoài giờ quy định",
      category: "An ninh",
      categoryColor: "bg-violet-100 text-violet-700",
      priority: "Low",
      reporter: "Phạm Thị D",
      reporterInitials: "TD",
      slaRemaining: "7 ngày",
      slaBreached: false,
      createdAt: "01/04",
      status: "New",
    },
  ],
  Assigned: [
    {
      id: "t5",
      code: "PA-032026-012",
      title: "Vỉa hè bị lấn chiếm buôn bán trên đường Lê Duẩn, Phường 1",
      category: "Giao thông",
      categoryColor: "bg-blue-100 text-blue-700",
      priority: "Normal",
      reporter: "Hoàng Văn E",
      reporterInitials: "VE",
      department: "Phòng Đô thị",
      assignee: "Nguyễn Hữu Toàn",
      slaRemaining: "3 ngày",
      slaBreached: false,
      createdAt: "31/03",
      status: "Assigned",
    },
    {
      id: "t6",
      code: "PA-032026-011",
      title: "Cây xanh ngã đổ chắn ngang đường Tôn Đức Thắng sau bão",
      category: "Hạ tầng",
      categoryColor: "bg-amber-100 text-amber-700",
      priority: "High",
      reporter: "Đỗ Thanh F",
      reporterInitials: "TF",
      department: "Phòng Hạ tầng",
      assignee: "Lê Văn Bình",
      slaRemaining: "1 ngày",
      slaBreached: false,
      createdAt: "30/03",
      status: "Assigned",
    },
  ],
  InProgress: [
    {
      id: "t7",
      code: "PA-032026-010",
      title: "Mất nước sinh hoạt kéo dài 3 ngày tại KDC Mỹ Trà",
      category: "Hạ tầng",
      categoryColor: "bg-amber-100 text-amber-700",
      priority: "High",
      reporter: "Vũ Thị G",
      reporterInitials: "TG",
      department: "Cty Cấp nước",
      assignee: "Trần Minh Quân",
      slaRemaining: "Quá hạn",
      slaBreached: true,
      createdAt: "29/03",
      status: "InProgress",
    },
    {
      id: "t8",
      code: "PA-032026-009",
      title: "Rác thải công nghiệp đổ trộm ven sông Tiền, xã An Bình",
      category: "Môi trường",
      categoryColor: "bg-emerald-100 text-emerald-700",
      priority: "High",
      reporter: "Bùi Văn H",
      reporterInitials: "VH",
      department: "Phòng TN-MT",
      assignee: "Phạm Văn Hùng",
      slaRemaining: "2 ngày",
      slaBreached: false,
      createdAt: "28/03",
      status: "InProgress",
    },
    {
      id: "t9",
      code: "PA-032026-008",
      title: "Đường dây điện hạ thế bị đứt, rủi ro điện giật",
      category: "Hạ tầng",
      categoryColor: "bg-amber-100 text-amber-700",
      priority: "High",
      reporter: "Ngô Quốc I",
      reporterInitials: "QI",
      department: "Điện lực ĐT",
      assignee: "Võ Hoàng Nam",
      slaRemaining: "Quá hạn",
      slaBreached: true,
      createdAt: "27/03",
      status: "InProgress",
    },
  ],
  Resolved: [
    {
      id: "t10",
      code: "PA-032026-007",
      title: "Cầu gỗ xuống cấp nguy hiểm tại xã Mỹ Tân",
      category: "Giao thông",
      categoryColor: "bg-blue-100 text-blue-700",
      priority: "High",
      reporter: "Lý Thị K",
      reporterInitials: "TK",
      department: "Phòng Hạ tầng GT",
      slaRemaining: "—",
      slaBreached: false,
      createdAt: "25/03",
      status: "Resolved",
    },
    {
      id: "t11",
      code: "PA-032026-006",
      title: "Ngập nước kéo dài trên đường Nguyễn Thái Học sau mưa lớn",
      category: "Hạ tầng",
      categoryColor: "bg-amber-100 text-amber-700",
      priority: "Normal",
      reporter: "Mai Văn L",
      reporterInitials: "VL",
      department: "Phòng Hạ tầng",
      slaRemaining: "—",
      slaBreached: false,
      createdAt: "24/03",
      status: "Resolved",
    },
  ],
  Closed: [
    {
      id: "t12",
      code: "PA-032026-005",
      title: "Xả rác bừa bãi tại khu vực chợ phường 3",
      category: "Môi trường",
      categoryColor: "bg-emerald-100 text-emerald-700",
      priority: "Normal",
      reporter: "Đinh Thị M",
      reporterInitials: "TM",
      department: "Phòng TN-MT",
      slaRemaining: "—",
      slaBreached: false,
      createdAt: "20/03",
      status: "Closed",
    },
    {
      id: "t13",
      code: "PA-032026-004",
      title: "Quán karaoke hoạt động sau 22h tại Phường 2",
      category: "An ninh",
      categoryColor: "bg-violet-100 text-violet-700",
      priority: "Low",
      reporter: "Cao Văn N",
      reporterInitials: "VN",
      department: "Công an TP",
      slaRemaining: "—",
      slaBreached: false,
      createdAt: "18/03",
      status: "Closed",
    },
  ],
});

const rejectedTickets = [
  {
    id: "r1",
    code: "PA-032026-003",
    title: "Yêu cầu xây dựng công viên mới tại khu dân cư Phú An",
    reporter: "Phan Thị O",
    reporterInitials: "TO",
    createdAt: "15/03",
    rejectionReason: "Không thuộc thẩm quyền UBND Tỉnh",
  },
  {
    id: "r2",
    code: "PA-022026-099",
    title: "Phản ánh trùng lặp — đường hư hỏng đã có mã PA-022026-050",
    reporter: "Trương Văn P",
    reporterInitials: "VP",
    createdAt: "10/03",
    rejectionReason: "Trùng lặp với phản ánh PA-022026-050",
  },
];
const rejectedExpanded = ref(false);

// ── Filters ──
const search = ref("");
const catFilter = ref("");
const deptFilter = ref("");
const prioFilter = ref("");
const slaOnly = ref(false);

const categoryOptions = [
  { value: "Giao thông", label: "Giao thông" },
  { value: "Môi trường", label: "Môi trường" },
  { value: "Hạ tầng", label: "Hạ tầng" },
  { value: "An ninh", label: "An ninh" },
  { value: "Y tế", label: "Y tế" },
];
const deptOptions = [
  { value: "Phòng Hạ tầng GT", label: "Phòng Hạ tầng GT" },
  { value: "Phòng TN-MT", label: "Phòng TN-MT" },
  { value: "Phòng Đô thị", label: "Phòng Đô thị" },
  { value: "Cty Cấp nước", label: "Cty Cấp nước" },
  { value: "Điện lực ĐT", label: "Điện lực ĐT" },
  { value: "Công an TP", label: "Công an TP" },
];
const prioOptions = [
  { value: "High", label: "Cao" },
  { value: "Normal", label: "Trung bình" },
  { value: "Low", label: "Thấp" },
];

const hasFilters = computed(
  () =>
    !!(
      search.value ||
      catFilter.value ||
      deptFilter.value ||
      prioFilter.value ||
      slaOnly.value
    ),
);

function clearFilters() {
  search.value = "";
  catFilter.value = "";
  deptFilter.value = "";
  prioFilter.value = "";
  slaOnly.value = false;
}

function isTicketVisible(t) {
  if (search.value) {
    const q = search.value.toLowerCase();
    if (!t.code.toLowerCase().includes(q) && !t.title.toLowerCase().includes(q))
      return false;
  }
  if (catFilter.value && t.category !== catFilter.value) return false;
  if (deptFilter.value && t.department !== deptFilter.value) return false;
  if (prioFilter.value && t.priority !== prioFilter.value) return false;
  if (slaOnly.value && !t.slaBreached) return false;
  return true;
}

function getFilteredTickets(status) {
  return tickets[status].filter(isTicketVisible);
}

const totalCount = computed(() =>
  Object.values(tickets).reduce((s, arr) => s + arr.length, 0),
);
const breachedCount = computed(
  () =>
    Object.values(tickets)
      .flat()
      .filter((t) => t.slaBreached).length,
);

function onDragChange(evt, toStatus) {
  if (evt.added) {
    evt.added.element.status = toStatus;
  }
}
</script>
