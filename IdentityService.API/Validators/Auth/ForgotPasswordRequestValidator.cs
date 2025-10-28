using FluentValidation;
using IdentityService.Shared.DTOs.Request;
using IdentityService.Shared.DTOs.Request.Auth;

namespace IdentityService.API.Validators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email wajib diisi.")
            .EmailAddress().WithMessage("Format email tidak valid.");
    }
}