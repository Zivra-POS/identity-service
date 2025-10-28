using FluentValidation;
using IdentityService.Shared.DTOs.Request;
using IdentityService.Shared.DTOs.Request.Auth;

namespace IdentityService.API.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username wajib diisi.")
            .MinimumLength(3).WithMessage("Username minimal {MinLength} karakter.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password wajib diisi.")
            .MinimumLength(6).WithMessage("Password minimal {MinLength} karakter.");
    }
}

