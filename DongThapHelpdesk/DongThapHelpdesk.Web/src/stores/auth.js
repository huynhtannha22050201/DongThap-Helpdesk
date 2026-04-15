import { defineStore } from "pinia";
import { ref, computed } from "vue";
import api from "@/services/api";

export const useAuthStore = defineStore("auth", () => {
  // ── State ───────────────────────────────────────────────
  const user = ref(null);
  const isLoading = ref(false);
  const _initialized = ref(false);

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
  async function login(phoneNumber, password, rememberMe = false) {
    isLoading.value = true;
    try {
      const { data } = await api.post("/auth/login", {
        phoneNumber,
        password,
        rememberMe,
      });
      if (data.success) {
        user.value = data.user;
        return { success: true };
      }
      return { success: false, message: data.message };
    } catch (error) {
      const msg = error.response?.data?.message || "Đăng nhập thất bại";
      return { success: false, message: msg };
    } finally {
      isLoading.value = false;
    }
  }

  async function initialize() {
    if (_initialized.value) return; // Đã fetch rồi → bỏ qua, không gọi API nữa
    _initialized.value = true;
    try {
      const { data } = await api.get("/auth/me");
      user.value = data.user ?? null;
    } catch {
      user.value = null; // 401 = chưa login → im lặng, KHÔNG redirect
    }
  }

  async function logout() {
    try {
      await api.post("/auth/logout");
    } finally {
      user.value = null;
      _initialized.value = false; // Reset cờ để lần đăng nhập sau fetch lại
    }
  }

  async function fetchMe() {
    try {
      const { data } = await api.get("/auth/me");
      user.value = data.user ?? null;
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
    initialize,
    login,
    logout,
    fetchMe,
  };
});
