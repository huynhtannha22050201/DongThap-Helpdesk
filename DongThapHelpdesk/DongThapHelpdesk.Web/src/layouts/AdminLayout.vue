<template>
  <div class="min-h-screen flex bg-slate-50">
    <!-- Sidebar -->
    <aside
      :class="[
        'fixed inset-y-0 left-0 z-30 flex flex-col bg-white border-r border-slate-200 transition-all duration-300',
        isCollapsed ? 'w-16' : 'w-[260px]',
      ]"
    >
      <!-- Logo -->
      <div
        class="h-16 flex items-center gap-3 px-4 bg-[var(--color-brand-red)] text-white"
      >
        <div
          class="w-9 h-9 bg-white/20 rounded-lg flex items-center justify-center font-bold text-sm shrink-0"
        >
          ĐT
        </div>
        <div v-show="!isCollapsed" class="overflow-hidden">
          <p class="font-semibold text-sm leading-tight truncate">
            Hệ thống Phản ánh
          </p>
          <p class="text-xs text-white/80 truncate">Tỉnh Đồng Tháp</p>
        </div>
      </div>

      <!-- Collapse Toggle -->
      <button
        @click="isCollapsed = !isCollapsed"
        class="absolute -right-3 top-20 w-6 h-6 bg-white border border-slate-200 rounded-full flex items-center justify-center shadow-sm hover:bg-slate-50 z-40"
      >
        <span class="text-slate-500 text-xs">{{
          isCollapsed ? "›" : "‹"
        }}</span>
      </button>

      <!-- Navigation -->
      <nav class="flex-1 overflow-y-auto py-4 px-3 space-y-6">
        <div v-for="group in visibleMenuGroups" :key="group.label">
          <p
            v-show="!isCollapsed"
            class="px-3 mb-2 text-xs font-semibold text-slate-400 uppercase tracking-wider"
          >
            {{ group.label }}
          </p>
          <ul class="space-y-1">
            <li v-for="item in group.items" :key="item.name">
              <RouterLink
                :to="item.to"
                :class="[
                  'flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors',
                  isActive(item.to)
                    ? 'bg-[var(--color-brand-red)] text-white'
                    : 'text-slate-600 hover:bg-slate-100',
                ]"
                :title="isCollapsed ? item.label : ''"
              >
                <component :is="item.icon" class="w-5 h-5 shrink-0" />
                <span v-show="!isCollapsed">{{ item.label }}</span>
              </RouterLink>
            </li>
          </ul>
        </div>
      </nav>

      <!-- User Info -->
      <div class="border-t border-slate-200 p-3">
        <div class="flex items-center gap-3">
          <div
            class="w-9 h-9 bg-slate-200 rounded-full flex items-center justify-center text-sm font-semibold text-slate-600 shrink-0"
          >
            {{ initials }}
          </div>
          <div v-show="!isCollapsed" class="overflow-hidden flex-1">
            <p class="text-sm font-medium text-slate-800 truncate">
              {{ auth.fullName }}
            </p>
            <span
              class="inline-block px-2 py-0.5 text-xs font-medium rounded-full bg-[var(--color-brand-red)] text-white"
            >
              {{ auth.userRole }}
            </span>
          </div>
        </div>
        <button
          @click="handleLogout"
          :class="[
            'mt-3 flex items-center gap-2 text-sm text-slate-500 hover:text-[var(--color-brand-red)] transition-colors w-full',
            isCollapsed ? 'justify-center' : 'px-1',
          ]"
        >
          <LogOut class="w-4 h-4" />
          <span v-show="!isCollapsed">Đăng xuất</span>
        </button>
      </div>
    </aside>

    <!-- Main Area -->
    <div
      :class="[
        'flex-1 flex flex-col transition-all duration-300',
        isCollapsed ? 'ml-16' : 'ml-[260px]',
      ]"
    >
      <!-- Top Header -->
      <header
        class="sticky top-0 z-20 h-14 bg-white border-b border-slate-200 flex items-center justify-between px-6"
      >
        <!-- Breadcrumb -->
        <div class="flex items-center gap-2 text-sm text-slate-500">
          <RouterLink to="/admin" class="hover:text-[var(--color-brand-red)]"
            >Trang chủ</RouterLink
          >
          <span v-if="currentPageTitle">›</span>
          <span v-if="currentPageTitle" class="text-slate-800 font-medium">{{
            currentPageTitle
          }}</span>
        </div>

        <!-- Right Actions -->
        <div class="flex items-center gap-4">
          <!-- Notification Bell -->
          <button
            class="relative p-2 text-slate-500 hover:text-slate-700 rounded-lg hover:bg-slate-100"
          >
            <Bell class="w-5 h-5" />
            <span
              class="absolute top-1.5 right-1.5 w-2 h-2 bg-[var(--color-danger)] rounded-full"
            ></span>
          </button>
          <!-- User -->
          <div class="flex items-center gap-2">
            <span class="text-sm font-medium text-slate-700">{{
              auth.fullName
            }}</span>
            <div
              class="w-8 h-8 bg-slate-200 rounded-full flex items-center justify-center text-xs font-semibold text-slate-600"
            >
              {{ initials }}
            </div>
          </div>
        </div>
      </header>

      <!-- Page Content -->
      <main class="flex-1 p-6">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { RouterLink, RouterView, useRoute, useRouter } from "vue-router";
import { useAuthStore } from "@/stores/auth";
import {
  LayoutDashboard,
  Inbox,
  Columns3,
  List,
  BarChart3,
  Map,
  Users,
  Tag,
  Building2,
  Bell,
  LogOut,
  Trophy,
} from "lucide-vue-next";

const auth = useAuthStore();
const route = useRoute();
const router = useRouter();
const isCollapsed = ref(false);

const initials = computed(() => {
  const name = auth.fullName || "";
  return name
    .split(" ")
    .map((w) => w[0])
    .join("")
    .slice(-2)
    .toUpperCase();
});

const currentPageTitle = computed(() => {
  const titles = {
    Dashboard: "Bảng điều khiển",
    NewTickets: "Tiếp nhận mới",
    KanbanBoard: "Bảng Kanban",
    TicketList: "Danh sách phản ánh",
    TicketDetail: "Chi tiết phản ánh",
    SlaReport: "Thống kê SLA",
    MapView: "Bản đồ phản ánh",
    UserManagement: "Quản lý người dùng",
    CategoryManagement: "Quản lý danh mục",
    DepartmentManagement: "Quản lý đơn vị",
    CitizenLeaderboard: "BXH Công dân",
  };
  return titles[route.name] || "";
});

// ── Menu theo phân quyền ──────────────────────────────────
const menuGroups = [
  {
    label: "Tổng quan",
    items: [
      {
        label: "Bảng điều khiển",
        to: "/admin",
        icon: LayoutDashboard,
        roles: ["Admin", "Dispatcher", "Assignee", "Manager"],
      },
    ],
  },
  {
    label: "Quản lý phản ánh",
    items: [
      {
        label: "Tiếp nhận mới",
        to: "/admin/tiep-nhan",
        icon: Inbox,
        roles: ["Admin", "Dispatcher"],
      },
      {
        label: "Bảng Kanban",
        to: "/admin/kanban",
        icon: Columns3,
        roles: ["Admin", "Dispatcher", "Assignee", "Manager"],
      },
      {
        label: "Danh sách phản ánh",
        to: "/admin/danh-sach",
        icon: List,
        roles: ["Admin", "Dispatcher", "Assignee", "Manager"],
      },
    ],
  },
  {
    label: "Báo cáo",
    items: [
      {
        label: "Thống kê SLA",
        to: "/admin/thong-ke-sla",
        icon: BarChart3,
        roles: ["Admin", "Manager"],
      },
      {
        label: "Bản đồ phản ánh",
        to: "/admin/ban-do",
        icon: Map,
        roles: ["Admin", "Manager"],
      },
      {
        label: "BXH Công dân",
        to: "/admin/bang-xep-hang",
        icon: Trophy,
        roles: ["Admin", "Manager"],
      },
    ],
  },
  {
    label: "Hệ thống",
    items: [
      {
        label: "Quản lý người dùng",
        to: "/admin/nguoi-dung",
        icon: Users,
        roles: ["Admin"],
      },
      {
        label: "Quản lý danh mục",
        to: "/admin/danh-muc",
        icon: Tag,
        roles: ["Admin", "Dispatcher"],
      },
      {
        label: "Quản lý đơn vị",
        to: "/admin/phong-ban",
        icon: Building2,
        roles: ["Admin"],
      },
    ],
  },
];

const visibleMenuGroups = computed(() => {
  return menuGroups
    .map((group) => ({
      ...group,
      items: group.items.filter((item) => item.roles.includes(auth.userRole)),
    }))
    .filter((group) => group.items.length > 0);
});

function isActive(path) {
  if (path === "/admin") return route.path === "/admin";
  return route.path.startsWith(path);
}

async function handleLogout() {
  await auth.logout();
  router.push({ name: "Login" });
}
</script>
