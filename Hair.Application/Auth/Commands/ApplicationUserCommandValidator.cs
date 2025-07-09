using FluentValidation;
using Hair.Application.Common.Dto.Auth;

namespace Hair.Application.Auth.Commands;

public class ApplicationUserCommandValidator : AbstractValidator<RegisterCommand>
{
    public ApplicationUserCommandValidator(IValidator<RegisterDto> registerDtoValidator)
    {
        RuleFor(x => x.Register).SetValidator(registerDtoValidator);
    }
}