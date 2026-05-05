using ReadX.Api.DTOs;
using ReadX.Api.Exceptions;
using ReadX.Api.Models;
using ReadX.Api.Models.Enums;
using ReadX.Api.Repositories.Interfaces;
using ReadX.Api.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ReadX.Api.Services.Implementations;

public class BorrowService : IBorrowService
{
    private readonly IMongoRepository<BorrowRecord> _borrowRepository;
    private readonly IMongoRepository<Book> _bookRepository;
    private readonly IMongoRepository<User> _userRepository;

    public BorrowService(IMongoRepository<BorrowRecord> borrowRepository, IMongoRepository<Book> bookRepository, IMongoRepository<User> userRepository)
    {
        _borrowRepository = borrowRepository;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
    }

    /// Member requests to borrow a book
    public async Task<BorrowResponse> RequestBorrowAsync(string userId, BorrowRequestDto request)
    {
        // Validate book and user existence
        var book = await _bookRepository.GetByIdAsync(request.BookId) ?? throw new BusinessException("BookNotFound", 404);
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new BusinessException("UserNotFound", 404);

        // Prevent duplicate active requests for the same book by the same user
        var existingActive = await _borrowRepository.GetOneAsync(b => b.UserId == userId && b.BookId == request.BookId && (b.Status == BorrowStatus.Pending || b.Status == BorrowStatus.Active));
        if (existingActive != null)
        {
            throw new BusinessException("ActiveBorrowExists", 409);
        }

        // Create initial pending borrow record
        var borrow = new BorrowRecord
        {
            UserId = userId,
            BookId = request.BookId,
            UserName = user.FullName,
            BookTitle = book.Title,
            BookCategory = book.Category,
            Status = BorrowStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            RequestedDurationDays = request.RequestedDurationDays // Duration requested by the user
        };

        await _borrowRepository.CreateAsync(borrow);
        return MapToResponse(borrow);
    }

    /// Admin gets all borrow records with optional status filtering
    public async Task<BorrowListResponse> GetAllBorrowsAsync(string? status, int page, int limit)
    {
        var filter = MongoDB.Driver.Builders<BorrowRecord>.Filter.Empty;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<BorrowStatus>(status, true, out var parsedStatus))
        {
            filter = MongoDB.Driver.Builders<BorrowRecord>.Filter.Eq(b => b.Status, parsedStatus);
        }

        var borrows = await _borrowRepository.GetAllAsync(filter, page, limit);
        var total = await _borrowRepository.CountAsync(filter);
        
        // Calculate counts for different statuses for the dashboard
        var counts = new BorrowCounts
        {
            Pending = await _borrowRepository.CountAsync(b => b.Status == BorrowStatus.Pending),
            Active = await _borrowRepository.CountAsync(b => b.Status == BorrowStatus.Active),
            Overdue = await _borrowRepository.CountAsync(b => b.Status == BorrowStatus.Overdue)
        };

        return new BorrowListResponse
        {
            Total = total,
            Counts = counts,
            Page = page,
            Limit = limit,
            Borrows = borrows.Select(MapToResponse).ToList()
        };
    }

    /// Admin approves a pending borrow request
    public async Task<BorrowResponse> ApproveBorrowAsync(string id)
    {
        var borrow = await _borrowRepository.GetByIdAsync(id) ?? throw new BusinessException("BorrowRecordNotFound", 404);
        if (borrow.Status != BorrowStatus.Pending) throw new BusinessException("BorrowNotPending", 409);

        var book = await _bookRepository.GetByIdAsync(borrow.BookId) ?? throw new BusinessException("BookNotFound", 404);
        
        // Ensure there are enough copies in stock
        if (book.AvailableCopies <= 0) throw new BusinessException("NotEnoughCopies", 409);

        // Update status and calculate due date
        borrow.Status = BorrowStatus.Active;
        borrow.BorrowedAt = DateTime.UtcNow;
        
        // Use requested duration or default to 14 days
        var duration = borrow.RequestedDurationDays > 0 ? borrow.RequestedDurationDays : 14;
        borrow.DueAt = DateTime.UtcNow.AddDays(duration);

        // Reduce available book copies
        book.AvailableCopies -= 1;
        
        await _bookRepository.UpdateAsync(book.Id, book);
        await _borrowRepository.UpdateAsync(id, borrow);

        return MapToResponse(borrow);
    }

    /// Admin rejects a pending borrow request
    public async Task<BorrowResponse> RejectBorrowAsync(string id)
    {
        var borrow = await _borrowRepository.GetByIdAsync(id) ?? throw new BusinessException("BorrowRecordNotFound", 404);
        if (borrow.Status != BorrowStatus.Pending) throw new BusinessException("BorrowNotPending", 409);

        borrow.Status = BorrowStatus.Rejected;
        await _borrowRepository.UpdateAsync(id, borrow);

        return MapToResponse(borrow);
    }

    /// Admin processes a book return
    public async Task<BorrowResponse> ReturnBorrowAsync(string id)
    {
        var borrow = await _borrowRepository.GetByIdAsync(id) ?? throw new BusinessException("BorrowRecordNotFound", 404);
        if (borrow.Status != BorrowStatus.Active && borrow.Status != BorrowStatus.Overdue) 
            throw new BusinessException("BorrowNotActiveOrOverdue", 409);

        // Increment available book copies
        var book = await _bookRepository.GetByIdAsync(borrow.BookId);
        if (book != null)
        {
            book.AvailableCopies += 1;
            await _bookRepository.UpdateAsync(book.Id, book);
        }

        borrow.Status = BorrowStatus.Returned;
        borrow.ReturnedAt = DateTime.UtcNow;

        await _borrowRepository.UpdateAsync(id, borrow);

        return MapToResponse(borrow);
    }

    /// Member gets their own borrow history
    public async Task<PaginatedResponse<BorrowResponse>> GetMyBorrowsAsync(string userId, string? status, int page, int limit)
    {
        var filter = MongoDB.Driver.Builders<BorrowRecord>.Filter.Eq(b => b.UserId, userId);
        
        if (!string.IsNullOrEmpty(status))
        {
            var statusList = status.Split(',').Select(s => Enum.Parse<BorrowStatus>(s, true)).ToList();
            var statusFilter = MongoDB.Driver.Builders<BorrowRecord>.Filter.In(b => b.Status, statusList);
            filter = MongoDB.Driver.Builders<BorrowRecord>.Filter.And(filter, statusFilter);
        }

        var borrows = await _borrowRepository.GetAllAsync(filter, page, limit);
        var total = await _borrowRepository.CountAsync(filter);

        return new PaginatedResponse<BorrowResponse>
        {
            Total = total,
            Page = page,
            Limit = limit,
            Data = borrows.Select(MapToResponse).ToList()
        };
    }

    /// Maps internal model to public DTO and calculates dynamic fields
    private static BorrowResponse MapToResponse(BorrowRecord b)
    {
        var response = new BorrowResponse
        {
            Id = b.Id,
            UserId = b.UserId,
            UserName = b.UserName,
            BookId = b.BookId,
            BookTitle = b.BookTitle,
            BookCategory = b.BookCategory,
            Status = b.Status.ToString().ToLower(),
            RequestedAt = b.RequestedAt,
            RequestedDurationDays = b.RequestedDurationDays,
            BorrowedAt = b.BorrowedAt,
            DueAt = b.DueAt,
            ReturnedAt = b.ReturnedAt
        };

        // Calculate days overdue or remaining
        if (b.DueAt.HasValue)
        {
            // If returned, calculate relative to return date. If not, relative to now.
            var referenceDate = b.ReturnedAt ?? DateTime.UtcNow;
            var remaining = (b.DueAt.Value - referenceDate).Days;
            
            if (remaining < 0)
            {
                response.DaysOverdue = Math.Abs(remaining);
                response.DaysRemaining = 0;
            }
            else
            {
                response.DaysRemaining = remaining;
                response.DaysOverdue = 0;
            }
        }
        else
        {
            response.DaysOverdue = 0;
            response.DaysRemaining = 0;
        }

        return response;
    }
}
