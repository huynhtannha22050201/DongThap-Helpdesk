import { createApp } from "vue";
import { createPinia } from "pinia";
import router from "./router";
import App from "./App.vue";
import "./assets/main.css";

const app = createApp(App);

import VueApexCharts from "vue3-apexcharts";

app.use(createPinia());
app.use(router);
app.use(VueApexCharts);

app.mount("#app");
