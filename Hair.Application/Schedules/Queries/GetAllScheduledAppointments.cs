using Hair.Application.Common.Dto.Schedule;
using Hair.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hair.Application.Schedules.Queries;

public record GetAllScheduledAppointments(Guid barberId) : IRequest<IList<GetAllSchedulesByBarberIdDto>>;

public class ProductDetailsQueryHandler(IHairDbContext dbContext) : IRequestHandler<GetAllScheduledAppointments, IList<GetAllSchedulesByBarberIdDto>>
{
    public async Task<IList<GetAllSchedulesByBarberIdDto>> Handle(GetAllScheduledAppointments request, CancellationToken cancellationToken)
    {
        var appointments = await dbContext.Appointments.Where(x => request.barberId == x.Barberid).ToListAsync();
        var result = appointments.Select(appointment => new GetAllSchedulesByBarberIdDto
        {
            barberId = appointment.Barberid,
            time = appointment.Time
        }).ToList();
        return result;
    }
}
