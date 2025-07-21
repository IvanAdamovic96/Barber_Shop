using System.Windows.Input;
using Hair.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Hair.Application.Auth.Commands;

public record DeleteCompanyOwnerCommand(string OwnerId) : IRequest<string>;

public class DeleteCompanyOwnerCommandHandler(UserManager<ApplicationUser> userManager) 
    : IRequestHandler<DeleteCompanyOwnerCommand, string>
{
    public async Task<string> Handle(DeleteCompanyOwnerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ownerToDelete = await userManager.FindByIdAsync(request.OwnerId);
            if (ownerToDelete == null)
            {
                throw new Exception("Owner does not exist.");
            }
            await userManager.DeleteAsync(ownerToDelete);
            return "Uspešno obrisan vlasnik!";
        }
        catch (Exception e)
        {
            throw new Exception("Greška prilikom brisanja vlasnika!");
        }
    }
}