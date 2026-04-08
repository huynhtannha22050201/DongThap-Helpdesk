<template>
  <div class="space-y-5">
    <!-- HEADER -->
    <div>
      <button
        @click="$router.back()"
        class="flex items-center gap-1.5 text-slate-500 hover:text-slate-700 mb-3 cursor-pointer transition-colors text-[14px]"
      >
        <ArrowLeft :size="16" /> Quay lại danh sách
      </button>

      <div
        class="flex flex-col lg:flex-row lg:items-start lg:justify-between gap-4"
      >
        <div>
          <div class="flex flex-wrap items-center gap-2.5 mb-2">
            <h1
              class="text-slate-800 font-mono font-bold"
              style="font-size: clamp(20px, 3vw, 26px)"
            >
              {{ ticket.code }}
            </h1>
            <StatusBadge :status="ticket.status" />
            <PriorityBadge :priority="ticket.priority" />
          </div>
          <p class="text-slate-600 max-w-2xl text-[15px] leading-[1.6]">
            {{ ticket.title }}
          </p>
          <div class="flex flex-wrap items-center gap-x-4 gap-y-1 mt-2">
            <span class="text-slate-400 flex items-center gap-1 text-[13px]"
              ><Calendar :size="13" /> {{ ticket.createdAt }}</span
            >
            <span class="text-slate-400 flex items-center gap-1 text-[13px]"
              ><Tag :size="13" /> {{ ticket.category }}</span
            >
            <span class="text-slate-400 flex items-center gap-1 text-[13px]"
              ><MapPin :size="13" /> {{ ticket.location.ward }},
              {{ ticket.location.district }}</span
            >
          </div>
        </div>

        <!-- Actions -->
        <div class="flex items-center gap-2 shrink-0 flex-wrap">
          <template v-if="ticket.status === 'New'">
            <button
              class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-[#DA251D] text-white hover:bg-[#b81f18] transition-colors cursor-pointer shadow-sm text-[14px] font-medium"
            >
              <UserPlus :size="16" /> Duyệt & Phân công
            </button>
            <button
              class="flex items-center gap-2 px-4 py-2.5 rounded-lg border border-red-200 text-red-600 bg-white hover:bg-red-50 transition-colors cursor-pointer text-[14px] font-medium"
            >
              <XCircle :size="16" /> Từ chối
            </button>
          </template>
          <template v-else-if="ticket.status === 'Assigned'">
            <button
              class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-[#DA251D] text-white hover:bg-[#b81f18] transition-colors cursor-pointer shadow-sm text-[14px] font-medium"
            >
              <Play :size="16" /> Bắt đầu xử lý
            </button>
          </template>
          <template v-else-if="ticket.status === 'InProgress'">
            <button
              class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-[#DA251D] text-white hover:bg-[#b81f18] transition-colors cursor-pointer shadow-sm text-[14px] font-medium"
            >
              <Send :size="16" /> Báo cáo kết quả
            </button>
          </template>
          <template v-else-if="ticket.status === 'Resolved'">
            <button
              class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-[#DA251D] text-white hover:bg-[#b81f18] transition-colors cursor-pointer shadow-sm text-[14px] font-medium"
            >
              <CheckCircle2 :size="16" /> Đóng phản ánh
            </button>
            <button
              class="flex items-center gap-2 px-4 py-2.5 rounded-lg border border-slate-200 text-slate-600 bg-white hover:bg-slate-50 transition-colors cursor-pointer text-[14px] font-medium"
            >
              <RotateCcw :size="16" /> Yêu cầu xử lý lại
            </button>
          </template>
          <div class="w-px h-8 bg-slate-200 hidden lg:block mx-1" />
          <button
            class="p-2.5 rounded-lg border border-slate-200 text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors cursor-pointer"
            title="In"
          >
            <Printer :size="16" />
          </button>
          <button
            class="p-2.5 rounded-lg border border-slate-200 text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors cursor-pointer"
            title="Chia sẻ"
          >
            <Share2 :size="16" />
          </button>
          <button
            class="p-2.5 rounded-lg border border-slate-200 text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors cursor-pointer"
            title="Thêm"
          >
            <MoreHorizontal :size="16" />
          </button>
        </div>
      </div>
    </div>

    <!-- TWO-COLUMN LAYOUT -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-5">
      <!-- LEFT COLUMN (2/3) -->
      <div class="lg:col-span-2 space-y-5">
        <!-- Thông tin phản ánh -->
        <SectionCard title="Thông tin phản ánh" :icon="FileText">
          <div class="mb-5">
            <p
              class="text-slate-700 whitespace-pre-line text-[14px] leading-[1.8]"
            >
              {{ ticket.description }}
            </p>
          </div>
          <div>
            <div class="flex items-center gap-2 mb-3">
              <ImageIcon :size="15" class="text-slate-400" />
              <span class="text-slate-500 text-[13px] font-medium"
                >Hình ảnh đính kèm ({{ ticket.attachments.length }})</span
              >
            </div>
            <div class="grid grid-cols-2 sm:grid-cols-3 gap-3">
              <button
                v-for="(img, i) in ticket.attachments"
                :key="i"
                @click="lightboxIdx = i"
                class="relative aspect-[4/3] rounded-lg overflow-hidden group cursor-pointer border border-slate-100"
              >
                <img
                  :src="img"
                  :alt="'Ảnh ' + (i + 1)"
                  class="w-full h-full object-cover transition-transform group-hover:scale-105"
                />
                <div
                  class="absolute inset-0 bg-black/0 group-hover:bg-black/20 transition-colors flex items-center justify-center"
                >
                  <ZoomIn
                    :size="20"
                    class="text-white opacity-0 group-hover:opacity-100 transition-opacity drop-shadow-lg"
                  />
                </div>
              </button>
            </div>
          </div>
        </SectionCard>

        <!-- Vị trí sự việc -->
        <SectionCard title="Vị trí sự việc" :icon="MapPin">
          <div
            class="relative rounded-xl overflow-hidden mb-4"
            style="height: 240px"
          >
            <img
              src="https://images.unsplash.com/photo-1758298135102-8d3617b32880?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxhZXJpYWwlMjB2aWV3JTIwZ3JlZW4lMjBkZWx0YSUyMHJpdmVyJTIwVmlldG5hbXxlbnwxfHx8fDE3NzUxMjE5NzJ8MA&ixlib=rb-4.1.0&q=80&w=1080"
              alt="Bản đồ"
              class="w-full h-full object-cover"
            />
            <div class="absolute inset-0 bg-slate-900/10" />
            <div
              class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2"
            >
              <div class="relative">
                <div
                  class="w-8 h-8 bg-[#DA251D] rounded-full border-[3px] border-white shadow-lg flex items-center justify-center"
                >
                  <MapPin :size="16" class="text-white" />
                </div>
                <div
                  class="absolute inset-0 w-8 h-8 rounded-full bg-[#DA251D]/30 animate-ping"
                />
              </div>
            </div>
            <div
              class="absolute bottom-3 left-3 bg-white/90 backdrop-blur-sm px-3 py-1.5 rounded-md shadow-sm text-[12px]"
            >
              <span class="text-slate-500"
                >{{ ticket.location.lat }}°N, {{ ticket.location.lng }}°E</span
              >
            </div>
          </div>
          <div class="flex items-start gap-2">
            <MapPin :size="16" class="text-slate-400 mt-0.5 shrink-0" />
            <div>
              <p class="text-slate-700 text-[14px] font-medium">
                {{ ticket.location.address }}
              </p>
              <p class="text-slate-500 mt-0.5 text-[13px]">
                {{ ticket.location.ward }}, {{ ticket.location.district }},
                {{ ticket.location.province }}
              </p>
            </div>
          </div>
        </SectionCard>

        <!-- Lịch sử xử lý -->
        <SectionCard title="Lịch sử xử lý" :icon="Clock">
          <div class="relative">
            <div
              v-for="(a, i) in activities"
              :key="a.id"
              class="relative flex gap-4 pb-6 last:pb-0"
            >
              <div
                v-if="i < activities.length - 1"
                class="absolute left-[15px] top-[32px] w-[2px] bg-slate-100 bottom-0"
              />
              <div
                :class="[
                  'w-8 h-8 rounded-full flex items-center justify-center shrink-0 z-10',
                  activityConfig[a.type].bg,
                  activityConfig[a.type].textColor,
                ]"
              >
                <component :is="activityConfig[a.type].icon" :size="14" />
              </div>
              <div class="flex-1 min-w-0 pt-0.5">
                <div class="flex flex-wrap items-center gap-x-2 gap-y-1 mb-1">
                  <span class="text-slate-700 text-[13px] font-semibold">{{
                    a.actor
                  }}</span>
                  <span class="text-slate-400 text-[12px]"
                    >({{ a.actorRole }})</span
                  >
                  <template v-if="a.statusFrom && a.statusTo">
                    <StatusBadge :status="a.statusFrom" size="sm" />
                    <ChevronRight :size="12" class="text-slate-300" />
                    <StatusBadge :status="a.statusTo" size="sm" />
                  </template>
                </div>
                <p class="text-slate-600 mb-1.5 text-[13px] leading-[1.6]">
                  {{ a.description }}
                </p>
                <span class="text-slate-400 text-[12px]">{{
                  a.timestamp
                }}</span>
              </div>
            </div>
          </div>

          <!-- Comment input -->
          <div class="mt-5 pt-5 border-t border-slate-100">
            <div class="flex gap-3">
              <div
                class="w-8 h-8 rounded-full bg-[#DA251D]/10 flex items-center justify-center shrink-0 text-[11px] font-semibold"
              >
                <span class="text-[#DA251D]">TA</span>
              </div>
              <div class="flex-1">
                <textarea
                  v-model="commentText"
                  placeholder="Thêm ghi chú nội bộ..."
                  class="w-full px-4 py-3 rounded-lg border border-slate-200 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10 outline-none resize-none transition-all text-slate-700 text-[14px] leading-[1.6]"
                  rows="3"
                />
                <div class="flex justify-end mt-2">
                  <button
                    :disabled="!commentText.trim()"
                    class="flex items-center gap-2 px-4 py-2 rounded-lg bg-[#DA251D] text-white hover:bg-[#b81f18] disabled:opacity-40 disabled:cursor-not-allowed transition-colors cursor-pointer text-[13px] font-medium"
                  >
                    <Send :size="14" /> Gửi ghi chú
                  </button>
                </div>
              </div>
            </div>
          </div>
        </SectionCard>
      </div>

      <!-- RIGHT COLUMN (1/3) -->
      <div class="space-y-5">
        <!-- Người phản ánh -->
        <SectionCard title="Người phản ánh" :icon="User">
          <div
            class="flex items-center gap-3 mb-4 pb-4 border-b border-slate-100"
          >
            <div
              class="w-12 h-12 rounded-full bg-slate-100 flex items-center justify-center text-slate-600 shrink-0 text-[16px] font-semibold"
            >
              {{ ticket.reporter.initials }}
            </div>
            <div>
              <p class="text-slate-800 text-[15px] font-semibold">
                {{ ticket.reporter.name }}
              </p>
              <p class="text-slate-400 text-[13px]">Công dân</p>
            </div>
          </div>
          <div class="space-y-1">
            <InfoRow
              label="Số điện thoại"
              :value="ticket.reporter.phone"
              :icon="Phone"
            />
            <InfoRow
              label="Địa chỉ"
              :value="ticket.reporter.address"
              :icon="MapPin"
            />
            <InfoRow label="Phường/Xã" :value="ticket.reporter.ward" />
            <InfoRow label="Quận/Huyện" :value="ticket.reporter.district" />
          </div>
          <a
            :href="'tel:' + ticket.reporter.phone"
            class="flex items-center justify-center gap-2 w-full mt-4 px-4 py-2.5 rounded-lg bg-emerald-50 text-emerald-700 border border-emerald-200 hover:bg-emerald-100 transition-colors cursor-pointer text-[14px] font-medium"
          >
            <Phone :size="15" /> Gọi điện
          </a>
        </SectionCard>

        <!-- Phân công xử lý -->
        <SectionCard title="Phân công xử lý" :icon="UserPlus">
          <div
            class="flex items-center gap-3 mb-4 pb-4 border-b border-slate-100"
          >
            <div
              class="w-10 h-10 rounded-full bg-indigo-50 flex items-center justify-center text-indigo-600 shrink-0 text-[13px] font-semibold"
            >
              {{ ticket.assignment.userInitials }}
            </div>
            <div>
              <p class="text-slate-800 text-[14px] font-semibold">
                {{ ticket.assignment.user }}
              </p>
              <p class="text-slate-400 text-[12px]">Người xử lý</p>
            </div>
          </div>
          <div class="space-y-1">
            <InfoRow
              label="Phòng ban"
              :value="ticket.assignment.department"
              :icon="Building2"
            />
            <InfoRow
              label="Phân công lúc"
              :value="ticket.assignment.assignedAt"
              :icon="Calendar"
            />
          </div>
          <button
            class="flex items-center justify-center gap-2 w-full mt-4 px-4 py-2.5 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors cursor-pointer text-[13px] font-medium"
          >
            <RotateCcw :size="14" /> Chuyển phân công
          </button>
        </SectionCard>

        <!-- SLA -->
        <SectionCard title="Thông tin SLA" :icon="Timer">
          <div class="space-y-3 mb-4">
            <InfoRow
              label="Thời hạn xử lý"
              :value="
                ticket.sla.hours + ' giờ (' + ticket.sla.hours / 24 + ' ngày)'
              "
              :icon="Clock"
            />
            <InfoRow
              label="Hạn chót"
              :value="ticket.sla.deadline"
              :icon="Calendar"
            />
          </div>
          <div class="mb-2">
            <div class="flex items-center justify-between mb-1.5">
              <span class="text-slate-500 text-[12px]"
                >Tiến trình thời gian</span
              >
              <span :class="slaTextColor" class="text-[13px] font-semibold"
                >{{ ticket.sla.progressPercent }}%</span
              >
            </div>
            <div class="w-full h-2.5 bg-slate-100 rounded-full overflow-hidden">
              <div
                :class="slaBarColor"
                class="h-full rounded-full transition-all duration-500"
                :style="{
                  width: Math.min(ticket.sla.progressPercent, 100) + '%',
                }"
              />
            </div>
          </div>
          <div
            :class="[ticket.sla.breached ? 'bg-red-50' : 'bg-slate-50']"
            class="flex items-center justify-center gap-2 py-3 rounded-lg mt-3"
          >
            <Timer :size="16" :class="slaTextColor" />
            <span :class="slaTextColor" class="text-[15px] font-semibold">
              {{ ticket.sla.breached ? "Quá hạn " : "Còn lại: "
              }}{{ ticket.sla.remainingText }}
            </span>
          </div>
        </SectionCard>

        <!-- Thông tin hệ thống -->
        <div class="bg-white rounded-xl shadow-sm border border-slate-100 p-5">
          <h4
            class="text-slate-500 mb-3 text-[12px] font-semibold uppercase tracking-[0.05em]"
          >
            Thông tin hệ thống
          </h4>
          <div class="space-y-2.5">
            <div
              v-for="item in sysInfo"
              :key="item.l"
              class="flex items-center justify-between"
            >
              <span class="text-slate-400 text-[13px]">{{ item.l }}</span>
              <span class="text-slate-700 text-[13px] font-medium">{{
                item.v
              }}</span>
            </div>
          </div>
          <button
            class="flex items-center justify-center gap-1.5 w-full mt-3 pt-3 border-t border-slate-100 text-slate-400 hover:text-slate-600 cursor-pointer transition-colors text-[13px]"
          >
            <Copy :size="13" /> Sao chép mã PA
          </button>
        </div>
      </div>
    </div>

    <!-- Lightbox -->
    <Teleport to="body">
      <div
        v-if="lightboxIdx !== null"
        class="fixed inset-0 z-50 bg-black/80 backdrop-blur-sm flex items-center justify-center p-4"
        @click="lightboxIdx = null"
      >
        <button
          @click="lightboxIdx = null"
          class="absolute top-4 right-4 text-white/80 hover:text-white cursor-pointer z-50 p-2"
        >
          <X :size="24" />
        </button>
        <button
          @click.stop="navLightbox(-1)"
          class="absolute left-4 top-1/2 -translate-y-1/2 text-white/70 hover:text-white bg-black/30 hover:bg-black/50 rounded-full p-2 cursor-pointer z-50"
        >
          <ChevronLeft :size="24" />
        </button>
        <button
          @click.stop="navLightbox(1)"
          class="absolute right-4 top-1/2 -translate-y-1/2 text-white/70 hover:text-white bg-black/30 hover:bg-black/50 rounded-full p-2 cursor-pointer z-50"
        >
          <ChevronRight :size="24" />
        </button>
        <img
          :src="ticket.attachments[lightboxIdx]"
          :alt="'Ảnh ' + (lightboxIdx + 1)"
          class="max-h-[85vh] max-w-[90vw] object-contain rounded-lg"
          @click.stop
        />
        <div
          class="absolute bottom-6 left-1/2 -translate-x-1/2 text-white/70 text-[14px]"
        >
          {{ lightboxIdx + 1 }} / {{ ticket.attachments.length }}
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, h } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  ArrowLeft,
  Calendar,
  Tag,
  MapPin,
  UserPlus,
  XCircle,
  Play,
  Send,
  CheckCircle2,
  RotateCcw,
  Printer,
  Share2,
  MoreHorizontal,
  FileText,
  ImageIcon,
  ZoomIn,
  Clock,
  Phone,
  Building2,
  User,
  Timer,
  AlertTriangle,
  ChevronRight,
  ChevronLeft,
  X,
  Copy,
  Shield,
  MessageSquare,
  CircleDot,
} from "lucide-vue-next";

const route = useRoute();
const router = useRouter();
const commentText = ref("");
const lightboxIdx = ref(null);

function navLightbox(dir) {
  if (lightboxIdx.value === null) return;
  const len = ticket.attachments.length;
  lightboxIdx.value = (lightboxIdx.value + dir + len) % len;
}

// ── Sub-components ──
const StatusBadge = {
  props: { status: String, size: { type: String, default: "md" } },
  setup(props) {
    const cfg = {
      New: { bg: "bg-blue-50 text-blue-700", dot: "bg-blue-500", label: "Mới" },
      Assigned: {
        bg: "bg-indigo-50 text-indigo-700",
        dot: "bg-indigo-500",
        label: "Đã giao",
      },
      InProgress: {
        bg: "bg-amber-50 text-amber-700",
        dot: "bg-amber-500",
        label: "Đang xử lý",
      },
      Resolved: {
        bg: "bg-emerald-50 text-emerald-700",
        dot: "bg-emerald-500",
        label: "Đã xử lý",
      },
      Closed: {
        bg: "bg-slate-100 text-slate-500",
        dot: "bg-slate-400",
        label: "Đã đóng",
      },
      Rejected: {
        bg: "bg-red-50 text-red-600",
        dot: "bg-red-500",
        label: "Từ chối",
      },
    };
    const c = computed(() => cfg[props.status] || cfg.New);
    const sz = computed(() =>
      props.size === "sm"
        ? "px-2 py-0.5 text-[11px]"
        : "px-2.5 py-1 text-[13px]",
    );
    return { c, sz };
  },
  template: `<span :class="['inline-flex items-center gap-1.5 rounded-full font-medium', c.bg, sz]"><span :class="['w-1.5 h-1.5 rounded-full', c.dot]" />{{ c.label }}</span>`,
};

const PriorityBadge = {
  props: ["priority"],
  components: { CircleDot },
  setup(props) {
    const cfg = {
      High: {
        bg: "bg-red-50 text-red-600 border-red-100",
        label: "Ưu tiên cao",
      },
      Normal: {
        bg: "bg-blue-50 text-blue-600 border-blue-100",
        label: "Ưu tiên TB",
      },
      Low: {
        bg: "bg-slate-50 text-slate-500 border-slate-200",
        label: "Ưu tiên thấp",
      },
    };
    const c = computed(() => cfg[props.priority] || cfg.Normal);
    return { c };
  },
  template: `<span :class="['inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full border text-[13px] font-medium', c.bg]"><CircleDot :size="12" /> {{ c.label }}</span>`,
};

const SectionCard = {
  props: ["title", "icon"],
  template: `
    <div class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden">
      <div class="flex items-center justify-between px-5 py-3.5 border-b border-slate-100">
        <div class="flex items-center gap-2">
          <component :is="icon" :size="18" class="text-[#DA251D]" />
          <h3 class="text-slate-800 text-[15px] font-semibold">{{ title }}</h3>
        </div>
      </div>
      <div class="p-5"><slot /></div>
    </div>
  `,
};

const InfoRow = {
  props: ["label", "value", "icon"],
  template: `
    <div class="flex items-start gap-3 py-2.5">
      <div v-if="icon" class="text-slate-400 mt-0.5 shrink-0"><component :is="icon" :size="15" /></div>
      <div class="min-w-0">
        <p class="text-slate-400 mb-0.5 text-[12px] font-medium">{{ label }}</p>
        <div class="text-slate-700 text-[14px]">{{ value }}</div>
      </div>
    </div>
  `,
};

// ── Activity config ──
const activityConfig = {
  Created: { bg: "bg-blue-50", textColor: "text-blue-600", icon: FileText },
  Reviewed: { bg: "bg-indigo-50", textColor: "text-indigo-600", icon: Shield },
  Assigned: {
    bg: "bg-violet-50",
    textColor: "text-violet-600",
    icon: UserPlus,
  },
  InProgress: { bg: "bg-amber-50", textColor: "text-amber-600", icon: Play },
  Resolved: {
    bg: "bg-emerald-50",
    textColor: "text-emerald-600",
    icon: CheckCircle2,
  },
  Closed: {
    bg: "bg-slate-50",
    textColor: "text-slate-600",
    icon: CheckCircle2,
  },
  Rejected: { bg: "bg-red-50", textColor: "text-red-600", icon: XCircle },
  Comment: {
    bg: "bg-slate-50",
    textColor: "text-slate-600",
    icon: MessageSquare,
  },
};

// ── SLA computed ──
const slaBarColor = computed(() => {
  if (ticket.sla.breached) return "bg-red-500";
  if (ticket.sla.progressPercent > 80) return "bg-amber-500";
  return "bg-emerald-500";
});
const slaTextColor = computed(() => {
  if (ticket.sla.breached) return "text-red-600";
  if (ticket.sla.progressPercent > 80) return "text-amber-600";
  return "text-emerald-600";
});

const sysInfo = [
  { l: "Mã phản ánh", v: "PA-032026-001" },
  { l: "Ngày tạo", v: "28/03/2026 09:15" },
  { l: "Cập nhật cuối", v: "01/04/2026 16:30" },
  { l: "Lĩnh vực", v: "Giao thông" },
];

// ── Mock data ──
const ticket = {
  code: "PA-032026-001",
  title:
    "Ổ gà lớn trên đường Nguyễn Huệ, đoạn gần ngã tư Lê Lợi gây nguy hiểm cho người tham gia giao thông",
  description:
    "Trên đường Nguyễn Huệ, đoạn từ ngã tư Lê Lợi đến cầu Đúc (Phường 2, TP. Cao Lãnh), xuất hiện nhiều ổ gà lớn với đường kính khoảng 50-80cm, sâu 10-15cm. Tình trạng này đã kéo dài hơn 2 tuần và gây nguy hiểm cho người đi xe máy, đặc biệt vào ban đêm do không có đèn chiếu sáng đầy đủ.\n\nĐã có ít nhất 3 vụ tai nạn nhỏ trong tuần qua do ổ gà này. Người dân xung quanh đã tự đặt biển cảnh báo tạm thời nhưng không đảm bảo an toàn.\n\nKính đề nghị các cơ quan chức năng sớm khắc phục tình trạng trên để đảm bảo an toàn giao thông cho người dân.",
  status: "InProgress",
  priority: "High",
  category: "Giao thông",
  createdAt: "28/03/2026 09:15",
  updatedAt: "01/04/2026 16:30",
  location: {
    address: "Đường Nguyễn Huệ, gần ngã tư Lê Lợi",
    ward: "Phường 2",
    district: "TP. Cao Lãnh",
    province: "Đồng Tháp",
    lat: 10.4612,
    lng: 105.6315,
  },
  reporter: {
    name: "Nguyễn Văn An",
    phone: "0901234567",
    address: "123 Nguyễn Huệ",
    ward: "Phường 2",
    district: "TP. Cao Lãnh",
    initials: "VA",
  },
  assignment: {
    department: "Phòng Hạ tầng Giao thông",
    user: "Lê Văn Bình",
    userInitials: "VB",
    assignedAt: "29/03/2026 14:30",
  },
  sla: {
    hours: 168,
    deadline: "04/04/2026 09:15",
    breached: false,
    remainingHours: 52,
    remainingText: "2 ngày 4 giờ",
    progressPercent: 69,
  },
  attachments: [
    "https://images.unsplash.com/photo-1767423656805-899ea2042559?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxwb3Rob2xlJTIwcm9hZCUyMGRhbWFnZSUyMFZpZXRuYW18ZW58MXx8fHwxNzc1MTIxOTY4fDA&ixlib=rb-4.1.0&q=80&w=1080",
    "https://images.unsplash.com/photo-1741996951192-f4762170f3cb?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxicm9rZW4lMjByb2FkJTIwYXNwaGFsdCUyMGNyYWNrJTIwc3VyZmFjZXxlbnwxfHx8fDE3NzUxMjE5Njh8MA&ixlib=rb-4.1.0&q=80&w=1080",
    "https://images.unsplash.com/photo-1632992826018-d091b6fc0e09?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxWaWV0bmFtJTIwcnVyYWwlMjByb2FkJTIwaW5mcmFzdHJ1Y3R1cmV8ZW58MXx8fHwxNzc1MTIxOTY5fDA&ixlib=rb-4.1.0&q=80&w=1080",
  ],
};

const activities = [
  {
    id: "a1",
    timestamp: "01/04/2026 16:30",
    actor: "Lê Văn Bình",
    actorRole: "Assignee",
    type: "Comment",
    description:
      "Đã khảo sát thực địa, xác nhận 5 vị trí ổ gà nghiêm trọng. Đang lên phương án sửa chữa, dự kiến hoàn thành trong 2 ngày.",
    statusFrom: null,
    statusTo: null,
  },
  {
    id: "a2",
    timestamp: "30/03/2026 08:00",
    actor: "Lê Văn Bình",
    actorRole: "Assignee",
    type: "InProgress",
    description: "Bắt đầu xử lý phản ánh — tiến hành khảo sát thực địa",
    statusFrom: "Assigned",
    statusTo: "InProgress",
  },
  {
    id: "a3",
    timestamp: "29/03/2026 14:30",
    actor: "Trần Thị Hương",
    actorRole: "Dispatcher",
    type: "Assigned",
    description:
      "Duyệt và phân công cho Phòng Hạ tầng Giao thông — Lê Văn Bình",
    statusFrom: "New",
    statusTo: "Assigned",
  },
  {
    id: "a4",
    timestamp: "29/03/2026 10:00",
    actor: "Trần Thị Hương",
    actorRole: "Dispatcher",
    type: "Reviewed",
    description:
      "Duyệt phản ánh — xác nhận nội dung hợp lệ, phân loại ưu tiên Cao",
    statusFrom: null,
    statusTo: null,
  },
  {
    id: "a5",
    timestamp: "28/03/2026 09:15",
    actor: "Nguyễn Văn An",
    actorRole: "Citizen",
    type: "Created",
    description: "Tạo phản ánh mới qua Cổng dịch vụ công trực tuyến",
    statusFrom: null,
    statusTo: null,
  },
];
</script>
