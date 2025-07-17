using System.Globalization;
using System.Text.RegularExpressions;
using Hair.Application.Common.Dto.Barber;
using Hair.Application.Common.Interfaces;
using Hair.Application.Common.Mappers;
using Hair.Domain.Entities;
using Hair.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hair.Infrastructure.Services;

public class BarberService (IHairDbContext dbContext, UserManager<ApplicationUser> userManager) : IBarberService
{
    public async Task<BarberResponseDto> BarberCreateAsync(BarberCreateDto barberCreateDto, CancellationToken cancellationToken)
    {
        var company = await dbContext.Companies.Where(x => x.Id == barberCreateDto.companyId).FirstOrDefaultAsync(cancellationToken);
        
        var applicationUser = new ApplicationUser
        {
            UserName = barberCreateDto.email,
            Email = barberCreateDto.email,
            PhoneNumber = barberCreateDto.phoneNumber,
            FirstName = barberCreateDto.barberName,
            LastName = barberCreateDto.barberName,
            Role = Role.Barber
        };
        var result = await userManager.CreateAsync(applicationUser, barberCreateDto.password);
        if (!result.Succeeded)
        {
            throw new Exception(
                "Failed to create identity user: " + string.Join(", ", result.Errors.Select(e => e.Description))
            );
        }


        Barber barber = new Barber(
            barberCreateDto.barberName,
            barberCreateDto.phoneNumber,
            barberCreateDto.email,
            barberCreateDto.individualStartTime,
            barberCreateDto.individualEndTime
        );

        if (!IsValidEmail(barberCreateDto.email))
        {
            throw new Exception("Invalid email address");
        }

        if (!IsValidSerbianPhoneNumber(barberCreateDto.phoneNumber))
        {
            throw new Exception("Invalid phone number");
        }
        var barberSaved = barberCreateDto.FromCreateDtoToEntityBarber().AddBarberCompany(company);
        barberSaved.SetApplicationUserId(applicationUser.Id);
        dbContext.Barbers.Add(barberSaved);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new BarberResponseDto(barber.BarberId, barber.BarberName, barber.PhoneNumber, barber.Email,
            barber.IndividualStartTime, barber.IndividualEndTime);
        
        /*
        var company = await dbContext.Companies.Where(x => x.Id == barberCreateDto.companyId).FirstOrDefaultAsync(cancellationToken);

        Barber barber = new Barber(
            barberCreateDto.barberName,
            barberCreateDto.phoneNumber,
            barberCreateDto.email,
            barberCreateDto.individualStartTime,
            barberCreateDto.individualEndTime)
            .AddBarberCompany(company);

        if (!IsValidEmail(barberCreateDto.email))
        {
            throw new Exception("Invalid email address");
        }

        if (!IsValidSerbianPhoneNumber(barberCreateDto.phoneNumber))
        {
            throw new Exception("Invalid phone number");
        }

        var barberSaved = barberCreateDto.FromCreateDtoToEntityBarber().AddBarberCompany(company);
        dbContext.Barbers.Add(barberSaved);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new BarberCreateDto(barberSaved.BarberId,barber.BarberName, barber.PhoneNumber,
                                barber.Email, barber.IndividualStartTime, barber.IndividualEndTime);
        */
    }
    

    public async Task<List<BarberDetailsDto>> GetAllBarbersAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var barbers = await dbContext.Barbers.Where(x => x.Company.Id == companyId)
            .Select(x=> new BarberDetailsDto(x.BarberId, x.BarberName, x.Company.CompanyName))
            .ToListAsync();

        return barbers;
    }

    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                string domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException e)
        {
            return false;
        }
        catch (ArgumentException e)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.(com|net|org|gov|rs|ac.rs)$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
    
    public bool IsValidSerbianPhoneNumber(string phoneNumber)
    {
        string pattern = @"^\+?381\s?(6\d{1})\s?\d{6,7}$";
        return Regex.IsMatch(phoneNumber, pattern);
    }
}