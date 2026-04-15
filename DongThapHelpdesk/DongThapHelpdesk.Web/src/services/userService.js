// services/userService.js
import api from "./api";

export const userService = {
  getAll: () => api.get("/users"),
  getStaff: () => api.get("/users/staff"),
  create: (data) => api.post("/users", data),
  update: (id, data) => api.put(`/users/${id}`, data),
  toggleLock: (id) => api.put(`/users/${id}/lock`), // Khóa/mở khóa tài khoản
  changePassword: (id, data) => api.put(`/users/${id}/password`, data),
  updateProfile: (data) => api.put("/users/profile", data), // Citizen tự cập nhật
};
