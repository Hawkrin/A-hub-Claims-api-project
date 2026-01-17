namespace ASP.Claims.API.API.Validators;

using ASP.Claims.API.API.DTOs;
using FluentValidation;
using System.Text.RegularExpressions;

public partial class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$")]
    private static partial Regex PasswordStrengthRegex();

    public RegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(50)
            .WithMessage("Username cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Username can only contain letters, numbers, and underscores.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .Must(password => PasswordStrengthRegex().IsMatch(password))
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one digit.");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid role specified.");
    }
}
