<template>
  <div class="space-y-5">
    <!-- Toast -->
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
          Quản lý Đơn vị
        </h1>
        <p class="text-slate-500 mt-0.5 text-[14px]">
          Quản lý đơn vị UBND xã/phường tiếp nhận phản ánh —
          {{ stats.total }} đơn vị
        </p>
      </div>
      <button
        @click="openModal(null)"
        class="flex items-center gap-2 px-5 py-2.5 rounded-xl bg-[#DA251D] text-white hover:bg-[#b81f18] transition-colors cursor-pointer shadow-sm shadow-[#DA251D]/20 self-start text-[14px] font-medium"
      >
        <Plus :size="17" /> Thêm đơn vị
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
            placeholder="Tìm theo tên hoặc mã đơn vị..."
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

    <!-- Loading -->
    <div
      v-if="loading && departments.length === 0"
      class="flex justify-center py-20"
    >
      <div
        class="animate-spin w-8 h-8 border-4 border-slate-200 border-t-[#DA251D] rounded-full"
      />
    </div>

    <!-- Error -->
    <div v-else-if="apiError" class="text-center py-20">
      <p class="text-red-500 mb-2">{{ apiError }}</p>
      <button
        @click="fetchDepartments"
        class="text-[#DA251D] underline text-sm"
      >
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
                  <SortBtn label="Tên đơn vị" field="name" />
                </th>
                <th
                  class="text-left px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
                >
                  Chủ tịch
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
                <td colspan="7" class="px-4 py-16 text-center">
                  <div class="flex flex-col items-center">
                    <div
                      class="w-14 h-14 rounded-full bg-slate-100 flex items-center justify-center mb-3"
                    >
                      <Building2 :size="24" class="text-slate-300" />
                    </div>
                    <p class="text-slate-500 text-[15px] font-medium">
                      Không tìm thấy đơn vị nào
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
                  {{ (currentPage - 1) * pageSize + i + 1 }}
                </td>
                <td class="px-4 py-3.5">
                  <div class="flex items-center gap-3">
                    <div
                      :class="[
                        'w-9 h-9 rounded-lg flex items-center justify-center shrink-0',
                        getDeptColor(d.code),
                      ]"
                    >
                      <Building2 :size="16" />
                    </div>
                    <div>
                      <span class="text-slate-800 text-[14px] font-medium">{{
                        d.name
                      }}</span>
                      <p class="text-slate-400 text-[11px] font-mono">
                        {{ d.code }}
                      </p>
                    </div>
                  </div>
                </td>
                <td class="px-4 py-3.5">
                  <div
                    v-if="d.responsibleUserName"
                    class="flex items-center gap-2.5"
                  >
                    <div
                      class="w-7 h-7 rounded-full bg-slate-100 flex items-center justify-center text-slate-500 shrink-0 text-[10px] font-semibold"
                    >
                      {{ getInitials(d.responsibleUserName) }}
                    </div>
                    <div>
                      <p class="text-slate-700 text-[13px] font-medium">
                        {{ d.responsibleUserName }}
                      </p>
                      <p
                        v-if="d.responsibleUserPhone"
                        class="text-slate-400 text-[11px]"
                      >
                        {{ d.responsibleUserPhone }}
                      </p>
                      <p
                        v-if="d.responsibleUserEmail"
                        class="text-slate-400 text-[11px]"
                      >
                        {{ d.responsibleUserEmail }}
                      </p>
                    </div>
                  </div>
                  <span v-else class="text-slate-300 text-[13px]"
                    >Chưa gán</span
                  >
                </td>
                <td class="px-4 py-3.5 text-center">
                  <div class="flex items-center justify-center gap-1.5">
                    <Users :size="13" class="text-slate-400" />
                    <span class="text-slate-700 font-medium">{{
                      d.staffCount
                    }}</span>
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

      <!-- Pagination -->
      <div
        v-if="totalPages > 1"
        class="flex items-center justify-between bg-white rounded-xl shadow-sm border border-slate-100 px-4 py-3"
      >
        <p class="text-slate-400 text-[13px]">
          Hiển thị {{ (currentPage - 1) * pageSize + 1 }} –
          {{ Math.min(currentPage * pageSize, totalItems) }} trên
          {{ totalItems }} đơn vị
        </p>
        <div class="flex items-center gap-1">
          <button
            @click="goToPage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="p-2 rounded-lg text-slate-400 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition cursor-pointer"
          >
            <ChevronLeft :size="16" />
          </button>
          <!-- Trang đầu + dấu ... -->
          <template v-if="visiblePages[0] > 1">
            <button
              @click="goToPage(1)"
              class="w-9 h-9 rounded-lg text-[13px] font-medium transition cursor-pointer text-slate-600 hover:bg-slate-100"
            >
              1
            </button>
            <span
              v-if="visiblePages[0] > 2"
              class="w-9 h-9 flex items-center justify-center text-slate-400 text-[13px]"
              >...</span
            >
          </template>
          <!-- Các trang hiển thị -->
          <button
            v-for="p in visiblePages"
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
          <!-- Dấu ... + trang cuối -->
          <template v-if="visiblePages[visiblePages.length - 1] < totalPages">
            <span
              v-if="visiblePages[visiblePages.length - 1] < totalPages - 1"
              class="w-9 h-9 flex items-center justify-center text-slate-400 text-[13px]"
              >...</span
            >
            <button
              @click="goToPage(totalPages)"
              class="w-9 h-9 rounded-lg text-[13px] font-medium transition cursor-pointer text-slate-600 hover:bg-slate-100"
            >
              {{ totalPages }}
            </button>
          </template>
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
          <Building2 :size="24" class="text-slate-300 mx-auto mb-3" />
          <p class="text-slate-500 text-[14px] font-medium">
            Không tìm thấy đơn vị nào
          </p>
        </div>
        <div
          v-for="d in filtered"
          :key="d.id"
          :class="[
            'bg-white rounded-xl border shadow-sm p-4',
            !d.isActive ? 'opacity-60' : '',
          ]"
        >
          <div class="flex items-start justify-between mb-3">
            <div class="flex items-center gap-2.5">
              <div
                :class="[
                  'w-9 h-9 rounded-lg flex items-center justify-center shrink-0',
                  getDeptColor(d.code),
                ]"
              >
                <Building2 :size="16" />
              </div>
              <div>
                <span class="text-slate-800 text-[15px] font-semibold">{{
                  d.name
                }}</span>
                <p class="text-slate-400 text-[11px] font-mono">{{ d.code }}</p>
              </div>
            </div>
            <span
              v-if="d.isActive"
              class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-emerald-50 text-emerald-600 text-[11px] font-medium"
              ><span class="w-1.5 h-1.5 rounded-full bg-emerald-500" /> Hoạt
              động</span
            >
            <span
              v-else
              class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-red-50 text-red-500 text-[11px] font-medium"
              ><span class="w-1.5 h-1.5 rounded-full bg-red-500" /> Tạm
              ngưng</span
            >
          </div>
          <p class="text-slate-500 mb-3 text-[13px]">
            Trưởng phòng: {{ d.responsibleUserName || "Chưa gán" }}
          </p>
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
                  v-if="editingDept"
                  :size="20"
                  class="text-[#DA251D]"
                /><Plus v-else :size="20" class="text-[#DA251D]" />
              </div>
              <div>
                <h2 class="text-slate-800 text-[18px] font-semibold">
                  {{ editingDept ? "Chỉnh sửa phòng ban" : "Thêm đơn vị mới" }}
                </h2>
                <p class="text-slate-400 text-[13px]">
                  {{
                    editingDept
                      ? "Cập nhật thông tin đơn vị"
                      : "Tạo đơn vị UBND xã/phường mới"
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
                  >Mã đơn vị <span class="text-red-500">*</span></label
                >
                <input
                  v-model="form.code"
                  placeholder="VD: XA-ANHOA"
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
                  >Tên đơn vị <span class="text-red-500">*</span></label
                >
                <input
                  v-model="form.name"
                  placeholder="VD: UBND xã An Hòa"
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
                >Chủ tịch</label
              >
              <div class="relative" ref="staffDropdownRef">
                <!-- Nút mở dropdown — hiển thị tên đã chọn -->
                <button
                  type="button"
                  @click="showStaffDropdown = !showStaffDropdown"
                  :class="[
                    inputCls,
                    normCls,
                    'text-left cursor-pointer flex items-center justify-between',
                  ]"
                  class="text-[14px]"
                >
                  <span
                    :class="
                      form.responsibleUserId
                        ? 'text-slate-800'
                        : 'text-slate-400'
                    "
                  >
                    {{ selectedStaffName || "— Không chỉ định —" }}
                  </span>
                  <ChevronDown :size="14" class="text-slate-400 shrink-0" />
                </button>
                <!-- Dropdown panel -->
                <div
                  v-if="showStaffDropdown"
                  class="absolute top-full left-0 right-0 mt-1 bg-white rounded-xl border border-slate-200 shadow-lg z-40 overflow-hidden"
                >
                  <!-- Ô tìm kiếm -->
                  <div class="px-3 pt-3 pb-2">
                    <div class="relative">
                      <Search
                        :size="14"
                        class="absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400"
                      />
                      <input
                        v-model="staffSearch"
                        type="text"
                        placeholder="Tìm theo tên..."
                        class="w-full pl-8 pr-3 py-2 rounded-lg border border-slate-200 bg-slate-50 text-[13px] outline-none focus:border-[#DA251D]/40 focus:ring-1 focus:ring-[#DA251D]/10"
                        @click.stop
                      />
                    </div>
                  </div>
                  <!-- Danh sách — max 5 items, cuộn để xem thêm -->
                  <div class="max-h-[200px] overflow-y-auto">
                    <!-- Option: Không chỉ định -->
                    <button
                      type="button"
                      @click="selectStaff('')"
                      :class="[
                        'w-full text-left px-3 py-2.5 text-[13px] transition-colors cursor-pointer',
                        !form.responsibleUserId
                          ? 'bg-slate-50 text-[#DA251D] font-medium'
                          : 'text-slate-500 hover:bg-slate-50',
                      ]"
                    >
                      — Không chỉ định —
                    </button>
                    <!-- Options: danh sách Assignee -->
                    <button
                      v-for="u in filteredStaff"
                      :key="u.id"
                      type="button"
                      @click="selectStaff(u.id)"
                      :class="[
                        'w-full text-left px-3 py-2.5 text-[13px] transition-colors cursor-pointer',
                        form.responsibleUserId === u.id
                          ? 'bg-[#DA251D]/5 text-[#DA251D] font-medium'
                          : 'text-slate-700 hover:bg-slate-50',
                      ]"
                    >
                      {{ u.fullName }}
                    </button>
                    <!-- Không tìm thấy -->
                    <p
                      v-if="filteredStaff.length === 0"
                      class="px-3 py-4 text-center text-slate-400 text-[13px]"
                    >
                      Không tìm thấy
                    </p>
                  </div>
                </div>
              </div>
            </div>
            <div v-if="editingDept">
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
import { ref, reactive, computed, onMounted, watch, nextTick } from "vue";
import api from "@/services/api";
import { departmentService } from "@/services/departmentService";
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
  AlertCircle,
  ChevronLeft,
  ChevronRight,
  ChevronDown,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
} from "lucide-vue-next";

const departments = ref([]);
const staffList = ref([]);
const totalItems = ref(0);
const totalPages = ref(1);
const currentPage = ref(1);
const pageSize = 5;
const loading = ref(false);
const apiError = ref(null);
const toast = ref({ show: false, message: "", type: "success" });
const modalRef = ref(null);
const showStatusDropdown = ref(false);
const showStaffDropdown = ref(false);
const staffSearch = ref("");
const staffDropdownRef = ref(null);
const stats = ref({ total: 0, active: 0, inactive: 0, hasManager: 0 });

function showToast(message, type = "success") {
  toast.value = { show: true, message, type };
  setTimeout(() => (toast.value.show = false), 3000);
}

// Tên chủ tịch đã chọn — hiển thị trên nút
const selectedStaffName = computed(() => {
  if (!form.responsibleUserId) return "";
  const user = staffList.value.find((u) => u.id === form.responsibleUserId);
  return user?.fullName || "";
});

// Lọc danh sách theo từ khóa + loại bỏ người đã là chủ tịch đơn vị khác
const filteredStaff = computed(() => {
  // Lấy ID những người đã là chủ tịch
  const assignedIds = new Set(
    departments.value
      .filter((d) => d.responsibleUserId)
      .map((d) => d.responsibleUserId),
  );

  let list = staffList.value.filter((u) => {
    if (assignedIds.has(u.id)) {
      // Giữ lại nếu đang edit và user này là chủ tịch hiện tại
      return editingDept.value?.responsibleUserId === u.id;
    }
    return true;
  });

  // Lọc theo từ khóa tìm kiếm
  if (staffSearch.value) {
    const q = staffSearch.value.toLowerCase();
    list = list.filter((u) => u.fullName.toLowerCase().includes(q));
  }

  return list;
});

// Chọn chủ tịch
function selectStaff(id) {
  form.responsibleUserId = id;
  showStaffDropdown.value = false;
  staffSearch.value = ""; // Reset tìm kiếm
}

const visiblePages = computed(() => {
  const maxVisible = 5;
  const total = totalPages.value;
  const current = currentPage.value;

  if (total <= maxVisible) {
    // Ít hơn 5 trang → hiện hết
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  // Tính start sao cho current nằm giữa
  let start = current - Math.floor(maxVisible / 2);
  start = Math.max(1, start);
  start = Math.min(start, total - maxVisible + 1);

  return Array.from({ length: maxVisible }, (_, i) => start + i);
});

// ── Fetch ──────────────────────────────────────────────
async function fetchDepartments() {
  loading.value = true;
  apiError.value = null;
  try {
    const isActive =
      statusFilter.value === "active"
        ? true
        : statusFilter.value === "inactive"
          ? false
          : undefined;
    const { data } = await departmentService.getPaged(
      currentPage.value,
      pageSize,
      search.value || undefined,
      isActive,
      sortField.value || undefined,
      sortField.value ? sortDir.value : undefined,
    );
    departments.value = data.items;
    totalItems.value = data.total;
    totalPages.value = data.totalPages;
    stats.value = data.stats;
  } catch (err) {
    apiError.value =
      err.response?.data?.message || "Không thể tải danh sách phòng ban";
  } finally {
    loading.value = false;
  }
}

async function fetchStaff() {
  try {
    const { data } = await api.get("/users/role/Assignee");
    staffList.value = data;
  } catch {
    staffList.value = [];
  }
}

const availableStaff = computed(() => {
  // Lấy tất cả responsibleUserId đang được gán
  const assignedIds = new Set(
    departments.value
      .filter((d) => d.responsibleUserId)
      .map((d) => d.responsibleUserId),
  );

  return staffList.value.filter((u) => {
    // Nếu user đã là chủ tịch của đơn vị khác → ẩn
    if (assignedIds.has(u.id)) {
      // Nhưng nếu đang edit và user này chính là chủ tịch hiện tại → vẫn hiện
      return editingDept.value?.responsibleUserId === u.id;
    }
    return true;
  });
});

function goToPage(page) {
  currentPage.value = page;
  fetchDepartments();
}

let searchTimeout = null;
function onSearchInput() {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    currentPage.value = 1;
    fetchDepartments();
  }, 300);
}

onMounted(() => {
  fetchDepartments();
  fetchStaff();
});

// ── Stats ──────────────────────────────────────────────
const statsCards = computed(() => [
  {
    label: "Tổng phòng ban",
    value: stats.value.total,
    icon: Building2,
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
    label: "Có chủ tịch",
    value: stats.value.hasManager,
    icon: Crown,
    color: "bg-amber-50 text-amber-600",
  },
]);

// ── Filter + Sort ──────────────────────────────────────
const search = ref("");
const statusFilter = ref("");
const sortField = ref("");
const sortDir = ref("desc");

function handleSort(f) {
  if (sortField.value === f)
    sortDir.value = sortDir.value === "asc" ? "desc" : "asc";
  else {
    sortField.value = f;
    sortDir.value = "asc";
  }
  currentPage.value = 1; // Sort lại → về trang đầu
  fetchDepartments(); // Gọi server với sort mới
}

const SortBtn = {
  props: ["label", "field"],
  components: { ArrowUp, ArrowDown, ArrowUpDown },
  setup() {
    return { sortField, sortDir, handleSort };
  },
  template: `<button @click="handleSort(field)" class="flex items-center gap-1 cursor-pointer group select-none"><span :class="sortField === field ? 'text-slate-800' : 'text-slate-500'">{{ label }}</span><ArrowUp v-if="sortField === field && sortDir === 'asc'" :size="13" class="text-[#DA251D]" /><ArrowDown v-else-if="sortField === field && sortDir === 'desc'" :size="13" class="text-[#DA251D]" /><ArrowUpDown v-else :size="13" class="text-slate-300 group-hover:text-slate-500 transition-colors" /></button>`,
};

const filtered = computed(() => departments.value);

// ── Helpers ────────────────────────────────────────────
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

function getDeptColor(code) {
  if (!code) return "bg-slate-100 text-slate-600";
  const index = departments.value.findIndex((d) => d.code === code);
  return COLOR_PALETTE[index % COLOR_PALETTE.length];
}

function getInitials(name) {
  if (!name) return "?";
  const parts = name.trim().split(/\s+/);
  if (parts.length >= 2)
    return (
      parts[parts.length - 2][0] + parts[parts.length - 1][0]
    ).toUpperCase();
  return parts[0][0].toUpperCase();
}

// ── Toggle ─────────────────────────────────────────────
async function handleToggle(id) {
  const dept = departments.value.find((d) => d.id === id);
  if (!dept) return;
  try {
    await departmentService.update(id, {
      name: dept.name,
      code: dept.code,
      responsibleUserId: dept.responsibleUserId,
      isActive: !dept.isActive,
    });
    showToast(
      dept.isActive
        ? `Đã tạm ngưng "${dept.name}"`
        : `Đã kích hoạt "${dept.name}"`,
    );
    await fetchDepartments();
  } catch (err) {
    showToast(err.response?.data?.message || "Có lỗi xảy ra", "error");
  }
}

// ── Modal ──────────────────────────────────────────────
const modalOpen = ref(false);
const editingDept = ref(null);
const saving = ref(false);
const form = reactive({
  name: "",
  code: "",
  responsibleUserId: "",
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
      code: dept.code,
      responsibleUserId: dept.responsibleUserId || "",
      isActive: dept.isActive,
    });
  else
    Object.assign(form, {
      name: "",
      code: "",
      responsibleUserId: "",
      isActive: true,
    });
  Object.keys(errors).forEach((k) => delete errors[k]);
  showStaffDropdown.value = false;
  staffSearch.value = "";
  modalOpen.value = true;
}

async function handleSave() {
  Object.keys(errors).forEach((k) => delete errors[k]);
  if (!form.name.trim()) errors.name = "Vui lòng nhập tên đơn vị";
  if (!form.code.trim()) errors.code = "Vui lòng nhập mã đơn vị";
  if (Object.keys(errors).length > 0) return;

  saving.value = true;
  try {
    if (editingDept.value) {
      await departmentService.update(editingDept.value.id, {
        name: form.name.trim(),
        code: form.code.trim(),
        responsibleUserId: form.responsibleUserId || null,
        isActive: form.isActive,
      });
      showToast(`Cập nhật "${form.name}" thành công`);
    } else {
      // Tạo mới — level Commune, tìm đơn vị Province làm parent
      const allDepts = (await departmentService.getAll()).data;
      const province = allDepts.find((d) => d.level === "Province");
      await departmentService.create({
        name: form.name.trim(),
        code: form.code.trim(),
        level: "Commune",
        parentId: province?.id || null,
        responsibleUserId: form.responsibleUserId || null,
      });
      showToast(`Tạo đơn vị "${form.name}" thành công`);
    }
    modalOpen.value = false;
    sortField.value = "";
    currentPage.value = 1;
    await fetchDepartments();
  } catch (err) {
    showToast(err.response?.data?.message || "Có lỗi xảy ra", "error");
  } finally {
    saving.value = false;
  }
}

// ── Click outside + modal focus + watch ────────────────
function onClickOutside(e) {
  if (!e.target.closest(".relative")) {
    showStatusDropdown.value = false;
  }
  // Đóng staff dropdown khi click ngoài
  if (staffDropdownRef.value && !staffDropdownRef.value.contains(e.target)) {
    showStaffDropdown.value = false;
  }
}

onMounted(() => document.addEventListener("click", onClickOutside));

watch(modalOpen, (val) => {
  if (val) nextTick(() => modalRef.value?.focus());
});

watch(statusFilter, () => {
  currentPage.value = 1;
  fetchDepartments();
});
</script>
