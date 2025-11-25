namespace ASP.Claims.API.API.Validators;

using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.Resources;
using FluentValidation;

public class PropertyClaimDtoValidator : AbstractValidator<PropertyClaimDto>
{
    public PropertyClaimDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Address)
           .NotEmpty()
           .WithMessage(ErrorMessages.ErrorMessage_AddressMissing_SE);

        RuleFor(x => x.PropertyDamageType)
            .IsInEnum()
            .WithMessage(ErrorMessages.ErrorMessage_TypeOfPropertyDamageMissing_SE);

        RuleFor(x => x.EstimatedDamageCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ErrorMessages.ErrorMessage_EstimatedDamageCostZero_SE);
    }
}