using FluentValidation;
using IdentityService.Shared.DTOs.Request.Role;

namespace IdentityService.API.Validators.Role;

public class RoleRequestValidator : AbstractValidator<RoleRequest>
{
    public RoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama wajib diisi.")
            .Length(3, 100).WithMessage("Nama harus antara {MinLength} hingga {MaxLength} karakter.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Deskripsi maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}