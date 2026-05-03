using FluentValidation;
using ReadX.Api.DTOs;

namespace ReadX.Api.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithErrorCode("ValidationFailed");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithErrorCode("ValidationFailed");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithErrorCode("ValidationFailed");
        RuleFor(x => x.PasswordConfirmation).Equal(x => x.Password).WithErrorCode("PasswordsDoNotMatch").WithMessage("Passwords do not match.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithErrorCode("ValidationFailed");
        RuleFor(x => x.Password).NotEmpty().WithErrorCode("ValidationFailed");
    }
}
