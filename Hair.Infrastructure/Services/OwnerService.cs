using Hair.Application.Common.Dto.Company;
using Hair.Application.Common.Interfaces;
using Hair.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hair.Infrastructure.Services;

public class OwnerService(IHairDbContext dbContext, ILogger<OwnerService> _logger) : IOwnerService
{
    public async Task<string> CreateHaircutByOwner(HaircutDto haircutDto, CancellationToken cancellationToken)
    {
        try
        {
            var newHaircut = new Haircut(haircutDto.Duration,haircutDto.HaircutType,haircutDto.Price);
            var company = await dbContext.Companies.Where(x => x.Id == haircutDto.CompanyId).FirstOrDefaultAsync();
            newHaircut.AddCompanyCompany(company);
            dbContext.Haircuts.Add(newHaircut);
            await dbContext.SaveChangesAsync(cancellationToken);
            return "Uspešno kreirana usluga!";
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            Console.WriteLine(e);
            throw new Exception("Greška prilikom kreiranja usluge! ", e);
        }
    }

    public async Task<string> UpdateHaircutAsync(HaircutResponseDto haircutResponseDto, CancellationToken cancellationToken)
    {
        try
        {
            var haircutToUpdate = await dbContext.Haircuts.FirstOrDefaultAsync(x => x.Id == haircutResponseDto.HaircutId,
                cancellationToken);
            if (haircutToUpdate == null)
            {
                throw new Exception("Nije pronadjena usluga za izmenu!");
            }
            haircutToUpdate.UpdateHaircutType(haircutResponseDto.HaircutType);
            haircutToUpdate.UpdatePrice(haircutResponseDto.Price);
            haircutToUpdate.UpdateDuration(haircutResponseDto.Duration);
            await dbContext.SaveChangesAsync(cancellationToken);
        
            return "Uspešno izmenjena usluga!";
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    public async Task<string> DeleteHaircutByHaircutId(Guid haircutId, CancellationToken cancellationToken)
    {
        try
        {
            var haircutToDelete = await dbContext.Haircuts.FirstOrDefaultAsync(x => x.Id == haircutId, cancellationToken);
            if (haircutToDelete == null)
            {
                throw new Exception("Nije pronadjena usluga za brisanje!");
            }
            dbContext.Haircuts.Remove(haircutToDelete);
            await dbContext.SaveChangesAsync(cancellationToken);

            return "Uspešno obrisana usluga!";
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }
}