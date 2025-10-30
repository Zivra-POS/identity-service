using FluentValidation;
using IdentityService.Shared.DTOs.Request.Store;

namespace IdentityService.API.Validators.Store;

public class StoreRequestValidator : AbstractValidator<StoreRequest>
{
    public StoreRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama toko wajib diisi.")
            .MaximumLength(256).WithMessage("Nama toko maksimal {MaxLength} karakter.");

        RuleFor(x => x.Address)
            .MaximumLength(1000).WithMessage("Alamat toko maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.Province)
            .MaximumLength(100).WithMessage("Provinsi maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Province));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Kota maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.District)
            .MaximumLength(100).WithMessage("Kecamatan maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.District));

        RuleFor(x => x.Rt)
            .MaximumLength(10).WithMessage("RT maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Rt));

        RuleFor(x => x.Rw)
            .MaximumLength(10).WithMessage("RW maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Rw));

        RuleFor(x => x.Phone)
            .MaximumLength(32).WithMessage("Nomor telepon maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}
