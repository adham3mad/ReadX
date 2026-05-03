using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadX.Api.DTOs;
using ReadX.Api.Services.Interfaces;
using System.Threading.Tasks;

namespace ReadX.Api.Controllers;

[ApiController]
[Route("books")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks([FromQuery] string? q, [FromQuery] string? category, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var response = await _bookService.GetBooksAsync(q, category, page, limit);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(string id)
    {
        var response = await _bookService.GetBookAsync(id);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateBook(CreateBookRequest request)
    {
        var response = await _bookService.CreateBookAsync(request);
        return Created($"/books/{response.Id}", response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateBook(string id, UpdateBookRequest request)
    {
        var response = await _bookService.UpdateBookAsync(id, request);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteBook(string id)
    {
        await _bookService.DeleteBookAsync(id);
        return NoContent();
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var response = await _bookService.GetCategoriesAsync();
        return Ok(response);
    }
}
