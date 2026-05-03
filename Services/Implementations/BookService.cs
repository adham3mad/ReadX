using ReadX.Api.DTOs;
using ReadX.Api.Exceptions;
using ReadX.Api.Models;
using ReadX.Api.Repositories.Interfaces;
using ReadX.Api.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReadX.Api.Services.Implementations;

public class BookService : IBookService
{
    private readonly IMongoRepository<Book> _bookRepository;
    private readonly IMongoRepository<BorrowRecord> _borrowRepository;

    public BookService(IMongoRepository<Book> bookRepository, IMongoRepository<BorrowRecord> borrowRepository)
    {
        _bookRepository = bookRepository;
        _borrowRepository = borrowRepository;
    }

    public async Task<PaginatedResponse<BookResponse>> GetBooksAsync(string? query, string? category, int page, int limit)
    {
        var filter = MongoDB.Driver.Builders<Book>.Filter.Empty;
        if (!string.IsNullOrEmpty(query))
        {
            var queryFilter = MongoDB.Driver.Builders<Book>.Filter.Or(
                MongoDB.Driver.Builders<Book>.Filter.Regex(b => b.Title, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                MongoDB.Driver.Builders<Book>.Filter.Regex(b => b.Author, new MongoDB.Bson.BsonRegularExpression(query, "i"))
            );
            filter = MongoDB.Driver.Builders<Book>.Filter.And(filter, queryFilter);
        }
        if (!string.IsNullOrEmpty(category))
        {
            filter = MongoDB.Driver.Builders<Book>.Filter.And(filter, MongoDB.Driver.Builders<Book>.Filter.Eq(b => b.Category, category));
        }

        var books = await _bookRepository.GetAllAsync(filter, page, limit);
        var total = await _bookRepository.CountAsync(filter);

        return new PaginatedResponse<BookResponse>
        {
            Total = total,
            Page = page,
            Limit = limit,
            Data = books.Select(MapToBookResponse).ToList()
        };
    }

    public async Task<BookResponse> GetBookAsync(string id)
    {
        var book = await _bookRepository.GetByIdAsync(id) ?? throw new BusinessException("BookNotFound", 404);
        var activeBorrowsCount = await _borrowRepository.CountAsync(b => b.BookId == id && b.Status == Models.Enums.BorrowStatus.Active);
        
        var response = MapToBookResponse(book);
        response.BorrowedCopies = (int)activeBorrowsCount;
        return response;
    }

    public async Task<BookResponse> CreateBookAsync(CreateBookRequest request)
    {
        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            Isbn = request.Isbn,
            Category = request.Category,
            CoverUrl = request.CoverUrl,
            Description = request.Description,
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.TotalCopies
        };

        await _bookRepository.CreateAsync(book);
        return MapToBookResponse(book);
    }

    public async Task<BookResponse> UpdateBookAsync(string id, UpdateBookRequest request)
    {
        var book = await _bookRepository.GetByIdAsync(id) ?? throw new BusinessException("BookNotFound", 404);

        if (request.Title != null) book.Title = request.Title;
        if (request.Author != null) book.Author = request.Author;
        if (request.Isbn != null) book.Isbn = request.Isbn;
        if (request.Category != null) book.Category = request.Category;
        if (request.CoverUrl != null) book.CoverUrl = request.CoverUrl;
        if (request.Description != null) book.Description = request.Description;
        
        if (request.TotalCopies.HasValue)
        {
            int borrowedCopies = book.TotalCopies - book.AvailableCopies;
            if (request.TotalCopies.Value < borrowedCopies)
            {
                throw new BusinessException("ValidationFailed", 422); // cannot set total less than currently borrowed
            }
            book.TotalCopies = request.TotalCopies.Value;
            book.AvailableCopies = book.TotalCopies - borrowedCopies;
        }

        await _bookRepository.UpdateAsync(id, book);
        return MapToBookResponse(book);
    }

    public async Task DeleteBookAsync(string id)
    {
        var book = await _bookRepository.GetByIdAsync(id) ?? throw new BusinessException("BookNotFound", 404);
        
        var hasActiveBorrows = await _borrowRepository.CountAsync(b => b.BookId == id && (b.Status == Models.Enums.BorrowStatus.Active || b.Status == Models.Enums.BorrowStatus.Pending));
        if (hasActiveBorrows > 0)
        {
            throw new BusinessException("CannotDeleteBookWithActiveBorrows", 409);
        }

        await _bookRepository.RemoveAsync(id);
    }

    public async Task<CategoryResponse> GetCategoriesAsync()
    {
        var books = await _bookRepository.GetAllAsync(b => true, 1, 10000); // simple approach, in real app use distinct aggregate
        var categories = books.Select(b => b.Category).Distinct().ToList();
        return new CategoryResponse { Categories = categories };
    }

    private static BookResponse MapToBookResponse(Book book)
    {
        return new BookResponse
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Isbn = book.Isbn,
            Category = book.Category,
            CoverUrl = book.CoverUrl,
            Description = book.Description,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies
        };
    }
}
