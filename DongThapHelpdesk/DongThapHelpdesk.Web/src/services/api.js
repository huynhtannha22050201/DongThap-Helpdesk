import axios from "axios";
import router from "@/router";

const api = axios.create({
  baseURL: "/api",
  withCredentials: true, // Gửi HttpOnly Cookie tự động
  headers: {
    "Content-Type": "application/json",
  },
});

// ── Response Interceptor: xử lý lỗi chung ──────────────
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;

    if (status === 401) {
      // Token hết hạn hoặc chưa đăng nhập
      router.push({ name: "Login" });
    }

    if (status === 403) {
      // Không có quyền truy cập
      router.push({ name: "Forbidden" });
    }

    return Promise.reject(error);
  },
);

export default api;
