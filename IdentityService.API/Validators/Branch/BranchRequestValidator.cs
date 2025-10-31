using FluentValidation;
using IdentityService.Shared.DTOs.Request.Branch;

namespace IdentityService.API.Validators.Branch;

public class BranchRequestValidator : AbstractValidator<BranchRequest>
{
    public BranchRequestValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithMessage("Store ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Branch name is required")
            .MaximumLength(100)
            .WithMessage("Branch name cannot exceed 100 characters");

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage("Branch code cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Code));

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("Address cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.Phone)
            .Matches(@"^[\+]?[1-9][\d]{0,15}$")
            .WithMessage("Please enter a valid phone number")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}
