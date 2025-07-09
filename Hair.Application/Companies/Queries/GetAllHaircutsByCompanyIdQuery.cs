using Hair.Application.Common.Dto.Company;
using Hair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hair.Application.Companies.Queries;

public record GetAllHaircutsByCompanyIdQuery(Guid CompanyId) : IRequest<List<HaircutDto>>;

public class GetAllHaircutsByCompanyIdQueryHandler(IHairDbContext dbContext) : IRequestHandler<GetAllHaircutsByCompanyIdQuery, List<HaircutDto>>
{
    public async Task<List<HaircutDto>> Handle(GetAllHaircutsByCompanyIdQuery request, CancellationToken cancellationToken)
    {
        var haircuts = await dbContext.Haircuts.Where(c => c.CompanyId == request.CompanyId).ToListAsync();
        var response = haircuts.Select(h => new HaircutDto(
            HaircutType: h.HaircutType,
            Price: h.Price,
            Duration: h.Duration,
            CompanyId: h.CompanyId
        )).ToList();
        return response;
    }
}