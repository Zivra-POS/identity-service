using FluentValidation;
using IdentityService.Shared.DTOs.Request;
using IdentityService.Shared.DTOs.Request.Auth;

namespace IdentityService.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor<string>(x => x.Username)
            .NotEmpty().WithMessage("Username wajib diisi.")
            .Length(3, 50).WithMessage("Username harus antara {MinLength} hingga {MaxLength} karakter.");

        RuleFor<string>(x => x.Email)
            .NotEmpty().WithMessage("Email wajib diisi.")
            .EmailAddress().WithMessage("Format email tidak valid.");

        RuleFor<string>(x => x.Password)
            .NotEmpty().WithMessage("Password wajib diisi.")
            .MinimumLength(6).WithMessage("Password minimal {MinLength} karakter.");

        // Address field validations
        RuleFor<string?>(x => x.Address)
            .MaximumLength(1000).WithMessage("Address maksimal {MaxLength} karakter.");

        RuleFor<string?>(x => x.Province)
            .MaximumLength(100).WithMessage("Province maksimal {MaxLength} karakter.");

        RuleFor<string?>(x => x.City)
            .MaximumLength(100).WithMessage("City maksimal {MaxLength} karakter.");

        RuleFor<string?>(x => x.District)
            .MaximumLength(100).WithMessage("District maksimal {MaxLength} karakter.");

        RuleFor<string?>(x => x.Rt)
            .MaximumLength(10).WithMessage("Rt maksimal {MaxLength} karakter.");

        RuleFor<string?>(x => x.Rw)
            .MaximumLength(10).WithMessage("Rw maksimal {MaxLength} karakter.");
    }
}
