<template>
  <div class="max-w-3xl mx-auto px-4 py-8">
    <!-- Page header -->
    <div class="mb-8">
      <RouterLink
        to="/"
        class="flex items-center gap-1.5 text-slate-500 hover:text-slate-700 mb-3 transition-colors text-[14px]"
      >
        <ArrowLeft :size="16" /> Về trang chủ
      </RouterLink>
      <h1 class="text-slate-800 text-[24px] sm:text-[28px] font-bold mb-1">
        Gửi Phản Ánh, Kiến Nghị
      </h1>
      <p class="text-slate-500 text-[14px]">
        Vui lòng điền đầy đủ thông tin để chúng tôi xử lý nhanh nhất
      </p>
    </div>

    <!-- Success state -->
    <div
      v-if="submitted"
      class="bg-white rounded-2xl shadow-sm border border-slate-100 p-8 sm:p-12 text-center"
    >
      <div
        class="w-16 h-16 rounded-full bg-emerald-100 flex items-center justify-center mx-auto mb-5"
      >
        <CheckCircle2 :size="32" class="text-emerald-600" />
      </div>
      <h2 class="text-slate-800 text-[22px] font-bold mb-2">
        Gửi phản ánh thành công!
      </h2>
      <p class="text-slate-500 text-[15px] mb-2">Mã phản ánh của bạn:</p>
      <p class="text-[#DA251D] font-mono text-[28px] font-bold mb-4">
        {{ ticketCode }}
      </p>
      <p class="text-slate-500 text-[14px] mb-6 max-w-md mx-auto leading-[1.6]">
        Bạn có thể dùng mã này để tra cứu tiến trình xử lý. Chúng tôi sẽ liên hệ
        qua số điện thoại bạn đã cung cấp.
      </p>
      <div class="flex flex-col sm:flex-row items-center justify-center gap-3">
        <RouterLink
          to="/tra-cuu"
          class="flex items-center gap-2 px-6 py-3 bg-[#DA251D] text-white rounded-xl font-medium hover:bg-[#b81f18] transition-colors cursor-pointer text-[14px]"
        >
          <Search :size="16" /> Tra cứu kết quả
        </RouterLink>
        <button
          @click="resetForm"
          class="flex items-center gap-2 px-6 py-3 border border-slate-200 text-slate-600 rounded-xl font-medium hover:bg-slate-50 transition-colors cursor-pointer text-[14px]"
        >
          <PenSquare :size="16" /> Gửi phản ánh khác
        </button>
      </div>
    </div>

    <!-- Form -->
    <form v-else @submit.prevent="handleSubmit" class="space-y-6">
      <!-- Section 1: Thông tin phản ánh -->
      <div class="bg-white rounded-2xl shadow-sm border border-slate-100 p-6">
        <div class="flex items-center gap-2.5 mb-5">
          <div
            class="w-8 h-8 rounded-lg bg-[#DA251D]/10 flex items-center justify-center"
          >
            <FileText :size="16" class="text-[#DA251D]" />
          </div>
          <h2 class="text-slate-800 text-[16px] font-semibold">
            Thông tin phản ánh
          </h2>
        </div>

        <div class="space-y-4">
          <div>
            <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
              >Tiêu đề phản ánh <span class="text-red-500">*</span></label
            >
            <input
              v-model="form.title"
              placeholder="Ví dụ: Ổ gà lớn trên đường Nguyễn Huệ"
              :class="[inputCls, errors.title ? errCls : normCls]"
              class="text-[14px]"
            />
            <p v-if="errors.title" class="text-red-500 mt-1 text-[12px]">
              {{ errors.title }}
            </p>
          </div>

          <div>
            <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
              >Lĩnh vực <span class="text-red-500">*</span></label
            >
            <select
              v-model="form.categoryId"
              :class="[
                inputCls,
                errors.categoryId ? errCls : normCls,
                'appearance-none cursor-pointer',
              ]"
              class="text-[14px]"
            >
              <option value="">— Chọn lĩnh vực —</option>
              <option v-for="c in categories" :key="c.id" :value="c.id">
                {{ c.name }}
              </option>
            </select>
            <p v-if="errors.categoryId" class="text-red-500 mt-1 text-[12px]">
              {{ errors.categoryId }}
            </p>
          </div>

          <div>
            <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
              >Mô tả chi tiết <span class="text-red-500">*</span></label
            >
            <textarea
              v-model="form.description"
              placeholder="Mô tả chi tiết tình trạng, mức độ nghiêm trọng, thời gian xảy ra..."
              :class="[
                inputCls,
                errors.description ? errCls : normCls,
                'resize-none',
              ]"
              class="text-[14px] leading-[1.6]"
              rows="4"
            />
            <p v-if="errors.description" class="text-red-500 mt-1 text-[12px]">
              {{ errors.description }}
            </p>
          </div>

          <!-- File upload -->
          <div>
            <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
              >Hình ảnh minh chứng</label
            >
            <div
              @click="$refs.fileInput.click()"
              @dragover.prevent="dragOver = true"
              @dragleave="dragOver = false"
              @drop.prevent="handleDrop"
              :class="[
                'border-2 border-dashed rounded-xl p-6 text-center cursor-pointer transition-all',
                dragOver
                  ? 'border-[#DA251D] bg-red-50/50'
                  : 'border-slate-200 hover:border-slate-300 bg-slate-50/50',
              ]"
            >
              <input
                ref="fileInput"
                type="file"
                multiple
                accept="image/*"
                class="hidden"
                @change="handleFileSelect"
              />
              <Upload :size="24" class="text-slate-400 mx-auto mb-2" />
              <p class="text-slate-600 text-[14px] font-medium">
                Nhấn để chọn hoặc kéo thả hình ảnh
              </p>
              <p class="text-slate-400 text-[12px] mt-1">
                PNG, JPG, JPEG — Tối đa 5 ảnh, mỗi ảnh 5MB
              </p>
            </div>
            <!-- Preview -->
            <div
              v-if="filePreviews.length > 0"
              class="grid grid-cols-3 sm:grid-cols-5 gap-3 mt-3"
            >
              <div
                v-for="(preview, i) in filePreviews"
                :key="i"
                class="relative aspect-square rounded-lg overflow-hidden border border-slate-200 group"
              >
                <img :src="preview" class="w-full h-full object-cover" />
                <button
                  @click="removeFile(i)"
                  type="button"
                  class="absolute top-1 right-1 w-6 h-6 rounded-full bg-red-500 text-white flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer"
                >
                  <X :size="12" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Section 2: Vị trí sự việc -->
      <div class="bg-white rounded-2xl shadow-sm border border-slate-100 p-6">
        <div class="flex items-center gap-2.5 mb-5">
          <div
            class="w-8 h-8 rounded-lg bg-blue-50 flex items-center justify-center"
          >
            <MapPin :size="16" class="text-blue-600" />
          </div>
          <h2 class="text-slate-800 text-[16px] font-semibold">
            Vị trí sự việc
          </h2>
        </div>

        <div class="space-y-4">
          <div
            ref="mapRef"
            class="rounded-xl overflow-hidden border border-slate-200 z-0"
            style="height: 250px"
          />
          <p
            v-if="form.latitude"
            class="text-slate-500 text-[12px] mt-1.5 flex items-center gap-1"
          >
            <MapPin :size="12" class="text-[#DA251D]" />
            {{ form.latitude.toFixed(4) }}°N, {{ form.longitude.toFixed(4) }}°E
            <span class="text-slate-300 mx-1">·</span>
            <span class="text-slate-400"
              >Nhấn vào bản đồ để thay đổi vị trí</span
            >
          </p>
          <button
            type="button"
            @click="getLocation"
            :disabled="gettingLocation"
            class="flex items-center gap-2 px-4 py-2.5 rounded-lg border border-blue-200 bg-blue-50 text-blue-700 hover:bg-blue-100 transition-colors cursor-pointer text-[13px] font-medium disabled:opacity-50"
          >
            <Loader2 v-if="gettingLocation" :size="15" class="animate-spin" />
            <Navigation v-else :size="15" />
            {{ gettingLocation ? "Đang lấy vị trí..." : "Lấy vị trí hiện tại" }}
          </button>

          <!-- ĐỊA CHỈ CỤ THỂ — có dropdown gợi ý forward geocode -->
          <div id="address-input-wrapper">
            <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
              >Địa chỉ cụ thể</label
            >
            <div class="relative">
              <input
                v-model="form.address"
                @input="onAddressInput"
                placeholder="Số nhà, tên đường, khu vực..."
                :class="[inputCls, normCls]"
                class="text-[14px]"
              />
              <!-- Loading khi đang geocode -->
              <Loader2
                v-if="geocoding"
                :size="16"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 animate-spin"
              />
              <!-- Dropdown gợi ý địa chỉ -->
              <div
                v-if="addressSuggestions.length > 0"
                class="absolute z-50 left-0 right-0 mt-1 bg-white border border-slate-200 rounded-xl shadow-lg overflow-hidden"
              >
                <ul class="max-h-[200px] overflow-y-auto">
                  <li
                    v-for="(s, i) in addressSuggestions"
                    :key="i"
                    @click="selectAddress(s)"
                    class="px-4 py-2.5 text-[13px] text-slate-700 hover:bg-slate-50 cursor-pointer transition-colors"
                  >
                    {{ s.formatted }}
                  </li>
                </ul>
              </div>
            </div>
          </div>

          <div>
            <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
              >Xã/Phường</label
            >
            <input
              v-model="form.ward"
              placeholder="Tự động điền khi chọn vị trí hoặc nhập tay..."
              :class="[inputCls, normCls]"
              class="text-[14px]"
            />
          </div>
        </div>
      </div>

      <!-- Section 3: Thông tin người phản ánh -->
      <div class="bg-white rounded-2xl shadow-sm border border-slate-100 p-6">
        <div class="flex items-center gap-2.5 mb-5">
          <div
            class="w-8 h-8 rounded-lg bg-emerald-50 flex items-center justify-center"
          >
            <User :size="16" class="text-emerald-600" />
          </div>
          <h2 class="text-slate-800 text-[16px] font-semibold">
            Thông tin người phản ánh
          </h2>
        </div>

        <div class="space-y-4">
          <div>
            <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
              >Họ và tên <span class="text-red-500">*</span></label
            >
            <div class="relative">
              <User
                :size="17"
                class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400"
              />
              <input
                v-model="form.reporterName"
                placeholder="Nhập họ và tên"
                :class="[
                  inputCls,
                  errors.reporterName ? errCls : normCls,
                  'pl-11',
                ]"
                class="text-[14px]"
              />
            </div>
            <p v-if="errors.reporterName" class="text-red-500 mt-1 text-[12px]">
              {{ errors.reporterName }}
            </p>
          </div>

          <div>
            <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
              >Số điện thoại <span class="text-red-500">*</span></label
            >
            <div class="relative">
              <Phone
                :size="17"
                class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400"
              />
              <input
                v-model="form.reporterPhone"
                type="tel"
                placeholder="0901234567"
                :class="[
                  inputCls,
                  errors.reporterPhone ? errCls : normCls,
                  'pl-11',
                ]"
                class="text-[14px]"
              />
            </div>
            <p
              v-if="errors.reporterPhone"
              class="text-red-500 mt-1 text-[12px]"
            >
              {{ errors.reporterPhone }}
            </p>
          </div>

          <div>
            <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
              >Địa chỉ liên hệ</label
            >
            <input
              v-model="form.reporterAddress"
              placeholder="Địa chỉ nhà (không bắt buộc)"
              :class="[inputCls, normCls]"
              class="text-[14px]"
            />
          </div>

          <!-- Terms -->
          <label class="flex items-start gap-3 cursor-pointer select-none pt-2">
            <div class="relative mt-0.5">
              <input
                type="checkbox"
                v-model="form.agreeTerms"
                class="sr-only peer"
              />
              <div
                class="w-[18px] h-[18px] rounded-md border-2 border-slate-300 peer-checked:border-[#DA251D] peer-checked:bg-[#DA251D] transition-all flex items-center justify-center"
              >
                <svg
                  v-if="form.agreeTerms"
                  width="11"
                  height="11"
                  viewBox="0 0 12 12"
                  fill="none"
                >
                  <path
                    d="M2.5 6L5 8.5L9.5 3.5"
                    stroke="white"
                    stroke-width="2"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                  />
                </svg>
              </div>
            </div>
            <span class="text-slate-600 text-[13px] leading-[1.6]">
              Tôi đồng ý với
              <a href="#" class="text-[#DA251D] hover:underline"
                >điều khoản sử dụng</a
              >
              và
              <a href="#" class="text-[#DA251D] hover:underline"
                >chính sách bảo mật</a
              >
              của hệ thống
            </span>
          </label>
          <p v-if="errors.agreeTerms" class="text-red-500 text-[12px]">
            {{ errors.agreeTerms }}
          </p>
        </div>
      </div>

      <!-- Actions -->
      <div class="flex flex-col sm:flex-row items-center justify-between gap-3">
        <RouterLink
          to="/"
          class="text-slate-500 hover:text-slate-700 transition-colors text-[14px]"
          >Hủy bỏ</RouterLink
        >
        <button
          type="submit"
          :disabled="submitting"
          class="w-full sm:w-auto flex items-center justify-center gap-2.5 px-8 py-3.5 bg-[#DA251D] text-white rounded-xl font-semibold hover:bg-[#b81f18] disabled:opacity-50 disabled:cursor-not-allowed transition-all cursor-pointer shadow-sm shadow-[#DA251D]/20 text-[15px]"
        >
          <Loader2 v-if="submitting" :size="18" class="animate-spin" />
          <Send v-else :size="18" />
          {{ submitting ? "Đang gửi..." : "Gửi Phản Ánh" }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted, nextTick } from "vue";
import api from "@/services/api";
import { RouterLink } from "vue-router";
import trackasiagl from "trackasia-gl";
import "trackasia-gl/dist/trackasia-gl.css";
import {
  ArrowLeft,
  FileText,
  MapPin,
  User,
  Phone,
  Upload,
  X,
  Send,
  Search,
  PenSquare,
  CheckCircle2,
  Loader2,
  Navigation,
} from "lucide-vue-next";

// ══════════════════════════════════════════════════════════
// CẤU HÌNH
// ══════════════════════════════════════════════════════════
const TRACKASIA_KEY = import.meta.env.VITE_TRACKASIA_KEY;
const MAP_STYLE = `https://maps.track-asia.com/styles/v2/streets.json?key=${TRACKASIA_KEY}`;
const DEFAULT_CENTER = [105.6324, 10.4538]; // [lng, lat] TP. Cao Lãnh
const DEFAULT_ZOOM = 13;

const inputCls =
  "w-full py-3 px-4 rounded-xl border bg-white outline-none transition-all";
const normCls =
  "border-slate-200 hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10";
const errCls = "border-red-300 ring-2 ring-red-100";

// ══════════════════════════════════════════════════════════
// STATE
// ══════════════════════════════════════════════════════════
const form = reactive({
  title: "",
  categoryId: "",
  description: "",
  address: "",
  ward: "",
  latitude: null,
  longitude: null,
  reporterName: "",
  reporterPhone: "",
  reporterAddress: "",
  agreeTerms: false,
});

const categories = ref([]);
const files = ref([]);
const filePreviews = ref([]);
const dragOver = ref(false);
const errors = reactive({});
const submitting = ref(false);
const submitted = ref(false);
const ticketCode = ref("");
const gettingLocation = ref(false);

// Forward geocode (nhập địa chỉ tay)
const geocoding = ref(false);
const addressSuggestions = ref([]);
let addressDebounce = null;

// Map
const mapRef = ref(null);
let map = null;
let marker = null;

// ══════════════════════════════════════════════════════════
// FORWARD GEOCODE — nhập địa chỉ tay → gợi ý → chọn → map bay tới
// ══════════════════════════════════════════════════════════
function onAddressInput() {
  clearTimeout(addressDebounce);
  addressSuggestions.value = [];

  const query = form.address.trim();
  if (query.length < 5) return;

  // Debounce 600ms — chờ người dùng ngừng gõ
  addressDebounce = setTimeout(() => forwardGeocode(query), 600);
}

async function forwardGeocode(query) {
  geocoding.value = true;
  try {
    // Thêm "Đồng Tháp" để kết quả chính xác hơn
    const searchQuery = query.toLowerCase().includes("đồng tháp")
      ? query
      : `${query}, Đồng Tháp`;

    const res = await fetch(
      `https://maps.track-asia.com/api/v2/geocode/json?address=${encodeURIComponent(searchQuery)}&key=${TRACKASIA_KEY}&new_admin=true&size=5`,
    );
    const data = await res.json();

    if (data.status === "OK" && data.results?.length > 0) {
      // Lọc trùng — chỉ giữ 1 kết quả cho mỗi formatted_address
      const seen = new Set();
      addressSuggestions.value = data.results
        .filter((r) => {
          if (seen.has(r.formatted_address)) return false;
          seen.add(r.formatted_address);
          return true;
        })
        .map((r) => ({
          formatted: r.formatted_address,
          lat: r.geometry.location.lat,
          lng: r.geometry.location.lng,
          components: r.address_components || [],
        }));
    } else {
      // Fallback Nominatim
      const nomRes = await fetch(
        `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(searchQuery)}&format=json&accept-language=vi&addressdetails=1&limit=5&countrycodes=vn`,
      );
      const nomData = await nomRes.json();
      addressSuggestions.value = nomData.map((r) => ({
        formatted: r.display_name,
        lat: parseFloat(r.lat),
        lng: parseFloat(r.lon),
        nominatimAddr: r.address || {},
      }));
    }
  } catch {
    addressSuggestions.value = [];
  } finally {
    geocoding.value = false;
  }
}

// Người dùng chọn 1 gợi ý từ dropdown
function selectAddress(suggestion) {
  form.address = suggestion.formatted;
  addressSuggestions.value = [];

  // Di chuyển map + đặt marker
  setMarker(suggestion.lat, suggestion.lng);

  // Autofill Xã/Phường
  let wardRaw = "";

  if (suggestion.components?.length > 0) {
    // TrackAsia V2: xã/phường nằm ở level_2
    wardRaw = findComp(suggestion.components, [
      "administrative_area_level_2",
      "ward",
      "sublocality_level_1",
      "sublocality",
      "neighborhood",
    ]);
  } else if (suggestion.nominatimAddr) {
    const addr = suggestion.nominatimAddr;
    wardRaw = addr.quarter || addr.suburb || addr.village || addr.town || "";
  }

  if (wardRaw) {
    matchWard(wardRaw);
  } else {
    matchWardFromAddress(suggestion.formatted);
  }
}

// ══════════════════════════════════════════════════════════
// CLICK NGOÀI → ĐÓNG DROPDOWNS
// ══════════════════════════════════════════════════════════
function handleClickOutside(e) {
  // Đóng dropdown gợi ý địa chỉ khi click ngoài
  const addrEl = document.getElementById("address-input-wrapper");
  if (addrEl && !addrEl.contains(e.target)) {
    addressSuggestions.value = [];
  }
}

// ══════════════════════════════════════════════════════════
// MAP INIT
// ══════════════════════════════════════════════════════════
onMounted(async () => {
  document.addEventListener("click", handleClickOutside);

  // Load danh mục
  try {
    const { data } = await api.get("/categories");
    categories.value = data;
  } catch (e) {
    console.error("Lỗi tải danh mục:", e);
  }

  // Init bản đồ TrackAsia
  await nextTick();
  if (!mapRef.value) return;

  map = new trackasiagl.Map({
    container: mapRef.value,
    style: MAP_STYLE,
    center: DEFAULT_CENTER,
    zoom: DEFAULT_ZOOM,
  });

  map.addControl(new trackasiagl.NavigationControl(), "top-right");

  // Click trên bản đồ → chọn vị trí
  map.on("click", (e) => {
    setMarker(e.lngLat.lat, e.lngLat.lng);
    reverseGeocode(e.lngLat.lat, e.lngLat.lng);
  });
});

onUnmounted(() => {
  document.removeEventListener("click", handleClickOutside);
});

// ══════════════════════════════════════════════════════════
// MAP MARKER
// ══════════════════════════════════════════════════════════
function setMarker(lat, lng) {
  form.latitude = lat;
  form.longitude = lng;

  if (marker) {
    marker.setLngLat([lng, lat]);
  } else {
    const el = document.createElement("div");
    el.style.cssText =
      "width:32px;height:32px;background:#DA251D;border:3px solid white;border-radius:50%;box-shadow:0 2px 8px rgba(0,0,0,0.3);cursor:pointer;";

    marker = new trackasiagl.Marker({ element: el, draggable: true })
      .setLngLat([lng, lat])
      .addTo(map);

    marker.on("dragend", () => {
      const lngLat = marker.getLngLat();
      form.latitude = lngLat.lat;
      form.longitude = lngLat.lng;
      reverseGeocode(lngLat.lat, lngLat.lng);
    });
  }

  map.flyTo({ center: [lng, lat], zoom: 16, duration: 800 });
}

// ══════════════════════════════════════════════════════════
// REVERSE GEOCODE — tọa độ → địa chỉ + autofill ward
// TrackAsia V2 sau sáp nhập:
//   level_1 = Tỉnh
//   level_2 = Xã/Phường (KHÔNG có cấp huyện riêng)
// ══════════════════════════════════════════════════════════
async function reverseGeocode(lat, lng) {
  try {
    const res = await fetch(
      `https://maps.track-asia.com/api/v2/geocode/json?latlng=${lat},${lng}&key=${TRACKASIA_KEY}&result_type=street_address&new_admin=true&size=1`,
    );
    const data = await res.json();

    if (data.status === "OK" && data.results?.length > 0) {
      const result = data.results[0];
      const components = result.address_components || [];

      // Trích xuất từng phần — đúng type mapping của TrackAsia V2
      const streetNumber = findComp(components, ["street_number"]);
      const route = findComp(components, ["route"]);
      const ward = findComp(components, [
        "administrative_area_level_2", // ★ TrackAsia V2 dùng level_2 cho xã/phường
        "ward",
        "sublocality_level_1",
        "sublocality",
        "neighborhood",
      ]);
      const province = findComp(components, ["administrative_area_level_1"]);

      // Ghép địa chỉ: "số đường, tên đường, xã/phường, tỉnh, Việt Nam"
      const streetPart =
        streetNumber && route
          ? `${streetNumber} ${route}`
          : route || streetNumber || "";

      const parts = [streetPart, ward, province, "Việt Nam"].filter(Boolean);

      // Dùng formatted_address gốc nếu chỉ ghép được quá ít phần
      form.address =
        parts.length >= 2 ? parts.join(", ") : result.formatted_address || "";

      // Autofill Xã/Phường
      if (ward) {
        matchWard(ward);
      } else {
        matchWardFromAddress(result.formatted_address || "");
      }
    } else {
      await fallbackNominatim(lat, lng);
    }
  } catch (err) {
    console.warn("TrackAsia lỗi, fallback Nominatim:", err);
    await fallbackNominatim(lat, lng);
  }
}

// ══════════════════════════════════════════════════════════
// HELPERS — dùng chung cho cả reverse + forward geocode
// ══════════════════════════════════════════════════════════

// Tìm component đầu tiên match 1 trong danh sách types
function findComp(components, typesList) {
  for (const type of typesList) {
    const found = components.find((c) => c.types?.includes(type));
    if (found) return found.long_name || found.short_name || "";
  }
  return "";
}

// Match wardRaw (từ API) vào wardList → fill form.ward
function matchWard(wardRaw) {
  if (wardRaw) {
    form.ward = wardRaw;
  }
}

// Fallback: tách xã/phường từ chuỗi formatted_address
// VD: "Đường ABC, Phường 2, Tỉnh Đồng Tháp, Việt Nam"
function matchWardFromAddress(address) {
  const match = address.match(/(Phường|Xã)\s+[^,]+/i);
  if (match) {
    form.ward = match[0].trim();
  }
}

// Fallback Nominatim khi TrackAsia không có kết quả
async function fallbackNominatim(lat, lng) {
  try {
    const res = await fetch(
      `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json&accept-language=vi&addressdetails=1&zoom=18`,
    );
    const data = await res.json();
    const addr = data.address || {};

    const parts = [
      addr.house_number && addr.road
        ? `${addr.house_number} ${addr.road}`
        : addr.road || "",
      addr.quarter || addr.suburb || addr.village || addr.town || "",
      addr.city_district || addr.county || "",
      addr.state || addr.city || "",
      "Việt Nam",
    ].filter(Boolean);

    form.address = parts.join(", ");

    const wardRaw =
      addr.quarter || addr.suburb || addr.village || addr.town || "";
    if (wardRaw) matchWard(wardRaw);
  } catch {
    console.warn("Reverse geocode thất bại");
  }
}

// ══════════════════════════════════════════════════════════
// GPS
// ══════════════════════════════════════════════════════════
function getLocation() {
  if (!navigator.geolocation) return;
  gettingLocation.value = true;
  navigator.geolocation.getCurrentPosition(
    (pos) => {
      const { latitude, longitude } = pos.coords;
      setMarker(latitude, longitude);
      reverseGeocode(latitude, longitude);
      gettingLocation.value = false;
    },
    (err) => {
      gettingLocation.value = false;
      // Chỉ báo lỗi khi thật sự bị từ chối (PERMISSION_DENIED)
      // Không báo khi timeout hoặc position unavailable — trình duyệt sẽ tự retry
      if (err.code === err.PERMISSION_DENIED) {
        alert(
          "Bạn đã từ chối quyền truy cập vị trí. Vui lòng bật GPS trong cài đặt trình duyệt.",
        );
      }
      // TIMEOUT hoặc POSITION_UNAVAILABLE → im lặng, không alert
    },
    {
      enableHighAccuracy: true,
      timeout: 15000, // Chờ tối đa 15 giây (mặc định là rất ngắn)
      maximumAge: 60000, // Chấp nhận vị trí cache trong 60 giây
    },
  );
}

// ══════════════════════════════════════════════════════════
// FILE UPLOAD
// ══════════════════════════════════════════════════════════
function handleFileSelect(e) {
  addFiles(Array.from(e.target.files));
}

function handleDrop(e) {
  dragOver.value = false;
  addFiles(Array.from(e.dataTransfer.files));
}

function addFiles(newFiles) {
  const imageFiles = newFiles.filter((f) => f.type.startsWith("image/"));
  const remaining = 5 - files.value.length;
  const toAdd = imageFiles.slice(0, remaining);
  toAdd.forEach((file) => {
    files.value.push(file);
    const reader = new FileReader();
    reader.onload = (e) => filePreviews.value.push(e.target.result);
    reader.readAsDataURL(file);
  });
}

function removeFile(index) {
  files.value.splice(index, 1);
  filePreviews.value.splice(index, 1);
}

// ══════════════════════════════════════════════════════════
// VALIDATION + SUBMIT
// ══════════════════════════════════════════════════════════
function validate() {
  Object.keys(errors).forEach((k) => delete errors[k]);
  if (!form.title.trim()) errors.title = "Vui lòng nhập tiêu đề";
  if (!form.categoryId) errors.categoryId = "Vui lòng chọn lĩnh vực";
  if (!form.description.trim()) errors.description = "Vui lòng nhập mô tả";
  if (!form.reporterName.trim()) errors.reporterName = "Vui lòng nhập họ tên";
  if (!form.reporterPhone.trim())
    errors.reporterPhone = "Vui lòng nhập số điện thoại";
  else if (!/^0\d{9}$/.test(form.reporterPhone))
    errors.reporterPhone = "SĐT không hợp lệ (10 chữ số)";
  if (!form.agreeTerms)
    errors.agreeTerms = "Bạn cần đồng ý với điều khoản sử dụng";
  return Object.keys(errors).length === 0;
}

async function handleSubmit() {
  if (!validate()) return;
  submitting.value = true;

  try {
    const fd = new FormData();
    fd.append("Title", form.title);
    fd.append("Description", form.description);
    fd.append("CategoryId", form.categoryId);
    fd.append("ReporterName", form.reporterName);
    fd.append("ReporterPhone", form.reporterPhone);

    if (form.address) fd.append("Address", form.address);
    if (form.reporterAddress)
      fd.append("ReporterAddress", form.reporterAddress);
    if (form.district) fd.append("ReporterDistrict", form.district);
    if (form.ward) fd.append("ReporterWard", form.ward);
    if (form.latitude) fd.append("Latitude", form.latitude);
    if (form.longitude) fd.append("Longitude", form.longitude);

    files.value.forEach((file) => {
      fd.append("Files", file);
    });

    const { data } = await api.post("/tickets", fd, {
      headers: { "Content-Type": "multipart/form-data" },
    });

    ticketCode.value = data.ticketCode;
    submitted.value = true;
    window.scrollTo({ top: 0, behavior: "smooth" });
  } catch (err) {
    const msg =
      err.response?.data?.message || "Gửi phản ánh thất bại, vui lòng thử lại";
    alert(msg);
  } finally {
    submitting.value = false;
  }
}

function resetForm() {
  Object.assign(form, {
    title: "",
    categoryId: "",
    description: "",
    address: "",
    district: "",
    ward: "",
    latitude: null,
    longitude: null,
    reporterName: "",
    reporterPhone: "",
    reporterAddress: "",
    agreeTerms: false,
  });
  files.value = [];
  filePreviews.value = [];
  submitted.value = false;
  ticketCode.value = "";
}
</script>
