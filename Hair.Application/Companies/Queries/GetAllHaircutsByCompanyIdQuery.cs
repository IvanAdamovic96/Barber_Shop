using Hair.Application.Common.Dto.Company;
using Hair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hair.Application.Companies.Queries;

public record GetAllHaircutsByCompanyIdQuery(Guid CompanyId) : IRequest<List<HaircutResponseDto>>;

public class GetAllHaircutsByCompanyIdQueryHandler(IHairDbContext dbContext) : IRequestHandler<GetAllHaircutsByCompanyIdQuery, List<HaircutResponseDto>>
{
    public async Task<List<HaircutResponseDto>> Handle(GetAllHaircutsByCompanyIdQuery request, CancellationToken cancellationToken)
    {
        var haircuts = await dbContext.Haircuts.Where(c => c.CompanyId == request.CompanyId).ToListAsync();
        var response = haircuts.Select(h => new HaircutResponseDto(
            HaircutId: h.Id,
            HaircutType: h.HaircutType,
            Price: h.Price,
            Duration: h.Duration,
            CompanyId: h.CompanyId
        )).ToList();
        return response;
    }
}