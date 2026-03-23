using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DongThapHelpdesk.Api.Constants;
using DongThapHelpdesk.Api.DTOs.Category;
using DongThapHelpdesk.Api.Services;

namespace DongThapHelpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _service;

    public CategoriesController(CategoryService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lấy danh mục dạng cây phân cấp
    /// Công khai - dùng khi người dân chọn loại báo cáo
    /// </summary>
    [HttpGet("tree")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTree()
    {
        var result = await _service.GetTreeAsync();
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh mục dạng phẳng
    /// Công khai - dùng trong dropdown
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetFlatAsync();
        return Ok(result);
    }

    /// <summary>Lấy chi tiết một danh mục</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(
            new { message = "Không tìm thấy danh mục" });
        return Ok(result);
    }

    /// <summary>
    /// Tạo danh mục mới
    /// Chỉ Dispatcher và Admin
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.AdminAndDispatcher)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request)
    {
        if (string.IsNullOrEmpty(request.Name) ||
            string.IsNullOrEmpty(request.Code))
            return BadRequest(
                new { message = "Tên và mã không được để trống" });

        var (success, message, data) =
            await _service.CreateAsync(request);

        if (!success)
            return BadRequest(new { message });

        return CreatedAtAction(
            nameof(GetById),
            new { id = data!.Id },
            data);
    }

    /// <summary>
    /// Cập nhật danh mục
    /// Chỉ Dispatcher và Admin
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.AdminAndDispatcher)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateCategoryRequest request)
    {
        var (success, message) =
            await _service.UpdateAsync(id, request);

        if (!success)
            return NotFound(new { message });

        return Ok(new { message });
    }

    /// <summary>
    /// Xóa mềm danh mục
    /// Chỉ Dispatcher và Admin
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.AdminAndDispatcher)]
    public async Task<IActionResult> Delete(string id)
    {
        var (success, message) =
            await _service.DeleteAsync(id);

        if (!success)
            return NotFound(new { message });

        return Ok(new { message });
    }
}