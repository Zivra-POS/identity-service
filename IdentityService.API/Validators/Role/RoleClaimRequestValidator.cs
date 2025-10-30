using FluentValidation;
using IdentityService.Shared.DTOs.Request.Role;
using IdentityService.Shared.DTOs.RoleClaim;

namespace IdentityService.API.Validators.Role;

public class RoleClaimRequestValidator : AbstractValidator<RoleClaimRequest>
{
    public RoleClaimRequestValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role Id wajib diisi.")
            .Must(id => id != Guid.Empty).WithMessage("Role Id tidak valid.");

        RuleFor(x => x.ClaimType)
            .NotEmpty().WithMessage("Claim Type wajib diisi.")
            .MaximumLength(150).WithMessage("Claim Type maksimal {MaxLength} karakter.");

        RuleFor(x => x.ClaimValue)
            .NotEmpty().WithMessage("Claim Value wajib diisi.")
            .MaximumLength(500).WithMessage("Claim Value maksimal {MaxLength} karakter.");
    }
}