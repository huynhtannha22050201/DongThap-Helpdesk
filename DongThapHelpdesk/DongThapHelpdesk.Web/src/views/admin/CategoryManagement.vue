<template>
  <div class="space-y-5">
    <Teleport to="body">
      <Transition name="fade">
        <div
          v-if="toast.show"
          :class="[
            'fixed top-4 right-4 z-[100] flex items-center gap-2 px-4 py-3 rounded-xl shadow-lg text-[14px] font-medium',
            toast.type === 'success'
              ? 'bg-emerald-500 text-white'
              : 'bg-red-500 text-white',
          ]"
        >
          <CheckCircle2 v-if="toast.type === 'success'" :size="16" />
          <AlertCircle v-else :size="16" />
          {{ toast.message }}
        </div>
      </Transition>
    </Teleport>
    <!-- Header -->
    <div
      class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3"
    >
      <div>
        <h1
          class="text-slate-800 font-semibold"
          style="font-size: clamp(22px, 3vw, 26px)"
        >
          Quản lý Danh mục Lĩnh vực
        </h1>
        <p class="text-slate-500 mt-0.5 text-[14px]">
          Quản lý các lĩnh vực phản ánh và thời hạn SLA mặc định —
          {{ categories.length }} danh mục
        </p>
      </div>
      <button
        @click="openModal(null)"
        class="flex items-center gap-2 px-5 py-2.5 rounded-xl bg-[#DA251D] text-white hover:bg-[#b81f18] transition-colors cursor-pointer shadow-sm shadow-[#DA251D]/20 self-start text-[14px] font-medium"
      >
        <Plus :size="17" /> Thêm danh mục
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
            @input="onSearchInput"
            type="text"
            placeholder="Tìm theo tên hoặc mã..."
            class="w-full pl-10 pr-4 py-2.5 rounded-lg border border-slate-200 bg-white hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10 outline-none transition-all text-[13px]"
          />
        </div>
        <div class="flex items-center gap-2">
          <div class="relative">
            <button
              @click="showStatusDropdown = !showStatusDropdown"
              :class="[
                'flex items-center gap-2 pl-3 pr-4 py-2.5 rounded-lg border bg-white cursor-pointer text-[13px] transition-colors',
                statusFilter
                  ? 'border-[#DA251D]/30 text-[#DA251D]'
                  : 'border-slate-200 text-slate-600 hover:border-slate-300',
              ]"
            >
              <Power :size="14" />
              {{
                statusFilter === "active"
                  ? "Hoạt động"
                  : statusFilter === "inactive"
                    ? "Tạm ngưng"
                    : "Trạng thái"
              }}
              <ChevronDown :size="14" class="text-slate-400" />
            </button>
            <div
              v-if="showStatusDropdown"
              class="absolute top-full left-0 mt-1 w-40 bg-white rounded-lg border border-slate-200 shadow-lg z-30 overflow-hidden"
            >
              <button
                @click="
                  statusFilter = 'active';
                  showStatusDropdown = false;
                "
                :class="[
                  'w-full flex items-center gap-2 px-3 py-2.5 text-[13px] text-left transition-colors cursor-pointer',
                  statusFilter === 'active'
                    ? 'bg-emerald-50 text-emerald-700'
                    : 'text-slate-600 hover:bg-slate-50',
                ]"
              >
                <span class="w-1.5 h-1.5 rounded-full bg-emerald-500" /> Hoạt
                động
              </button>
              <button
                @click="
                  statusFilter = 'inactive';
                  showStatusDropdown = false;
                "
                :class="[
                  'w-full flex items-center gap-2 px-3 py-2.5 text-[13px] text-left transition-colors cursor-pointer',
                  statusFilter === 'inactive'
                    ? 'bg-red-50 text-red-600'
                    : 'text-slate-600 hover:bg-slate-50',
                ]"
              >
                <span class="w-1.5 h-1.5 rounded-full bg-red-500" /> Tạm ngưng
              </button>
            </div>
          </div>
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
    <!-- ✅ THÊM: Loading state -->
    <div
      v-if="loading && categories.length === 0"
      class="flex justify-center py-20"
    >
      <div
        class="animate-spin w-8 h-8 border-4 border-slate-200 border-t-[#DA251D] rounded-full"
      />
    </div>

    <!-- ✅ THÊM: Error state -->
    <div v-else-if="apiError" class="text-center py-20">
      <p class="text-red-500 mb-2">{{ apiError }}</p>
      <button @click="fetchCategories" class="text-[#DA251D] underline text-sm">
        Thử lại
      </button>
    </div>
    <template v-else>
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
                  class="text-left px-4 py-3.5 border-b border-slate-100 font-medium"
                >
                  <SortBtn label="Mã" field="code" />
                </th>
                <th
                  class="text-left px-4 py-3.5 border-b border-slate-100 font-medium"
                >
                  <SortBtn label="Tên lĩnh vực" field="name" />
                </th>
                <th
                  class="text-left px-4 py-3.5 border-b border-slate-100 font-medium hidden lg:table-cell text-slate-500"
                >
                  Mô tả
                </th>
                <th
                  class="text-left px-4 py-3.5 border-b border-slate-100 font-medium"
                >
                  <SortBtn label="SLA (giờ)" field="defaultSlaHours" />
                </th>
                <th
                  class="text-left px-4 py-3.5 border-b border-slate-100 font-medium hidden xl:table-cell text-center text-slate-500"
                >
                  Phản ánh
                </th>
                <th
                  class="text-left px-4 py-3.5 border-b border-slate-100 font-medium"
                >
                  <SortBtn label="Trạng thái" field="isActive" />
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
                      <Layers :size="24" class="text-slate-300" />
                    </div>
                    <p class="text-slate-500 text-[15px] font-medium">
                      Không tìm thấy danh mục nào
                    </p>
                  </div>
                </td>
              </tr>
              <tr
                v-for="(c, i) in filtered"
                :key="c.id"
                :class="[
                  'border-b border-slate-50 hover:bg-slate-50/50 transition-colors',
                  !c.isActive ? 'opacity-60' : '',
                ]"
              >
                <td class="px-4 py-3.5 text-center text-slate-400 font-medium">
                  {{ i + 1 }}
                </td>
                <td class="px-4 py-3.5">
                  <span
                    :class="[
                      'inline-flex items-center px-2.5 py-1 rounded-lg font-mono text-[12px] font-semibold',
                      getCategoryColor(c.code),
                    ]"
                    >{{ c.code }}</span
                  >
                </td>
                <td class="px-4 py-3.5">
                  <span class="text-slate-800 text-[14px] font-medium">{{
                    c.name
                  }}</span>
                </td>
                <td class="px-4 py-3.5 text-slate-500 hidden lg:table-cell">
                  <span class="block max-w-[280px] truncate">{{
                    c.description
                  }}</span>
                </td>
                <td class="px-4 py-3.5">
                  <div class="flex items-center gap-1.5">
                    <Clock :size="13" class="text-slate-400" />
                    <span class="text-slate-700 font-medium"
                      >{{ c.defaultSlaHours }}h</span
                    >
                    <span class="text-slate-400 text-[12px]"
                      >({{ Math.floor(c.defaultSlaHours / 24) }}d)</span
                    >
                  </div>
                </td>
                <td
                  class="px-4 py-3.5 text-center hidden xl:table-cell text-slate-600 font-medium"
                >
                  {{ c.ticketCount }}
                </td>
                <td class="px-4 py-3.5">
                  <span
                    v-if="c.isActive"
                    class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-emerald-50 text-emerald-700 text-[12px] font-medium"
                    ><span class="w-1.5 h-1.5 rounded-full bg-emerald-500" />
                    Hoạt động</span
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
                      @click="openModal(c)"
                      class="p-2 rounded-lg text-slate-400 hover:text-blue-600 hover:bg-blue-50 transition-colors cursor-pointer"
                    >
                      <Edit3 :size="15" />
                    </button>
                    <button
                      @click="handleToggle(c.id)"
                      :class="[
                        'p-2 rounded-lg transition-colors cursor-pointer',
                        c.isActive
                          ? 'text-slate-400 hover:text-red-600 hover:bg-red-50'
                          : 'text-slate-400 hover:text-emerald-600 hover:bg-emerald-50',
                      ]"
                    >
                      <ToggleRight
                        v-if="c.isActive"
                        :size="18"
                        class="text-emerald-500"
                      />
                      <ToggleLeft v-else :size="18" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <!-- Pagination — sau Desktop Table -->
      <div
        v-if="totalPages > 1"
        class="flex items-center justify-between bg-white rounded-xl shadow-sm border border-slate-100 px-4 py-3"
      >
        <p class="text-slate-400 text-[13px]">
          Hiển thị {{ (currentPage - 1) * pageSize + 1 }} –
          {{ Math.min(currentPage * pageSize, totalItems) }} trên
          {{ totalItems }} danh mục
        </p>
        <div class="flex items-center gap-1">
          <button
            @click="goToPage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="p-2 rounded-lg text-slate-400 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition cursor-pointer"
          >
            <ChevronLeft :size="16" />
          </button>
          <button
            v-for="p in totalPages"
            :key="p"
            @click="goToPage(p)"
            :class="[
              'w-9 h-9 rounded-lg text-[13px] font-medium transition cursor-pointer',
              p === currentPage
                ? 'bg-[#DA251D] text-white shadow-sm'
                : 'text-slate-600 hover:bg-slate-100',
            ]"
          >
            {{ p }}
          </button>
          <button
            @click="goToPage(currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="p-2 rounded-lg text-slate-400 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition cursor-pointer"
          >
            <ChevronRight :size="16" />
          </button>
        </div>
      </div>
      <!-- Mobile cards -->
      <div class="md:hidden space-y-3">
        <div
          v-if="filtered.length === 0"
          class="bg-white rounded-xl border border-slate-100 shadow-sm p-10 text-center"
        >
          <Layers :size="24" class="text-slate-300 mx-auto mb-3" />
          <p class="text-slate-500 text-[14px] font-medium">
            Không tìm thấy danh mục nào
          </p>
        </div>
        <div
          v-for="c in filtered"
          :key="c.id"
          :class="[
            'bg-white rounded-xl border shadow-sm p-4',
            !c.isActive ? 'opacity-60' : '',
          ]"
        >
          <div class="flex items-start justify-between mb-3">
            <div class="flex items-center gap-2.5">
              <!-- Bỏ COLORS[c.color], dùng màu mặc định vì API không trả field color -->
              <span
                :class="[
                  'inline-flex items-center px-2.5 py-1 rounded-lg text-[12px] font-semibold',
                  getCategoryColor(c.code),
                ]"
              >
                {{ c.code }}
              </span>
              <span class="text-slate-800 text-[15px] font-semibold">{{
                c.name
              }}</span>
            </div>
            <span
              v-if="c.isActive"
              class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-emerald-50 text-emerald-600 text-[11px] font-medium"
            >
              <span class="w-1.5 h-1.5 rounded-full bg-emerald-500" /> Hoạt động
            </span>
            <span
              v-else
              class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-red-50 text-red-500 text-[11px] font-medium"
            >
              <span class="w-1.5 h-1.5 rounded-full bg-red-500" /> Tạm ngưng
            </span>
          </div>

          <!-- Thay description bằng parentCategoryName từ API -->
          <p
            v-if="c.parentCategoryName"
            class="text-slate-500 mb-3 text-[13px]"
          >
            Thuộc: {{ c.parentCategoryName }}
          </p>
          <p v-else class="text-slate-400 italic mb-3 text-[13px]">
            Danh mục gốc
          </p>

          <!-- Bỏ ticketCount vì API không trả, chỉ giữ SLA -->
          <div class="flex items-center gap-4 mb-3 text-slate-500 text-[13px]">
            <span class="flex items-center gap-1">
              <Clock :size="13" /> SLA: {{ c.defaultSlaHours }}h
              <span class="text-slate-400 text-[12px]">
                ({{ Math.floor(c.defaultSlaHours / 24) }}d)
              </span>
            </span>
          </div>

          <!-- Actions giữ nguyên -->
          <div class="flex items-center gap-2 pt-3 border-t border-slate-100">
            <button
              @click="openModal(c)"
              class="flex-1 flex items-center justify-center gap-1.5 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors cursor-pointer text-[13px] font-medium"
            >
              <Edit3 :size="14" /> Sửa
            </button>
            <button
              @click="handleToggle(c.id)"
              :class="[
                'flex-1 flex items-center justify-center gap-1.5 py-2 rounded-lg border transition-colors cursor-pointer text-[13px] font-medium',
                c.isActive
                  ? 'border-red-200 text-red-600 hover:bg-red-50'
                  : 'border-emerald-200 text-emerald-600 hover:bg-emerald-50',
              ]"
            >
              <Power :size="14" /> {{ c.isActive ? "Tạm ngưng" : "Kích hoạt" }}
            </button>
          </div>
        </div>
      </div>
    </template>
    <!-- Modal -->
    <Teleport to="body">
      <div
        v-if="modalOpen"
        @keydown.esc="modalOpen = false"
        tabindex="0"
        ref="modalRef"
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
                  v-if="editingCat"
                  :size="20"
                  class="text-[#DA251D]"
                /><Plus v-else :size="20" class="text-[#DA251D]" />
              </div>
              <div>
                <h2 class="text-slate-800 text-[18px] font-semibold">
                  {{ editingCat ? "Chỉnh sửa danh mục" : "Thêm danh mục mới" }}
                </h2>
                <p class="text-slate-400 text-[13px]">
                  {{
                    editingCat
                      ? "Cập nhật thông tin lĩnh vực"
                      : "Tạo lĩnh vực phản ánh mới"
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
            <div class="grid grid-cols-3 gap-4">
              <div>
                <label
                  class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                  >Mã lĩnh vực <span class="text-red-500">*</span></label
                >
                <input
                  v-model="form.code"
                  placeholder="GT-OGA"
                  :class="[inputCls, errors.code ? errCls : normCls]"
                  class="text-[14px] font-mono uppercase"
                />
                <p v-if="errors.code" class="text-red-500 mt-1 text-[12px]">
                  {{ errors.code }}
                </p>
              </div>
              <div class="col-span-2">
                <label
                  class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                  >Tên lĩnh vực <span class="text-red-500">*</span></label
                >
                <input
                  v-model="form.name"
                  placeholder="Tên lĩnh vực"
                  :class="[inputCls, errors.name ? errCls : normCls]"
                  class="text-[14px]"
                />
                <p v-if="errors.name" class="text-red-500 mt-1 text-[12px]">
                  {{ errors.name }}
                </p>
              </div>
            </div>
            <div>
              <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                >Mô tả</label
              >
              <textarea
                v-model="form.description"
                placeholder="Mô tả chi tiết lĩnh vực..."
                :class="[inputCls, normCls, 'resize-none']"
                class="text-[14px] leading-[1.6]"
                rows="3"
              />
            </div>
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label
                  class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                  >SLA mặc định (giờ) <span class="text-red-500">*</span></label
                >
                <input
                  v-model.number="form.defaultSlaHours"
                  type="number"
                  min="1"
                  :class="[inputCls, errors.defaultSlaHours ? errCls : normCls]"
                  class="text-[14px]"
                />
                <p
                  v-if="errors.defaultSlaHours"
                  class="text-red-500 mt-1 text-[12px]"
                >
                  {{ errors.defaultSlaHours }}
                </p>
                <p class="text-slate-400 mt-1 text-[12px]">
                  = {{ Math.floor(form.defaultSlaHours / 24) }} ngày
                  {{ form.defaultSlaHours % 24 }} giờ
                </p>
              </div>
              <div>
                <label
                  class="text-slate-700 mb-1.5 block text-[13px] font-medium"
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
                    :class="
                      form.isActive ? 'text-emerald-700' : 'text-slate-500'
                    "
                    class="text-[14px] font-medium"
                    >{{ form.isActive ? "Hoạt động" : "Tạm ngưng" }}</span
                  >
                </button>
              </div>
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
              <template v-else-if="editingCat"
                ><CheckCircle2 :size="16" /> Cập nhật</template
              >
              <template v-else><Plus :size="16" /> Tạo danh mục</template>
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch, nextTick } from "vue";
import { categoryService } from "@/services/categoryService";
import {
  Search,
  Plus,
  Edit3,
  ToggleLeft,
  ToggleRight,
  X,
  Clock,
  ChevronLeft,
  ChevronRight,
  ChevronDown,
  CheckCircle2,
  Loader2,
  Layers,
  Power,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
  AlertCircle, // thêm icon cho toast/error
} from "lucide-vue-next";

const categories = ref([]);
const totalItems = ref(0);
const totalPages = ref(1);
const currentPage = ref(1);
const pageSize = 5; // Cố định 5 items/trang
const loading = ref(false);
const apiError = ref(null);
const toast = ref({ show: false, message: "", type: "success" });
const modalRef = ref(null);
const showStatusDropdown = ref(false);
const stats = ref({ total: 0, active: 0, inactive: 0, avgSla: 0 });

// ── Helper: hiển thị toast ──────────────────────────────
function showToast(message, type = "success") {
  toast.value = { show: true, message, type };
  setTimeout(() => (toast.value.show = false), 3000);
}

// ── Fetch danh mục từ API ───────────────────────────────
async function fetchCategories() {
  loading.value = true;
  apiError.value = null;
  try {
    const isActive =
      statusFilter.value === "active"
        ? true
        : statusFilter.value === "inactive"
          ? false
          : undefined;

    const { data } = await categoryService.getPaged(
      currentPage.value,
      pageSize,
      search.value || undefined,
      isActive,
      sortField.value || undefined, // THÊM
      sortDir.value || undefined, // THÊM
    );
    categories.value = data.items;
    totalItems.value = data.total;
    totalPages.value = data.totalPages;
    stats.value = data.stats;
  } catch (err) {
    apiError.value = err.response?.data?.message || "Không thể tải danh mục";
  } finally {
    loading.value = false;
  }
}

function goToPage(page) {
  currentPage.value = page;
  fetchCategories();
}

let searchTimeout = null;
function onSearchInput() {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    currentPage.value = 1; // Reset về trang 1 khi search
    fetchCategories();
  }, 300); // Debounce 300ms
}

onMounted(() => {
  fetchCategories();
});

const statsCards = computed(() => [
  {
    label: "Tổng danh mục",
    value: stats.value.total,
    icon: Layers,
    color: "bg-blue-50 text-blue-600",
  },
  {
    label: "Đang hoạt động",
    value: stats.value.active,
    icon: CheckCircle2,
    color: "bg-emerald-50 text-emerald-600",
  },
  {
    label: "Tạm ngưng",
    value: stats.value.inactive,
    icon: Power,
    color: "bg-red-50 text-red-500",
  },
  {
    label: "SLA trung bình",
    value: stats.value.avgSla ? stats.value.avgSla + "h" : "0h",
    icon: Clock,
    color: "bg-violet-50 text-violet-600",
  },
]);

const search = ref("");
const statusFilter = ref("");
const sortField = ref("");
const sortDir = ref("desc");

function handleSort(f) {
  if (sortField.value === f) {
    sortDir.value = sortDir.value === "asc" ? "desc" : "asc";
  } else {
    sortField.value = f;
    sortDir.value = "asc";
  }
  currentPage.value = 1;
  fetchCategories();
}

const SortBtn = {
  props: ["label", "field"],
  components: { ArrowUp, ArrowDown, ArrowUpDown },
  setup(props) {
    return { sortField, sortDir, handleSort };
  },
  template: `<button @click="handleSort(field)" class="flex items-center gap-1 cursor-pointer group select-none"><span :class="sortField === field ? 'text-slate-800' : 'text-slate-500'">{{ label }}</span><ArrowUp v-if="sortField === field && sortDir === 'asc'" :size="13" class="text-[#DA251D]" /><ArrowDown v-else-if="sortField === field && sortDir === 'desc'" :size="13" class="text-[#DA251D]" /><ArrowUpDown v-else :size="13" class="text-slate-300 group-hover:text-slate-500 transition-colors" /></button>`,
};

const filtered = computed(() => categories.value);

async function handleToggle(id) {
  const cat = categories.value.find((c) => c.id === id);
  if (!cat) return;

  try {
    await categoryService.update(id, {
      name: cat.name,
      code: cat.code,
      description: cat.description,
      defaultSlaHours: cat.defaultSlaHours,
      isActive: !cat.isActive,
    });
    showToast(
      cat.isActive
        ? `Đã tạm ngưng "${cat.name}"`
        : `Đã kích hoạt "${cat.name}"`,
    );
    await fetchCategories();
  } catch (err) {
    showToast(err.response?.data?.message || "Có lỗi xảy ra", "error");
  }
}

// Modal
const modalOpen = ref(false);
const editingCat = ref(null);
const saving = ref(false);
const form = reactive({
  code: "",
  name: "",
  description: "",
  defaultSlaHours: 168,
  isActive: true,
});
const errors = reactive({});
const inputCls =
  "w-full py-3 px-4 rounded-xl border bg-white outline-none transition-all";
const normCls =
  "border-slate-200 hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10";
const errCls = "border-red-300 ring-2 ring-red-100";

function openModal(cat) {
  editingCat.value = cat;
  if (cat)
    Object.assign(form, {
      code: cat.code,
      name: cat.name,
      description: cat.description,
      defaultSlaHours: cat.defaultSlaHours,
      isActive: cat.isActive,
    });
  else
    Object.assign(form, {
      code: "",
      name: "",
      description: "",
      defaultSlaHours: 168,
      isActive: true,
    });
  Object.keys(errors).forEach((k) => delete errors[k]);
  modalOpen.value = true;
}

async function handleSave() {
  // ── Validate ────────────────────────────────────────
  Object.keys(errors).forEach((k) => delete errors[k]);
  if (!form.code.trim()) errors.code = "Vui lòng nhập mã";
  if (!form.name.trim()) errors.name = "Vui lòng nhập tên";
  if (form.defaultSlaHours <= 0) errors.defaultSlaHours = "SLA phải > 0";
  if (Object.keys(errors).length > 0) return;

  saving.value = true;
  try {
    if (editingCat.value) {
      // CẬP NHẬT
      await categoryService.update(editingCat.value.id, {
        name: form.name,
        code: form.code,
        description: form.description,
        defaultSlaHours: Number(form.defaultSlaHours),
        isActive: form.isActive,
      });
      showToast(`Cập nhật danh mục "${form.name}" thành công`);
    } else {
      // TẠO MỚI
      sortField.value = "";
      sortDir.value = "desc";
      currentPage.value = 1;
      await categoryService.create({
        name: form.name,
        code: form.code,
        description: form.description,
        defaultSlaHours: Number(form.defaultSlaHours),
      });
      showToast(`Tạo danh mục "${form.name}" thành công`);
    }
    modalOpen.value = false;
    await fetchCategories(); // Reload danh sách từ DB
  } catch (err) {
    // Hiển thị lỗi từ backend (ví dụ: "Mã danh mục đã tồn tại")
    const msg = err.response?.data?.message || "Có lỗi xảy ra";
    showToast(msg, "error");
  } finally {
    saving.value = false;
  }
}

const COLOR_PALETTE = [
  "bg-blue-100 text-blue-700",
  "bg-emerald-100 text-emerald-700",
  "bg-amber-100 text-amber-700",
  "bg-violet-100 text-violet-700",
  "bg-pink-100 text-pink-700",
  "bg-cyan-100 text-cyan-700",
  "bg-lime-100 text-lime-700",
  "bg-orange-100 text-orange-700",
  "bg-rose-100 text-rose-700",
  "bg-teal-100 text-teal-700",
  "bg-indigo-100 text-indigo-700",
  "bg-yellow-100 text-yellow-700",
];

function getCategoryColor(code) {
  if (!code) return "bg-slate-100 text-slate-600";
  // Tìm vị trí của danh mục trong danh sách → mỗi vị trí 1 màu khác nhau
  const index = categories.value.findIndex((c) => c.code === code);
  return COLOR_PALETTE[index % COLOR_PALETTE.length];
}

function onClickOutside(e) {
  if (!e.target.closest(".relative")) {
    showStatusDropdown.value = false;
  }
}
onMounted(() => document.addEventListener("click", onClickOutside));

watch(modalOpen, (val) => {
  if (val) {
    nextTick(() => modalRef.value?.focus());
  }
});

watch(statusFilter, () => {
  currentPage.value = 1;
  fetchCategories();
});
</script>
