﻿using System.Globalization;
using System.Text.RegularExpressions;
using Hair.Application.Common.Dto.Barber;
using Hair.Application.Common.Interfaces;
using Hair.Application.Common.Mappers;
using Hair.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hair.Infrastructure.Services;

public class BarberService (IHairDbContext dbContext) : IBarberService
{
    public async Task<BarberCreateDto> BarberCreateAsync(BarberCreateDto barberCreateDto, CancellationToken cancellationToken)
    {
        Guid barberId = Guid.NewGuid();
        
        
        var company = await dbContext.Companies.Where(x => x.Id == barberCreateDto.companyId).FirstOrDefaultAsync(cancellationToken);
        
        Barber barber = new Barber(
            barberCreateDto.barberName, 
            barberCreateDto.phoneNumber, 
            barberCreateDto.email,
            barberCreateDto.individualStartTime,
            barberCreateDto.individualEndTime)
        {
            BarberId = barberId
        };

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
        return new BarberCreateDto(barberSaved.BarberId,barber.BarberName, barber.PhoneNumber, barber.Email, barber.IndividualStartTime, barber.IndividualEndTime);
    }
    
    public static bool IsValidEmail(string email)
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
    
    public static bool IsValidSerbianPhoneNumber(string phoneNumber)
    {
        // Regex za više formata (prilagodite prema potrebi)
        string pattern = @"^\+?381\s?(6\d{1})\s?\d{7,8}$";
        return Regex.IsMatch(phoneNumber, pattern);
    }
}