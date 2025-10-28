using FluentValidation;
using IdentityService.Shared.DTOs.Request.Branch;

namespace IdentityService.API.Validators.Branch;

public class BranchRequestValidator : AbstractValidator<BranchRequest>
{
    public BranchRequestValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("StoreId wajib diisi.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name wajib diisi.")
            .MaximumLength(256).WithMessage("Name maksimal {MaxLength} karakter.");

        RuleFor(x => x.Code)
            .MaximumLength(100).WithMessage("Code maksimal {MaxLength} karakter.");

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

        RuleFor(x => x.Phone)
            .MaximumLength(32).WithMessage("Phone maksimal {MaxLength} karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}

