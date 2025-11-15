using FluentValidation;
using IdentityService.Shared.DTOs.Request.User;

namespace IdentityService.API.Validators.User;

public class UserClaimRequestValidator : AbstractValidator<UserClaimRequest>
{
    public UserClaimRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id wajib diisi.")
            .Must(id => id != Guid.Empty).WithMessage("User Id tidak valid.");

        RuleFor(x => x.ClaimType)
            .NotEmpty().WithMessage("Claim Type wajib diisi.")
            .MaximumLength(150).WithMessage("Claim Type maksimal {MaxLength} karakter.");

        RuleFor(x => x.ClaimValue)
            .NotEmpty().WithMessage("Claim Value wajib diisi.")
            .MaximumLength(500).WithMessage("Claim Value maksimal {MaxLength} karakter.");
    }
}
