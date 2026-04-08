import { defineStore } from "pinia";
import { ref, computed } from "vue";
import api from "@/services/api";

export const useAuthStore = defineStore("auth", () => {
  // ── State ───────────────────────────────────────────────
  // TODO: Xóa mock data này khi gắn API thật
  const user = ref({
    fullName: "Nguyễn Văn An",
    role: "Dispatcher", // Đổi thành 'Dispatcher', 'Assignee', 'Manager' để test
    phoneNumber: "0901234567",
    email: "admin@dongthap.gov.vn",
  });
  const isLoading = ref(false);

  // ── Getters ─────────────────────────────────────────────
  const isLoggedIn = computed(() => !!user.value);
  const userRole = computed(() => user.value?.role || null);
  const fullName = computed(() => user.value?.fullName || "");

  const isAdmin = computed(() => userRole.value === "Admin");
  const isDispatcher = computed(() => userRole.value === "Dispatcher");
  const isAssignee = computed(() => userRole.value === "Assignee");
  const isManager = computed(() => userRole.value === "Manager");
  const isCitizen = computed(() => userRole.value === "Citizen");

  // Kiểm tra có phải staff (không phải Citizen)
  const isStaff = computed(() =>
    ["Admin", "Dispatcher", "Assignee", "Manager"].includes(userRole.value),
  );

  // ── Actions ─────────────────────────────────────────────
  async function login(phoneNumber, password) {
    isLoading.value = true;
    try {
      const { data } = await api.post("/auth/login", {
        phoneNumber,
        password,
      });
      user.value = data.user;
      return { success: true };
    } catch (error) {
      const msg = error.response?.data?.message || "Đăng nhập thất bại";
      return { success: false, message: msg };
    } finally {
      isLoading.value = false;
    }
  }

  async function logout() {
    try {
      await api.post("/auth/logout");
    } finally {
      user.value = null;
    }
  }

  async function fetchMe() {
    try {
      const { data } = await api.get("/auth/me");
      user.value = data;
    } catch {
      user.value = null;
    }
  }

  return {
    user,
    isLoading,
    isLoggedIn,
    userRole,
    fullName,
    isAdmin,
    isDispatcher,
    isAssignee,
    isManager,
    isCitizen,
    isStaff,
    login,
    logout,
    fetchMe,
  };
});
