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
          Quản lý Phòng ban
        </h1>
        <p class="text-slate-500 mt-0.5 text-[14px]">
          Quản lý cơ cấu tổ chức và phòng ban xử lý phản ánh —
          {{ departments.length }} phòng ban
        </p>
      </div>
      <button
        @click="openModal(null)"
        class="flex items-center gap-2 px-5 py-2.5 rounded-xl bg-[#DA251D] text-white hover:bg-[#b81f18] transition-colors cursor-pointer shadow-sm shadow-[#DA251D]/20 self-start text-[14px] font-medium"
      >
        <Plus :size="17" /> Thêm phòng ban
      </button>
    </div>

    <!-- Stats -->
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

    <!-- Toolbar -->
    <div
      class="bg-white rounded-xl shadow-sm border border-slate-100 px-4 py-3.5"
    >
      <div class="flex flex-col sm:flex-row gap-3 items-start sm:items-center">
        <div class="relative flex-1 min-w-0 w-full sm:max-w-[300px]">
          <Search
            :size="16"
            class="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
          />
          <input
            v-model="search"
            type="text"
            placeholder="Tìm theo tên hoặc trưởng phòng..."
            class="w-full pl-10 pr-4 py-2.5 rounded-lg border border-slate-200 bg-white hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10 outline-none transition-all text-[13px]"
          />
        </div>
        <div class="flex items-center gap-2">
          <select
            v-model="statusFilter"
            class="appearance-none pl-3 pr-8 py-2.5 rounded-lg border border-slate-200 bg-white text-slate-600 cursor-pointer text-[13px]"
          >
            <option value="">Trạng thái</option>
            <option value="active">Hoạt động</option>
            <option value="inactive">Tạm ngưng</option>
          </select>
          <button
            v-if="search || statusFilter"
            @click="
              search = '';
              statusFilter = '';
            "
            class="flex items-center gap-1 px-2.5 py-2.5 text-slate-400 hover:text-slate-600 cursor-pointer text-[13px]"
          >
            <X :size="14" /> Xóa lọc
          </button>
        </div>
      </div>
    </div>

    <!-- Desktop Table -->
    <div
      class="hidden md:block bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden"
    >
      <div class="overflow-x-auto">
        <table class="w-full text-[13px]">
          <thead>
            <tr class="bg-slate-50/70">
              <th
                class="text-left px-4 py-3.5 border-b border-slate-100 w-14 text-center font-medium text-slate-400"
              >
                STT
              </th>
              <th
                class="text-left px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Tên phòng ban
              </th>
              <th
                class="text-left px-4 py-3.5 border-b border-slate-100 font-medium hidden lg:table-cell text-slate-500"
              >
                Mô tả
              </th>
              <th
                class="text-left px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Trưởng phòng
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Số cán bộ
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 font-medium hidden xl:table-cell text-slate-500"
              >
                Phản ánh
              </th>
              <th
                class="text-left px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Trạng thái
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 w-28 font-medium text-slate-500"
              >
                Hành động
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="filtered.length === 0">
              <td colspan="8" class="px-4 py-16 text-center">
                <div class="flex flex-col items-center">
                  <div
                    class="w-14 h-14 rounded-full bg-slate-100 flex items-center justify-center mb-3"
                  >
                    <Building2 :size="24" class="text-slate-300" />
                  </div>
                  <p class="text-slate-500 text-[15px] font-medium">
                    Không tìm thấy phòng ban nào
                  </p>
                </div>
              </td>
            </tr>
            <tr
              v-for="(d, i) in filtered"
              :key="d.id"
              :class="[
                'border-b border-slate-50 hover:bg-slate-50/50 transition-colors',
                !d.isActive ? 'opacity-60' : '',
              ]"
            >
              <td class="px-4 py-3.5 text-center text-slate-400 font-medium">
                {{ i + 1 }}
              </td>
              <td class="px-4 py-3.5">
                <div class="flex items-center gap-3">
                  <div
                    :class="[
                      'w-9 h-9 rounded-lg flex items-center justify-center shrink-0',
                      d.color,
                    ]"
                  >
                    <Building2 :size="16" />
                  </div>
                  <span class="text-slate-800 text-[14px] font-medium">{{
                    d.name
                  }}</span>
                </div>
              </td>
              <td class="px-4 py-3.5 text-slate-500 hidden lg:table-cell">
                <span class="block max-w-[250px] truncate">{{
                  d.description
                }}</span>
              </td>
              <td class="px-4 py-3.5">
                <div class="flex items-center gap-2.5">
                  <div
                    class="w-7 h-7 rounded-full bg-slate-100 flex items-center justify-center text-slate-500 shrink-0 text-[10px] font-semibold"
                  >
                    {{ d.managerInitials }}
                  </div>
                  <div>
                    <p class="text-slate-700 text-[13px] font-medium">
                      {{ d.manager }}
                    </p>
                    <p class="text-slate-400 text-[11px]">
                      {{ d.managerPhone }}
                    </p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-3.5 text-center">
                <div class="flex items-center justify-center gap-1.5">
                  <Users :size="13" class="text-slate-400" /><span
                    class="text-slate-700 font-medium"
                    >{{ d.staffCount }}</span
                  >
                </div>
              </td>
              <td
                class="px-4 py-3.5 text-center hidden xl:table-cell text-slate-600 font-medium"
              >
                {{ d.ticketCount }}
              </td>
              <td class="px-4 py-3.5">
                <span
                  v-if="d.isActive"
                  class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-emerald-50 text-emerald-700 text-[12px] font-medium"
                  ><span class="w-1.5 h-1.5 rounded-full bg-emerald-500" /> Hoạt
                  động</span
                >
                <span
                  v-else
                  class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-red-50 text-red-600 text-[12px] font-medium"
                  ><span class="w-1.5 h-1.5 rounded-full bg-red-500" /> Tạm
                  ngưng</span
                >
              </td>
              <td class="px-4 py-3.5">
                <div class="flex items-center justify-center gap-1">
                  <button
                    @click="openModal(d)"
                    class="p-2 rounded-lg text-slate-400 hover:text-blue-600 hover:bg-blue-50 transition-colors cursor-pointer"
                  >
                    <Edit3 :size="15" />
                  </button>
                  <button
                    @click="handleToggle(d.id)"
                    :class="[
                      'p-2 rounded-lg transition-colors cursor-pointer',
                      d.isActive
                        ? 'text-slate-400 hover:text-red-600 hover:bg-red-50'
                        : 'text-slate-400 hover:text-emerald-600 hover:bg-emerald-50',
                    ]"
                  >
                    <ToggleRight
                      v-if="d.isActive"
                      :size="18"
                      class="text-emerald-500"
                    /><ToggleLeft v-else :size="18" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Mobile cards -->
    <div class="md:hidden space-y-3">
      <div
        v-for="d in filtered"
        :key="d.id"
        :class="[
          'bg-white rounded-xl border shadow-sm p-4',
          !d.isActive ? 'opacity-60' : '',
        ]"
      >
        <div class="flex items-start justify-between mb-2">
          <div class="flex items-center gap-2.5">
            <div
              :class="[
                'w-9 h-9 rounded-lg flex items-center justify-center shrink-0',
                d.color,
              ]"
            >
              <Building2 :size="16" />
            </div>
            <span class="text-slate-800 text-[15px] font-semibold">{{
              d.name
            }}</span>
          </div>
          <span
            v-if="d.isActive"
            class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-emerald-50 text-emerald-600 shrink-0 text-[11px] font-medium"
            ><span class="w-1.5 h-1.5 rounded-full bg-emerald-500" /> Hoạt
            động</span
          >
          <span
            v-else
            class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-red-50 text-red-500 shrink-0 text-[11px] font-medium"
            ><span class="w-1.5 h-1.5 rounded-full bg-red-500" /> Tạm
            ngưng</span
          >
        </div>
        <p class="text-slate-500 mb-3 line-clamp-2 text-[13px]">
          {{ d.description }}
        </p>
        <div class="flex items-center gap-4 mb-3 text-slate-500 text-[13px]">
          <span class="flex items-center gap-1.5"
            ><Crown :size="13" /> {{ d.manager }}</span
          >
          <span class="flex items-center gap-1.5"
            ><Users :size="13" /> {{ d.staffCount }} cán bộ</span
          >
        </div>
        <div class="flex items-center gap-2 pt-3 border-t border-slate-100">
          <button
            @click="openModal(d)"
            class="flex-1 flex items-center justify-center gap-1.5 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors cursor-pointer text-[13px] font-medium"
          >
            <Edit3 :size="14" /> Sửa
          </button>
          <button
            @click="handleToggle(d.id)"
            :class="[
              'flex-1 flex items-center justify-center gap-1.5 py-2 rounded-lg border transition-colors cursor-pointer text-[13px] font-medium',
              d.isActive
                ? 'border-red-200 text-red-600 hover:bg-red-50'
                : 'border-emerald-200 text-emerald-600 hover:bg-emerald-50',
            ]"
          >
            <Power :size="14" /> {{ d.isActive ? "Tạm ngưng" : "Kích hoạt" }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <Teleport to="body">
      <div
        v-if="modalOpen"
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
      >
        <div
          class="absolute inset-0 bg-black/40 backdrop-blur-sm"
          @click="modalOpen = false"
        />
        <div
          class="relative bg-white rounded-2xl shadow-2xl w-full max-w-[480px] max-h-[90vh] overflow-y-auto"
        >
          <div
            class="flex items-center justify-between px-6 py-5 border-b border-slate-100"
          >
            <div class="flex items-center gap-3">
              <div
                class="w-10 h-10 rounded-xl bg-[#DA251D]/10 flex items-center justify-center"
              >
                <Edit3
                  v-if="editingDept"
                  :size="20"
                  class="text-[#DA251D]"
                /><Plus v-else :size="20" class="text-[#DA251D]" />
              </div>
              <div>
                <h2 class="text-slate-800 text-[18px] font-semibold">
                  {{
                    editingDept ? "Chỉnh sửa phòng ban" : "Thêm phòng ban mới"
                  }}
                </h2>
                <p class="text-slate-400 text-[13px]">
                  {{
                    editingDept
                      ? "Cập nhật thông tin phòng ban"
                      : "Tạo phòng ban mới trong hệ thống"
                  }}
                </p>
              </div>
            </div>
            <button
              @click="modalOpen = false"
              class="p-2 rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600 transition-colors cursor-pointer"
            >
              <X :size="20" />
            </button>
          </div>
          <form @submit.prevent="handleSave" class="px-6 py-5 space-y-4">
            <div>
              <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                >Tên phòng ban <span class="text-red-500">*</span></label
              >
              <input
                v-model="form.name"
                placeholder="Nhập tên phòng ban"
                :class="[inputCls, errors.name ? errCls : normCls]"
                class="text-[14px]"
              />
              <p v-if="errors.name" class="text-red-500 mt-1 text-[12px]">
                {{ errors.name }}
              </p>
            </div>
            <div>
              <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                >Mô tả</label
              >
              <textarea
                v-model="form.description"
                placeholder="Mô tả chức năng, nhiệm vụ..."
                :class="[inputCls, normCls, 'resize-none']"
                class="text-[14px] leading-[1.6]"
                rows="3"
              />
            </div>
            <div>
              <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                >Trưởng phòng <span class="text-red-500">*</span></label
              >
              <select
                v-model="form.manager"
                :class="[
                  inputCls,
                  errors.manager ? errCls : normCls,
                  'appearance-none cursor-pointer',
                ]"
                class="text-[14px]"
              >
                <option value="">— Chọn trưởng phòng —</option>
                <option v-for="m in MANAGERS" :key="m" :value="m">
                  {{ m }}
                </option>
              </select>
              <p v-if="errors.manager" class="text-red-500 mt-1 text-[12px]">
                {{ errors.manager }}
              </p>
            </div>
            <div>
              <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                >Trạng thái</label
              >
              <button
                type="button"
                @click="form.isActive = !form.isActive"
                :class="[
                  'w-full flex items-center gap-3 py-3 px-4 rounded-xl border transition-all cursor-pointer',
                  form.isActive
                    ? 'border-emerald-200 bg-emerald-50/50'
                    : 'border-slate-200 bg-slate-50/50',
                ]"
              >
                <ToggleRight
                  v-if="form.isActive"
                  :size="24"
                  class="text-emerald-500"
                /><ToggleLeft v-else :size="24" class="text-slate-400" />
                <span
                  :class="form.isActive ? 'text-emerald-700' : 'text-slate-500'"
                  class="text-[14px] font-medium"
                  >{{ form.isActive ? "Hoạt động" : "Tạm ngưng" }}</span
                >
              </button>
            </div>
          </form>
          <div
            class="flex items-center justify-end gap-3 px-6 py-4 border-t border-slate-100 bg-slate-50/50 rounded-b-2xl"
          >
            <button
              @click="modalOpen = false"
              class="px-5 py-2.5 rounded-xl text-slate-600 hover:bg-slate-100 transition-colors cursor-pointer text-[14px] font-medium"
            >
              Hủy
            </button>
            <button
              @click="handleSave"
              :disabled="saving"
              class="flex items-center gap-2 px-5 py-2.5 rounded-xl bg-[#DA251D] text-white hover:bg-[#b81f18] disabled:opacity-50 transition-colors cursor-pointer shadow-sm text-[14px] font-medium"
            >
              <Loader2 v-if="saving" :size="16" class="animate-spin" />
              <template v-if="saving">Đang lưu...</template>
              <template v-else-if="editingDept"
                ><CheckCircle2 :size="16" /> Cập nhật</template
              >
              <template v-else><Plus :size="16" /> Tạo phòng ban</template>
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from "vue";
import {
  Search,
  Plus,
  Edit3,
  ToggleLeft,
  ToggleRight,
  X,
  Building2,
  Users,
  CheckCircle2,
  Power,
  Loader2,
  Crown,
} from "lucide-vue-next";

const AVATAR_COLORS = [
  "bg-blue-100 text-blue-700",
  "bg-emerald-100 text-emerald-700",
  "bg-violet-100 text-violet-700",
  "bg-amber-100 text-amber-700",
  "bg-rose-100 text-rose-700",
  "bg-cyan-100 text-cyan-700",
  "bg-pink-100 text-pink-700",
  "bg-indigo-100 text-indigo-700",
];
const MANAGERS = [
  "Đặng Minh Tuấn",
  "Lê Quốc Việt",
  "Ngô Thanh Hải",
  "Trần Văn Đức",
  "Trần Đức Anh",
  "Hoàng Thị Linh",
  "Phạm Minh Khoa",
  "Trần Thị Hương",
  "Võ Văn Phú",
];

const departments = ref([
  {
    id: "d1",
    name: "Phòng Giao thông",
    description:
      "Quản lý hạ tầng giao thông, đường sá, cầu cống, tín hiệu giao thông",
    manager: "Đặng Minh Tuấn",
    managerInitials: "MT",
    managerPhone: "0901111007",
    staffCount: 8,
    ticketCount: 342,
    isActive: true,
    color: AVATAR_COLORS[0],
  },
  {
    id: "d2",
    name: "Phòng Tài nguyên - Môi trường",
    description:
      "Quản lý tài nguyên thiên nhiên, bảo vệ môi trường, xử lý ô nhiễm",
    manager: "Lê Quốc Việt",
    managerInitials: "QV",
    managerPhone: "0901111009",
    staffCount: 6,
    ticketCount: 218,
    isActive: true,
    color: AVATAR_COLORS[1],
  },
  {
    id: "d3",
    name: "Phòng Hạ tầng Kỹ thuật",
    description: "Quản lý hệ thống cấp thoát nước, điện chiếu sáng, cây xanh",
    manager: "Ngô Thanh Hải",
    managerInitials: "TH",
    managerPhone: "0901111013",
    staffCount: 7,
    ticketCount: 187,
    isActive: true,
    color: AVATAR_COLORS[2],
  },
  {
    id: "d4",
    name: "Phòng Đô thị",
    description:
      "Quản lý quy hoạch đô thị, trật tự xây dựng, vỉa hè lòng lề đường",
    manager: "Trần Văn Đức",
    managerInitials: "VĐ",
    managerPhone: "0901111014",
    staffCount: 5,
    ticketCount: 156,
    isActive: true,
    color: AVATAR_COLORS[3],
  },
  {
    id: "d5",
    name: "Công an TP. Cao Lãnh",
    description: "Đảm bảo an ninh trật tự, xử lý vi phạm hành chính",
    manager: "Trần Đức Anh",
    managerInitials: "ĐA",
    managerPhone: "0901111006",
    staffCount: 12,
    ticketCount: 89,
    isActive: true,
    color: AVATAR_COLORS[4],
  },
  {
    id: "d6",
    name: "Phòng Kinh tế",
    description: "Quản lý hoạt động kinh doanh, thương mại, giá cả thị trường",
    manager: "Hoàng Thị Linh",
    managerInitials: "TL",
    managerPhone: "0901111015",
    staffCount: 4,
    ticketCount: 45,
    isActive: true,
    color: AVATAR_COLORS[5],
  },
  {
    id: "d7",
    name: "Phòng Giáo dục",
    description:
      "Quản lý giáo dục mầm non, tiểu học, THCS, cơ sở vật chất trường học",
    manager: "Phạm Minh Khoa",
    managerInitials: "MK",
    managerPhone: "0901111016",
    staffCount: 3,
    ticketCount: 67,
    isActive: true,
    color: AVATAR_COLORS[6],
  },
  {
    id: "d8",
    name: "Văn phòng UBND",
    description: "Điều phối chung các phản ánh, kiến nghị của công dân",
    manager: "Trần Thị Hương",
    managerInitials: "TH",
    managerPhone: "0901111001",
    staffCount: 5,
    ticketCount: 0,
    isActive: true,
    color: AVATAR_COLORS[7],
  },
  {
    id: "d9",
    name: "Phòng Nông nghiệp",
    description:
      "Quản lý sản xuất nông nghiệp, thủy lợi, phòng chống thiên tai",
    manager: "Võ Văn Phú",
    managerInitials: "VP",
    managerPhone: "0901111017",
    staffCount: 4,
    ticketCount: 43,
    isActive: false,
    color: AVATAR_COLORS[0],
  },
]);

const statsCards = computed(() => [
  {
    label: "Tổng phòng ban",
    value: departments.value.length,
    icon: Building2,
    color: "bg-blue-50 text-blue-600",
  },
  {
    label: "Đang hoạt động",
    value: departments.value.filter((d) => d.isActive).length,
    icon: CheckCircle2,
    color: "bg-emerald-50 text-emerald-600",
  },
  {
    label: "Tổng cán bộ",
    value: departments.value.reduce((s, d) => s + d.staffCount, 0),
    icon: Users,
    color: "bg-violet-50 text-violet-600",
  },
  {
    label: "Trưởng phòng",
    value: new Set(departments.value.map((d) => d.manager)).size,
    icon: Crown,
    color: "bg-amber-50 text-amber-600",
  },
]);

const search = ref("");
const statusFilter = ref("");

const filtered = computed(() => {
  let list = [...departments.value];
  if (search.value) {
    const q = search.value.toLowerCase();
    list = list.filter(
      (d) =>
        d.name.toLowerCase().includes(q) || d.manager.toLowerCase().includes(q),
    );
  }
  if (statusFilter.value)
    list = list.filter((d) =>
      statusFilter.value === "active" ? d.isActive : !d.isActive,
    );
  return list;
});

function handleToggle(id) {
  const d = departments.value.find((d) => d.id === id);
  if (d) d.isActive = !d.isActive;
}

const modalOpen = ref(false);
const editingDept = ref(null);
const saving = ref(false);
const form = reactive({
  name: "",
  description: "",
  manager: "",
  isActive: true,
});
const errors = reactive({});
const inputCls =
  "w-full py-3 px-4 rounded-xl border bg-white outline-none transition-all";
const normCls =
  "border-slate-200 hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10";
const errCls = "border-red-300 ring-2 ring-red-100";

function openModal(dept) {
  editingDept.value = dept;
  if (dept)
    Object.assign(form, {
      name: dept.name,
      description: dept.description,
      manager: dept.manager,
      isActive: dept.isActive,
    });
  else
    Object.assign(form, {
      name: "",
      description: "",
      manager: "",
      isActive: true,
    });
  Object.keys(errors).forEach((k) => delete errors[k]);
  modalOpen.value = true;
}

function handleSave() {
  Object.keys(errors).forEach((k) => delete errors[k]);
  if (!form.name.trim()) errors.name = "Vui lòng nhập tên phòng ban";
  if (!form.manager) errors.manager = "Vui lòng chọn trưởng phòng";
  if (Object.keys(errors).length > 0) return;
  saving.value = true;
  setTimeout(() => {
    saving.value = false;
    modalOpen.value = false;
  }, 600);
}
</script>
