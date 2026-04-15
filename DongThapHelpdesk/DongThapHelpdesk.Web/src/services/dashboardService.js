// services/dashboardService.js
import api from "./api";

export const dashboardService = {
  getStats: () => api.get("/dashboard/stats"),
  getHeatmap: () => api.get("/dashboard/heatmap"),
  getSla: () => api.get("/dashboard/sla"),

  // Export CSV — trả về blob để download file
  exportCsv: (params) =>
    api.get("/dashboard/export", {
      params, // { status?, departmentId?, fromDate?, toDate? }
      responseType: "blob", // Nhận file CSV dạng binary
    }),
};
