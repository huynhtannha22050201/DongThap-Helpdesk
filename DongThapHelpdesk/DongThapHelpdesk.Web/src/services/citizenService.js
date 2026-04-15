// services/citizenService.js
import api from "./api";

export const citizenService = {
  getMyTickets: () => api.get("/citizen/my-tickets"),
  getMyPoints: () => api.get("/citizen/my-points"),
  getPointHistory: () => api.get("/citizen/my-points/history"),
  getPointHistoryByMonth: (year, month) =>
    api.get(`/citizen/my-points/history/${year}/${month}`),
  getMonthlyLeaderboard: () => api.get("/citizen/leaderboard/monthly"),
  getQuarterlyLeaderboard: () => api.get("/citizen/leaderboard/quarterly"),
};
