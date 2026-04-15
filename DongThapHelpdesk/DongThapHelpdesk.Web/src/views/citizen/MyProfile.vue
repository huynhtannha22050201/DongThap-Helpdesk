<template>
  <!-- Loading state -->
  <div v-if="loading" class="flex items-center justify-center py-20">
    <Loader2 class="w-8 h-8 text-[var(--color-brand-red)] animate-spin" />
  </div>

  <!-- Error state -->
  <div v-else-if="error" class="max-w-lg mx-auto py-20 text-center">
    <AlertCircle class="w-12 h-12 text-red-400 mx-auto mb-4" />
    <p class="text-slate-600 mb-4">{{ error }}</p>
    <button
      @click="fetchData"
      class="px-4 py-2 bg-[var(--color-brand-red)] text-white rounded-xl text-sm font-medium hover:bg-[#b81f18]"
    >
      Thử lại
    </button>
  </div>
  <div v-else>
    <div class="min-h-screen bg-slate-50 pb-8">
      <!-- ═══ PROFILE HERO — compact, app-like ═══ -->
      <section class="relative overflow-hidden">
        <!-- Background gradient -->
        <div
          class="absolute inset-0 bg-gradient-to-b from-[#DA251D] to-[#b81f18]"
        />
        <!-- Subtle pattern overlay -->
        <div
          class="absolute inset-0 opacity-[0.06]"
          :style="{
            backgroundImage: `url(${lotusPattern})`,
            backgroundSize: '200px',
            backgroundRepeat: 'repeat',
          }"
        />

        <div class="relative px-4 pt-4 pb-20">
          <!-- Top bar -->
          <div class="flex items-center justify-between mb-5">
            <RouterLink
              to="/"
              class="w-8 h-8 rounded-full bg-white/15 flex items-center justify-center text-white"
            >
              <ArrowLeft :size="16" />
            </RouterLink>
            <div class="flex gap-2">
              <button
                class="w-8 h-8 rounded-full bg-white/15 flex items-center justify-center text-white relative"
              >
                <Bell :size="16" />
                <span
                  v-if="unreadCount > 0"
                  class="absolute -top-0.5 -right-0.5 w-4 h-4 rounded-full bg-[#FFC627] text-[9px] font-bold text-slate-800 flex items-center justify-center"
                >
                  {{ unreadCount }}
                </span>
              </button>
              <!-- Settings dropdown -->
              <div class="relative" ref="settingsRef">
                <button
                  @click="showSettings = !showSettings"
                  class="w-8 h-8 bg-white/20 rounded-full flex items-center justify-center hover:bg-white/30 transition-colors"
                >
                  <Settings class="w-5 h-5 text-white" />
                </button>

                <!-- Dropdown menu -->
                <div
                  v-if="showSettings"
                  class="absolute right-0 top-12 w-56 bg-white rounded-xl shadow-lg border border-slate-200 py-2 z-50"
                >
                  <button
                    @click="
                      showPasswordModal = true;
                      showSettings = false;
                    "
                    class="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-slate-700 hover:bg-slate-50 transition-colors"
                  >
                    <KeyRound class="w-4 h-4 text-slate-400" />
                    Đổi mật khẩu
                  </button>
                  <div class="my-1 border-t border-slate-100" />
                  <button
                    @click="handleLogout"
                    class="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-red-600 hover:bg-red-50 transition-colors"
                  >
                    <LogOut class="w-4 h-4" />
                    Đăng xuất
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Profile row -->
          <div class="flex items-center gap-3.5">
            <!-- Avatar -->
            <div class="relative shrink-0">
              <div
                class="w-16 h-16 rounded-2xl bg-white shadow-lg flex items-center justify-center text-[22px] font-bold text-slate-600"
              >
                {{ initials }}
              </div>
              <!-- Tier badge overlapping -->
              <div
                class="absolute -top-1.5 -right-1.5 w-6 h-6 rounded-full bg-[#FFC627] border-2 border-[#DA251D] flex items-center justify-center text-[10px] font-extrabold text-white shadow"
              >
                {{ tierLevel }}
              </div>
            </div>

            <!-- Info -->
            <div class="flex-1 min-w-0 text-white">
              <h1 class="text-[18px] font-bold truncate">
                {{ profile?.fullName }}
              </h1>
              <p class="text-white/70 text-[12px] mt-0.5 truncate">
                {{
                  [profile?.address, profile?.ward]
                    .filter(Boolean)
                    .join(", ") || "Chưa cập nhật"
                }}
              </p>
              <div class="flex items-center gap-2 mt-1.5">
                <span
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-white/15 text-[10px] font-semibold"
                >
                  <span class="w-1.5 h-1.5 rounded-full bg-emerald-400" />
                  {{ tierLabel }}
                </span>
                <span
                  v-if="profile?.consecutiveMonths"
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-[#FFC627]/20 text-[#FFC627] text-[10px] font-semibold"
                >
                  <Flame :size="9" /> {{ profile.consecutiveMonths }} tháng
                </span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- ═══ FLOATING STATS CARD — overlaps hero ═══ -->
      <section class="px-4 -mt-14 relative z-10">
        <div class="bg-white rounded-2xl shadow-lg shadow-slate-200/60 p-4">
          <!-- Points row -->
          <div class="flex items-center justify-between mb-3.5">
            <div class="flex items-center gap-2">
              <div
                class="w-9 h-9 rounded-xl bg-gradient-to-br from-amber-400 to-amber-500 flex items-center justify-center shadow-sm"
              >
                <Star :size="16" class="text-white" />
              </div>
              <div>
                <p
                  class="text-[20px] font-extrabold text-slate-800 leading-none"
                >
                  {{ points.totalPoints }}
                </p>
                <p class="text-slate-400 text-[11px]">Tổng điểm</p>
              </div>
            </div>
            <!-- Tier ring mini -->
            <div class="relative w-11 h-11">
              <svg viewBox="0 0 44 44" class="w-full h-full -rotate-90">
                <circle
                  cx="22"
                  cy="22"
                  r="18"
                  fill="none"
                  stroke="#f1f5f9"
                  stroke-width="4"
                />
                <circle
                  cx="22"
                  cy="22"
                  r="18"
                  fill="none"
                  stroke="#FFC627"
                  stroke-width="4"
                  stroke-linecap="round"
                  :stroke-dasharray="2 * Math.PI * 18"
                  :stroke-dashoffset="2 * Math.PI * 18 * (1 - tierProgress)"
                />
              </svg>
              <span
                class="absolute inset-0 flex items-center justify-center text-[10px] font-bold text-amber-600"
              >
                {{ Math.round(tierProgress * 100) }}%
              </span>
            </div>
          </div>

          <!-- Stats grid — compact 4 cols -->
          <div class="grid grid-cols-4 gap-1">
            <div class="text-center py-2 rounded-xl bg-blue-50/70">
              <p class="text-[16px] font-bold text-slate-800">
                {{ points.totalTicketsSubmitted }}
              </p>
              <p class="text-[10px] text-slate-400 mt-0.5">Đã gửi</p>
            </div>
            <div class="text-center py-2 rounded-xl bg-emerald-50/70">
              <p class="text-[16px] font-bold text-emerald-600">
                {{ points.totalTicketsApproved }}
              </p>
              <p class="text-[10px] text-slate-400 mt-0.5">Duyệt</p>
            </div>
            <div class="text-center py-2 rounded-xl bg-red-50/70">
              <p class="text-[16px] font-bold text-red-500">
                {{ points.totalTicketsRejected }}
              </p>
              <p class="text-[10px] text-slate-400 mt-0.5">Từ chối</p>
            </div>
            <div class="text-center py-2 rounded-xl bg-slate-50">
              <p class="text-[16px] font-bold text-emerald-600">
                {{ points.approvalRate }}%
              </p>
              <p class="text-[10px] text-slate-400 mt-0.5">Tỷ lệ</p>
            </div>
          </div>
        </div>
      </section>

      <!-- ═══ QUICK ACTIONS — big touch targets ═══ -->
      <section class="px-4 mt-4">
        <div class="grid grid-cols-3 gap-2.5">
          <RouterLink
            to="/gui-phan-anh"
            class="flex flex-col items-center gap-1.5 py-4 rounded-2xl bg-white shadow-sm border border-slate-100 active:scale-[0.97] transition-all"
          >
            <div
              class="w-11 h-11 rounded-xl bg-red-50 flex items-center justify-center"
            >
              <Send :size="18" class="text-[#DA251D]" />
            </div>
            <span class="text-[12px] font-semibold text-[#DA251D]"
              >Gửi phản ánh</span
            >
          </RouterLink>
          <RouterLink
            to="/tra-cuu"
            class="flex flex-col items-center gap-1.5 py-4 rounded-2xl bg-white shadow-sm border border-slate-100 active:scale-[0.97] transition-all"
          >
            <div
              class="w-11 h-11 rounded-xl bg-blue-50 flex items-center justify-center"
            >
              <Search :size="18" class="text-blue-600" />
            </div>
            <span class="text-[12px] font-semibold text-blue-600">Tra cứu</span>
          </RouterLink>
          <button
            @click="showPointHistory = !showPointHistory"
            class="flex flex-col items-center gap-1.5 py-4 rounded-2xl bg-white shadow-sm border border-slate-100 active:scale-[0.97] transition-all cursor-pointer"
          >
            <div
              class="w-11 h-11 rounded-xl bg-amber-50 flex items-center justify-center"
            >
              <Trophy :size="18" class="text-amber-600" />
            </div>
            <span class="text-[12px] font-semibold text-amber-600"
              >Điểm thưởng</span
            >
          </button>
        </div>
      </section>

      <!-- ═══ MY TICKETS — main content ═══ -->
      <section class="px-4 mt-5">
        <div class="flex items-center justify-between mb-3">
          <h2 class="text-[16px] font-bold text-slate-800">Phản ánh của tôi</h2>
          <span class="text-[12px] text-slate-400"
            >{{ filteredTickets.length }} kết quả</span
          >
        </div>

        <!-- Scrollable tabs -->
        <div class="flex gap-2 overflow-x-auto pb-2 no-scrollbar">
          <button
            v-for="tab in ticketTabs"
            :key="tab.key"
            @click="ticketFilter = tab.key"
            :class="[
              'flex items-center gap-1 px-3 py-1.5 rounded-full text-[12px] font-semibold whitespace-nowrap transition-all cursor-pointer shrink-0',
              ticketFilter === tab.key
                ? 'bg-[#DA251D] text-white shadow-sm'
                : 'bg-white text-slate-500 border border-slate-200',
            ]"
          >
            {{ tab.label }}
            <span
              :class="[
                'text-[10px] px-1.5 py-0.5 rounded-full min-w-[18px] text-center font-bold',
                ticketFilter === tab.key ? 'bg-white/25' : 'bg-slate-100',
              ]"
              >{{ tab.count }}</span
            >
          </button>
        </div>

        <!-- Ticket cards — compact -->
        <div class="mt-3 space-y-2.5">
          <div
            v-for="t in filteredTickets"
            :key="t.id"
            class="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden active:bg-slate-50 transition-colors"
          >
            <!-- Main row — tappable -->
            <div class="p-3.5 cursor-pointer" @click="toggleTicket(t.id)">
              <div class="flex items-start justify-between gap-2">
                <div class="flex-1 min-w-0">
                  <!-- Code + status inline -->
                  <div class="flex items-center gap-1.5 mb-1">
                    <span
                      :class="[
                        'w-1.5 h-1.5 rounded-full shrink-0',
                        STATUS_CFG[t.status].dot,
                      ]"
                    />
                    <span class="font-mono text-[11px] text-slate-400">{{
                      t.ticketCode
                    }}</span>
                    <span
                      :class="[
                        'px-1.5 py-0.5 rounded text-[10px] font-semibold',
                        STATUS_CFG[t.status].bg,
                        STATUS_CFG[t.status].text,
                      ]"
                      >{{ STATUS_CFG[t.status].label }}</span
                    >
                  </div>
                  <!-- Title -->
                  <p
                    class="text-[13px] font-medium text-slate-700 line-clamp-2 leading-[1.5]"
                  >
                    {{ t.title }}
                  </p>
                  <!-- Meta -->
                  <div class="flex items-center gap-2 mt-1.5">
                    <span
                      class="text-[10px] text-slate-400 bg-slate-50 px-1.5 py-0.5 rounded font-medium"
                      >{{ t.categoryName }}</span
                    >
                    <span class="text-[10px] text-slate-300">{{
                      formatDate(t.createdAt)
                    }}</span>
                  </div>
                </div>
                <ChevronDown
                  :size="16"
                  :class="[
                    'text-slate-300 shrink-0 mt-0.5 transition-transform',
                    expandedTicket === t.id && 'rotate-180',
                  ]"
                />
              </div>
            </div>

            <!-- Expandable detail -->
            <div
              v-if="expandedTicket === t.id"
              class="border-t border-slate-100 px-3.5 py-3 bg-slate-50/60 space-y-2"
            >
              <p class="text-[12px] text-slate-500 leading-[1.6]">
                {{ t.description || "Chưa có mô tả." }}
              </p>
              <div
                v-if="t.resultNote"
                class="p-2.5 rounded-lg bg-emerald-50 border border-emerald-100"
              >
                <p class="text-[11px] font-semibold text-emerald-700">
                  ✓ Kết quả xử lý
                </p>
                <p class="text-[11px] text-emerald-600 mt-0.5">
                  {{ t.resultNote }}
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- Empty -->
        <div v-if="filteredTickets.length === 0" class="text-center py-10">
          <Inbox :size="36" class="text-slate-200 mx-auto mb-2" />
          <p class="text-slate-400 text-[13px]">Không có phản ánh nào</p>
        </div>
      </section>

      <!-- ═══ ACHIEVEMENTS — horizontal scroll cards ═══ -->
      <section class="mt-5">
        <div class="flex items-center justify-between px-4 mb-3">
          <h2 class="text-[16px] font-bold text-slate-800">Thành tựu</h2>
          <span class="text-[12px] text-slate-400"
            >{{ achievedCount }}/{{ achievements.length }}</span
          >
        </div>
        <!-- Horizontal scroll — no scrollbar, thumb-friendly -->
        <div class="flex gap-2.5 overflow-x-auto px-4 pb-2 no-scrollbar">
          <div
            v-for="a in achievements"
            :key="a.id"
            :class="[
              'shrink-0 w-[140px] p-3 rounded-2xl border transition-all',
              a.achieved
                ? 'bg-white border-slate-200 shadow-sm'
                : 'bg-slate-50 border-slate-100 opacity-50',
            ]"
          >
            <div
              :class="[
                'w-9 h-9 rounded-xl flex items-center justify-center mb-2',
                a.achieved ? a.iconBg : 'bg-slate-200',
              ]"
            >
              <component
                :is="a.icon"
                :size="16"
                :class="a.achieved ? a.iconColor : 'text-slate-400'"
              />
            </div>
            <p
              :class="[
                'text-[12px] font-bold leading-tight',
                a.achieved ? 'text-slate-800' : 'text-slate-400',
              ]"
            >
              {{ a.name }}
            </p>
            <p
              class="text-[10px] text-slate-400 mt-0.5 line-clamp-2 leading-[1.4]"
            >
              {{ a.desc }}
            </p>
            <!-- Progress bar for unachieved -->
            <div v-if="!a.achieved && a.progress != null" class="mt-2">
              <div class="w-full h-1 bg-slate-200 rounded-full overflow-hidden">
                <div
                  class="h-full bg-violet-400 rounded-full"
                  :style="{
                    width: Math.min(100, (a.progress / a.total) * 100) + '%',
                  }"
                />
              </div>
              <p class="text-[9px] text-slate-400 mt-0.5">
                {{ a.progress }}/{{ a.total }}
              </p>
            </div>
            <!-- Rarity badge -->
            <div v-if="a.achieved && a.rarity" class="mt-2">
              <span
                class="text-[9px] font-bold px-1.5 py-0.5 rounded bg-violet-100 text-violet-600"
                >{{ a.rarity }}</span
              >
            </div>
          </div>
        </div>
      </section>

      <!-- ═══ POINT HISTORY — collapsible ═══ -->
      <section class="px-4 mt-5" v-if="showPointHistory">
        <div
          class="bg-white rounded-2xl shadow-sm border border-slate-100 overflow-hidden"
        >
          <div
            class="flex items-center justify-between p-4 cursor-pointer"
            @click="pointHistoryExpanded = !pointHistoryExpanded"
          >
            <div class="flex items-center gap-2.5">
              <div
                class="w-8 h-8 rounded-lg bg-slate-100 flex items-center justify-center"
              >
                <Clock :size="14" class="text-slate-500" />
              </div>
              <div>
                <p class="text-[14px] font-bold text-slate-800">Lịch sử điểm</p>
                <p class="text-[11px] text-slate-400">
                  {{ points.totalPoints }} điểm · {{ pointHistory.length }} hoạt
                  động
                </p>
              </div>
            </div>
            <ChevronDown
              :size="16"
              :class="[
                'text-slate-400 transition-transform',
                pointHistoryExpanded && 'rotate-180',
              ]"
            />
          </div>

          <div v-if="pointHistoryExpanded" class="border-t border-slate-100">
            <div
              v-for="(ph, i) in pointHistory"
              :key="ph.id"
              :class="[
                'flex items-center gap-3 px-4 py-3',
                i < pointHistory.length - 1 && 'border-b border-slate-50',
              ]"
            >
              <!-- Icon -->
              <div
                :class="[
                  'w-7 h-7 rounded-full flex items-center justify-center shrink-0',
                  ph.points > 0 ? 'bg-emerald-50' : 'bg-red-50',
                ]"
              >
                <component
                  :is="ph.points > 0 ? CheckCircle2 : XCircle"
                  :size="12"
                  :class="ph.points > 0 ? 'text-emerald-500' : 'text-red-400'"
                />
              </div>
              <!-- Content -->
              <div class="flex-1 min-w-0">
                <p class="text-[12px] text-slate-600 truncate">{{ ph.note }}</p>
                <p
                  v-if="ph.ticketCode"
                  class="text-[10px] text-slate-300 mt-0.5"
                >
                  {{ ph.ticketCode }}
                </p>
              </div>
              <!-- Points + date -->
              <div class="text-right shrink-0">
                <p
                  :class="[
                    'text-[13px] font-bold',
                    ph.points > 0 ? 'text-emerald-600' : 'text-red-500',
                  ]"
                >
                  {{ ph.points > 0 ? "+" : "" }}{{ ph.points }}
                </p>
                <p class="text-[10px] text-slate-300">
                  {{ formatDateShort(ph.createdAt) }}
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- ═══ RANK BADGE — compact footer card ═══ -->
      <section class="px-4 mt-5">
        <div
          class="bg-gradient-to-r from-amber-50 to-orange-50 rounded-2xl border border-amber-100/80 p-4 flex items-center gap-3"
        >
          <div
            class="w-11 h-11 rounded-xl bg-white shadow-sm flex items-center justify-center shrink-0"
          >
            <Trophy :size="20" class="text-amber-500" />
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-[14px] font-bold text-slate-800">
              Xếp hạng <span class="text-[#DA251D]">#{{ myRank }}</span> tháng
              này
            </p>
            <p class="text-[11px] text-slate-500 mt-0.5">
              {{ points.currentMonthPoints }} điểm · Top {{ topPercent }}% công
              dân tích cực
            </p>
          </div>
          <ChevronRight :size="16" class="text-slate-400 shrink-0" />
        </div>
      </section>
    </div>
    <!-- Modal đổi mật khẩu -->
    <div
      v-if="showPasswordModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      @click.self="closePasswordModal"
    >
      <div class="bg-white rounded-2xl shadow-xl w-full max-w-md mx-4 p-6">
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-lg font-bold text-slate-800">Đổi mật khẩu</h3>
          <button
            @click="closePasswordModal"
            class="w-8 h-8 flex items-center justify-center rounded-full hover:bg-slate-100"
          >
            <X class="w-5 h-5 text-slate-400" />
          </button>
        </div>

        <!-- Form -->
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1"
              >Mật khẩu hiện tại</label
            >
            <div class="relative">
              <input
                v-model="passwordForm.currentPassword"
                :type="showCurrentPw ? 'text' : 'password'"
                placeholder="Nhập mật khẩu hiện tại"
                class="w-full px-4 py-2.5 border border-slate-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-brand-red)]/20 focus:border-[var(--color-brand-red)]"
              />
              <button
                type="button"
                @click="showCurrentPw = !showCurrentPw"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
              >
                <component :is="showCurrentPw ? EyeOff : Eye" class="w-4 h-4" />
              </button>
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1"
              >Mật khẩu mới</label
            >
            <div class="relative">
              <input
                v-model="passwordForm.newPassword"
                :type="showNewPw ? 'text' : 'password'"
                placeholder="Nhập mật khẩu mới"
                class="w-full px-4 py-2.5 border border-slate-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-brand-red)]/20 focus:border-[var(--color-brand-red)]"
              />
              <button
                type="button"
                @click="showNewPw = !showNewPw"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
              >
                <component :is="showNewPw ? EyeOff : Eye" class="w-4 h-4" />
              </button>
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1"
              >Xác nhận mật khẩu mới</label
            >
            <div class="relative">
              <input
                v-model="passwordForm.confirmPassword"
                :type="showConfirmPw ? 'text' : 'password'"
                placeholder="Nhập lại mật khẩu mới"
                class="w-full px-4 py-2.5 border border-slate-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-brand-red)]/20 focus:border-[var(--color-brand-red)]"
              />
              <button
                type="button"
                @click="showConfirmPw = !showConfirmPw"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
              >
                <component :is="showConfirmPw ? EyeOff : Eye" class="w-4 h-4" />
              </button>
            </div>
          </div>

          <!-- Thông báo lỗi/thành công -->
          <p v-if="passwordError" class="text-sm text-red-600">
            {{ passwordError }}
          </p>
          <p v-if="passwordSuccess" class="text-sm text-green-600">
            {{ passwordSuccess }}
          </p>
        </div>

        <!-- Actions -->
        <div class="flex gap-3 mt-6">
          <button
            @click="closePasswordModal"
            class="flex-1 py-2.5 text-sm font-medium text-slate-700 bg-slate-100 rounded-xl hover:bg-slate-200 transition-colors"
          >
            Hủy
          </button>
          <button
            @click="handleChangePassword"
            :disabled="changingPassword"
            class="flex-1 py-2.5 text-sm font-semibold text-white bg-[var(--color-brand-red)] rounded-xl hover:bg-[#b81f18] disabled:opacity-50 transition-colors flex items-center justify-center gap-2"
          >
            <Loader2 v-if="changingPassword" class="w-4 h-4 animate-spin" />
            {{ changingPassword ? "Đang xử lý..." : "Xác nhận" }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "@/stores/auth";
import api from "@/services/api";
import {
  // Icon đã có
  User,
  Phone,
  Mail,
  MapPin,
  Award,
  FileText,
  CheckCircle2,
  XCircle,
  TrendingUp,
  Clock,
  Settings,
  KeyRound,
  LogOut,
  X,
  Eye,
  EyeOff,
  Loader2,
  AlertCircle,
  // Icon template cũ đang dùng — BẮT BUỘC phải import
  ArrowLeft,
  Bell,
  Flame,
  Star,
  Send,
  Search,
  Trophy,
  ChevronDown,
  Inbox,
  ChevronRight,
} from "lucide-vue-next";

const router = useRouter();
const auth = useAuthStore();

// ══════════════════════════════════════════════════════════
// STATE
// ══════════════════════════════════════════════════════════
const loading = ref(true);
const error = ref("");

const profile = ref(null); // /api/auth/me
const points = ref(null); // /api/citizen/my-points
const tickets = ref([]); // /api/citizen/my-tickets
const pointHistory = ref([]); // /api/citizen/my-points/history

// ── Biến template cũ đang dùng ───────────────────────────
const lotusPattern = "/images/lotus-pattern.png"; // ảnh tĩnh trong public/
const unreadCount = ref(0); // chưa gắn notification API — để 0
const showPointHistory = ref(false);
const pointHistoryExpanded = ref(false);
const expandedTicket = ref(null);
const ticketFilter = ref("all");

// ── Tier system (tính từ totalPoints) ─────────────────────
const TIERS = [
  { level: 1, label: "Công dân mới", min: 0, max: 49 },
  { level: 2, label: "Công dân tích cực", min: 50, max: 149 },
  { level: 3, label: "Công dân tiêu biểu", min: 150, max: 499 },
  { level: 4, label: "Công dân gương mẫu", min: 500, max: 999 },
  { level: 5, label: "Đại sứ cộng đồng", min: 1000, max: Infinity },
];

const currentTier = computed(() => {
  const tp = points.value?.totalPoints ?? 0;
  return TIERS.find((t) => tp >= t.min && tp <= t.max) || TIERS[0];
});
const tierLevel = computed(() => currentTier.value.level);
const tierLabel = computed(() => currentTier.value.label);
const tierProgress = computed(() => {
  const tp = points.value?.totalPoints ?? 0;
  const tier = currentTier.value;
  if (tier.max === Infinity) return 1;
  return Math.min((tp - tier.min) / (tier.max - tier.min + 1), 1);
});

// ── Initials avatar ───────────────────────────────────────
const initials = computed(() => {
  if (!profile.value?.fullName) return "?";
  return profile.value.fullName
    .split(" ")
    .map((w) => w[0])
    .slice(-2)
    .join("")
    .toUpperCase();
});

// ── Ticket status config — dùng cho template card ─────────
const STATUS_CFG = {
  New: {
    label: "Mới",
    bg: "bg-blue-50",
    text: "text-blue-700",
    dot: "bg-blue-500",
  },
  UnderReview: {
    label: "Đang duyệt",
    bg: "bg-cyan-50",
    text: "text-cyan-700",
    dot: "bg-cyan-500",
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
  PendingVerification: {
    label: "Chờ xác minh",
    bg: "bg-violet-50",
    text: "text-violet-700",
    dot: "bg-violet-500",
  },
  Closed: {
    label: "Đã đóng",
    bg: "bg-slate-100",
    text: "text-slate-500",
    dot: "bg-slate-400",
  },
  Rejected: {
    label: "Từ chối",
    bg: "bg-red-50",
    text: "text-red-600",
    dot: "bg-red-500",
  },
};

// ── Ticket tabs + filter ──────────────────────────────────
const ticketTabs = computed(() => {
  const all = tickets.value;
  return [
    { key: "all", label: "Tất cả", count: all.length },
    {
      key: "active",
      label: "Đang xử lý",
      count: all.filter((t) =>
        [
          "New",
          "UnderReview",
          "Assigned",
          "InProgress",
          "PendingVerification",
        ].includes(t.status),
      ).length,
    },
    {
      key: "closed",
      label: "Đã đóng",
      count: all.filter((t) => t.status === "Closed").length,
    },
    {
      key: "rejected",
      label: "Từ chối",
      count: all.filter((t) => t.status === "Rejected").length,
    },
  ];
});

const filteredTickets = computed(() => {
  if (ticketFilter.value === "all") return tickets.value;
  if (ticketFilter.value === "closed")
    return tickets.value.filter((t) => t.status === "Closed");
  if (ticketFilter.value === "rejected")
    return tickets.value.filter((t) => t.status === "Rejected");
  // active
  return tickets.value.filter((t) =>
    [
      "New",
      "UnderReview",
      "Assigned",
      "InProgress",
      "PendingVerification",
    ].includes(t.status),
  );
});

const toggleTicket = (id) => {
  expandedTicket.value = expandedTicket.value === id ? null : id;
};

// ── Achievements (mock tạm — BE chưa có API) ─────────────
const achievements = computed(() => {
  const tp = points.value?.totalPoints ?? 0;
  const approved = points.value?.totalTicketsApproved ?? 0;
  return [
    {
      id: 1,
      name: "Báo cáo đầu tiên",
      desc: "Gửi 1 phản ánh",
      icon: Send,
      iconBg: "bg-blue-50",
      iconColor: "text-blue-500",
      achieved: approved >= 1,
    },
    {
      id: 2,
      name: "Công dân tích cực",
      desc: "Đạt 50 điểm",
      icon: Star,
      iconBg: "bg-amber-50",
      iconColor: "text-amber-500",
      achieved: tp >= 50,
    },
    {
      id: 3,
      name: "5 phản ánh hợp lệ",
      desc: "5 PA được duyệt",
      icon: CheckCircle2,
      iconBg: "bg-emerald-50",
      iconColor: "text-emerald-500",
      achieved: approved >= 5,
    },
    {
      id: 4,
      name: "Đại sứ cộng đồng",
      desc: "Đạt 1000 điểm",
      icon: Trophy,
      iconBg: "bg-purple-50",
      iconColor: "text-purple-500",
      achieved: tp >= 1000,
    },
  ];
});
const achievedCount = computed(
  () => achievements.value.filter((a) => a.achieved).length,
);

// ── Rank (mock tạm — BE chưa có ranking cá nhân) ─────────
const myRank = ref("—");
const topPercent = ref("—");

// ── Format helpers ────────────────────────────────────────
const formatDate = (dateStr) => {
  if (!dateStr) return "—";
  return new Date(dateStr).toLocaleDateString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
};

const formatDateShort = (dateStr) => {
  if (!dateStr) return "";
  return new Date(dateStr).toLocaleDateString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
  });
};

// ══════════════════════════════════════════════════════════
// FETCH DATA
// ══════════════════════════════════════════════════════════
const fetchData = async () => {
  loading.value = true;
  error.value = "";
  try {
    profile.value = auth.user;
    const [pointsRes, ticketsRes, historyRes] = await Promise.all([
      api.get("/citizen/my-points"),
      api.get("/citizen/my-tickets"),
      api.get("/citizen/my-points/history"),
    ]);
    points.value = pointsRes.data;
    tickets.value = ticketsRes.data;
    pointHistory.value = historyRes.data;
  } catch (err) {
    error.value =
      err.response?.data?.message || "Không thể tải dữ liệu. Vui lòng thử lại.";
  } finally {
    loading.value = false;
  }
};

onMounted(() => fetchData());

// ══════════════════════════════════════════════════════════
// SETTINGS DROPDOWN
// ══════════════════════════════════════════════════════════
const showSettings = ref(false);
const settingsRef = ref(null);

const handleClickOutside = (e) => {
  if (settingsRef.value && !settingsRef.value.contains(e.target)) {
    showSettings.value = false;
  }
};
onMounted(() => document.addEventListener("click", handleClickOutside));
onUnmounted(() => document.removeEventListener("click", handleClickOutside));

// ── Đăng xuất ─────────────────────────────────────────────
const handleLogout = async () => {
  await auth.logout();
  router.push({ name: "Home" });
};

// ══════════════════════════════════════════════════════════
// ĐỔI MẬT KHẨU MODAL
// ══════════════════════════════════════════════════════════
const showPasswordModal = ref(false);
const changingPassword = ref(false);
const passwordError = ref("");
const passwordSuccess = ref("");
const showCurrentPw = ref(false);
const showNewPw = ref(false);
const showConfirmPw = ref(false);

const passwordForm = ref({
  currentPassword: "",
  newPassword: "",
  confirmPassword: "",
});

const closePasswordModal = () => {
  showPasswordModal.value = false;
  passwordError.value = "";
  passwordSuccess.value = "";
  showCurrentPw.value = false;
  showNewPw.value = false;
  showConfirmPw.value = false;
  passwordForm.value = {
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  };
};

const handleChangePassword = async () => {
  passwordError.value = "";
  passwordSuccess.value = "";

  if (passwordForm.value.newPassword.length < 6) {
    passwordError.value = "Mật khẩu mới phải có ít nhất 6 ký tự";
    return;
  }
  if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
    passwordError.value = "Mật khẩu xác nhận không khớp";
    return;
  }

  changingPassword.value = true;
  try {
    const { data } = await api.put("/users/change-password", {
      currentPassword: passwordForm.value.currentPassword,
      newPassword: passwordForm.value.newPassword,
      confirmPassword: passwordForm.value.confirmPassword,
    });
    passwordSuccess.value = data.message || "Đổi mật khẩu thành công";
    setTimeout(() => closePasswordModal(), 1500);
  } catch (err) {
    passwordError.value =
      err.response?.data?.message || "Đổi mật khẩu thất bại";
  } finally {
    changingPassword.value = false;
  }
};
</script>

<style scoped>
/* Ẩn scrollbar nhưng vẫn scroll được — cần cho horizontal scroll trên mobile */
.no-scrollbar::-webkit-scrollbar {
  display: none;
}
.no-scrollbar {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
</style>
