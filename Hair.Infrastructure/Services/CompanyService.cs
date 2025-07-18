using Hair.Application.Common.Dto.Barber;
using Hair.Application.Common.Dto.Company;
using Hair.Application.Common.Interfaces;
using Hair.Application.Common.Mappers;
using Hair.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hair.Infrastructure.Services;

public class CompanyService (IHairDbContext dbContext, IWebHostEnvironment hostEnvironment) : ICompanyService
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
            var fullUrl = $"http://localhost:5045/images/companies/{uniqueFileName}";
            urls.Add(fullUrl);
        }
        
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
        imageUrl = await UploadImageAsync(image);
        /*
        for (int i = 0; i < image.Count; i++)
        {
            if (image is not null)
            {
                
            }
        }*/
        
        Company company = new Company(companyName);
        company.AddImage(imageUrl);
        
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CompanyCreateDto(company.CompanyName, company.ImageUrl);
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
    }
    
    public async Task<List<CompanyDetailsDto>> GetAllCompaniesAsync(CompanyDetailsDto companyDetailsDto, CancellationToken cancellationToken)
    {
        var companies = await dbContext.Companies.ToListAsync(cancellationToken);

        var result = companies.Select(x => new CompanyDetailsDto()
        {
            CompanyId = x.Id,
            CompanyName = x.CompanyName,
            ImageUrl = x.ImageUrl
        }).ToList();
        return result;
    }

    public async Task<CompanyDetailsDto> GetCompanyDetailsById(Guid CompanyId, CancellationToken cancellationToken)
    {
        var company = await dbContext.Companies.Where(c => c.Id == CompanyId).FirstOrDefaultAsync(cancellationToken);
        var toReturnCompanyDetails = new CompanyDetailsDto(company.Id, company.CompanyName, company.ImageUrl);
        return toReturnCompanyDetails;
    }

    public async Task<string> DeleteCompanyByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken)
    {
        try
        {
            var companyToDelete = await dbContext.Companies.Where(c => c.Id == companyId).FirstOrDefaultAsync(cancellationToken);
        
            if (companyToDelete == null)
            {
                throw new Exception($"Company {companyId} does not exist");
            }

            if (companyToDelete.ImageUrl != null && companyToDelete.ImageUrl.Any())
            {
                foreach (var image in companyToDelete.ImageUrl)
                {
                    if (string.IsNullOrWhiteSpace(image)) continue;

                    var relativePath = image.Replace("http://localhost:5045/", "");

                    if (relativePath.StartsWith("images/companies/", StringComparison.OrdinalIgnoreCase))
                    {
                        var filePath = Path.Combine(hostEnvironment.WebRootPath, relativePath);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        else
                        {
                            throw new Exception($"Image file {filePath} does not exist");
                        }
                    }
                    else
                    {
                        throw new Exception($"Error deleting file {image} for property with id: {companyId}");
                    }
                }
            }
            
            dbContext.Companies.Remove(companyToDelete);
            await dbContext.SaveChangesAsync(cancellationToken);
            return $"Successfully deleted company with {companyId}";
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to delete company {companyId}", e);
        }
        
    }
    
    
    
    
}