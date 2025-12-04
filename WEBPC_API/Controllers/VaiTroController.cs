using Microsoft.AspNetCore.Mvc;
using WEBPC_API.Models.DTOs.Requests;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaiTroController : ControllerBase
    {
        private readonly IUserService _userService;

        public VaiTroController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _userService.GetAllVaiTros();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VaiTroRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.CreateVaiTro(request);
            if (result)
            {
                return Ok(new { message = "Tạo vai trò thành công." });
            }
            return BadRequest(new { message = "Tạo vai trò thất bại." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.DeleteVaiTro(id);
            if (result)
            {
                return Ok(new { message = "Xóa vai trò thành công." });
            }
            return BadRequest(new { message = "Không tìm thấy vai trò hoặc lỗi khi xóa." });
        }
    }
}