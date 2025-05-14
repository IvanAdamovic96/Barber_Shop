using Hair.Application.Common.Dto.Barber;
using Hair.Application.Common.Dto.Company;
using Hair.Application.Common.Interfaces;
using Hair.Application.Common.Mappers;
using Hair.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hair.Infrastructure.Services;

public class CompanyService (IHairDbContext dbContext) : ICompanyService
{
    
    public async Task<List<string>> UploadImageAsync([FromForm] IList<IFormFile> images)
    {
        if (images == null || images.Count == 0)
            throw new ArgumentException("Image file is empty.");

        // folder: wwwroot/images/companies
        var folderName = Path.Combine("wwwroot", "images", "companies");
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), folderName);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var urls = new List<string>();
        for (int i = 0; i < images.Count; i++)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(images[i].FileName);
            var filePath = Path.Combine(folderPath, uniqueFileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await images[i].CopyToAsync(stream);
            }
            var fullUrl = $"http://localhost:5045/images/companies/{uniqueFileName}"; // Change localhost:5000 if necessary
            urls.Add(fullUrl);
            //return fullUrl;
        }
        
        //var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(images.FileName);
        // Return full URL path
        return urls;
    }
    
    
    
    public async Task<CompanyCreateDto> CreateCompanyAsync(string companyName, IList<IFormFile?> image,
        CancellationToken cancellationToken)
    {
        var x = await dbContext.Companies.Where(c => c.CompanyName == companyName)
            .FirstOrDefaultAsync();
        if (x is  not null)
        {
            throw new Exception($"Company {companyName} already exists");
        }

        IList<string?> imageUrl = null;
        for (int i = 0; i < image.Count; i++)
        {
            if (image != null)
            {
                imageUrl = await UploadImageAsync(image);
            }
        }
        
        Company company = new Company(companyName);
        company.AddImage(imageUrl);
        
        //var companySaved = companyCreate.FromCreateDtoToEntity();
        
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CompanyCreateDto(company.CompanyName, company.ImagesUrl);
    }
    
    
    
    
    

    public async Task<List<BarberFullDetailsDto>> CompanyDetailsByIdAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var barbers = await dbContext.Barbers.Include(x => x.Company)
            .Where(x => x.Company.Id == companyId)
            .ToListAsync(cancellationToken);


        if (barbers == null || barbers.Count == 0)
        {
            return new List<BarberFullDetailsDto>();
        }

        return barbers.Select(barber =>
            new BarberFullDetailsDto(barber.BarberId, barber.BarberName, barber.Company.CompanyName)).ToList();

        //return barbers.Select(barber => new BarberDetailsDto(barber.BarberName, barber.Company?.CompanyName ?? "No company")).ToList();
    }

    
    
    
    
    public async Task<List<CompanyDetailsDto>> GetAllCompaniesAsync(CompanyDetailsDto companyDetailsDto, CancellationToken cancellationToken)
    {
        var companies = await dbContext.Companies.ToListAsync(cancellationToken);

        var result = companies.Select(x => new CompanyDetailsDto()
        {
            CompanyId = x.Id,
            CompanyName = x.CompanyName,
            ImageUrl = x.ImagesUrl
        }).ToList();
        return result;
    }
}