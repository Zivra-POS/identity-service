using FluentValidation;
using IdentityService.Shared.Constants;
using IdentityService.Shared.DTOs.Request.Auth;

namespace IdentityService.API.Validators.Auth;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id user wajib diisi.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(256).WithMessage("Display name maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Nomor Telepon maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.ProfilImage)
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

        // Address validations
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
