using FluentValidation;
using IdentityService.Shared.DTOs.Request.UserBranch;

namespace IdentityService.API.Validators.UserBranch;

public class UserBranchRequestValidator : AbstractValidator<UserBranchRequest>
{
    public UserBranchRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id wajib diisi.");

        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch Id wajib diisi.");
    }
}
