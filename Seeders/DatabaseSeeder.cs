using ReadX.Api.Models;
using ReadX.Api.Models.Enums;
using ReadX.Api.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace ReadX.Api.Seeders;

public class DatabaseSeeder
{
    private readonly IMongoRepository<User> _userRepository;
    private readonly IMongoRepository<Book> _bookRepository;
    private readonly IMongoRepository<BorrowRecord> _borrowRepository;

    public DatabaseSeeder(
        IMongoRepository<User> userRepository,
        IMongoRepository<Book> bookRepository,
        IMongoRepository<BorrowRecord> borrowRepository)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _borrowRepository = borrowRepository;
    }

    public async Task SeedAsync()
    {
        var adminCount = await _userRepository.CountAsync(u => u.Role == UserRole.Admin);
        if (adminCount == 0)
        {
            // 1. Seed Admin
            var adminUser = new User
            {
                FullName = "System Admin",
                Email = "admin@readx.com",
                Phone = "01000000000",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                Role = UserRole.Admin
            };
            await _userRepository.CreateAsync(adminUser);

            // 2. Seed Members
            var member1 = new User
            {
                FullName = "Adham Emad",
                Email = "Adham@example.com",
                Phone = "01111111111",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                Role = UserRole.Member
            };
            var member2 = new User
            {
                FullName = "Youssef Ashraf",
                Email = "Homosany@example.com",
                Phone = "01222222222",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                Role = UserRole.Member
            };
            
            var member3 = new User
            {
                FullName = "Youssef Elshenawy",
                Email = "Mensh@example.com",
                Phone = "01222222222",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                Role = UserRole.Member
            };
            var member4 = new User
            {
                FullName = "Mina Safwat",
                Email = "Mina@example.com",
                Phone = "01222222222",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                Role = UserRole.Member
            };
            var member5 = new User
            {
                FullName = "Shahd Mohamed",
                Email = "Shahd@example.com",
                Phone = "01222222222",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                Role = UserRole.Member
            };

            var member6 = new User
            {
                FullName = "Amr Mustafa",
                Email = "Amor@example.com",
                Phone = "01222222222",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                Role = UserRole.Member
            };
        
            await _userRepository.CreateAsync(member1);
            await _userRepository.CreateAsync(member2);
            await _userRepository.CreateAsync(member3);
            await _userRepository.CreateAsync(member4);
            await _userRepository.CreateAsync(member5);
            await _userRepository.CreateAsync(member6);

            // 3. Seed Books
            var book1 = new Book
            {
                Title = "The Pragmatic Programmer",
                Author = "Andy Hunt",
                Category = "Programming",
                TotalCopies = 5,
                AvailableCopies = 4, // 1 is borrowed
                Description = "A masterclass in software engineering."
            };
            var book2 = new Book
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Category = "Programming",
                TotalCopies = 3,
                AvailableCopies = 3,
                Description = "A Handbook of Agile Software Craftsmanship."
            };
            var book3 = new Book
            {
                Title = "Design Patterns",
                Author = "Erich Gamma",
                Category = "Software Engineering",
                TotalCopies = 2,
                AvailableCopies = 2,
                Description = "Elements of Reusable Object-Oriented Software."
            };
            await _bookRepository.CreateAsync(book1);
            await _bookRepository.CreateAsync(book2);
            await _bookRepository.CreateAsync(book3);

            // 4. Seed Borrow Records
            var borrow1 = new BorrowRecord
            {
                UserId = member1.Id,
                UserName = member1.FullName,
                BookId = book1.Id,
                BookTitle = book1.Title,
                BookCategory = book1.Category,
                Status = BorrowStatus.Active,
                RequestedAt = DateTime.UtcNow.AddDays(-2),
                BorrowedAt = DateTime.UtcNow.AddDays(-1),
                DueAt = DateTime.UtcNow.AddDays(13)
            };
            
            var borrow2 = new BorrowRecord
            {
                UserId = member2.Id,
                UserName = member2.FullName,
                BookId = book2.Id,
                BookTitle = book2.Title,
                BookCategory = book2.Category,
                Status = BorrowStatus.Pending,
                RequestedAt = DateTime.UtcNow.AddHours(-5)
            };

            await _borrowRepository.CreateAsync(borrow1);
            await _borrowRepository.CreateAsync(borrow2);
        }
    }
}
