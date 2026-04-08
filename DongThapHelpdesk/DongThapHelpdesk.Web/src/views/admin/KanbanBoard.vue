<template>
  <div class="flex flex-col h-full">
    <!-- HEADER -->
    <div class="shrink-0 mb-4">
      <div class="flex items-center justify-between mb-1">
        <div>
          <h1 class="text-2xl font-bold text-slate-800">Bảng Kanban</h1>
          <p class="text-sm text-slate-500 mt-1">
            Quản lý luồng xử lý phản ánh — {{ totalCount }} phản ánh
            <span v-if="breachedCount > 0" class="text-red-500 font-medium">
              · {{ breachedCount }} quá hạn
            </span>
          </p>
        </div>
        <button
          @click="refreshData"
          class="flex items-center gap-1.5 px-3 py-2 rounded-xl text-sm border border-slate-200 bg-white text-slate-600 hover:bg-slate-50 shadow-sm transition"
        >
          <RefreshCw :size="14" />
          Làm mới
        </button>
      </div>

      <!-- Bộ lọc -->
      <div
        class="bg-white rounded-xl shadow-sm border border-slate-100 p-3 mt-3"
      >
        <div class="flex flex-wrap items-center gap-3">
          <div class="relative flex-1 min-w-[200px]">
            <Search
              :size="14"
              class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
            />
            <input
              v-model="search"
              type="text"
              placeholder="Tìm theo mã PA hoặc tiêu đề..."
              class="w-full pl-9 pr-3 py-2 rounded-lg border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500"
            />
          </div>
          <FilterDropdown
            v-model="catFilter"
            label="Lĩnh vực"
            :icon="Tag"
            :options="catOptions"
          />
          <FilterDropdown
            v-model="deptFilter"
            label="Phòng ban"
            :icon="Building2"
            :options="deptOptions"
          />
          <FilterDropdown
            v-model="prioFilter"
            label="Mức độ"
            :icon="CircleDot"
            :options="prioOptions"
          />
          <button
            @click="slaOnly = !slaOnly"
            class="flex items-center gap-1 px-3 py-2 rounded-lg border text-sm font-medium transition cursor-pointer"
            :class="
              slaOnly
                ? 'bg-red-50 border-red-200 text-red-600'
                : 'bg-white border-slate-200 text-slate-500 hover:border-slate-300'
            "
          >
            <AlertTriangle :size="14" /> Quá hạn SLA
          </button>
          <button
            v-if="hasFilters"
            @click="clearFilters"
            class="flex items-center gap-1 px-2.5 py-2 text-slate-400 hover:text-slate-600 transition cursor-pointer text-sm"
          >
            <X :size="14" /> Xóa lọc
          </button>
        </div>
      </div>

      <!-- Hiện role hiện tại — đổi trong auth.js dòng role: "Admin" để test -->
      <div class="flex items-center gap-2 mt-3 px-1">
        <span class="text-xs text-slate-400">Vai trò hiện tại:</span>
        <span
          class="text-xs font-semibold px-2 py-0.5 rounded-full"
          :class="{
            'bg-red-100 text-red-700': currentRole === 'Admin',
            'bg-blue-100 text-blue-700': currentRole === 'Dispatcher',
            'bg-amber-100 text-amber-700': currentRole === 'Assignee',
            'bg-slate-100 text-slate-600': currentRole === 'Manager',
          }"
        >
          {{ roleLabel }}
        </span>
        <span v-if="isDragDisabled" class="text-xs text-slate-400 italic"
          >— chỉ xem, không kéo thả</span
        >
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
              <h3 :class="[col.accent]" class="text-sm font-semibold">
                {{ col.label }}
              </h3>
              <span
                class="px-1.5 py-0.5 rounded-full bg-slate-100 text-slate-500 text-xs font-semibold"
              >
                {{ getFilteredTickets(col.key).length }}
              </span>
            </div>
            <button class="text-slate-300 hover:text-slate-500 transition p-1">
              <MoreHorizontal :size="16" />
            </button>
          </div>

          <!-- Drop zone — Admin + Manager không được kéo thả -->
          <draggable
            :list="tickets[col.key]"
            group="kanban"
            item-key="id"
            :disabled="currentRole === 'Manager'"
            class="flex-1 rounded-xl p-2 space-y-2.5 transition-all min-h-[200px] bg-slate-50/50 border border-transparent"
            ghost-class="opacity-40"
            drag-class="kanban-dragging"
            @change="(evt) => onDragChange(evt, col.key)"
          >
            <template #item="{ element }">
              <div
                v-show="isTicketVisible(element)"
                @click="
                  currentRole !== 'Admin' &&
                  $router.push({
                    name: 'TicketDetail',
                    params: { id: element.id },
                  })
                "
                :class="[
                  'bg-white rounded-lg border shadow-sm transition-all group',
                  isDragDisabled
                    ? 'cursor-default'
                    : 'cursor-grab active:cursor-grabbing hover:shadow-md hover:border-slate-200',
                  element.slaBreached
                    ? 'border-red-200 bg-red-50/30'
                    : 'border-slate-100',
                ]"
              >
                <!-- Card header -->
                <div class="p-3 pb-2">
                  <div class="flex items-center justify-between mb-1.5">
                    <span class="text-xs font-bold text-red-600">{{
                      element.code
                    }}</span>
                    <div class="flex items-center gap-1.5">
                      <span
                        class="w-2 h-2 rounded-full ring-2"
                        :class="
                          getPriorityCfg(element.priority).color +
                          ' ' +
                          getPriorityCfg(element.priority).ring
                        "
                        :title="getPriorityCfg(element.priority).label"
                      ></span>
                      <span class="text-xs text-slate-400">{{
                        getPriorityCfg(element.priority).label
                      }}</span>
                    </div>
                  </div>
                  <p
                    class="text-sm font-medium text-slate-800 line-clamp-2 leading-snug"
                  >
                    {{ element.title }}
                  </p>
                </div>

                <!-- Tags -->
                <div class="px-3 pb-2 flex flex-wrap gap-1.5">
                  <span
                    :class="element.categoryColor"
                    class="px-2 py-0.5 rounded text-[11px] font-medium"
                  >
                    {{ element.category }}
                  </span>
                  <span
                    v-if="element.department"
                    class="px-2 py-0.5 rounded bg-slate-100 text-slate-600 text-[11px] font-medium"
                  >
                    {{ element.department }}
                  </span>
                </div>

                <!-- Footer -->
                <div class="px-3 pb-2.5 flex items-center justify-between">
                  <div class="flex items-center gap-1.5">
                    <div
                      class="w-5 h-5 rounded-full bg-slate-200 text-slate-600 flex items-center justify-center text-[9px] font-bold"
                    >
                      {{ element.reporterInitials }}
                    </div>
                    <span class="text-slate-500 text-[11px]">{{
                      element.reporter
                    }}</span>
                  </div>
                  <div class="flex items-center gap-1.5">
                    <span
                      v-if="element.slaBreached"
                      class="inline-flex items-center gap-0.5 text-red-500 text-[11px] font-medium"
                    >
                      <AlertTriangle :size="11" /> QUÁ HẠN
                    </span>
                    <span
                      v-else-if="
                        element.slaRemaining && element.slaRemaining !== '—'
                      "
                      class="inline-flex items-center gap-1 text-slate-400 text-[11px]"
                    >
                      <Clock :size="11" /> {{ element.slaRemaining }}
                    </span>
                    <span v-else class="text-slate-300 text-[11px]">{{
                      element.createdAt
                    }}</span>
                  </div>
                </div>
              </div>
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
                <p class="text-slate-400 text-sm">Trống</p>
              </div>
            </template>
          </draggable>
        </div>
      </div>
    </div>

    <!-- REJECTED SECTION (thu gọn) -->
    <div class="shrink-0 px-1 pb-2">
      <div
        class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden mt-4"
      >
        <button
          @click="rejectedExpanded = !rejectedExpanded"
          class="w-full flex items-center justify-between px-5 py-3.5 hover:bg-slate-50/50 transition cursor-pointer"
        >
          <div class="flex items-center gap-2.5">
            <div
              class="w-7 h-7 rounded-lg bg-red-500 text-white flex items-center justify-center"
            >
              <Ban :size="15" />
            </div>
            <h3 class="text-red-600 text-sm font-semibold">Đã từ chối</h3>
            <span
              class="px-1.5 py-0.5 rounded-full bg-red-50 text-red-500 text-xs font-semibold"
            >
              {{ tickets.Rejected.length }}
            </span>
          </div>
          <ChevronDown
            :size="16"
            class="text-slate-400 transition-transform"
            :class="rejectedExpanded ? 'rotate-180' : ''"
          />
        </button>
        <div v-show="rejectedExpanded" class="px-5 pb-4">
          <div class="flex gap-3 overflow-x-auto pb-2">
            <div
              v-for="t in tickets.Rejected"
              :key="t.id"
              class="min-w-[260px] max-w-[280px] bg-red-50/50 border border-red-100 rounded-lg p-3 flex-shrink-0"
            >
              <div class="flex items-center justify-between mb-1">
                <span class="text-xs font-bold text-red-600">{{ t.code }}</span>
                <span class="text-xs text-slate-400">{{ t.createdAt }}</span>
              </div>
              <p class="text-sm text-slate-700 font-medium truncate">
                {{ t.title }}
              </p>
              <p
                v-if="t.rejectionReason"
                class="text-xs text-red-500 mt-1 truncate"
              >
                Lý do: {{ t.rejectionReason }}
              </p>
              <div class="flex items-center gap-1.5 mt-2">
                <div
                  class="w-5 h-5 rounded-full bg-slate-200 text-slate-600 flex items-center justify-center text-[9px] font-bold"
                >
                  {{ t.reporterInitials }}
                </div>
                <span class="text-slate-500 text-[11px]">{{ t.reporter }}</span>
              </div>
            </div>
            <div
              v-if="tickets.Rejected.length === 0"
              class="py-4 text-center w-full"
            >
              <p class="text-sm text-slate-400">Chưa có phản ánh bị từ chối</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════
         MODAL 1: Duyệt & Giao việc (New → Assigned)
         Actor: Dispatcher/Admin
         API: PUT /approve + PUT /assign
    ════════════════════════════════════════════════════════ -->
    <div
      v-if="approveModal.show"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="cancelTransition"
    >
      <div class="bg-white rounded-lg shadow-xl w-full max-w-md mx-4">
        <div
          class="flex items-center justify-between p-4 border-b border-slate-100"
        >
          <h3 class="text-lg font-semibold text-slate-800">
            Duyệt & Giao việc
          </h3>
          <button
            @click="cancelTransition"
            class="p-1 text-slate-400 hover:text-slate-600"
          >
            <X :size="20" />
          </button>
        </div>
        <div class="p-4 space-y-4">
          <div>
            <p class="text-sm text-slate-500 mb-0.5">Phản ánh</p>
            <p class="text-sm font-semibold text-slate-800">
              {{ approveModal.ticket?.code }} — {{ approveModal.ticket?.title }}
            </p>
          </div>

          <!-- Mức ưu tiên -->
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

          <!-- Chọn đơn vị -->
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1"
              >Giao cho đơn vị <span class="text-red-500">*</span></label
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
              >Ghi chú</label
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
            @click="cancelTransition"
            class="px-4 py-2.5 text-sm font-medium text-slate-600 hover:text-slate-800 transition"
          >
            Huỷ
          </button>
          <button
            @click="confirmApproveAndAssign"
            :disabled="!approveModal.departmentId"
            class="px-5 py-2.5 rounded-xl text-sm font-medium text-white bg-red-600 hover:bg-red-700 shadow-sm disabled:opacity-50 disabled:cursor-not-allowed transition"
          >
            Xác nhận duyệt
          </button>
        </div>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════
         MODAL 2: Từ chối (New → Rejected)
         Actor: Dispatcher/Admin
         API: PUT /reject
    ════════════════════════════════════════════════════════ -->
    <div
      v-if="rejectModal.show"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="cancelTransition"
    >
      <div class="bg-white rounded-lg shadow-xl w-full max-w-md mx-4">
        <div
          class="flex items-center justify-between p-4 border-b border-slate-100"
        >
          <h3 class="text-lg font-semibold text-slate-800">Từ chối phản ánh</h3>
          <button
            @click="cancelTransition"
            class="p-1 text-slate-400 hover:text-slate-600"
          >
            <X :size="20" />
          </button>
        </div>
        <div class="p-4 space-y-4">
          <div>
            <p class="text-sm text-slate-500 mb-0.5">Phản ánh</p>
            <p class="text-sm font-semibold text-slate-800">
              {{ rejectModal.ticket?.code }} — {{ rejectModal.ticket?.title }}
            </p>
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1"
              >Lý do từ chối <span class="text-red-500">*</span></label
            >
            <textarea
              v-model="rejectModal.reason"
              rows="3"
              placeholder="Nhập lý do từ chối..."
              class="w-full px-3 py-2 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 resize-none"
            ></textarea>
          </div>
          <div class="flex flex-wrap gap-2">
            <button
              v-for="r in quickRejectReasons"
              :key="r"
              @click="rejectModal.reason = r"
              class="px-3 py-1.5 text-xs font-medium rounded-full border border-slate-200 text-slate-600 hover:border-red-200 hover:text-red-600 hover:bg-red-50 transition"
            >
              {{ r }}
            </button>
          </div>
        </div>
        <div
          class="flex items-center justify-end gap-2 p-4 border-t border-slate-100 bg-slate-50/50 rounded-b-lg"
        >
          <button
            @click="cancelTransition"
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

    <!-- ═══════════════════════════════════════════════════════
         MODAL 3: Báo cáo kết quả (InProgress → PendingVerification)
         Actor: Assignee
         API: PUT /result (multipart/form-data)
    ════════════════════════════════════════════════════════ -->
    <div
      v-if="resultModal.show"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="cancelTransition"
    >
      <div class="bg-white rounded-lg shadow-xl w-full max-w-md mx-4">
        <div
          class="flex items-center justify-between p-4 border-b border-slate-100"
        >
          <h3 class="text-lg font-semibold text-slate-800">
            Báo cáo kết quả xử lý
          </h3>
          <button
            @click="cancelTransition"
            class="p-1 text-slate-400 hover:text-slate-600"
          >
            <X :size="20" />
          </button>
        </div>
        <div class="p-4 space-y-4">
          <div>
            <p class="text-sm text-slate-500 mb-0.5">Phản ánh</p>
            <p class="text-sm font-semibold text-slate-800">
              {{ resultModal.ticket?.code }} — {{ resultModal.ticket?.title }}
            </p>
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1"
              >Nội dung kết quả <span class="text-red-500">*</span></label
            >
            <textarea
              v-model="resultModal.note"
              rows="4"
              placeholder="Mô tả chi tiết kết quả xử lý..."
              class="w-full px-3 py-2 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 resize-none"
            ></textarea>
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1"
              >Ảnh minh chứng</label
            >
            <div
              class="border-2 border-dashed border-slate-200 rounded-lg p-6 text-center hover:border-slate-300 transition cursor-pointer"
            >
              <ImageIcon :size="24" class="mx-auto text-slate-400 mb-2" />
              <p class="text-sm text-slate-500">Kéo thả hoặc chọn ảnh</p>
              <p class="text-xs text-slate-400 mt-1">JPG, PNG · Tối đa 10MB</p>
            </div>
          </div>
        </div>
        <div
          class="flex items-center justify-end gap-2 p-4 border-t border-slate-100 bg-slate-50/50 rounded-b-lg"
        >
          <button
            @click="cancelTransition"
            class="px-4 py-2.5 text-sm font-medium text-slate-600 hover:text-slate-800 transition"
          >
            Huỷ
          </button>
          <button
            @click="confirmResult"
            :disabled="!resultModal.note.trim()"
            class="px-5 py-2.5 rounded-xl text-sm font-medium text-white bg-red-600 hover:bg-red-700 shadow-sm disabled:opacity-50 disabled:cursor-not-allowed transition"
          >
            Gửi kết quả
          </button>
        </div>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════
         MODAL 4: Chuyển trả (InProgress → Assigned)
         Actor: Assignee
         API: PUT /reassign
    ════════════════════════════════════════════════════════ -->
    <div
      v-if="reassignModal.show"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="cancelTransition"
    >
      <div class="bg-white rounded-lg shadow-xl w-full max-w-md mx-4">
        <div
          class="flex items-center justify-between p-4 border-b border-slate-100"
        >
          <h3 class="text-lg font-semibold text-slate-800">
            Chuyển trả phản ánh
          </h3>
          <button
            @click="cancelTransition"
            class="p-1 text-slate-400 hover:text-slate-600"
          >
            <X :size="20" />
          </button>
        </div>
        <div class="p-4 space-y-4">
          <div>
            <p class="text-sm text-slate-500 mb-0.5">Phản ánh</p>
            <p class="text-sm font-semibold text-slate-800">
              {{ reassignModal.ticket?.code }} —
              {{ reassignModal.ticket?.title }}
            </p>
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1"
              >Lý do chuyển trả <span class="text-red-500">*</span></label
            >
            <textarea
              v-model="reassignModal.reason"
              rows="3"
              placeholder="Nhập lý do chuyển trả hồ sơ..."
              class="w-full px-3 py-2 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 resize-none"
            ></textarea>
          </div>
        </div>
        <div
          class="flex items-center justify-end gap-2 p-4 border-t border-slate-100 bg-slate-50/50 rounded-b-lg"
        >
          <button
            @click="cancelTransition"
            class="px-4 py-2.5 text-sm font-medium text-slate-600 hover:text-slate-800 transition"
          >
            Huỷ
          </button>
          <button
            @click="confirmReassign"
            :disabled="!reassignModal.reason.trim()"
            class="px-5 py-2.5 rounded-xl text-sm font-medium text-white bg-amber-600 hover:bg-amber-700 shadow-sm disabled:opacity-50 disabled:cursor-not-allowed transition"
          >
            Xác nhận chuyển trả
          </button>
        </div>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════
         MODAL 5: Xác nhận đóng (PendingVerification → Closed)
         Actor: Dispatcher/Admin
         API: PUT /close
    ════════════════════════════════════════════════════════ -->
    <div
      v-if="closeModal.show"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="cancelTransition"
    >
      <div class="bg-white rounded-lg shadow-xl w-full max-w-sm mx-4">
        <div
          class="flex items-center justify-between p-4 border-b border-slate-100"
        >
          <h3 class="text-lg font-semibold text-slate-800">Đóng phản ánh</h3>
          <button
            @click="cancelTransition"
            class="p-1 text-slate-400 hover:text-slate-600"
          >
            <X :size="20" />
          </button>
        </div>
        <div class="p-4">
          <p class="text-sm text-slate-600">
            Xác nhận đóng phản ánh
            <strong class="text-red-600">{{ closeModal.ticket?.code }}</strong
            >?
          </p>
          <p class="text-sm text-slate-500 mt-2">
            {{ closeModal.ticket?.title }}
          </p>
          <p class="text-xs text-slate-400 mt-3">
            Phản ánh sẽ chuyển sang trạng thái "Đã đóng" và thông báo cho người
            dân.
          </p>
        </div>
        <div
          class="flex items-center justify-end gap-2 p-4 border-t border-slate-100 bg-slate-50/50 rounded-b-lg"
        >
          <button
            @click="cancelTransition"
            class="px-4 py-2.5 text-sm font-medium text-slate-600 hover:text-slate-800 transition"
          >
            Huỷ
          </button>
          <button
            @click="confirmClose"
            class="px-5 py-2.5 rounded-xl text-sm font-medium text-white bg-red-600 hover:bg-red-700 shadow-sm transition"
          >
            Xác nhận đóng
          </button>
        </div>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════
         MODAL 6: Tiếp nhận xử lý (Assigned → InProgress)
         Actor: Assignee
         API: PUT /inprogress
    ════════════════════════════════════════════════════════ -->
    <div
      v-if="startModal.show"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="cancelTransition"
    >
      <div class="bg-white rounded-lg shadow-xl w-full max-w-sm mx-4">
        <div
          class="flex items-center justify-between p-4 border-b border-slate-100"
        >
          <h3 class="text-lg font-semibold text-slate-800">Bắt đầu xử lý</h3>
          <button
            @click="cancelTransition"
            class="p-1 text-slate-400 hover:text-slate-600"
          >
            <X :size="20" />
          </button>
        </div>
        <div class="p-4">
          <p class="text-sm text-slate-600">
            Tiếp nhận xử lý phản ánh
            <strong class="text-red-600">{{ startModal.ticket?.code }}</strong
            >?
          </p>
          <p class="text-sm text-slate-500 mt-2">
            {{ startModal.ticket?.title }}
          </p>
          <p class="text-xs text-amber-600 mt-3 flex items-center gap-1">
            <Clock :size="12" /> SLA sẽ bắt đầu tính từ thời điểm này
          </p>
        </div>
        <div
          class="flex items-center justify-end gap-2 p-4 border-t border-slate-100 bg-slate-50/50 rounded-b-lg"
        >
          <button
            @click="cancelTransition"
            class="px-4 py-2.5 text-sm font-medium text-slate-600 hover:text-slate-800 transition"
          >
            Huỷ
          </button>
          <button
            @click="confirmStart"
            class="px-5 py-2.5 rounded-xl text-sm font-medium text-white bg-amber-600 hover:bg-amber-700 shadow-sm transition"
          >
            Bắt đầu xử lý
          </button>
        </div>
      </div>
    </div>

    <!-- Toast thông báo -->
    <div
      v-if="toast.show"
      class="fixed bottom-6 right-6 z-50 flex items-center gap-2 px-4 py-3 rounded-xl shadow-lg text-sm font-medium transition-all"
      :class="
        toast.type === 'success'
          ? 'bg-green-600 text-white'
          : toast.type === 'error'
            ? 'bg-red-600 text-white'
            : 'bg-amber-500 text-white'
      "
    >
      <CheckCircle2 v-if="toast.type === 'success'" :size="16" />
      <AlertTriangle v-else-if="toast.type === 'error'" :size="16" />
      <Ban v-else :size="16" />
      {{ toast.message }}
    </div>
  </div>
</template>

<style>
.kanban-dragging {
  transform: rotate(-2deg) scale(1.03) !important;
  box-shadow: 0 20px 25px -5px rgb(0 0 0 / 0.1) !important;
  outline: 2px solid rgba(218, 37, 29, 0.2) !important;
  outline-offset: 2px;
}
</style>

<script setup>
import { ref, reactive, computed } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "@/stores/auth";
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
  Clock,
  UserPlus,
  Play,
  CheckCircle2,
  ShieldCheck,
  ImageIcon,
} from "lucide-vue-next";

const router = useRouter();
const auth = useAuthStore();

// Lấy role từ auth store — đổi role trong stores/auth.js để test
const currentRole = computed(() => auth.userRole || "Dispatcher");
const roleLabel = computed(() => {
  const map = {
    Admin: "Quản trị viên",
    Dispatcher: "Điều phối viên",
    Assignee: "Cán bộ xử lý",
    Manager: "Lãnh đạo",
  };
  return map[currentRole.value] || currentRole.value;
});

// ══════════════════════════════════════════════════════════
// MA TRẬN CHUYỂN TRẠNG THÁI HỢP LỆ
// Admin + Manager: chỉ xem, KHÔNG kéo thả
// Dispatcher: duyệt, từ chối, đóng
// Assignee: tiếp nhận, báo cáo KQ, chuyển trả
// ══════════════════════════════════════════════════════════
const TRANSITIONS = {
  "New→Assigned": { roles: ["Dispatcher"], modal: "approve" },
  "New→Rejected": { roles: ["Dispatcher"], modal: "reject" },
  "Assigned→InProgress": { roles: ["Assignee"], modal: "start" },
  "InProgress→PendingVerification": { roles: ["Assignee"], modal: "result" },
  "InProgress→Assigned": { roles: ["Assignee"], modal: "reassign" },
  "PendingVerification→Closed": { roles: ["Dispatcher"], modal: "close" },
};

// ── Filter sub-component ────────────────────────────────
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

// ── Column config — dynamic theo role ────────────────────
// Assignee chỉ thấy 3 cột: Đã giao → Đang xử lý → Chờ xác nhận
const allColumns = [
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
    key: "PendingVerification",
    label: "Chờ xác nhận",
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
const assigneeColumns = ["Assigned", "InProgress", "PendingVerification"];

const columns = computed(() => {
  if (currentRole.value === "Assignee") {
    return allColumns.filter((c) => assigneeColumns.includes(c.key));
  }
  return allColumns;
});

// Draggable disabled cho Admin + Manager (chỉ xem)
const isDragDisabled = computed(() =>
  ["Admin", "Manager"].includes(currentRole.value),
);

// Mock departmentId của Assignee — sẽ thay bằng auth.user.departmentId
const myDepartmentId = "dept1";

// ── Filter state ────────────────────────────────────────
const search = ref("");
const catFilter = ref("");
const deptFilter = ref("");
const prioFilter = ref("");
const slaOnly = ref(false);
const rejectedExpanded = ref(false);

const catOptions = [
  { value: "Giao thông", label: "Giao thông" },
  { value: "Môi trường", label: "Môi trường" },
  { value: "An ninh", label: "An ninh trật tự" },
  { value: "Điện nước", label: "Điện - Nước" },
];
const deptOptions = [
  { value: "Phòng Hạ tầng", label: "Phòng Hạ tầng" },
  { value: "Phòng TN-MT", label: "Phòng TN&MT" },
  { value: "Cty Cấp nước", label: "Cty Cấp nước" },
  { value: "Phòng Đô thị", label: "Phòng Đô thị" },
];
const prioOptions = [
  { value: "Low", label: "Thấp" },
  { value: "Normal", label: "Bình thường" },
  { value: "High", label: "Cao" },
  { value: "Urgent", label: "Khẩn cấp" },
];
const priorityOptions = [
  { value: "Low", label: "Thấp" },
  { value: "Normal", label: "TB" },
  { value: "High", label: "Cao" },
  { value: "Urgent", label: "Khẩn" },
];
const departments = [
  { id: "dept1", name: "Phòng Hạ tầng Kỹ thuật" },
  { id: "dept2", name: "Phòng TN & Môi trường" },
  { id: "dept3", name: "Công an TP. Cao Lãnh" },
  { id: "dept4", name: "Cty Cấp nước Đồng Tháp" },
  { id: "dept5", name: "UBND Phường 1" },
];
const quickRejectReasons = [
  "Không có hình ảnh minh chứng",
  "Nội dung không rõ ràng",
  "Trùng lặp phản ánh đã có",
  "Không thuộc phạm vi tiếp nhận",
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
  // Assignee chỉ thấy ticket thuộc phòng ban mình
  if (
    currentRole.value === "Assignee" &&
    t.departmentId &&
    t.departmentId !== myDepartmentId
  )
    return false;
  return true;
}

// Trọng số ưu tiên: Urgent > High > Normal > Low
const PRIORITY_WEIGHT = { Urgent: 0, High: 1, Normal: 2, Low: 3 };

// Sắp xếp 3 tầng: priority → createdAt (sớm nhất trước) → slaHours (ngắn nhất trước)
function sortTickets(arr) {
  return [...arr].sort((a, b) => {
    // 1. Ưu tiên cao hơn lên trước
    const pa = PRIORITY_WEIGHT[a.priority] ?? 2;
    const pb = PRIORITY_WEIGHT[b.priority] ?? 2;
    if (pa !== pb) return pa - pb;
    // 2. Nộp sớm hơn lên trước
    const da = new Date(a.createdAtDate || 0).getTime();
    const db = new Date(b.createdAtDate || 0).getTime();
    if (da !== db) return da - db;
    // 3. SLA ngắn hơn lên trước (đốc thúc tiến độ)
    return (a.slaHours || 999) - (b.slaHours || 999);
  });
}

function getFilteredTickets(status) {
  const filtered = tickets[status].filter(isTicketVisible);
  return sortTickets(filtered);
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

function getPriorityCfg(priority) {
  const cfg = {
    High: { color: "bg-red-500", ring: "ring-red-200", label: "Cao" },
    Urgent: { color: "bg-red-600", ring: "ring-red-300", label: "Khẩn" },
    Normal: { color: "bg-blue-500", ring: "ring-blue-200", label: "TB" },
    Low: { color: "bg-slate-400", ring: "ring-slate-200", label: "Thấp" },
  };
  return cfg[priority] || cfg.Normal;
}

// ══════════════════════════════════════════════════════════
// MOCK TICKET DATA
// ══════════════════════════════════════════════════════════
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
      department: null,
      departmentId: null,
      slaRemaining: "6 ngày",
      slaHours: 144,
      slaBreached: false,
      createdAt: "02/04",
      createdAtDate: "2026-04-02T08:00:00",
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
      department: null,
      departmentId: null,
      slaRemaining: "5 ngày",
      slaHours: 120,
      slaBreached: false,
      createdAt: "02/04",
      createdAtDate: "2026-04-02T10:00:00",
      status: "New",
    },
    {
      id: "t5",
      code: "PA-042026-005",
      title: "Đèn tín hiệu giao thông hư tại ngã tư Hùng Vương",
      category: "Giao thông",
      categoryColor: "bg-blue-100 text-blue-700",
      priority: "High",
      reporter: "Lê Văn C",
      reporterInitials: "LC",
      department: null,
      departmentId: null,
      slaRemaining: "4 ngày",
      slaHours: 96,
      slaBreached: false,
      createdAt: "03/04",
      createdAtDate: "2026-04-03T07:30:00",
      status: "New",
    },
    {
      id: "t9",
      code: "PA-042026-009",
      title: "Quán nhậu mở nhạc xuyên đêm gây mất trật tự",
      category: "An ninh",
      categoryColor: "bg-orange-100 text-orange-700",
      priority: "Urgent",
      reporter: "Phạm Văn D",
      reporterInitials: "PD",
      department: null,
      departmentId: null,
      slaRemaining: "1 ngày",
      slaHours: 24,
      slaBreached: false,
      createdAt: "04/04",
      createdAtDate: "2026-04-04T22:00:00",
      status: "New",
    },
  ],
  Assigned: [
    {
      id: "t3",
      code: "PA-032026-012",
      title: "Vỉa hè bị lấn chiếm buôn bán trên đường Lê Duẩn, Phường 1",
      category: "Giao thông",
      categoryColor: "bg-blue-100 text-blue-700",
      priority: "Normal",
      reporter: "Hoàng Văn E",
      reporterInitials: "VE",
      department: "Phòng Đô thị",
      departmentId: "dept4",
      slaRemaining: "3 ngày",
      slaHours: 72,
      slaBreached: false,
      createdAt: "28/03",
      createdAtDate: "2026-03-28T09:00:00",
      status: "Assigned",
    },
    {
      id: "t8",
      code: "PA-032026-011",
      title: "Cây xanh ngã đổ chắn ngang đường Tôn Đức Thắng sau bão",
      category: "Môi trường",
      categoryColor: "bg-emerald-100 text-emerald-700",
      priority: "High",
      reporter: "Đỗ Thanh F",
      reporterInitials: "TF",
      department: "Phòng Hạ tầng",
      departmentId: "dept1",
      slaRemaining: "1 ngày",
      slaHours: 24,
      slaBreached: false,
      createdAt: "30/03",
      createdAtDate: "2026-03-30T06:00:00",
      status: "Assigned",
    },
    {
      id: "t13",
      code: "PA-042026-013",
      title: "Hố ga mất nắp gây nguy hiểm trên đường Phạm Hữu Lầu",
      category: "Giao thông",
      categoryColor: "bg-blue-100 text-blue-700",
      priority: "Urgent",
      reporter: "Trương Văn Đạt",
      reporterInitials: "VĐ",
      department: "Phòng Hạ tầng",
      departmentId: "dept1",
      slaRemaining: "12 giờ",
      slaHours: 12,
      slaBreached: false,
      createdAt: "07/04",
      createdAtDate: "2026-04-07T14:00:00",
      status: "Assigned",
    },
  ],
  InProgress: [
    {
      id: "t4",
      code: "PA-032026-010",
      title: "Mất nước sinh hoạt kéo dài 3 ngày tại KDC Mỹ Trà",
      category: "Điện nước",
      categoryColor: "bg-amber-100 text-amber-700",
      priority: "High",
      reporter: "Vũ Thị G",
      reporterInitials: "TG",
      department: "Cty Cấp nước",
      departmentId: "dept4",
      slaRemaining: null,
      slaHours: 0,
      slaBreached: true,
      createdAt: "27/03",
      createdAtDate: "2026-03-27T10:00:00",
      status: "InProgress",
    },
    {
      id: "t10",
      code: "PA-032026-009",
      title: "Rác thải công nghiệp đổ trộm ven sông Tiền, xã An Bình",
      category: "Môi trường",
      categoryColor: "bg-emerald-100 text-emerald-700",
      priority: "High",
      reporter: "Bùi Văn H",
      reporterInitials: "VH",
      department: "Phòng TN-MT",
      departmentId: "dept2",
      slaRemaining: "2 ngày",
      slaHours: 48,
      slaBreached: false,
      createdAt: "29/03",
      createdAtDate: "2026-03-29T08:00:00",
      status: "InProgress",
    },
    {
      id: "t11",
      code: "PA-032026-015",
      title: "Cống thoát nước bị vỡ gây ngập đường Lê Lợi",
      category: "Giao thông",
      categoryColor: "bg-blue-100 text-blue-700",
      priority: "Normal",
      reporter: "Ngô Thanh I",
      reporterInitials: "TI",
      department: "Phòng Hạ tầng",
      departmentId: "dept1",
      slaRemaining: "4 ngày",
      slaHours: 96,
      slaBreached: false,
      createdAt: "01/04",
      createdAtDate: "2026-04-01T11:00:00",
      status: "InProgress",
    },
  ],
  PendingVerification: [
    {
      id: "t6",
      code: "PA-032026-007",
      title: "Cầu gỗ xuống cấp nguy hiểm tại xã Mỹ Tân",
      category: "Giao thông",
      categoryColor: "bg-blue-100 text-blue-700",
      priority: "High",
      reporter: "Lý Thị K",
      reporterInitials: "TK",
      department: "Phòng Hạ tầng",
      departmentId: "dept1",
      slaRemaining: "—",
      slaHours: 0,
      slaBreached: false,
      createdAt: "25/03",
      createdAtDate: "2026-03-25T09:00:00",
      status: "PendingVerification",
    },
    {
      id: "t12",
      code: "PA-032026-006",
      title: "Ngập nước kéo dài trên đường Nguyễn Thái Học sau mưa lớn",
      category: "Giao thông",
      categoryColor: "bg-blue-100 text-blue-700",
      priority: "Normal",
      reporter: "Mai Văn L",
      reporterInitials: "VL",
      department: "Phòng Hạ tầng",
      departmentId: "dept1",
      slaRemaining: "—",
      slaHours: 0,
      slaBreached: false,
      createdAt: "24/03",
      createdAtDate: "2026-03-24T15:00:00",
      status: "PendingVerification",
    },
  ],
  Closed: [
    {
      id: "t7",
      code: "PA-032026-003",
      title: "Xả rác trộm bến phà Cao Lãnh đã được dọn dẹp",
      category: "Môi trường",
      categoryColor: "bg-emerald-100 text-emerald-700",
      priority: "Normal",
      reporter: "Trần Minh M",
      reporterInitials: "TM",
      department: "Phòng TN-MT",
      departmentId: "dept2",
      slaRemaining: "—",
      slaHours: 0,
      slaBreached: false,
      createdAt: "20/03",
      createdAtDate: "2026-03-20T10:00:00",
      status: "Closed",
    },
  ],
  Rejected: [
    {
      id: "r1",
      code: "PA-042026-008",
      title: "Phản ánh không rõ nội dung",
      category: "Giao thông",
      categoryColor: "bg-blue-100 text-blue-700",
      priority: "Low",
      reporter: "Ẩn danh",
      reporterInitials: "AD",
      department: null,
      departmentId: null,
      slaRemaining: "—",
      slaHours: 0,
      slaBreached: false,
      createdAt: "03/04",
      createdAtDate: "2026-04-03T08:00:00",
      status: "Rejected",
      rejectionReason: "Nội dung không rõ ràng, thiếu hình ảnh",
    },
  ],
});

// ══════════════════════════════════════════════════════════
// SNAPSHOT — lưu trạng thái trước khi kéo để rollback
// ══════════════════════════════════════════════════════════
// ══════════════════════════════════════════════════════════
// SNAPSHOT — lưu trạng thái để rollback khi huỷ modal
// Cần deep copy vì vuedraggable mutate trực tiếp array
// ══════════════════════════════════════════════════════════
let snapshot = null;

function saveSnapshot() {
  snapshot = JSON.parse(
    JSON.stringify(
      Object.fromEntries(Object.keys(tickets).map((k) => [k, tickets[k]])),
    ),
  );
}

function rollback() {
  if (!snapshot) return;
  for (const key of Object.keys(snapshot)) {
    tickets[key].splice(0, tickets[key].length, ...snapshot[key]);
  }
  snapshot = null;
}

// ══════════════════════════════════════════════════════════
// MODAL STATE
// ══════════════════════════════════════════════════════════
const approveModal = ref({
  show: false,
  ticket: null,
  priority: "Normal",
  departmentId: "",
  note: "",
});
const rejectModal = ref({ show: false, ticket: null, reason: "" });
const resultModal = ref({ show: false, ticket: null, note: "" });
const reassignModal = ref({ show: false, ticket: null, reason: "" });
const closeModal = ref({ show: false, ticket: null });
const startModal = ref({ show: false, ticket: null });

// Ticket đang chờ xác nhận transition
let pendingTicket = null;
let pendingFromStatus = null;
let pendingToStatus = null;

// Toast notification
const toast = ref({ show: false, message: "", type: "success" });
function showToast(message, type = "success") {
  toast.value = { show: true, message, type };
  setTimeout(() => {
    toast.value.show = false;
  }, 3000);
}

// ══════════════════════════════════════════════════════════
// XỬ LÝ KÉO THẢ — trái tim của Kanban workflow
// ══════════════════════════════════════════════════════════
function onDragChange(evt, toStatus) {
  if (!evt.added) return;

  const ticket = evt.added.element;
  const fromStatus = ticket.status;

  if (fromStatus === toStatus) return;

  const transitionKey = `${fromStatus}→${toStatus}`;
  const rule = TRANSITIONS[transitionKey];

  // ── Validate 1: Transition có tồn tại không? ──────────
  if (!rule) {
    showToast(
      `Không thể chuyển từ "${getStatusLabel(fromStatus)}" sang "${getStatusLabel(toStatus)}"`,
      "error",
    );
    // Rollback: đưa ticket về cột cũ thủ công
    const idx = tickets[toStatus].findIndex((t) => t.id === ticket.id);
    if (idx > -1) tickets[toStatus].splice(idx, 1);
    tickets[fromStatus].push(ticket);
    return;
  }

  // ── Validate 2: Role có quyền không? ──────────────────
  if (!rule.roles.includes(currentRole.value)) {
    showToast(
      `Quyền "${roleLabel.value}" không được phép thực hiện thao tác này`,
      "error",
    );
    const idx = tickets[toStatus].findIndex((t) => t.id === ticket.id);
    if (idx > -1) tickets[toStatus].splice(idx, 1);
    tickets[fromStatus].push(ticket);
    return;
  }

  // ── Lưu thông tin để rollback nếu huỷ modal ──────────
  pendingTicket = ticket;
  pendingFromStatus = fromStatus;
  pendingToStatus = toStatus;

  // ── Mở modal tương ứng ────────────────────────────────
  switch (rule.modal) {
    case "approve":
      approveModal.value = {
        show: true,
        ticket,
        priority: "Normal",
        departmentId: "",
        note: "",
      };
      break;
    case "reject":
      rejectModal.value = { show: true, ticket, reason: "" };
      break;
    case "start":
      startModal.value = { show: true, ticket };
      break;
    case "result":
      resultModal.value = { show: true, ticket, note: "" };
      break;
    case "reassign":
      reassignModal.value = { show: true, ticket, reason: "" };
      break;
    case "close":
      closeModal.value = { show: true, ticket };
      break;
  }
}

// ── Huỷ transition → rollback về cột cũ ────────────────
function cancelTransition() {
  // Đưa ticket về cột cũ khi huỷ modal
  if (pendingTicket && pendingFromStatus && pendingToStatus) {
    const idx = tickets[pendingToStatus].findIndex(
      (t) => t.id === pendingTicket.id,
    );
    if (idx > -1) tickets[pendingToStatus].splice(idx, 1);
    tickets[pendingFromStatus].push(pendingTicket);
  }
  approveModal.value.show = false;
  rejectModal.value.show = false;
  resultModal.value.show = false;
  reassignModal.value.show = false;
  closeModal.value.show = false;
  startModal.value.show = false;
  pendingTicket = null;
  pendingFromStatus = null;
  pendingToStatus = null;
}

// ── Xác nhận Duyệt + Giao việc ─────────────────────────
// TODO: gọi PUT /api/tickets/{id}/approve rồi PUT /api/tickets/{id}/assign
function confirmApproveAndAssign() {
  if (!pendingTicket) return;
  pendingTicket.status = "Assigned";
  pendingTicket.priority = approveModal.value.priority;
  const dept = departments.find(
    (d) => d.id === approveModal.value.departmentId,
  );
  pendingTicket.department = dept?.name || "";
  approveModal.value.show = false;
  showToast(`Đã duyệt và giao ${pendingTicket.code} cho ${dept?.name}`);
  pendingTicket = null;
  pendingFromStatus = null;
  pendingToStatus = null;
}

// ── Xác nhận Từ chối ────────────────────────────────────
// TODO: gọi PUT /api/tickets/{id}/reject
function confirmReject() {
  if (!pendingTicket) return;
  pendingTicket.status = "Rejected";
  pendingTicket.rejectionReason = rejectModal.value.reason;
  // Nếu ticket đang ở cột toStatus (do vuedraggable đã kéo sang), chuyển vào Rejected
  if (pendingToStatus !== "Rejected") {
    const idx = tickets[pendingToStatus].findIndex(
      (t) => t.id === pendingTicket.id,
    );
    if (idx > -1) tickets[pendingToStatus].splice(idx, 1);
    tickets.Rejected.push(pendingTicket);
  }
  rejectModal.value.show = false;
  showToast(`Đã từ chối ${pendingTicket.code}`);
  pendingTicket = null;
  pendingFromStatus = null;
  pendingToStatus = null;
}

// ── Xác nhận Bắt đầu xử lý ─────────────────────────────
// TODO: gọi PUT /api/tickets/{id}/inprogress
function confirmStart() {
  if (!pendingTicket) return;
  pendingTicket.status = "InProgress";
  startModal.value.show = false;
  showToast(`Đã tiếp nhận xử lý ${pendingTicket.code}`);
  pendingTicket = null;
  pendingFromStatus = null;
  pendingToStatus = null;
}

// ── Xác nhận Báo cáo kết quả ────────────────────────────
// TODO: gọi PUT /api/tickets/{id}/result (multipart/form-data)
function confirmResult() {
  if (!pendingTicket) return;
  pendingTicket.status = "PendingVerification";
  resultModal.value.show = false;
  showToast(`Đã gửi kết quả xử lý ${pendingTicket.code}`);
  pendingTicket = null;
  pendingFromStatus = null;
  pendingToStatus = null;
}

// ── Xác nhận Chuyển trả ─────────────────────────────────
// TODO: gọi PUT /api/tickets/{id}/reassign
function confirmReassign() {
  if (!pendingTicket) return;
  pendingTicket.status = "Assigned";
  reassignModal.value.show = false;
  showToast(`Đã chuyển trả ${pendingTicket.code}`, "warning");
  pendingTicket = null;
  pendingFromStatus = null;
  pendingToStatus = null;
}

// ── Xác nhận Đóng phản ánh ──────────────────────────────
// TODO: gọi PUT /api/tickets/{id}/close
function confirmClose() {
  if (!pendingTicket) return;
  pendingTicket.status = "Closed";
  closeModal.value.show = false;
  showToast(`Đã đóng phản ánh ${pendingTicket.code}`);
  pendingTicket = null;
  pendingFromStatus = null;
  pendingToStatus = null;
}

// ── Helpers ─────────────────────────────────────────────
function getStatusLabel(status) {
  const map = {
    New: "Mới",
    Assigned: "Đã giao",
    InProgress: "Đang xử lý",
    PendingVerification: "Chờ xác nhận",
    Closed: "Đã đóng",
    Rejected: "Từ chối",
  };
  return map[status] || status;
}

function refreshData() {
  // TODO: gọi GET /api/tickets rồi phân nhóm theo status
  showToast("Đã làm mới dữ liệu");
}
</script>
