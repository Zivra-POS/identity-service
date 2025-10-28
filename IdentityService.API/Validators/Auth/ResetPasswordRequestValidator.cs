using FluentValidation;
using IdentityService.Shared.DTOs.Request;
using IdentityService.Shared.DTOs.Request.Auth;

namespace IdentityService.API.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token reset wajib diisi.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password baru wajib diisi.")
            .MinimumLength(6).WithMessage("Password minimal {MinLength} karakter.")
            .MaximumLength(100).WithMessage("Password maksimal {MaxLength} karakter.");
    }
}

