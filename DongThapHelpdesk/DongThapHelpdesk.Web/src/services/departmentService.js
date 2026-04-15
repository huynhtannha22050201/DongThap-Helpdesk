// services/departmentService.js
import api from "./api";

export const departmentService = {
  getAll: () => api.get("/departments"),
  getPaged(page = 1, pageSize = 5, search, isActive, sortBy, sortDir) {
    return api.get("/departments/paged", {
      params: { page, pageSize, search, isActive, sortBy, sortDir },
    });
  },
  create: (data) => api.post("/departments", data),
  update: (id, data) => api.put(`/departments/${id}`, data),
  delete: (id) => api.delete(`/departments/${id}`),
};
