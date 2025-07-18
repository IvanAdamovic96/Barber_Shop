using Hair.Application.Common.Dto.Auth;
using Hair.Application.Common.Dto.Company;
using Hair.Application.Common.Interfaces;
using Hair.Domain.Entities;
using Hair.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hair.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
    IHairDbContext dbContext) : IAuthService
{
    public async Task<AuthResponseDto> Login(LoginDto loginDto, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || user.Email != loginDto.Email)
        {
            throw new Exception("Email adresa nije validna");
        }
        
        var password = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!password)
        {
            throw new Exception("Lozinka nije validna");
        }

        var ownersCompanies = await dbContext.ApplicationUserCompany
            .Where(i => i.ApplicationUserId == user.Id)
            .Select(c => c.CompanyId).ToListAsync();
        
        //var allOwnersCompanies = await dbContext.Companies.ToListAsync(cancellationToken);

        var roleName = Enum.GetName(typeof(Role), user.Role);
        var result = await signInManager.PasswordSignInAsync(user, loginDto.Password, isPersistent: false, lockoutOnFailure: false);
        return new AuthResponseDto(user.Email, roleName, ownersCompanies);
    }

    public async Task<AuthLevelDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken)
    {
        if (registerDto.Password != registerDto.ConfirmPassword)
            throw new Exception("Lozinke se ne podudaraju!");

        var existingUser = await userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
            throw new Exception("Ova email adresa već postoji!");

        var user = new ApplicationUser
        {

            UserName = registerDto.Email,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Role = Role.RegisteredUser
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception(errorMsg);
        }


        var roleName = Enum.GetName(typeof(Role), user.Role);
        await userManager.AddToRoleAsync(user, roleName);


        await signInManager.SignInAsync(user, isPersistent: false);

        return new AuthLevelDto(user.Email, roleName);

    }

    public async Task<CompanyOwnerResponseDto> CreateCompanyOwnerAsync(CompanyOwnerDto companyOwnerDto, CancellationToken cancellationToken)
    {
        try
        {

            var companyExistCheck = await dbContext.ApplicationUserCompany
                .Where(x => x.CompanyId == companyOwnerDto.CompanyId).FirstOrDefaultAsync();

            var companyOwnerExistCheck = await userManager.FindByEmailAsync(companyOwnerDto.Email);

            //provera da li taj user sto bi trebao da bude vlasnik postoji sa tim id-jem u medju tabeli 
            /* var ownerExists = await dbContext.ApplicationUserCompany.
                 Where(x => x.ApplicationUserId == companyOwnerExistCheck.Id).FirstOrDefaultAsync();*/

            if (companyOwnerExistCheck != null && companyOwnerExistCheck.Role == Role.CompanyOwner)
            {
                throw new Exception($"Vlasnik kompanije {companyOwnerDto.Email} već postoji!");
            }


            var appUser = new ApplicationUser()
            {
                UserName = companyOwnerDto.Email,
                Email = companyOwnerDto.Email,
                PhoneNumber = companyOwnerDto.PhoneNumber,
                FirstName = companyOwnerDto.FirstName,
                LastName = companyOwnerDto.LastName,
                Role = Role.CompanyOwner
            };

            var result = await userManager.CreateAsync(appUser, companyOwnerDto.Password);

            if (!result.Succeeded)
            {
                var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errorMsg);
            }

            //company.CompanyOwnerId = appUser.Id;
            //company.CompanyOwnerId = comapnyExistcheck.CompanyId.ToString();
            var appUserCompany = new ApplicationUserCompany()
            {
                CompanyId = companyOwnerDto.CompanyId,
                ApplicationUserId = appUser.Id,
                Id = new Guid()
            };
            await dbContext.ApplicationUserCompany.AddAsync(appUserCompany, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);


            return new CompanyOwnerResponseDto(
                appUser.Email,
                //appUser.CompanyId, 
                appUser.FirstName,
                appUser.LastName,
                appUser.PhoneNumber
            );
        }
        catch (Exception ex)
        {
            throw new Exception($"Greška prilikom kreiranja vlasnika: {ex.Message}");
        }

        /*
        try
        {

            var exists = await userManager.Users
                .Where(u => u.CompanyId == companyOwnerDto.CompanyId && u.Role == Role.CompanyOwner)
                .FirstOrDefaultAsync(cancellationToken);

            if (exists != null)
            {
                throw new Exception($"Kompanija {companyOwnerDto.CompanyId} već postoji");
            }

            var company = await dbContext.Companies
                .FirstOrDefaultAsync(c => c.Id == companyOwnerDto.CompanyId, cancellationToken);

            var appUser = new ApplicationUser()
            {
                UserName = companyOwnerDto.Email,
                Email = companyOwnerDto.Email,
                PhoneNumber = companyOwnerDto.PhoneNumber,
                FirstName = companyOwnerDto.FirstName,
                LastName = companyOwnerDto.LastName,
                CompanyId = companyOwnerDto.CompanyId,
                Role = Role.CompanyOwner
            };
            var result = await userManager.CreateAsync(appUser, companyOwnerDto.Password);

            if (!result.Succeeded)
            {
                var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errorMsg);
            }

            company.CompanyOwnerId = appUser.Id;
            await dbContext.SaveChangesAsync(cancellationToken);


            return new CompanyOwnerResponseDto
            (appUser.Email,
                appUser.CompanyId,
                appUser.FirstName,
                appUser.LastName,
                appUser.PhoneNumber);
        }
        catch (Exception ex)
        {
            throw new Exception($"Greška prilikom kreiranja vlasnika: {ex.Message}");
        }
        */
    }
    
    public async Task<AssignCompanyOwnerDto> AssignCompanyOwnerAsync(AssignCompanyOwnerDto assignCompanyOwnerDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var appUserCompany = new ApplicationUserCompany()
            {
                CompanyId = assignCompanyOwnerDto.CompanyId,
                ApplicationUserId = assignCompanyOwnerDto.ApplicationUserId.ToString(),
                Id = new Guid()
            };
            await dbContext.ApplicationUserCompany.AddAsync(appUserCompany, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

        }
        catch (Exception ex)
        {
            throw new Exception("Nije moguće dodeliti kompaniju vlasniku!", ex);
        }

        return assignCompanyOwnerDto;
    }
    
    public async Task<bool> CheckIfCompanyOwnerExistsAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.ApplicationUserCompany
            .Where(u => u.CompanyId == companyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (exists != null)
        {
            return true;
        }
        
        return false;
        
        /*
        var exists = await userManager.Users
            .Where(u => u.CompanyId == companyId && u.Role == Role.CompanyOwner)
            .FirstOrDefaultAsync(cancellationToken);

        if (exists != null)
        {
            return true;
            //hrow new Exception($"Company {companyId} already exists");
        }

        return false;
        */
    }
}