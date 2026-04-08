import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "@/stores/auth";

// ── Layouts ───────────────────────────────────────────────
const AdminLayout = () => import("@/layouts/AdminLayout.vue");
const CitizenLayout = () => import("@/layouts/CitizenLayout.vue");

// ── Auth Pages ────────────────────────────────────────────
const LoginPage = () => import("@/views/auth/LoginPage.vue");

// ── Citizen Pages ─────────────────────────────────────────
const HomePage = () => import("@/views/citizen/HomePage.vue");
const SubmitTicket = () => import("@/views/citizen/SubmitTicket.vue");
const TrackTicket = () => import("@/views/citizen/TrackTicket.vue");
const MyProfile = () => import("@/views/citizen/MyProfile.vue");

// ── Admin Pages ───────────────────────────────────────────
const Dashboard = () => import("@/views/admin/Dashboard.vue");
const KanbanBoard = () => import("@/views/admin/KanbanBoard.vue");
const TicketList = () => import("@/views/admin/TicketList.vue");
const TicketDetail = () => import("@/views/admin/TicketDetail.vue");
const NewTickets = () => import("@/views/admin/NewTickets.vue");
const UserManagement = () => import("@/views/admin/UserManagement.vue");
const CategoryManagement = () => import("@/views/admin/CategoryManagement.vue");
const DepartmentManagement = () =>
  import("@/views/admin/DepartmentManagement.vue");
const CitizenLeaderboardView = () =>
  import("@/views/admin/CitizenLeaderboard.vue");
const SlaReport = () => import("@/views/admin/SlaReport.vue");
const MapView = () => import("@/views/admin/MapView.vue");

// ── Error Pages ───────────────────────────────────────────
const Forbidden = () => import("@/views/errors/Forbidden.vue");
const NotFound = () => import("@/views/errors/NotFound.vue");

const routes = [
  // ══ Auth ════════════════════════════════════════════════
  {
    path: "/login",
    name: "Login",
    component: LoginPage,
    meta: { guest: true },
  },

  // ══ Citizen Portal ═════════════════════════════════════
  {
    path: "/",
    component: CitizenLayout,
    children: [
      {
        path: "",
        name: "Home",
        component: HomePage,
      },
      {
        path: "gui-phan-anh",
        name: "SubmitTicket",
        component: SubmitTicket,
      },
      {
        path: "tra-cuu",
        name: "TrackTicket",
        component: TrackTicket,
      },
      {
        path: "tai-khoan",
        name: "MyProfile",
        component: MyProfile,
        meta: { requiresAuth: true, roles: ["Citizen"] },
      },
    ],
  },

  // ══ Admin Dashboard ════════════════════════════════════
  {
    path: "/admin",
    component: AdminLayout,
    meta: {
      requiresAuth: true,
      roles: ["Admin", "Dispatcher", "Assignee", "Manager"],
    },
    children: [
      {
        path: "",
        name: "Dashboard",
        component: Dashboard,
      },
      {
        path: "tiep-nhan",
        name: "NewTickets",
        component: NewTickets,
        meta: { roles: ["Admin", "Dispatcher"] },
      },
      {
        path: "kanban",
        name: "KanbanBoard",
        component: KanbanBoard,
      },
      {
        path: "danh-sach",
        name: "TicketList",
        component: TicketList,
      },
      {
        path: "phan-anh/:id",
        name: "TicketDetail",
        component: TicketDetail,
        props: true,
      },
      {
        path: "thong-ke-sla",
        name: "SlaReport",
        component: SlaReport,
        meta: { roles: ["Admin", "Manager"] },
      },
      {
        path: "ban-do",
        name: "MapView",
        component: MapView,
        meta: { roles: ["Admin", "Manager"] },
      },
      {
        path: "nguoi-dung",
        name: "UserManagement",
        component: UserManagement,
        meta: { roles: ["Admin"] },
      },
      {
        path: "danh-muc",
        name: "CategoryManagement",
        component: CategoryManagement,
        meta: { roles: ["Admin", "Dispatcher"] },
      },
      {
        path: "phong-ban",
        name: "DepartmentManagement",
        component: DepartmentManagement,
        meta: { roles: ["Admin"] },
      },
      {
        path: "bang-xep-hang",
        name: "CitizenLeaderboardView",
        component: CitizenLeaderboardView,
        meta: { roles: ["Admin", "Manager"] },
      },
    ],
  },

  // ══ Error Pages ════════════════════════════════════════
  { path: "/403", name: "Forbidden", component: Forbidden },
  { path: "/:pathMatch(.*)*", name: "NotFound", component: NotFound },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() {
    return { top: 0 };
  },
});

// // ── Navigation Guards ─────────────────────────────────────
// router.beforeEach(async (to, from, next) => {
//   const auth = useAuthStore();

//   // Nếu chưa fetch user lần nào thì thử fetch
//   if (!auth.user) {
//     await auth.fetchMe();
//   }

//   // Trang chỉ dành cho guest (login) — nếu đã đăng nhập thì redirect
//   if (to.meta.guest && auth.isLoggedIn) {
//     return next(auth.isStaff ? { name: "Dashboard" } : { name: "Home" });
//   }

//   // Trang yêu cầu đăng nhập
//   if (to.meta.requiresAuth && !auth.isLoggedIn) {
//     return next({ name: "Login", query: { redirect: to.fullPath } });
//   }

//   // Kiểm tra role
//   const allowedRoles =
//     to.meta.roles || to.matched.find((r) => r.meta.roles)?.meta.roles;
//   if (allowedRoles && !allowedRoles.includes(auth.userRole)) {
//     return next({ name: "Forbidden" });
//   }

//   next();
// });

export default router;
