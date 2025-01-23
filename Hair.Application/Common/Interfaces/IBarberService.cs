using Hair.Application.Common.Dto.Barber;

namespace Hair.Application.Common.Interfaces;

public interface IBarberService
{
    Task<BarberCreateDto> BarberCreateAsync(
        BarberCreateDto barberCreateDto, 
        CancellationToken cancellationToken
    );
}