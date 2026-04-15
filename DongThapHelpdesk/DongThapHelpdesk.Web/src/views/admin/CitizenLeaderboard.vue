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
          Bảng xếp hạng Công dân tích cực
        </h1>
        <p class="text-slate-500 mt-0.5 text-[14px]">
          Thống kê người dân đóng góp nhiều nhất — Dữ liệu phục vụ khen thưởng
        </p>
      </div>
      <div class="flex items-center gap-2">
        <div
          class="flex items-center bg-white border border-slate-200 rounded-lg overflow-hidden"
        >
          <button
            v-for="p in periodOptions"
            :key="p.key"
            @click="activePeriod = p.key"
            :class="[
              'px-4 py-2.5 transition-colors cursor-pointer text-[13px] font-medium',
              activePeriod === p.key
                ? 'bg-[#DA251D] text-white'
                : 'text-slate-600 hover:bg-slate-50',
            ]"
          >
            {{ p.label }}
          </button>
        </div>
      </div>
    </div>

    <!-- Stats overview -->
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

    <!-- Top 3 Podium -->
    <div
      class="bg-gradient-to-br from-[#DA251D] via-[#DA251D] to-[#8B0000] rounded-2xl p-6 sm:p-8 lg:p-10 relative overflow-hidden"
    >
      <!-- Background decorations -->
      <div class="absolute inset-0 opacity-[0.06]">
        <div
          class="absolute top-0 right-0 w-80 h-80 bg-white rounded-full -translate-y-1/2 translate-x-1/3"
        />
        <div
          class="absolute bottom-0 left-0 w-52 h-52 bg-white rounded-full translate-y-1/2 -translate-x-1/4"
        />
        <div
          class="absolute top-1/2 left-1/3 w-32 h-32 bg-white rounded-full opacity-50"
        />
      </div>

      <div class="relative">
        <!-- Header -->
        <div class="text-center mb-14">
          <div
            class="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-[#FFC627]/20 backdrop-blur-sm mb-3"
          >
            <Trophy :size="16" class="text-[#FFC627]" />
            <span class="text-[#FFC627] text-[13px] font-semibold">{{
              activePeriod === "month" ? "Tháng 4/2026" : "Quý 2/2026"
            }}</span>
          </div>
          <h2 class="text-white text-[22px] sm:text-[26px] font-bold">
            Top 3 Công dân Tiêu biểu
          </h2>
          <p class="text-white/50 text-[13px] mt-1">
            Những người dân có đóng góp tích cực nhất trong kỳ
          </p>
        </div>

        <!-- Podium layout: 2nd - 1st - 3rd -->
        <div
          class="flex items-end justify-center gap-4 sm:gap-6 max-w-3xl mx-auto pb-2"
        >
          <!-- 2nd place -->
          <div class="flex-1 max-w-[210px] self-end">
            <div
              class="bg-gradient-to-b from-white/15 to-white/5 backdrop-blur-sm rounded-2xl border border-white/15 p-5 sm:p-6 text-center transition-all hover:from-white/20 hover:to-white/10 hover:shadow-lg hover:shadow-black/10"
            >
              <!-- Avatar -->
              <div class="relative inline-block mb-4">
                <div
                  class="w-18 h-18 sm:w-20 sm:h-20 rounded-full bg-gradient-to-br from-slate-200 via-slate-100 to-slate-300 flex items-center justify-center text-[22px] sm:text-[26px] font-bold text-slate-600 mx-auto ring-4 ring-white/20 shadow-xl"
                  style="width: 80px; height: 80px"
                >
                  {{ top3[1].initials }}
                </div>
                <div
                  class="absolute -bottom-2 left-1/2 -translate-x-1/2 w-7 h-7 rounded-full bg-gradient-to-br from-slate-300 to-slate-400 flex items-center justify-center text-[13px] font-bold text-white shadow-md ring-2 ring-white/30"
                >
                  2
                </div>
              </div>
              <h3 class="text-white text-[15px] font-semibold truncate">
                {{ top3[1].name }}
              </h3>
              <p class="text-white/40 text-[12px] mb-4 truncate">
                {{ top3[1].ward }}, {{ top3[1].district }}
              </p>
              <!-- Points -->
              <div class="flex items-center justify-center gap-1.5 mb-3">
                <Star :size="16" class="text-[#FFC627]" fill="#FFC627" />
                <span class="text-white text-[24px] font-extrabold">{{
                  top3[1].periodPoints
                }}</span>
              </div>
              <div
                class="flex items-center justify-center gap-2 text-white/35 text-[11px]"
              >
                <span>{{ top3[1].approved }} PA</span>
                <span class="w-1 h-1 rounded-full bg-white/20" />
                <span>{{ top3[1].rate }}%</span>
              </div>
            </div>
          </div>

          <!-- 1st place (tallest) -->
          <div class="flex-1 max-w-[230px] -mt-6">
            <div
              class="relative bg-gradient-to-b from-white/20 to-white/8 backdrop-blur-sm rounded-2xl border border-[#FFC627]/25 p-5 sm:p-7 text-center transition-all hover:from-white/25 hover:to-white/12 hover:shadow-xl hover:shadow-black/10"
            >
              <!-- Crown floating above -->
              <div class="absolute -top-5 left-1/2 -translate-x-1/2">
                <div
                  class="w-10 h-10 rounded-full bg-[#FFC627] flex items-center justify-center shadow-lg shadow-[#FFC627]/40 animate-bounce"
                  style="animation-duration: 2s"
                >
                  <Crown :size="20" class="text-[#8B4513]" />
                </div>
              </div>
              <!-- Avatar -->
              <div class="relative inline-block mb-4 mt-4">
                <div
                  class="rounded-full bg-gradient-to-br from-[#FFC627] via-[#F59E0B] to-[#D97706] flex items-center justify-center font-bold text-[#8B4513] mx-auto ring-4 ring-[#FFC627]/30 shadow-2xl text-[28px] sm:text-[32px]"
                  style="width: 96px; height: 96px"
                >
                  {{ top3[0].initials }}
                </div>
                <div
                  class="absolute -bottom-2 left-1/2 -translate-x-1/2 w-8 h-8 rounded-full bg-gradient-to-br from-[#FFC627] to-[#F59E0B] flex items-center justify-center text-[14px] font-bold text-[#8B4513] shadow-lg ring-2 ring-white/40"
                >
                  1
                </div>
              </div>
              <h3
                class="text-white text-[17px] sm:text-[19px] font-bold mt-2 truncate"
              >
                {{ top3[0].name }}
              </h3>
              <p class="text-white/40 text-[12px] sm:text-[13px] mb-4 truncate">
                {{ top3[0].ward }}, {{ top3[0].district }}
              </p>
              <!-- Points highlight -->
              <div
                class="inline-flex items-center gap-2 px-5 py-2.5 rounded-full bg-[#FFC627]/20 border border-[#FFC627]/25 mb-3"
              >
                <Star :size="18" class="text-[#FFC627]" fill="#FFC627" />
                <span
                  class="text-[#FFC627] text-[28px] sm:text-[32px] font-extrabold leading-none"
                  >{{ top3[0].periodPoints }}</span
                >
                <span class="text-[#FFC627]/50 text-[12px]">điểm</span>
              </div>
              <div
                class="flex items-center justify-center gap-3 text-white/40 text-[12px]"
              >
                <span class="flex items-center gap-1"
                  ><CheckCircle2 :size="12" class="text-emerald-400" />
                  {{ top3[0].approved }} phản ánh</span
                >
                <span class="flex items-center gap-1"
                  ><Target :size="12" class="text-blue-400" />
                  {{ top3[0].rate }}%</span
                >
              </div>
            </div>
          </div>

          <!-- 3rd place -->
          <div class="flex-1 max-w-[210px] self-end">
            <div
              class="bg-gradient-to-b from-white/12 to-white/4 backdrop-blur-sm rounded-2xl border border-white/10 p-5 sm:p-6 text-center transition-all hover:from-white/18 hover:to-white/8 hover:shadow-lg hover:shadow-black/10 mt-6"
            >
              <!-- Avatar -->
              <div class="relative inline-block mb-4">
                <div
                  class="rounded-full bg-gradient-to-br from-amber-500 via-amber-600 to-amber-700 flex items-center justify-center font-bold text-amber-100 mx-auto ring-4 ring-amber-500/20 shadow-xl text-[22px] sm:text-[26px]"
                  style="width: 80px; height: 80px"
                >
                  {{ top3[2].initials }}
                </div>
                <div
                  class="absolute -bottom-2 left-1/2 -translate-x-1/2 w-7 h-7 rounded-full bg-gradient-to-br from-amber-600 to-amber-700 flex items-center justify-center text-[13px] font-bold text-white shadow-md ring-2 ring-white/20"
                >
                  3
                </div>
              </div>
              <h3 class="text-white text-[15px] font-semibold truncate">
                {{ top3[2].name }}
              </h3>
              <p class="text-white/40 text-[12px] mb-4 truncate">
                {{ top3[2].ward }}, {{ top3[2].district }}
              </p>
              <!-- Points -->
              <div class="flex items-center justify-center gap-1.5 mb-3">
                <Star :size="16" class="text-[#FFC627]" fill="#FFC627" />
                <span class="text-white text-[24px] font-extrabold">{{
                  top3[2].periodPoints
                }}</span>
              </div>
              <div
                class="flex items-center justify-center gap-2 text-white/35 text-[11px]"
              >
                <span>{{ top3[2].approved }} PA</span>
                <span class="w-1 h-1 rounded-full bg-white/20" />
                <span>{{ top3[2].rate }}%</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Full leaderboard table -->
    <div
      class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden"
    >
      <div
        class="flex items-center justify-between px-5 py-4 border-b border-slate-100"
      >
        <h3 class="text-slate-800 text-[16px] font-semibold">
          Bảng xếp hạng đầy đủ — Top {{ leaderboard.length }}
        </h3>
        <div class="relative">
          <Search
            :size="16"
            class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
          />
          <input
            v-model="searchQuery"
            placeholder="Tìm người dân..."
            class="pl-9 pr-4 py-2 rounded-lg border border-slate-200 text-[13px] outline-none focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10"
          />
        </div>
      </div>

      <!-- Desktop table -->
      <div class="hidden md:block overflow-x-auto">
        <table class="w-full text-[13px]">
          <thead>
            <tr class="bg-slate-50/70">
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 w-16 font-medium text-slate-400"
              >
                Hạng
              </th>
              <th
                class="text-left px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Họ tên
              </th>
              <th
                class="text-left px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Địa chỉ
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Điểm {{ activePeriod === "month" ? "tháng" : "quý" }}
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Tổng điểm
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                PA được duyệt
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Tỷ lệ duyệt
              </th>
              <th
                class="text-center px-4 py-3.5 border-b border-slate-100 font-medium text-slate-500"
              >
                Đề xuất
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="c in filteredLeaderboard"
              :key="c.rank"
              @click="openProfile(getCitizenDetail(c))"
              class="border-b border-slate-50 hover:bg-slate-50/50 transition-colors cursor-pointer"
            >
              <td class="px-4 py-3.5 text-center">
                <div
                  v-if="c.rank <= 3"
                  :class="[
                    'w-8 h-8 rounded-full flex items-center justify-center mx-auto text-[13px] font-bold',
                    c.rank === 1
                      ? 'bg-[#FFC627] text-[#8B4513]'
                      : c.rank === 2
                        ? 'bg-slate-200 text-slate-700'
                        : 'bg-amber-100 text-amber-700',
                  ]"
                >
                  {{ c.rank }}
                </div>
                <span v-else class="text-slate-400 font-medium">{{
                  c.rank
                }}</span>
              </td>
              <td class="px-4 py-3.5">
                <div class="flex items-center gap-3">
                  <div
                    :class="[
                      'w-9 h-9 rounded-full flex items-center justify-center shrink-0 text-[12px] font-semibold',
                      c.avatarColor,
                    ]"
                  >
                    {{ c.initials }}
                  </div>
                  <div>
                    <p class="text-slate-800 text-[14px] font-medium">
                      {{ c.name }}
                    </p>
                    <p class="text-slate-400 text-[12px]">{{ c.phone }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-3.5 text-slate-500">
                {{ c.ward }}, {{ c.district }}
              </td>
              <td class="px-4 py-3.5 text-center">
                <span
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-[#FFC627]/15 text-[#8B4513] text-[13px] font-bold"
                >
                  <Star :size="12" class="text-[#FFC627]" fill="#FFC627" />
                  {{ c.periodPoints }}
                </span>
              </td>
              <td class="px-4 py-3.5 text-center text-slate-700 font-medium">
                {{ c.totalPoints }}
              </td>
              <td class="px-4 py-3.5 text-center text-slate-600">
                {{ c.approved }}/{{ c.submitted }}
              </td>
              <td class="px-4 py-3.5 text-center">
                <div class="flex items-center justify-center gap-2">
                  <div
                    class="w-16 h-2 bg-slate-100 rounded-full overflow-hidden"
                  >
                    <div
                      :class="[
                        c.rate >= 80
                          ? 'bg-emerald-500'
                          : c.rate >= 50
                            ? 'bg-amber-500'
                            : 'bg-red-500',
                      ]"
                      class="h-full rounded-full"
                      :style="{ width: c.rate + '%' }"
                    />
                  </div>
                  <span class="text-slate-600 text-[12px] font-medium"
                    >{{ c.rate }}%</span
                  >
                </div>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span
                  v-if="c.rank <= 3"
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-emerald-50 text-emerald-700 text-[11px] font-semibold"
                >
                  <Award :size="12" /> Khen thưởng
                </span>
                <span
                  v-else-if="c.rank <= 5"
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-blue-50 text-blue-600 text-[11px] font-medium"
                >
                  Biểu dương
                </span>
                <span v-else class="text-slate-300 text-[12px]">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Mobile cards -->
      <div class="md:hidden divide-y divide-slate-50">
        <div
          v-for="c in filteredLeaderboard"
          :key="c.rank"
          @click="openProfile(getCitizenDetail(c))"
          class="px-4 py-4"
        >
          <div class="flex items-center gap-3 mb-2">
            <div
              v-if="c.rank <= 3"
              :class="[
                'w-8 h-8 rounded-full flex items-center justify-center text-[13px] font-bold shrink-0',
                c.rank === 1
                  ? 'bg-[#FFC627] text-[#8B4513]'
                  : c.rank === 2
                    ? 'bg-slate-200 text-slate-700'
                    : 'bg-amber-100 text-amber-700',
              ]"
            >
              {{ c.rank }}
            </div>
            <span
              v-else
              class="w-8 h-8 flex items-center justify-center text-slate-400 font-medium text-[14px] shrink-0"
              >{{ c.rank }}</span
            >
            <div class="flex-1 min-w-0">
              <p class="text-slate-800 text-[14px] font-semibold truncate">
                {{ c.name }}
              </p>
              <p class="text-slate-400 text-[12px]">
                {{ c.ward }}, {{ c.district }}
              </p>
            </div>
            <div class="text-right shrink-0">
              <div class="flex items-center gap-1">
                <Star :size="14" class="text-[#FFC627]" fill="#FFC627" />
                <span class="text-slate-800 text-[18px] font-bold">{{
                  c.periodPoints
                }}</span>
              </div>
              <p class="text-slate-400 text-[11px]">
                {{ c.approved }} PA duyệt
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Point system info -->
    <div class="bg-white rounded-xl shadow-sm border border-slate-100 p-5">
      <h3
        class="text-slate-800 text-[15px] font-semibold mb-4 flex items-center gap-2"
      >
        <Info :size="16" class="text-blue-500" /> Quy chế tính điểm
      </h3>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
        <div
          v-for="rule in pointRules"
          :key="rule.action"
          class="flex items-center gap-3 p-3 rounded-lg bg-slate-50"
        >
          <div
            :class="[
              'w-10 h-10 rounded-lg flex items-center justify-center shrink-0',
              rule.points > 0
                ? 'bg-emerald-100 text-emerald-600'
                : 'bg-red-100 text-red-500',
            ]"
          >
            <span class="text-[14px] font-bold"
              >{{ rule.points > 0 ? "+" : "" }}{{ rule.points }}</span
            >
          </div>
          <div>
            <p class="text-slate-700 text-[13px] font-medium">
              {{ rule.action }}
            </p>
            <p class="text-slate-400 text-[12px]">{{ rule.desc }}</p>
          </div>
        </div>
      </div>
      <div class="mt-4 pt-4 border-t border-slate-100">
        <p class="text-slate-500 text-[13px] leading-[1.6]">
          Điểm tháng được reset vào ngày 01 hàng tháng. Điểm quý được reset vào
          đầu mỗi quý. Top 3 công dân có điểm cao nhất mỗi quý sẽ được UBND tỉnh
          xem xét khen thưởng.
        </p>
      </div>
    </div>
    <Teleport to="body">
      <div
        v-if="selectedCitizen"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
        @click.self="closeProfile"
      >
        <div
          class="bg-white rounded-2xl shadow-2xl w-full max-w-lg mx-4 max-h-[90vh] overflow-y-auto"
        >
          <!-- Header -->
          <div
            class="relative bg-gradient-to-br from-slate-800 to-slate-900 rounded-t-2xl px-6 py-6 text-center"
          >
            <button
              @click="closeProfile"
              class="absolute top-4 right-4 p-1.5 rounded-lg text-white/50 hover:text-white hover:bg-white/10 transition cursor-pointer"
            >
              <X :size="18" />
            </button>

            <!-- Avatar -->
            <div
              :class="[
                'w-16 h-16 rounded-full flex items-center justify-center text-[22px] font-bold mx-auto mb-3 ring-4 ring-white/15',
                selectedCitizen.avatarColor,
              ]"
            >
              {{ selectedCitizen.initials }}
            </div>
            <h2 class="text-white text-[18px] font-bold">
              {{ selectedCitizen.name }}
            </h2>
            <p
              class="text-white/50 text-[13px] flex items-center justify-center gap-1 mt-1"
            >
              <MapPin :size="12" />
              {{ selectedCitizen.ward }}, {{ selectedCitizen.district }}
            </p>
            <p
              class="text-white/40 text-[12px] flex items-center justify-center gap-1 mt-0.5"
            >
              <Phone :size="11" />
              {{ selectedCitizen.phone }}
            </p>

            <!-- Điểm nổi bật -->
            <div class="flex items-center justify-center gap-4 mt-4">
              <div class="text-center">
                <div class="flex items-center justify-center gap-1">
                  <Star :size="14" class="text-[#FFC627]" fill="#FFC627" />
                  <span class="text-[#FFC627] text-[22px] font-extrabold">{{
                    selectedCitizen.periodPoints
                  }}</span>
                </div>
                <span class="text-white/40 text-[11px]">Điểm kỳ này</span>
              </div>
              <div class="w-px h-8 bg-white/10"></div>
              <div class="text-center">
                <span class="text-white text-[22px] font-extrabold">{{
                  selectedCitizen.totalPoints
                }}</span>
                <p class="text-white/40 text-[11px]">Tổng tích lũy</p>
              </div>
              <div class="w-px h-8 bg-white/10"></div>
              <div class="text-center">
                <span class="text-emerald-400 text-[22px] font-extrabold"
                  >{{ selectedCitizen.rate }}%</span
                >
                <p class="text-white/40 text-[11px]">Tỷ lệ duyệt</p>
              </div>
            </div>
          </div>

          <!-- Body -->
          <div class="p-6 space-y-5">
            <!-- Thống kê PA -->
            <div class="flex items-center gap-3">
              <div class="flex-1 bg-blue-50 rounded-xl p-3 text-center">
                <p class="text-blue-700 text-[20px] font-bold">
                  {{ selectedCitizen.submitted }}
                </p>
                <p class="text-blue-500 text-[11px]">Đã gửi</p>
              </div>
              <div class="flex-1 bg-emerald-50 rounded-xl p-3 text-center">
                <p class="text-emerald-700 text-[20px] font-bold">
                  {{ selectedCitizen.approved }}
                </p>
                <p class="text-emerald-500 text-[11px]">Được duyệt</p>
              </div>
              <div class="flex-1 bg-red-50 rounded-xl p-3 text-center">
                <p class="text-red-700 text-[20px] font-bold">
                  {{ selectedCitizen.submitted - selectedCitizen.approved }}
                </p>
                <p class="text-red-500 text-[11px]">Bị từ chối</p>
              </div>
            </div>

            <!-- Điểm theo tháng -->
            <div>
              <h4
                class="text-slate-800 text-[14px] font-semibold mb-2.5 flex items-center gap-1.5"
              >
                <Calendar :size="14" class="text-slate-400" /> Điểm theo tháng
              </h4>
              <div class="space-y-2">
                <div
                  v-for="m in selectedCitizen.monthlyPoints"
                  :key="m.month"
                  class="flex items-center gap-3"
                >
                  <span class="text-[12px] text-slate-500 w-16">{{
                    m.month
                  }}</span>
                  <div
                    class="flex-1 h-5 bg-slate-100 rounded-full overflow-hidden"
                  >
                    <div
                      class="h-full bg-gradient-to-r from-blue-500 to-blue-400 rounded-full transition-all"
                      :style="{
                        width:
                          Math.min(
                            100,
                            (m.points /
                              Math.max(
                                ...selectedCitizen.monthlyPoints.map(
                                  (x) => x.points,
                                ),
                              )) *
                              100,
                          ) + '%',
                      }"
                    ></div>
                  </div>
                  <span
                    class="text-[12px] font-semibold text-slate-700 w-10 text-right"
                    >{{ m.points }}</span
                  >
                </div>
              </div>
            </div>

            <!-- Điểm theo quý -->
            <div>
              <h4
                class="text-slate-800 text-[14px] font-semibold mb-2.5 flex items-center gap-1.5"
              >
                <TrendingUpIcon :size="14" class="text-slate-400" /> Điểm theo
                quý
              </h4>
              <div class="flex items-center gap-3">
                <div
                  v-for="q in selectedCitizen.quarterlyPoints"
                  :key="q.quarter"
                  class="flex-1 bg-slate-50 rounded-xl p-3 text-center"
                >
                  <p class="text-slate-800 text-[18px] font-bold">
                    {{ q.points }}
                  </p>
                  <p class="text-slate-400 text-[11px]">{{ q.quarter }}</p>
                </div>
              </div>
            </div>

            <!-- Phản ánh tiêu biểu -->
            <div>
              <h4
                class="text-slate-800 text-[14px] font-semibold mb-2.5 flex items-center gap-1.5"
              >
                <FileText :size="14" class="text-slate-400" /> Phản ánh tiêu
                biểu
              </h4>
              <div class="space-y-2">
                <div
                  v-for="t in selectedCitizen.topTickets"
                  :key="t.code"
                  class="flex items-center gap-3 p-2.5 rounded-lg bg-slate-50 hover:bg-slate-100 transition"
                >
                  <div class="flex-1 min-w-0">
                    <span
                      class="text-[#DA251D] font-mono text-[12px] font-bold"
                      >{{ t.code }}</span
                    >
                    <p class="text-slate-700 text-[12px] truncate mt-0.5">
                      {{ t.title }}
                    </p>
                  </div>
                  <span
                    class="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-medium shrink-0"
                    :class="getStatusLabel(t.status).class"
                  >
                    {{ getStatusLabel(t.status).label }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- Footer -->
          <div
            class="px-6 py-4 border-t border-slate-100 bg-slate-50/50 rounded-b-2xl"
          >
            <button
              @click="closeProfile"
              class="w-full py-2.5 rounded-xl text-[14px] font-medium text-slate-600 hover:bg-slate-100 transition cursor-pointer"
            >
              Đóng
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import {
  Trophy,
  Star,
  Award,
  Search,
  Users,
  TrendingUp,
  Target,
  Info,
  Crown,
  CheckCircle2,
  X,
  MapPin,
  Phone,
  Calendar,
  FileText,
  TrendingUp as TrendingUpIcon,
} from "lucide-vue-next";

// ── Modal hồ sơ công dân ──
const selectedCitizen = ref(null);

function openProfile(citizen) {
  selectedCitizen.value = citizen;
}

function closeProfile() {
  selectedCitizen.value = null;
}

function getCitizenDetail(c) {
  return c;
}

function getStatusLabel(status) {
  const map = {
    Closed: { label: "Đã đóng", class: "bg-slate-100 text-slate-500" },
    PendingVerification: {
      label: "Chờ xác minh",
      class: "bg-violet-50 text-violet-700",
    },
    InProgress: { label: "Đang xử lý", class: "bg-amber-50 text-amber-700" },
  };
  return map[status] || { label: status, class: "bg-slate-100 text-slate-600" };
}

const activePeriod = ref("month");
const searchQuery = ref("");
const periodOptions = [
  { key: "month", label: "Theo tháng" },
  { key: "quarter", label: "Theo quý" },
];

const AVATAR_COLORS = [
  "bg-blue-100 text-blue-700",
  "bg-emerald-100 text-emerald-700",
  "bg-violet-100 text-violet-700",
  "bg-amber-100 text-amber-700",
  "bg-rose-100 text-rose-700",
  "bg-cyan-100 text-cyan-700",
  "bg-pink-100 text-pink-700",
  "bg-indigo-100 text-indigo-700",
  "bg-lime-100 text-lime-700",
  "bg-orange-100 text-orange-700",
];

const leaderboard = [
  {
    rank: 1,
    id: "citizen1",
    name: "Võ Thị Phương",
    initials: "TP",
    phone: "0905555555",
    ward: "Phường 2",
    district: "TP. Cao Lãnh",
    periodPoints: 85,
    totalPoints: 240,
    submitted: 18,
    approved: 16,
    rate: 89,
    avatarColor: AVATAR_COLORS[0],
    monthlyPoints: [
      { month: "01/2026", points: 25 },
      { month: "02/2026", points: 35 },
      { month: "03/2026", points: 45 },
      { month: "04/2026", points: 85 },
    ],
    quarterlyPoints: [
      { quarter: "Q1/2026", points: 105 },
      { quarter: "Q2/2026", points: 85 },
    ],
    topTickets: [
      {
        code: "PA-042026-003",
        title: "Cống thoát nước tắc nghẽn tại Phường 2",
        status: "Closed",
      },
      {
        code: "PA-032026-011",
        title: "Đèn chiếu sáng hư hỏng trên đường Lê Lợi",
        status: "Closed",
      },
      {
        code: "PA-032026-005",
        title: "Ổ gà nguy hiểm tại ngã tư Nguyễn Huệ",
        status: "PendingVerification",
      },
    ],
  },
  {
    rank: 2,
    id: "citizen2",
    name: "Nguyễn Văn An",
    initials: "VA",
    phone: "0904444444",
    ward: "Phường 1",
    district: "TP. Cao Lãnh",
    periodPoints: 72,
    totalPoints: 195,
    submitted: 15,
    approved: 12,
    rate: 80,
    avatarColor: AVATAR_COLORS[1],
    monthlyPoints: [
      { month: "01/2026", points: 30 },
      { month: "02/2026", points: 40 },
      { month: "03/2026", points: 53 },
      { month: "04/2026", points: 72 },
    ],
    quarterlyPoints: [
      { quarter: "Q1/2026", points: 123 },
      { quarter: "Q2/2026", points: 72 },
    ],
    topTickets: [
      {
        code: "PA-042026-012",
        title: "Lấn chiếm vỉa hè kinh doanh tại chợ Phường 1",
        status: "Closed",
      },
      {
        code: "PA-022026-008",
        title: "Rác thải sinh hoạt đổ trộm ven đường",
        status: "Closed",
      },
      {
        code: "PA-012026-022",
        title: "Cây xanh ngã đổ cản trở giao thông",
        status: "Closed",
      },
    ],
  },
  {
    rank: 3,
    id: "citizen3",
    name: "Trần Minh Hoàng",
    initials: "MH",
    phone: "0906666666",
    ward: "Phường 3",
    district: "TP. Cao Lãnh",
    periodPoints: 65,
    totalPoints: 180,
    submitted: 14,
    approved: 13,
    rate: 93,
    avatarColor: AVATAR_COLORS[2],
    monthlyPoints: [
      { month: "01/2026", points: 30 },
      { month: "02/2026", points: 35 },
      { month: "03/2026", points: 50 },
      { month: "04/2026", points: 65 },
    ],
    quarterlyPoints: [
      { quarter: "Q1/2026", points: 115 },
      { quarter: "Q2/2026", points: 65 },
    ],
    topTickets: [
      {
        code: "PA-042026-045",
        title: "Tiếng ồn quá mức từ cơ sở mộc sau 22h",
        status: "InProgress",
      },
      {
        code: "PA-032026-033",
        title: "Đường dây điện chùng xuống gần nhà dân",
        status: "Closed",
      },
      {
        code: "PA-022026-015",
        title: "Nắp hố ga bị mất trộm gây nguy hiểm",
        status: "Closed",
      },
    ],
  },
  {
    rank: 4,
    id: "citizen4",
    name: "Lê Thị Kim Ngân",
    initials: "KN",
    phone: "0907777777",
    ward: "Phường 6",
    district: "TP. Cao Lãnh",
    periodPoints: 55,
    totalPoints: 155,
    submitted: 12,
    approved: 10,
    rate: 83,
    avatarColor: AVATAR_COLORS[3],
    monthlyPoints: [
      { month: "01/2026", points: 25 },
      { month: "02/2026", points: 35 },
      { month: "03/2026", points: 40 },
      { month: "04/2026", points: 55 },
    ],
    quarterlyPoints: [
      { quarter: "Q1/2026", points: 100 },
      { quarter: "Q2/2026", points: 55 },
    ],
    topTickets: [
      {
        code: "PA-042026-088",
        title: "Xe tải đỗ trái phép cản trở ngõ ra vào",
        status: "PendingVerification",
      },
      {
        code: "PA-032026-067",
        title: "Đốt rác gây khói mù mịt khu dân cư",
        status: "Closed",
      },
      {
        code: "PA-022026-044",
        title: "Biển báo giao thông bị che khuất",
        status: "Closed",
      },
    ],
  },
  {
    rank: 5,
    id: "citizen5",
    name: "Phạm Quốc Đạt",
    initials: "QĐ",
    phone: "0908888888",
    ward: "Xã Mỹ Tân",
    district: "H. Cao Lãnh",
    periodPoints: 50,
    totalPoints: 140,
    submitted: 11,
    approved: 9,
    rate: 82,
    avatarColor: AVATAR_COLORS[4],
    monthlyPoints: [
      { month: "01/2026", points: 20 },
      { month: "02/2026", points: 30 },
      { month: "03/2026", points: 40 },
      { month: "04/2026", points: 50 },
    ],
    quarterlyPoints: [
      { quarter: "Q1/2026", points: 90 },
      { quarter: "Q2/2026", points: 50 },
    ],
    topTickets: [
      {
        code: "PA-042026-112",
        title: "Nước sinh hoạt có mùi lạ, màu đục",
        status: "InProgress",
      },
      {
        code: "PA-022026-089",
        title: "Hành vi xả thải trực tiếp xuống kênh",
        status: "Closed",
      },
      {
        code: "PA-012026-055",
        title: "Đường nông thôn bị sạt lở sau mưa lớn",
        status: "Closed",
      },
    ],
  },
  {
    rank: 6,
    id: "citizen6",
    name: "Huỳnh Thanh Tâm",
    initials: "TT",
    phone: "0909111222",
    ward: "Phường 4",
    district: "TP. Sa Đéc",
    periodPoints: 45,
    totalPoints: 130,
    submitted: 10,
    approved: 8,
    rate: 80,
    avatarColor: AVATAR_COLORS[5],
    monthlyPoints: [
      { month: "01/2026", points: 25 },
      { month: "02/2026", points: 30 },
      { month: "03/2026", points: 30 },
      { month: "04/2026", points: 45 },
    ],
    quarterlyPoints: [
      { quarter: "Q1/2026", points: 85 },
      { quarter: "Q2/2026", points: 45 },
    ],
    topTickets: [
      {
        code: "PA-042026-145",
        title: "Công trình xây dựng không che chắn bụi",
        status: "Closed",
      },
      {
        code: "PA-032026-123",
        title: "Đàn chó thả rông gây nguy hiểm trong công viên",
        status: "Closed",
      },
      {
        code: "PA-012026-098",
        title: "Biển quảng cáo sai quy định chắn tầm nhìn",
        status: "Closed",
      },
    ],
  },
  {
    rank: 7,
    id: "citizen7",
    name: "Đặng Văn Phúc",
    initials: "VP",
    phone: "0909333444",
    ward: "Phường 1",
    district: "TP. Sa Đéc",
    periodPoints: 40,
    totalPoints: 115,
    submitted: 9,
    approved: 7,
    rate: 78,
    avatarColor: AVATAR_COLORS[6],
    monthlyPoints: [
      { month: "01/2026", points: 20 },
      { month: "02/2026", points: 25 },
      { month: "03/2026", points: 30 },
      { month: "04/2026", points: 40 },
    ],
    quarterlyPoints: [
      { quarter: "Q1/2026", points: 75 },
      { quarter: "Q2/2026", points: 40 },
    ],
    topTickets: [
      {
        code: "PA-042026-166",
        title: "Hư hỏng thiết bị tập thể dục tại công viên",
        status: "PendingVerification",
      },
      {
        code: "PA-022026-154",
        title: "Tụ tập buôn bán hàng rong trước cổng trường",
        status: "Closed",
      },
      {
        code: "PA-022026-132",
        title: "Đường ống cấp nước bị vỡ rò rỉ trên vỉa hè",
        status: "Closed",
      },
    ],
  },
  {
    rank: 8,
    id: "citizen8",
    name: "Bùi Thị Hồng",
    initials: "TH",
    phone: "0909555666",
    ward: "Xã An Bình",
    district: "H. Cao Lãnh",
    periodPoints: 35,
    totalPoints: 100,
    submitted: 8,
    approved: 6,
    rate: 75,
    avatarColor: AVATAR_COLORS[7],
    monthlyPoints: [
      { month: "01/2026", points: 15 },
      { month: "02/2026", points: 25 },
      { month: "03/2026", points: 25 },
      { month: "04/2026", points: 35 },
    ],
    quarterlyPoints: [
      { quarter: "Q1/2026", points: 65 },
      { quarter: "Q2/2026", points: 35 },
    ],
    topTickets: [
      {
        code: "PA-042026-201",
        title: "Khói bụi mịt mù từ lò gạch thủ công",
        status: "InProgress",
      },
      {
        code: "PA-032026-188",
        title: "Cầu dân sinh có dấu hiệu nứt mố cầu",
        status: "Closed",
      },
      {
        code: "PA-012026-176",
        title: "Kênh mương nội đồng bị bồi lấp",
        status: "Closed",
      },
    ],
  },
  {
    rank: 9,
    id: "citizen9",
    name: "Cao Minh Trí",
    initials: "MT",
    phone: "0909777888",
    ward: "Phường 2",
    district: "TP. Cao Lãnh",
    periodPoints: 30,
    totalPoints: 90,
    submitted: 7,
    approved: 5,
    rate: 71,
    avatarColor: AVATAR_COLORS[8],
    monthlyPoints: [
      { month: "01/2026", points: 10 },
      { month: "02/2026", points: 25 },
      { month: "03/2026", points: 25 },
      { month: "04/2026", points: 30 },
    ],
    quarterlyPoints: [
      { quarter: "Q1/2026", points: 60 },
      { quarter: "Q2/2026", points: 30 },
    ],
    topTickets: [
      {
        code: "PA-042026-234",
        title: "Quán karaoke hoạt động quá giờ quy định",
        status: "Closed",
      },
      {
        code: "PA-022026-222",
        title: "Trụ đèn giao thông nhấp nháy liên tục",
        status: "Closed",
      },
      {
        code: "PA-012026-205",
        title: "Rễ cây làm bong tróc lớp gạch vỉa hè",
        status: "Closed",
      },
    ],
  },
  {
    rank: 10,
    id: "citizen10",
    name: "Ngô Thị Mai Anh",
    initials: "MA",
    phone: "0909999000",
    ward: "Phường 3",
    district: "TX. Hồng Ngự",
    periodPoints: 25,
    totalPoints: 75,
    submitted: 6,
    approved: 5,
    rate: 83,
    avatarColor: AVATAR_COLORS[9],
    monthlyPoints: [
      { month: "01/2026", points: 10 },
      { month: "02/2026", points: 20 },
      { month: "03/2026", points: 20 },
      { month: "04/2026", points: 25 },
    ],
    quarterlyPoints: [
      { quarter: "Q1/2026", points: 50 },
      { quarter: "Q2/2026", points: 25 },
    ],
    topTickets: [
      {
        code: "PA-042026-267",
        title: "Rác y tế vứt bừa bãi tại bãi rác sinh hoạt",
        status: "PendingVerification",
      },
      {
        code: "PA-032026-255",
        title: "Tuyến đê bao có hiện tượng sụt lún",
        status: "Closed",
      },
      {
        code: "PA-022026-241",
        title: "Bãi tập kết vật liệu xây dựng lấn chiếm lề đường",
        status: "Closed",
      },
    ],
  },
];

const top3 = computed(() => leaderboard.slice(0, 3));

const filteredLeaderboard = computed(() => {
  if (!searchQuery.value) return leaderboard;
  const q = searchQuery.value.toLowerCase();
  return leaderboard.filter(
    (c) =>
      c.name.toLowerCase().includes(q) ||
      c.ward.toLowerCase().includes(q) ||
      c.district.toLowerCase().includes(q),
  );
});

const statsCards = [
  {
    label: "Tổng công dân tham gia",
    value: "847",
    icon: Users,
    color: "bg-blue-50 text-blue-600",
  },
  {
    label: "Hoạt động trong tháng",
    value: "156",
    icon: TrendingUp,
    color: "bg-emerald-50 text-emerald-600",
  },
  {
    label: "Tỷ lệ duyệt TB",
    value: "82%",
    icon: Target,
    color: "bg-amber-50 text-amber-600",
  },
  {
    label: "Đề xuất khen thưởng",
    value: "3",
    icon: Award,
    color: "bg-violet-50 text-violet-600",
  },
];

const pointRules = [
  {
    action: "Phản ánh được duyệt",
    points: 10,
    desc: "Báo cáo hợp lệ được kiểm duyệt",
  },
  {
    action: "Phản ánh đã xử lý xong",
    points: 5,
    desc: "Báo cáo được đóng thành công",
  },
  {
    action: "Đánh giá kết quả",
    points: 2,
    desc: "Gửi đánh giá chất lượng xử lý",
  },
  {
    action: "PA đầu tiên trong tháng",
    points: 5,
    desc: "Thưởng khuyến khích hoạt động",
  },
  {
    action: "Hoạt động liên tiếp",
    points: 3,
    desc: "Gửi phản ánh nhiều ngày liên tiếp",
  },
  {
    action: "Phản ánh bị từ chối",
    points: -5,
    desc: "Báo cáo spam hoặc không hợp lệ",
  },
];
</script>
