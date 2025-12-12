using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Models.DTOs.Responses;
using WEBPC_API.Models.Entities;
using WEBPC_API.Repositories.Interfaces;
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

        public async Task<List<CategoryResponse>> GetAllCategoriesAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<CategoryResponse> GetCategoryByIdAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) throw new KeyNotFoundException("Không tìm thấy danh mục");
            return MapToResponse(category);
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
        {
            // Validate: Nếu có chọn cha, kiểm tra cha có tồn tại không
            if (request.MaDanhMucCha.HasValue && request.MaDanhMucCha.Value > 0)
            {
                var parent = await _repo.GetByIdAsync(request.MaDanhMucCha.Value);
                if (parent == null) throw new Exception("Mã danh mục cha không tồn tại");
            }

            var newCategory = new DanhMuc
            {
                TenDanhMuc = request.TenDanhMuc,
                MoTa = request.MoTa,
                MaDanhMucCha = (request.MaDanhMucCha.HasValue && request.MaDanhMucCha.Value > 0)
                               ? request.MaDanhMucCha
                               : null
            };

            var created = await _repo.CreateAsync(newCategory);

            // Reload lại để lấy thông tin Include (Tên cha) nếu cần thiết
            return await GetCategoryByIdAsync(created.MaDanhMuc);
        }

        public async Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) throw new KeyNotFoundException("Không tìm thấy danh mục");

            if (!string.IsNullOrEmpty(request.TenDanhMuc))
                category.TenDanhMuc = request.TenDanhMuc;

            if (request.MoTa != null)
                category.MoTa = request.MoTa;

            // Logic cập nhật cha
            if (request.MaDanhMucCha.HasValue)
            {
                int newParentId = request.MaDanhMucCha.Value;

                // Trường hợp muốn xóa cha (Set null) -> Client gửi 0 hoặc -1 (tùy quy ước, ở đây check <= 0)
                if (newParentId <= 0)
                {
                    category.MaDanhMucCha = null;
                }
                else
                {
                    if (newParentId == id)
                        throw new Exception("Một danh mục không thể làm cha của chính nó!");

                    var parent = await _repo.GetByIdAsync(newParentId);
                    if (parent == null) throw new Exception("Mã danh mục cha không tồn tại");

                    category.MaDanhMucCha = newParentId;
                }
            }

            await _repo.UpdateAsync(category);
            return await GetCategoryByIdAsync(id); // Return lại để lấy info mới nhất
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;

            // Kiểm tra ràng buộc: Nếu có danh mục con thì không cho xóa (tùy nghiệp vụ)
            if (category.DanhMucCons != null && category.DanhMucCons.Any())
            {
                throw new Exception("Không thể xóa danh mục này vì đang chứa các danh mục con.");
            }

            // Kiểm tra ràng buộc: Nếu có sản phẩm thì không cho xóa
            // (Cần thêm logic check SanPhamRepo nếu muốn chặn chặt chẽ hơn)

            await _repo.DeleteAsync(id);
            return true;
        }

        // Helper method chuyển đổi Entity -> DTO
        private CategoryResponse MapToResponse(DanhMuc dm)
        {
            return new CategoryResponse
            {
                MaDanhMuc = dm.MaDanhMuc,
                TenDanhMuc = dm.TenDanhMuc,
                MoTa = dm.MoTa,
                MaDanhMucCha = dm.MaDanhMucCha,
                TenDanhMucCha = dm.DanhMucCha?.TenDanhMuc, // Lấy tên cha từ Include
                SoLuongDanhMucCon = dm.DanhMucCons?.Count ?? 0
            };
        }
    }
}