using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;
using WEBPC_API.Models.Interfaces;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class DanhMucService : IDanhMucService
    {
        private readonly IDanhMucRepository _repo;

        public DanhMucService(IDanhMucRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(x => new CategoryResponse
            {
                MaDanhMuc = x.MaDanhMuc,
                TenDanhMuc = x.TenDanhMuc,
                MoTa = x.MoTa,
                SoLuongSanPham = x.SanPhams?.Count ?? 0
            });
        }

        public async Task<CategoryResponse> GetCategoryByIdAsync(int id)
        {
            var x = await _repo.GetByIdAsync(id);
            if (x == null) return null;

            return new CategoryResponse
            {
                MaDanhMuc = x.MaDanhMuc,
                TenDanhMuc = x.TenDanhMuc,
                MoTa = x.MoTa,
                SoLuongSanPham = x.SanPhams?.Count ?? 0
            };
        }

        public async Task<bool> CreateCategoryAsync(CreateCategoryRequest request)
        {
            // Kiểm tra trùng tên
            if (await _repo.IsNameExistsAsync(request.TenDanhMuc))
                throw new Exception("Tên danh mục đã tồn tại.");

            var newItem = new DanhMuc
            {
                TenDanhMuc = request.TenDanhMuc,
                MoTa = request.MoTa
            };

            await _repo.AddAsync(newItem);
            return true;
        }

        public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return false;

            // Check trùng tên chỉ khi người dùng CÓ gửi tên mới lên
            if (!string.IsNullOrEmpty(request.TenDanhMuc))
            {
                // Nếu tên mới khác tên cũ thì mới check trùng
                if (item.TenDanhMuc != request.TenDanhMuc && await _repo.IsNameExistsAsync(request.TenDanhMuc))
                    throw new Exception("Tên danh mục mới bị trùng.");

                item.TenDanhMuc = request.TenDanhMuc;
            }

            if (!string.IsNullOrEmpty(request.MoTa))
            {
                item.MoTa = request.MoTa;
            }

            await _repo.UpdateAsync(item);
            return true;
        }

        public async Task<string> DeleteCategoryAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return "Không tìm thấy danh mục.";

            // Logic an toàn: Không cho xóa nếu đang có sản phẩm
            if (await _repo.HasProductsAsync(id))
                return "Không thể xóa danh mục này vì đang có sản phẩm bên trong.";

            await _repo.DeleteAsync(id);
            return "OK";
        }
    }
}