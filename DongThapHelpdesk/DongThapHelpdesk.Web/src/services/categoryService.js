// services/categoryService.js
import api from "./api";

export const categoryService = {
  getAll: () => api.get("/categories"),
  getPaged: (page, pageSize, search, isActive, sortField, sortDir) =>
    api.get("/categories/paged", {
      params: { page, pageSize, search, isActive, sortField, sortDir },
    }),
  create: (data) => api.post("/categories", data),
  update: (id, data) => api.put(`/categories/${id}`, data),
  delete: (id) => api.delete(`/categories/${id}`),
};
