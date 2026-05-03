using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using ReadX.Api.Services.Interfaces;

namespace ReadX.Api.Services.Implementations;

public class LocalizationService : ILocalizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly Dictionary<string, Dictionary<string, string>> _translations = new()
    {
        {
            "en", new Dictionary<string, string>
            {
                { "UserNotFound", "User not found." },
                { "InvalidCredentials", "Invalid email or password." },
                { "EmailAlreadyRegistered", "Email is already registered." },
                { "PasswordsDoNotMatch", "Passwords do not match." },
                { "BookNotFound", "Book not found." },
                { "NotEnoughCopies", "Not enough available copies." },
                { "ActiveBorrowExists", "You already have a pending or active borrow for this book." },
                { "BorrowRecordNotFound", "Borrow record not found." },
                { "BorrowNotPending", "Borrow record is not in a pending state." },
                { "BorrowNotActiveOrOverdue", "Borrow record is not active or overdue." },
                { "MemberHasActiveBorrows", "Cannot delete: member has active borrows." },
                { "ValidationFailed", "Validation failed." },
                { "Unauthorized", "Unauthorized access." },
                { "Forbidden", "You do not have permission to perform this action." }
            }
        },
        {
            "ar", new Dictionary<string, string>
            {
                { "UserNotFound", "المستخدم غير موجود." },
                { "InvalidCredentials", "البريد الإلكتروني أو كلمة المرور غير صحيحة." },
                { "EmailAlreadyRegistered", "البريد الإلكتروني مسجل بالفعل." },
                { "PasswordsDoNotMatch", "كلمات المرور غير متطابقة." },
                { "BookNotFound", "الكتاب غير موجود." },
                { "NotEnoughCopies", "لا توجد نسخ متاحة كافية." },
                { "ActiveBorrowExists", "لديك بالفعل طلب استعارة قيد الانتظار أو نشط لهذا الكتاب." },
                { "BorrowRecordNotFound", "سجل الاستعارة غير موجود." },
                { "BorrowNotPending", "سجل الاستعارة ليس في حالة قيد الانتظار." },
                { "BorrowNotActiveOrOverdue", "سجل الاستعارة ليس نشطاً أو متأخراً." },
                { "MemberHasActiveBorrows", "لا يمكن الحذف: العضو لديه استعارات نشطة." },
                { "ValidationFailed", "فشل التحقق من صحة البيانات." },
                { "Unauthorized", "وصول غير مصرح به." },
                { "Forbidden", "ليس لديك صلاحية للقيام بهذا الإجراء." }
            }
        }
    };

    public LocalizationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetMessage(string key, string? lang = null)
    {
        if (string.IsNullOrEmpty(lang))
        {
            var headerLang = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString();
            lang = !string.IsNullOrEmpty(headerLang) && headerLang.StartsWith("ar", System.StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
        }

        if (_translations.TryGetValue(lang, out var langDict))
        {
            if (langDict.TryGetValue(key, out var message))
            {
                return message;
            }
        }

        // Fallback to English if key not found in Arabic
        if (lang != "en" && _translations["en"].TryGetValue(key, out var fallbackMessage))
        {
            return fallbackMessage;
        }

        return key; // return key if translation is missing entirely
    }
}
