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
          Quản lý người dùng
        </h1>
        <p class="text-slate-500 mt-0.5 text-[14px]">
          Quản lý tài khoản cán bộ trong hệ thống — {{ users.length }} tài khoản
        </p>
      </div>
      <button
        @click="openModal(null)"
        class="flex items-center gap-2 px-5 py-2.5 rounded-xl bg-[#DA251D] text-white hover:bg-[#b81f18] transition-colors cursor-pointer shadow-sm shadow-[#DA251D]/20 self-start text-[14px] font-medium"
      >
        <UserPlus :size="17" /> Thêm cán bộ
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
      <div class="flex flex-col lg:flex-row gap-3 items-start lg:items-center">
        <div class="relative flex-1 min-w-0 w-full lg:max-w-[300px]">
          <Search
            :size="16"
            class="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
          />
          <input
            v-model="search"
            type="text"
            placeholder="Tìm theo tên hoặc SĐT..."
            class="w-full pl-10 pr-4 py-2.5 rounded-lg border border-slate-200 bg-white hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10 outline-none transition-all text-[13px]"
          />
        </div>
        <div class="flex flex-wrap items-center gap-2">
          <select
            v-model="roleFilter"
            class="appearance-none pl-8 pr-8 py-2.5 rounded-lg border border-slate-200 bg-white text-slate-600 hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10 outline-none transition-all cursor-pointer text-[13px]"
          >
            <option value="">Vai trò</option>
            <option value="Dispatcher">Điều phối viên</option>
            <option value="Assignee">Cán bộ xử lý</option>
            <option value="Manager">Quản lý</option>
          </select>
          <select
            v-model="deptFilter"
            class="appearance-none pl-3 pr-8 py-2.5 rounded-lg border border-slate-200 bg-white text-slate-600 hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10 outline-none transition-all cursor-pointer text-[13px]"
          >
            <option value="">Phòng ban</option>
            <option v-for="d in DEPARTMENTS" :key="d" :value="d">
              {{ d }}
            </option>
          </select>
          <select
            v-model="statusFilter"
            class="appearance-none pl-3 pr-8 py-2.5 rounded-lg border border-slate-200 bg-white text-slate-600 hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10 outline-none transition-all cursor-pointer text-[13px]"
          >
            <option value="">Trạng thái</option>
            <option value="active">Hoạt động</option>
            <option value="locked">Đã khóa</option>
          </select>
          <button
            v-if="hasFilters"
            @click="clearFilters"
            class="flex items-center gap-1 px-3 py-2.5 text-slate-400 hover:text-slate-600 transition-colors cursor-pointer text-[13px]"
          >
            <X :size="14" /> Xóa lọc
          </button>
        </div>
        <div class="hidden lg:flex items-center gap-1 ml-auto">
          <button
            class="p-2.5 rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600 transition-colors cursor-pointer"
          >
            <RefreshCw :size="16" />
          </button>
          <button
            class="p-2.5 rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600 transition-colors cursor-pointer"
          >
            <Download :size="16" />
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
                class="text-left px-4 py-3.5 border-b border-slate-100 w-14 text-center font-medium"
              >
                <span class="text-slate-400">STT</span>
              </th>
              <th
                v-for="col in tableCols"
                :key="col.field"
                :class="[
                  'text-left px-4 py-3.5 border-b border-slate-100 font-medium',
                  col.hide || '',
                ]"
              >
                <button
                  @click="handleSort(col.field)"
                  class="flex items-center gap-1 cursor-pointer group select-none"
                >
                  <span
                    :class="
                      sortField === col.field
                        ? 'text-slate-800'
                        : 'text-slate-500'
                    "
                    >{{ col.label }}</span
                  >
                  <ArrowUp
                    v-if="sortField === col.field && sortDir === 'asc'"
                    :size="13"
                    class="text-[#DA251D]"
                  />
                  <ArrowDown
                    v-else-if="sortField === col.field && sortDir === 'desc'"
                    :size="13"
                    class="text-[#DA251D]"
                  />
                  <ArrowUpDown
                    v-else
                    :size="13"
                    class="text-slate-300 group-hover:text-slate-500 transition-colors"
                  />
                </button>
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 w-28 font-medium"
              >
                <span class="text-slate-500">Hành động</span>
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="paginated.length === 0">
              <td colspan="8" class="px-4 py-16 text-center">
                <div class="flex flex-col items-center">
                  <div
                    class="w-14 h-14 rounded-full bg-slate-100 flex items-center justify-center mb-3"
                  >
                    <Users :size="24" class="text-slate-300" />
                  </div>
                  <p class="text-slate-500 text-[15px] font-medium">
                    Không tìm thấy cán bộ nào
                  </p>
                </div>
              </td>
            </tr>
            <tr
              v-for="(u, i) in paginated"
              :key="u.id"
              :class="[
                'border-b border-slate-50 hover:bg-slate-50/50 transition-colors',
                !u.isActive ? 'opacity-60' : '',
              ]"
            >
              <td class="px-4 py-3.5 text-center text-slate-400 font-medium">
                {{ (page - 1) * pageSize + i + 1 }}
              </td>
              <td class="px-4 py-3.5">
                <div class="flex items-center gap-3">
                  <div
                    :class="[
                      'w-9 h-9 rounded-full flex items-center justify-center shrink-0 text-[12px] font-semibold',
                      u.avatarColor,
                    ]"
                  >
                    {{ u.initials }}
                  </div>
                  <div class="min-w-0">
                    <p class="text-slate-800 truncate text-[14px] font-medium">
                      {{ u.fullName }}
                    </p>
                    <p class="text-slate-400 xl:hidden truncate text-[12px]">
                      {{ u.department }}
                    </p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-3.5 text-slate-600">{{ u.phone }}</td>
              <td class="px-4 py-3.5 text-slate-600 hidden lg:table-cell">
                <span class="truncate block max-w-[200px]">{{ u.email }}</span>
              </td>
              <td class="px-4 py-3.5">
                <span
                  :class="[
                    'inline-flex items-center px-2.5 py-1 rounded-full text-[12px] font-medium',
                    ROLE_CONFIG[u.role].bg,
                  ]"
                  >{{ ROLE_CONFIG[u.role].label }}</span
                >
              </td>
              <td class="px-4 py-3.5 text-slate-600 hidden xl:table-cell">
                <span class="truncate block max-w-[160px]">{{
                  u.department
                }}</span>
              </td>
              <td class="px-4 py-3.5">
                <span
                  v-if="u.isActive"
                  class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-emerald-50 text-emerald-700 text-[12px] font-medium"
                  ><span class="w-1.5 h-1.5 rounded-full bg-emerald-500" /> Hoạt
                  động</span
                >
                <span
                  v-else
                  class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-red-50 text-red-600 text-[12px] font-medium"
                  ><span class="w-1.5 h-1.5 rounded-full bg-red-500" /> Đã
                  khóa</span
                >
              </td>
              <td class="px-4 py-3.5">
                <div class="flex items-center justify-center gap-1">
                  <button
                    @click="openModal(u)"
                    class="p-2 rounded-lg text-slate-400 hover:text-blue-600 hover:bg-blue-50 transition-colors cursor-pointer"
                    title="Sửa"
                  >
                    <Edit3 :size="15" />
                  </button>
                  <button
                    @click="openLockConfirm(u)"
                    :class="[
                      'p-2 rounded-lg transition-colors cursor-pointer',
                      u.isActive
                        ? 'text-slate-400 hover:text-red-600 hover:bg-red-50'
                        : 'text-slate-400 hover:text-emerald-600 hover:bg-emerald-50',
                    ]"
                    :title="u.isActive ? 'Khóa' : 'Mở khóa'"
                  >
                    <Lock v-if="u.isActive" :size="15" />
                    <Unlock v-else :size="15" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div
        v-if="filtered.length > 0"
        class="flex flex-col sm:flex-row items-center justify-between px-5 py-3.5 border-t border-slate-100 gap-3"
      >
        <p class="text-slate-400 text-[13px]">
          Hiển thị {{ (page - 1) * pageSize + 1 }}–{{
            Math.min(page * pageSize, filtered.length)
          }}
          trên {{ filtered.length }} kết quả
        </p>
        <div class="flex items-center gap-1">
          <button
            @click="page = 1"
            :disabled="page === 1"
            class="p-2 rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600 disabled:opacity-30 disabled:cursor-not-allowed transition-colors cursor-pointer"
          >
            <ChevronsLeft :size="16" />
          </button>
          <button
            @click="page = Math.max(1, page - 1)"
            :disabled="page === 1"
            class="p-2 rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600 disabled:opacity-30 disabled:cursor-not-allowed transition-colors cursor-pointer"
          >
            <ChevronLeft :size="16" />
          </button>
          <button
            v-for="p in totalPages"
            :key="p"
            @click="page = p"
            :class="[
              'w-9 h-9 rounded-lg transition-colors cursor-pointer text-[13px] font-medium',
              p === page
                ? 'bg-[#DA251D] text-white shadow-sm'
                : 'text-slate-500 hover:bg-slate-100',
            ]"
          >
            {{ p }}
          </button>
          <button
            @click="page = Math.min(totalPages, page + 1)"
            :disabled="page === totalPages"
            class="p-2 rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600 disabled:opacity-30 disabled:cursor-not-allowed transition-colors cursor-pointer"
          >
            <ChevronRight :size="16" />
          </button>
          <button
            @click="page = totalPages"
            :disabled="page === totalPages"
            class="p-2 rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600 disabled:opacity-30 disabled:cursor-not-allowed transition-colors cursor-pointer"
          >
            <ChevronsRight :size="16" />
          </button>
        </div>
      </div>
    </div>

    <!-- Mobile cards -->
    <div class="md:hidden space-y-3">
      <div
        v-if="paginated.length === 0"
        class="bg-white rounded-xl border border-slate-100 shadow-sm p-10 text-center"
      >
        <Users :size="24" class="text-slate-300 mx-auto mb-3" />
        <p class="text-slate-500 text-[15px] font-medium">
          Không tìm thấy cán bộ nào
        </p>
      </div>
      <div
        v-for="u in paginated"
        :key="u.id"
        :class="[
          'bg-white rounded-xl border shadow-sm p-4',
          !u.isActive ? 'opacity-60' : '',
        ]"
      >
        <div class="flex items-start gap-3 mb-3">
          <div
            :class="[
              'w-10 h-10 rounded-full flex items-center justify-center shrink-0 text-[13px] font-semibold',
              u.avatarColor,
            ]"
          >
            {{ u.initials }}
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-slate-800 truncate text-[15px] font-semibold">
              {{ u.fullName }}
            </p>
            <div class="flex items-center gap-2 mt-1">
              <span
                :class="[
                  'inline-flex items-center px-2 py-0.5 rounded-full text-[11px] font-medium',
                  ROLE_CONFIG[u.role].bg,
                ]"
                >{{ ROLE_CONFIG[u.role].label }}</span
              >
              <span
                v-if="u.isActive"
                class="inline-flex items-center gap-1 text-emerald-600 text-[11px] font-medium"
                ><span class="w-1.5 h-1.5 rounded-full bg-emerald-500" /> Hoạt
                động</span
              >
              <span
                v-else
                class="inline-flex items-center gap-1 text-red-500 text-[11px] font-medium"
                ><span class="w-1.5 h-1.5 rounded-full bg-red-500" /> Đã
                khóa</span
              >
            </div>
          </div>
        </div>
        <div class="space-y-1.5 mb-3 text-slate-500 text-[13px]">
          <div class="flex items-center gap-2">
            <Phone :size="13" /> {{ u.phone }}
          </div>
          <div class="flex items-center gap-2">
            <Mail :size="13" /> <span class="truncate">{{ u.email }}</span>
          </div>
          <div class="flex items-center gap-2">
            <Building2 :size="13" /> {{ u.department }}
          </div>
        </div>
        <div class="flex items-center gap-2 pt-3 border-t border-slate-100">
          <button
            @click="openModal(u)"
            class="flex-1 flex items-center justify-center gap-1.5 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors cursor-pointer text-[13px] font-medium"
          >
            <Edit3 :size="14" /> Sửa
          </button>
          <button
            @click="openLockConfirm(u)"
            :class="[
              'flex-1 flex items-center justify-center gap-1.5 py-2 rounded-lg border transition-colors cursor-pointer text-[13px] font-medium',
              u.isActive
                ? 'border-red-200 text-red-600 hover:bg-red-50'
                : 'border-emerald-200 text-emerald-600 hover:bg-emerald-50',
            ]"
          >
            <Lock v-if="u.isActive" :size="14" />
            {{ u.isActive ? "Khóa" : "Mở khóa" }}
            <Unlock v-if="!u.isActive" :size="14" />
          </button>
        </div>
      </div>
    </div>

    <!-- CREATE/EDIT MODAL -->
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
          class="relative bg-white rounded-2xl shadow-2xl w-full max-w-[520px] max-h-[90vh] overflow-y-auto"
        >
          <div
            class="flex items-center justify-between px-6 py-5 border-b border-slate-100"
          >
            <div class="flex items-center gap-3">
              <div
                class="w-10 h-10 rounded-xl bg-[#DA251D]/10 flex items-center justify-center"
              >
                <Edit3 v-if="editingUser" :size="20" class="text-[#DA251D]" />
                <UserPlus v-else :size="20" class="text-[#DA251D]" />
              </div>
              <div>
                <h2 class="text-slate-800 text-[18px] font-semibold">
                  {{ editingUser ? "Chỉnh sửa cán bộ" : "Thêm cán bộ mới" }}
                </h2>
                <p class="text-slate-400 text-[13px]">
                  {{
                    editingUser
                      ? "Cập nhật thông tin tài khoản"
                      : "Tạo tài khoản cán bộ trong hệ thống"
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
                >Họ và tên <span class="text-red-500">*</span></label
              >
              <div class="relative">
                <User
                  :size="17"
                  class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400"
                />
                <input
                  v-model="form.fullName"
                  placeholder="Nhập họ và tên"
                  :class="[
                    inputCls,
                    errors.fullName ? errCls : normCls,
                    'pl-11',
                  ]"
                  class="text-[14px]"
                />
              </div>
              <p v-if="errors.fullName" class="text-red-500 mt-1 text-[12px]">
                {{ errors.fullName }}
              </p>
            </div>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label
                  class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                  >Số điện thoại <span class="text-red-500">*</span></label
                >
                <div class="relative">
                  <Phone
                    :size="17"
                    class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400"
                  />
                  <input
                    v-model="form.phone"
                    type="tel"
                    placeholder="0901234567"
                    :class="[
                      inputCls,
                      errors.phone ? errCls : normCls,
                      'pl-11',
                    ]"
                    class="text-[14px]"
                  />
                </div>
                <p v-if="errors.phone" class="text-red-500 mt-1 text-[12px]">
                  {{ errors.phone }}
                </p>
              </div>
              <div>
                <label
                  class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                  >Email <span class="text-red-500">*</span></label
                >
                <div class="relative">
                  <Mail
                    :size="17"
                    class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400"
                  />
                  <input
                    v-model="form.email"
                    type="email"
                    placeholder="email@dongthap.gov.vn"
                    :class="[
                      inputCls,
                      errors.email ? errCls : normCls,
                      'pl-11',
                    ]"
                    class="text-[14px]"
                  />
                </div>
                <p v-if="errors.email" class="text-red-500 mt-1 text-[12px]">
                  {{ errors.email }}
                </p>
              </div>
            </div>
            <div v-if="!editingUser">
              <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                >Mật khẩu <span class="text-red-500">*</span></label
              >
              <div class="relative">
                <Lock
                  :size="17"
                  class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400"
                />
                <input
                  v-model="form.password"
                  :type="showPw ? 'text' : 'password'"
                  placeholder="Tối thiểu 6 ký tự"
                  :class="[
                    inputCls,
                    errors.password ? errCls : normCls,
                    'pl-11 pr-11',
                  ]"
                  class="text-[14px]"
                />
                <button
                  type="button"
                  @click="showPw = !showPw"
                  class="absolute right-3 top-1/2 -translate-y-1/2 p-1.5 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors cursor-pointer"
                >
                  <EyeOff v-if="showPw" :size="16" /><Eye v-else :size="16" />
                </button>
              </div>
              <p v-if="errors.password" class="text-red-500 mt-1 text-[12px]">
                {{ errors.password }}
              </p>
            </div>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label
                  class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                  >Vai trò <span class="text-red-500">*</span></label
                >
                <select
                  v-model="form.role"
                  :class="[
                    inputCls,
                    errors.role ? errCls : normCls,
                    'pl-4 pr-9 appearance-none cursor-pointer',
                  ]"
                  class="text-[14px]"
                >
                  <option value="">— Chọn vai trò —</option>
                  <option value="Dispatcher">Điều phối viên</option>
                  <option value="Assignee">Cán bộ xử lý</option>
                  <option value="Manager">Quản lý</option>
                </select>
                <p v-if="errors.role" class="text-red-500 mt-1 text-[12px]">
                  {{ errors.role }}
                </p>
              </div>
              <div>
                <label
                  class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                  >Phòng ban <span class="text-red-500">*</span></label
                >
                <select
                  v-model="form.department"
                  :class="[
                    inputCls,
                    errors.department ? errCls : normCls,
                    'pl-4 pr-9 appearance-none cursor-pointer',
                  ]"
                  class="text-[14px]"
                >
                  <option value="">— Chọn phòng ban —</option>
                  <option v-for="d in DEPARTMENTS" :key="d" :value="d">
                    {{ d }}
                  </option>
                </select>
                <p
                  v-if="errors.department"
                  class="text-red-500 mt-1 text-[12px]"
                >
                  {{ errors.department }}
                </p>
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
              class="flex items-center gap-2 px-5 py-2.5 rounded-xl bg-[#DA251D] text-white hover:bg-[#b81f18] disabled:opacity-50 disabled:cursor-not-allowed transition-colors cursor-pointer shadow-sm text-[14px] font-medium"
            >
              <Loader2 v-if="saving" :size="16" class="animate-spin" />
              <template v-if="saving">Đang lưu...</template>
              <template v-else-if="editingUser"
                ><CheckCircle2 :size="16" /> Cập nhật</template
              >
              <template v-else><UserPlus :size="16" /> Tạo tài khoản</template>
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- LOCK CONFIRM MODAL -->
    <Teleport to="body">
      <div
        v-if="lockUser"
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
      >
        <div
          class="absolute inset-0 bg-black/40 backdrop-blur-sm"
          @click="lockUser = null"
        />
        <div
          class="relative bg-white rounded-2xl shadow-2xl w-full max-w-[400px] p-6 text-center"
        >
          <div
            :class="[
              'w-14 h-14 rounded-2xl mx-auto mb-4 flex items-center justify-center',
              lockUser.isActive ? 'bg-red-50' : 'bg-emerald-50',
            ]"
          >
            <Lock v-if="lockUser.isActive" :size="26" class="text-red-500" />
            <Unlock v-else :size="26" class="text-emerald-500" />
          </div>
          <h3 class="text-slate-800 mb-2 text-[18px] font-semibold">
            {{ lockUser.isActive ? "Khóa tài khoản?" : "Mở khóa tài khoản?" }}
          </h3>
          <p class="text-slate-500 mb-1 text-[14px] leading-[1.6]">
            {{
              lockUser.isActive
                ? `Tài khoản của "${lockUser.fullName}" sẽ bị khóa và không thể đăng nhập hệ thống.`
                : `Tài khoản của "${lockUser.fullName}" sẽ được kích hoạt lại và có thể đăng nhập.`
            }}
          </p>
          <div class="flex items-center justify-center gap-3 mt-6">
            <button
              @click="lockUser = null"
              class="px-5 py-2.5 rounded-xl text-slate-600 hover:bg-slate-100 transition-colors cursor-pointer text-[14px] font-medium"
            >
              Hủy
            </button>
            <button
              @click="handleToggleLock"
              :class="[
                'flex items-center gap-2 px-5 py-2.5 rounded-xl text-white transition-colors cursor-pointer shadow-sm text-[14px] font-medium',
                lockUser.isActive
                  ? 'bg-red-500 hover:bg-red-600'
                  : 'bg-emerald-500 hover:bg-emerald-600',
              ]"
            >
              <Lock v-if="lockUser.isActive" :size="15" />
              {{ lockUser.isActive ? "Khóa" : "Mở khóa" }}
              <Unlock v-if="!lockUser.isActive" :size="15" />
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
  UserPlus,
  Edit3,
  Lock,
  Unlock,
  X,
  Eye,
  EyeOff,
  Phone,
  Mail,
  Building2,
  Users,
  UserCheck,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
  CheckCircle2,
  RefreshCw,
  Download,
  User,
  Loader2,
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
} from "lucide-vue-next";

const ROLE_CONFIG = {
  Dispatcher: { label: "Điều phối viên", bg: "bg-indigo-50 text-indigo-700" },
  Assignee: { label: "Cán bộ xử lý", bg: "bg-emerald-50 text-emerald-700" },
  Manager: { label: "Quản lý", bg: "bg-amber-50 text-amber-700" },
  Admin: { label: "Quản trị viên", bg: "bg-red-50 text-red-600" },
};
const DEPARTMENTS = [
  "Phòng Giao thông",
  "Phòng Môi trường",
  "Phòng Hạ tầng",
  "Phòng Đô thị",
  "Phòng Kinh tế",
  "Công an TP. Cao Lãnh",
  "Văn phòng UBND",
];
const AVATAR_COLORS = [
  "bg-blue-100 text-blue-700",
  "bg-emerald-100 text-emerald-700",
  "bg-violet-100 text-violet-700",
  "bg-amber-100 text-amber-700",
  "bg-rose-100 text-rose-700",
  "bg-cyan-100 text-cyan-700",
  "bg-pink-100 text-pink-700",
];

const users = ref([
  {
    id: "u1",
    fullName: "Trần Thị Hương",
    phone: "0901111001",
    email: "huong.tt@dongthap.gov.vn",
    role: "Dispatcher",
    department: "Văn phòng UBND",
    isActive: true,
    initials: "TH",
    avatarColor: AVATAR_COLORS[0],
  },
  {
    id: "u2",
    fullName: "Lê Văn Bình",
    phone: "0901111002",
    email: "binh.lv@dongthap.gov.vn",
    role: "Assignee",
    department: "Phòng Giao thông",
    isActive: true,
    initials: "VB",
    avatarColor: AVATAR_COLORS[1],
  },
  {
    id: "u3",
    fullName: "Nguyễn Hữu Toàn",
    phone: "0901111003",
    email: "toan.nh@dongthap.gov.vn",
    role: "Assignee",
    department: "Phòng Đô thị",
    isActive: true,
    initials: "HT",
    avatarColor: AVATAR_COLORS[2],
  },
  {
    id: "u4",
    fullName: "Phạm Văn Hùng",
    phone: "0901111004",
    email: "hung.pv@dongthap.gov.vn",
    role: "Assignee",
    department: "Phòng Môi trường",
    isActive: true,
    initials: "VH",
    avatarColor: AVATAR_COLORS[3],
  },
  {
    id: "u5",
    fullName: "Võ Hoàng Nam",
    phone: "0901111005",
    email: "nam.vh@dongthap.gov.vn",
    role: "Assignee",
    department: "Phòng Hạ tầng",
    isActive: false,
    initials: "HN",
    avatarColor: AVATAR_COLORS[4],
  },
  {
    id: "u6",
    fullName: "Trần Đức Anh",
    phone: "0901111006",
    email: "anh.td@dongthap.gov.vn",
    role: "Assignee",
    department: "Công an TP. Cao Lãnh",
    isActive: true,
    initials: "ĐA",
    avatarColor: AVATAR_COLORS[5],
  },
  {
    id: "u7",
    fullName: "Đặng Minh Tuấn",
    phone: "0901111007",
    email: "tuan.dm@dongthap.gov.vn",
    role: "Manager",
    department: "Phòng Giao thông",
    isActive: true,
    initials: "MT",
    avatarColor: AVATAR_COLORS[6],
  },
  {
    id: "u8",
    fullName: "Nguyễn Thị Lan",
    phone: "0901111008",
    email: "lan.nt@dongthap.gov.vn",
    role: "Dispatcher",
    department: "Văn phòng UBND",
    isActive: true,
    initials: "TL",
    avatarColor: AVATAR_COLORS[0],
  },
  {
    id: "u9",
    fullName: "Lê Quốc Việt",
    phone: "0901111009",
    email: "viet.lq@dongthap.gov.vn",
    role: "Manager",
    department: "Phòng Môi trường",
    isActive: true,
    initials: "QV",
    avatarColor: AVATAR_COLORS[1],
  },
  {
    id: "u10",
    fullName: "Huỳnh Thanh Phong",
    phone: "0901111010",
    email: "phong.ht@dongthap.gov.vn",
    role: "Assignee",
    department: "Phòng Kinh tế",
    isActive: false,
    initials: "TP",
    avatarColor: AVATAR_COLORS[2],
  },
  {
    id: "u11",
    fullName: "Bùi Thị Mai",
    phone: "0901111011",
    email: "mai.bt@dongthap.gov.vn",
    role: "Dispatcher",
    department: "Văn phòng UBND",
    isActive: true,
    initials: "TM",
    avatarColor: AVATAR_COLORS[3],
  },
  {
    id: "u12",
    fullName: "Phan Văn Tài",
    phone: "0901111012",
    email: "tai.pv@dongthap.gov.vn",
    role: "Assignee",
    department: "Phòng Hạ tầng",
    isActive: true,
    initials: "VT",
    avatarColor: AVATAR_COLORS[4],
  },
]);

// Stats
const statsCards = computed(() => [
  {
    label: "Tổng tài khoản",
    value: users.value.length,
    icon: Users,
    color: "bg-blue-50 text-blue-600",
  },
  {
    label: "Đang hoạt động",
    value: users.value.filter((u) => u.isActive).length,
    icon: UserCheck,
    color: "bg-emerald-50 text-emerald-600",
  },
  {
    label: "Đã khóa",
    value: users.value.filter((u) => !u.isActive).length,
    icon: Lock,
    color: "bg-red-50 text-red-500",
  },
  {
    label: "Phòng ban",
    value: new Set(users.value.map((u) => u.department)).size,
    icon: Building2,
    color: "bg-violet-50 text-violet-600",
  },
]);

// Filters & sort
const search = ref("");
const roleFilter = ref("");
const deptFilter = ref("");
const statusFilter = ref("");
const sortField = ref("fullName");
const sortDir = ref("asc");
const page = ref(1);
const pageSize = 8;

const hasFilters = computed(
  () =>
    !!(
      search.value ||
      roleFilter.value ||
      deptFilter.value ||
      statusFilter.value
    ),
);
function clearFilters() {
  search.value = "";
  roleFilter.value = "";
  deptFilter.value = "";
  statusFilter.value = "";
  page.value = 1;
}

const tableCols = [
  { field: "fullName", label: "Họ tên" },
  { field: "phone", label: "Số điện thoại" },
  { field: "email", label: "Email", hide: "hidden lg:table-cell" },
  { field: "role", label: "Vai trò" },
  { field: "department", label: "Phòng ban", hide: "hidden xl:table-cell" },
  { field: "isActive", label: "Trạng thái" },
];

function handleSort(field) {
  if (sortField.value === field)
    sortDir.value = sortDir.value === "asc" ? "desc" : "asc";
  else {
    sortField.value = field;
    sortDir.value = "asc";
  }
}

const filtered = computed(() => {
  let list = [...users.value];
  if (search.value) {
    const q = search.value.toLowerCase();
    list = list.filter(
      (u) => u.fullName.toLowerCase().includes(q) || u.phone.includes(q),
    );
  }
  if (roleFilter.value) list = list.filter((u) => u.role === roleFilter.value);
  if (deptFilter.value)
    list = list.filter((u) => u.department === deptFilter.value);
  if (statusFilter.value)
    list = list.filter((u) =>
      statusFilter.value === "active" ? u.isActive : !u.isActive,
    );
  list.sort((a, b) => {
    let va = String(a[sortField.value]),
      vb = String(b[sortField.value]);
    if (typeof a[sortField.value] === "boolean") {
      va = a[sortField.value] ? "1" : "0";
      vb = b[sortField.value] ? "1" : "0";
    }
    return sortDir.value === "asc"
      ? va.localeCompare(vb)
      : vb.localeCompare(va);
  });
  return list;
});
const totalPages = computed(() =>
  Math.max(1, Math.ceil(filtered.value.length / pageSize)),
);
const paginated = computed(() =>
  filtered.value.slice((page.value - 1) * pageSize, page.value * pageSize),
);

// Modal
const modalOpen = ref(false);
const editingUser = ref(null);
const saving = ref(false);
const showPw = ref(false);
const form = reactive({
  fullName: "",
  phone: "",
  email: "",
  password: "",
  role: "",
  department: "",
});
const errors = reactive({});

const inputCls =
  "w-full py-3 px-4 rounded-xl border bg-white outline-none transition-all";
const normCls =
  "border-slate-200 hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10";
const errCls = "border-red-300 ring-2 ring-red-100";

function openModal(user) {
  editingUser.value = user;
  if (user) {
    Object.assign(form, {
      fullName: user.fullName,
      phone: user.phone,
      email: user.email,
      password: "",
      role: user.role,
      department: user.department,
    });
  } else {
    Object.assign(form, {
      fullName: "",
      phone: "",
      email: "",
      password: "",
      role: "",
      department: "",
    });
  }
  Object.keys(errors).forEach((k) => delete errors[k]);
  showPw.value = false;
  modalOpen.value = true;
}

function handleSave() {
  Object.keys(errors).forEach((k) => delete errors[k]);
  if (!form.fullName.trim()) errors.fullName = "Vui lòng nhập họ tên";
  if (!form.phone.trim()) errors.phone = "Vui lòng nhập SĐT";
  else if (!/^0\d{9}$/.test(form.phone))
    errors.phone = "SĐT không hợp lệ (10 chữ số)";
  if (!form.email.trim()) errors.email = "Vui lòng nhập email";
  if (!editingUser.value && !form.password.trim())
    errors.password = "Vui lòng nhập mật khẩu";
  else if (!editingUser.value && form.password.length < 6)
    errors.password = "Mật khẩu tối thiểu 6 ký tự";
  if (!form.role) errors.role = "Vui lòng chọn vai trò";
  if (!form.department) errors.department = "Vui lòng chọn phòng ban";
  if (Object.keys(errors).length > 0) return;
  saving.value = true;
  setTimeout(() => {
    saving.value = false;
    modalOpen.value = false;
  }, 600);
}

// Lock
const lockUser = ref(null);
function openLockConfirm(user) {
  lockUser.value = user;
}
function handleToggleLock() {
  const u = users.value.find((u) => u.id === lockUser.value.id);
  if (u) u.isActive = !u.isActive;
  lockUser.value = null;
}
</script>
