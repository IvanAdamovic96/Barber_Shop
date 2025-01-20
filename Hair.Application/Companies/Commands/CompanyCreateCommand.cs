using Hair.Application.Common.Dto.Company;
using Hair.Application.Common.Interfaces;
using Hair.Application.Common.Mappers;
using Hair.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hair.Application.Companies.Commands;

public record CompanyCreateCommand(CompanyCreateDto Company) : IRequest<CompanyCreateDto?>;

public class CompanyCreateCommandHandler(IHairDbContext dbContext)
    : IRequestHandler<CompanyCreateCommand, CompanyCreateDto?>
{
    public async Task<CompanyCreateDto?> Handle(CompanyCreateCommand request, CancellationToken cancellationToken)
    {
        Guid companyId = Guid.NewGuid();
        Company company = new Company(request.Company.CompanyName)
        {
            Id = companyId,
        };
        var x = await dbContext.Companies.Where(c => c.CompanyName == request.Company.CompanyName)
            .FirstOrDefaultAsync();
        if (x is  not null)
        {
            throw new Exception($"Company {request.Company.CompanyName} already exists");
        }
        var companySaved = request.Company.FromCreateDtoToEntity();
        dbContext.Companies.Add(companySaved);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CompanyCreateDto(company.CompanyName);

    }
}









