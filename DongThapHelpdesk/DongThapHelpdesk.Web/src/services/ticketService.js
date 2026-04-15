// services/ticketService.js
import api from "./api";

export const ticketService = {
  // ── Tạo phản ánh (multipart/form-data vì có file upload) ──
  create: (formData) =>
    api.post("/tickets", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    }),

  // ── Lấy danh sách & chi tiết ──
  getAll: () => api.get("/tickets"),
  getById: (id) => api.get(`/tickets/${id}`),
  getByCode: (code) => api.get(`/tickets/code/${code}`),
  getAssigned: () => api.get("/tickets/assigned"), // Ticket của đơn vị mình
  getActivities: (id) => api.get(`/tickets/${id}/activities`),

  // ── Workflow actions — khớp với TicketController ──
  approve: (id, data) => api.put(`/tickets/${id}/approve`, data),
  // data = { priority, departmentId, assigneeId?, note? }

  reject: (id, data) => api.put(`/tickets/${id}/reject`, data),
  // data = { reason }

  assign: (id, data) => api.put(`/tickets/${id}/assign`, data),
  // data = { departmentId, assigneeId?, note? }

  startProgress: (id) => api.put(`/tickets/${id}/inprogress`),

  submitResult: (id, formData) =>
    api.put(`/tickets/${id}/result`, formData, {
      headers: { "Content-Type": "multipart/form-data" }, // Có ảnh minh chứng
    }),

  reassign: (id, data) => api.put(`/tickets/${id}/reassign`, data),
  // data = { reason }

  close: (id) => api.put(`/tickets/${id}/close`),

  // ── Đánh giá ──
  rate: (id, data) => api.post(`/tickets/${id}/rating`, data),
  // data = { stars, comment? }
};
