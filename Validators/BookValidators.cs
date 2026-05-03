using FluentValidation;
using ReadX.Api.DTOs;

namespace ReadX.Api.Validators;

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithErrorCode("ValidationFailed");
        RuleFor(x => x.Author).NotEmpty().WithErrorCode("ValidationFailed");
        RuleFor(x => x.Category).NotEmpty().WithErrorCode("ValidationFailed");
        RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(1).WithErrorCode("ValidationFailed");
    }
}

public class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
    public UpdateBookRequestValidator()
    {
        RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(1).When(x => x.TotalCopies.HasValue).WithErrorCode("ValidationFailed");
    }
}
