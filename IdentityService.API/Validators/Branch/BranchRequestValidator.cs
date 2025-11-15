using FluentValidation;
using IdentityService.Shared.DTOs.Request.Branch;

namespace IdentityService.API.Validators.Branch;

public class BranchRequestValidator : AbstractValidator<BranchRequest>
{
    public BranchRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nama cabang wajib diisi.")
            .MaximumLength(50)
            .WithMessage("Nama cabang tidak boleh lebih dari 50 karakter.");

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage("Kode cabang tidak boleh lebih dari 50 karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Code));

        RuleFor(x => x.Address)
            .MaximumLength(4000)
            .WithMessage("Alamat tidak boleh lebih dari 4000 karakter.")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage(
                "Nomor telepon tidak valid. Nomor harus dimulai dengan 08 dan terdiri dari 10 hingga 15 digit.");
    }
}