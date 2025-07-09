using Hair.Application.Common.Dto.Auth;
using Hair.Application.Common.Interfaces;
using MediatR;

namespace Hair.Application.Auth.Commands;

public record CreateCompanyOwnerCommand(CompanyOwnerDto CompanyOwnerDto) : IRequest<CompanyOwnerResponseDto>;

public class CreateCompanyOwnerCommandHandler(IAuthService authService) : IRequestHandler<CreateCompanyOwnerCommand, CompanyOwnerResponseDto>
{
    public async Task<CompanyOwnerResponseDto> Handle(CreateCompanyOwnerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var companyOwner = await authService.CreateCompanyOwnerAsync(request.CompanyOwnerDto, cancellationToken);
            return companyOwner;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
}