using Newtonsoft.Json;

namespace WEBPC_API.Helpers
{
    public class LocationHelper
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://esgoo.net/api-tinhthanh";

        public LocationHelper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Các class model dùng nội bộ để map dữ liệu JSON trả về từ API esgoo
        public class LocationResponse
        {
            public int error { get; set; }
            public string error_text { get; set; }
            public List<LocationData> data { get; set; }
        }

        public class LocationData
        {
            public string id { get; set; }
            public string name { get; set; }
            public string name_en { get; set; }
            public string full_name { get; set; }
            public string full_name_en { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
        }

        // 1. Lấy danh sách Tỉnh/Thành
        public async Task<List<LocationData>> GetProvincesAsync()
        {
            var response = await _httpClient.GetStringAsync($"{BASE_URL}/1/0.htm");
            var result = JsonConvert.DeserializeObject<LocationResponse>(response);
            return result?.error == 0 ? result.data : new List<LocationData>();
        }

        // 2. Lấy danh sách Quận/Huyện theo Tỉnh
        public async Task<List<LocationData>> GetDistrictsAsync(string provinceId)
        {
            var response = await _httpClient.GetStringAsync($"{BASE_URL}/2/{provinceId}.htm");
            var result = JsonConvert.DeserializeObject<LocationResponse>(response);
            return result?.error == 0 ? result.data : new List<LocationData>();
        }

        // 3. Lấy danh sách Phường/Xã theo Quận
        public async Task<List<LocationData>> GetWardsAsync(string districtId)
        {
            var response = await _httpClient.GetStringAsync($"{BASE_URL}/3/{districtId}.htm");
            var result = JsonConvert.DeserializeObject<LocationResponse>(response);
            return result?.error == 0 ? result.data : new List<LocationData>();
        }

        // 4. Hàm kiểm tra và lấy tên đầy đủ của bộ 3 ID (Dùng khi Create/Update Address)
        public async Task<(string Tinh, string Huyen, string Xa)> GetAddressNamesAsync(string pId, string dId, string wId)
        {
            // Lấy list tỉnh
            var provinces = await GetProvincesAsync();
            var province = provinces.FirstOrDefault(p => p.id == pId);
            if (province == null) return (null, null, null);

            // Lấy list huyện
            var districts = await GetDistrictsAsync(pId);
            var district = districts.FirstOrDefault(d => d.id == dId);
            if (district == null) return (province.name, null, null);

            // Lấy list xã
            var wards = await GetWardsAsync(dId);
            var ward = wards.FirstOrDefault(w => w.id == wId);
            if (ward == null) return (province.name, district.name, null);

            return (province.name, district.name, ward.name);
        }
    }
}