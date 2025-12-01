namespace ASP.Claims.API.API.Validators;

using ASP.Claims.API.API.DTOs.Claims;
using ASP.Claims.API.Resources;
using FluentValidation;
using System.Text.RegularExpressions;

public partial class VehicleClaimDtoValidator : AbstractValidator<VehicleClaimDto>
{
    /// <summary>
    /// ABC123: 3 letters + 3 digits
    /// </summary>
    [GeneratedRegex(@"^[A-Za-z]{3}\d{3}$")]
    private static partial Regex ABC123Regex();

    /// <summary>
    /// ABC12D: 3 letters + 2 digits + 1 letter
    /// </summary>
    [GeneratedRegex(@"^[A-Za-z]{3}\d{2}[A-Za-z]{1}$")]
    private static partial Regex ABC12DRegex();

    public VehicleClaimDtoValidator()
    {
        RuleFor(x => x.RegistrationNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ErrorMessages.ErrorMessage_RegistrationNumberMissing_SE)
            .Must(value => ABC123Regex().IsMatch(value) || ABC12DRegex().IsMatch(value))
            .WithMessage(ErrorMessages.ErrorMessage_RegistrationNumberFormat_SE);

        RuleFor(x => x.PlaceOfAccident)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}