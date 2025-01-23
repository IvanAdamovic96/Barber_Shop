﻿using Hair.Application.Common.Dto.Barber;
using Hair.Application.Common.Dto.Company;
using Hair.Application.Common.Interfaces;
using Hair.Application.Common.Mappers;
using Hair.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hair.Infrastructure.Services;

public class CompanyService (IHairDbContext dbContext) : ICompanyService
{
    public async Task<CompanyCreateDto> CreateCompanyAsync(CompanyCreateDto companyCreate,
        CancellationToken cancellationToken)
    {
        Guid companyId = Guid.NewGuid();
        Company company = new Company(companyCreate.CompanyName)
        {
            Id = companyId,
        };
        var x = await dbContext.Companies.Where(c => c.CompanyName == companyCreate.CompanyName)
            .FirstOrDefaultAsync();
        if (x is  not null)
        {
            throw new Exception($"Company {companyCreate.CompanyName} already exists");
        }
        var companySaved = companyCreate.FromCreateDtoToEntity();
        dbContext.Companies.Add(companySaved);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CompanyCreateDto(company.CompanyName);
    }

    public async Task<List<BarberDetailsDto>> CompanyDetailsByIdAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var barbers = await dbContext.Barbers.Include(x => x.Company)
            .Where(x => x.Company.Id == companyId)
            .ToListAsync(cancellationToken);


        if (barbers == null || barbers.Count == 0)
        {
            return new List<BarberDetailsDto>();
        }


        return barbers.Select(barber =>
            new BarberDetailsDto(barber.BarberName, barber.Company?.CompanyName ?? "No company")).ToList();
    }
}