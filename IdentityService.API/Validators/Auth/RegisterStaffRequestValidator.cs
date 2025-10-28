using FluentValidation;
using IdentityService.Shared.Constants;
using IdentityService.Shared.DTOs.Request.Auth;

namespace IdentityService.API.Validators.Auth;

public class RegisterStaffRequestValidator : AbstractValidator<RegisterStaffRequest>
{
    public RegisterStaffRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username wajib diisi.")
            .MinimumLength(3).WithMessage("Username minimal {MinLength} karakter.")
            .MaximumLength(100).WithMessage("Username maksimal {MaxLength} karakter.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email wajib diisi.")
            .EmailAddress().WithMessage("Format email tidak valid.")
            .MaximumLength(256).WithMessage("Email maksimal {MaxLength} karakter.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password wajib diisi.")
            .MinimumLength(6).WithMessage("Password minimal {MinLength} karakter.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(256).WithMessage("Display name maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(32).WithMessage("Nomor telepon maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.ProfileImage)
            .Custom((file, context) =>
            {
                if (file == null) return;

                if (file.Length > Config.MaxImageBytes)
                {
                    context.AddFailure("ProfilImage", $"Ukuran gambar maksimal {Config.MaxImageBytes / (1024 * 1024)} MB.");
                    return;
                }

                var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (string.IsNullOrWhiteSpace(file.ContentType) || !allowed.Contains(file.ContentType.ToLowerInvariant()))
                {
                    context.AddFailure("ProfilImage", "Tipe file harus berupa gambar (jpeg, png, gif, webp).");
                }
            });

        RuleForEach(x => x.RoleIDs)
            .NotEmpty().WithMessage("Setiap RoleID tidak boleh kosong.")
            .When(x => x.RoleIDs != null && x.RoleIDs.Length > 0);

        RuleFor(x => x.Address)
            .MaximumLength(1000).WithMessage("Address maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.Province)
            .MaximumLength(100).WithMessage("Province maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Province));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.District)
            .MaximumLength(100).WithMessage("District maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.District));

        RuleFor(x => x.Rt)
            .MaximumLength(10).WithMessage("Rt maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Rt));

        RuleFor(x => x.Rw)
            .MaximumLength(10).WithMessage("Rw maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Rw));
    }
}