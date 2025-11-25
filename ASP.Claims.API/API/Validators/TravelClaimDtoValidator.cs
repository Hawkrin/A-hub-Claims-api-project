namespace ASP.Claims.API.API.Validators;

using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.Resources;
using FluentValidation;

public class TravelClaimDtoValidator : AbstractValidator<TravelClaimDto>
{
    public TravelClaimDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Country)
          .IsInEnum()
          .WithMessage(ErrorMessages.ErrorMessage_CountryMissing_SE);

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage(ErrorMessages.ErrorMessage_StartDateMissing_SE);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage(ErrorMessages.ErrorMessage_EndDateBeforeStartDate_SE);

        RuleFor(x => x.ReportedDate)
            .Must((claim, reportedDate) => reportedDate <= claim.EndDate.AddDays(14))
            .WithMessage(ErrorMessages.ErrorMessage_ReportedTooLate_SE);

        RuleFor(x => x.IncidentType)
            .IsInEnum()
            .WithMessage(ErrorMessages.ErrorMessage_TypeOfIncidentMissing_SE);
    }
}
