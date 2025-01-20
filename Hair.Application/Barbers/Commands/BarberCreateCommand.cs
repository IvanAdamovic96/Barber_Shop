using Hair.Application.Common.Dto.Barber;
using Hair.Application.Common.Interfaces;
using Hair.Application.Common.Mappers;
using Hair.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hair.Application.Barbers.Commands;

public record BarberCreateCommand(BarberCreateDto Barber) : IRequest<BarberCreateDto?>;

public class BarberCreateCommandHandler(IHairDbContext dbContext) : IRequestHandler<BarberCreateCommand, BarberCreateDto?>
{
    public async Task<BarberCreateDto?> Handle(BarberCreateCommand request, CancellationToken cancellationToken)
    {
        Guid barberId = Guid.NewGuid();
        
        
        var company = await dbContext.Companies.Where(x => x.Id == request.Barber.companyId).FirstOrDefaultAsync(cancellationToken);
        
        Barber barber = new Barber(request.Barber.barberName, request.Barber.phoneNumber, request.Barber.email,request.Barber.individualStartTime,request.Barber.individualEndTime)
        {
            BarberId = barberId
        };

        var barberSaved = request.Barber.FromCreateDtoToEntityBarber().AddBarberCompany(company);
        dbContext.Barbers.Add(barberSaved);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new BarberCreateDto(barberSaved.BarberId,barber.BarberName, barber.PhoneNumber, barber.Email, barber.IndividualStartTime, barber.IndividualEndTime);
    }
}