using FluentValidation;
using Hair.Application.Common.Dto.Company;

namespace Hair.Application.Common.Validators;

public class CompanyCreateDtoValidator : AbstractValidator<CompanyCreateDto>
{
    public CompanyCreateDtoValidator()
    {
        RuleFor(x => x.CompanyName).NotNull();
        
    }
}