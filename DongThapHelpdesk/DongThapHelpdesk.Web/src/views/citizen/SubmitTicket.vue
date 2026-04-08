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
        PA-042026-013
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
          <!-- Map placeholder -->
          <div
            class="relative rounded-xl overflow-hidden border border-slate-200 bg-slate-100"
            style="height: 200px"
          >
            <div
              v-if="!form.latitude"
              class="absolute inset-0 flex flex-col items-center justify-center"
            >
              <MapPin :size="32" class="text-slate-300 mb-2" />
              <p class="text-slate-400 text-[13px]">
                Nhấn nút bên dưới để lấy vị trí
              </p>
            </div>
            <div
              v-else
              class="absolute inset-0 flex items-center justify-center bg-emerald-50"
            >
              <div class="text-center">
                <div
                  class="w-10 h-10 rounded-full bg-[#DA251D] flex items-center justify-center mx-auto mb-2 shadow-lg"
                >
                  <MapPin :size="20" class="text-white" />
                </div>
                <p class="text-slate-600 text-[13px] font-medium">
                  {{ form.latitude.toFixed(4) }}°N,
                  {{ form.longitude.toFixed(4) }}°E
                </p>
              </div>
            </div>
          </div>

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

          <div>
            <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
              >Địa chỉ cụ thể</label
            >
            <input
              v-model="form.address"
              placeholder="Số nhà, tên đường, khu vực..."
              :class="[inputCls, normCls]"
              class="text-[14px]"
            />
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                >Quận/Huyện</label
              >
              <select
                v-model="form.district"
                :class="[inputCls, normCls, 'appearance-none cursor-pointer']"
                class="text-[14px]"
              >
                <option value="">— Chọn Quận/Huyện —</option>
                <option v-for="d in districts" :key="d" :value="d">
                  {{ d }}
                </option>
              </select>
            </div>
            <div>
              <label class="text-slate-700 mb-1.5 block text-[13px] font-medium"
                >Phường/Xã</label
              >
              <select
                v-model="form.ward"
                :class="[inputCls, normCls, 'appearance-none cursor-pointer']"
                class="text-[14px]"
              >
                <option value="">— Chọn Phường/Xã —</option>
                <option v-for="w in wards" :key="w" :value="w">{{ w }}</option>
              </select>
            </div>
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
import { ref, reactive } from "vue";
import { RouterLink } from "vue-router";
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

const inputCls =
  "w-full py-3 px-4 rounded-xl border bg-white outline-none transition-all";
const normCls =
  "border-slate-200 hover:border-slate-300 focus:border-[#DA251D]/40 focus:ring-2 focus:ring-[#DA251D]/10";
const errCls = "border-red-300 ring-2 ring-red-100";

const categories = [
  { id: "gt", name: "Giao thông" },
  { id: "mt", name: "Môi trường" },
  { id: "ht", name: "Hạ tầng đô thị" },
  { id: "an", name: "An ninh trật tự" },
  { id: "yt", name: "Y tế - Sức khỏe" },
  { id: "gd", name: "Giáo dục - Đào tạo" },
  { id: "khac", name: "Khác" },
];

const districts = [
  "TP. Cao Lãnh",
  "TP. Sa Đéc",
  "H. Tháp Mười",
  "H. Cao Lãnh",
  "H. Thanh Bình",
  "H. Lấp Vò",
  "H. Lai Vung",
  "H. Châu Thành",
  "H. Hồng Ngự",
  "TX. Hồng Ngự",
  "H. Tam Nông",
  "H. Tân Hồng",
];
const wards = [
  "Phường 1",
  "Phường 2",
  "Phường 3",
  "Phường 4",
  "Phường 6",
  "Phường Mỹ Phú",
  "Xã Mỹ Tân",
  "Xã Mỹ Trà",
  "Xã An Bình",
];

const form = reactive({
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

const files = ref([]);
const filePreviews = ref([]);
const dragOver = ref(false);
const errors = reactive({});
const submitting = ref(false);
const submitted = ref(false);
const gettingLocation = ref(false);

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

function getLocation() {
  if (!navigator.geolocation) return;
  gettingLocation.value = true;
  navigator.geolocation.getCurrentPosition(
    (pos) => {
      form.latitude = pos.coords.latitude;
      form.longitude = pos.coords.longitude;
      gettingLocation.value = false;
    },
    () => {
      gettingLocation.value = false;
    },
    { enableHighAccuracy: true },
  );
}

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

function handleSubmit() {
  if (!validate()) return;
  submitting.value = true;
  setTimeout(() => {
    submitting.value = false;
    submitted.value = true;
    window.scrollTo({ top: 0, behavior: "smooth" });
  }, 1500);
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
}
</script>
