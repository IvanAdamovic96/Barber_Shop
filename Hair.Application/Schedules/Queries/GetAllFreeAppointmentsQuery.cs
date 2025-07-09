using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using Hair.Application.Common.Dto.Schedule;
using Hair.Application.Common.Interfaces;
using Hair.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hair.Application.Schedules.Queries;

public record GetAllFreeAppointmentsQuery(DateTime SelectedDate, Guid BarberId): IRequest<List<FreeAppointmentsCheckDto>>;

public class GetAllFreAppointmentsHandler(IHairDbContext dbContext) : IRequestHandler<GetAllFreeAppointmentsQuery, List<FreeAppointmentsCheckDto>>
{
    public async Task<List<FreeAppointmentsCheckDto>> Handle(GetAllFreeAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var occupiedAppointments = await dbContext.Appointments
            .Where(x => x.Time.Date == request.SelectedDate.Date && x.Barberid == request.BarberId)
            .ToListAsync(cancellationToken);

        var occupiedTimes = occupiedAppointments
            .Select(app => app.Time) // Čuvamo puni DateTime sa vremenom
            .ToList();
        
        
        var barberWorkTime = await dbContext.Barbers
            .Where(x=> x.BarberId == request.BarberId)
            .FirstOrDefaultAsync(cancellationToken);
        
        var startTime = request.SelectedDate.Date.AddHours(barberWorkTime.IndividualStartTime.Hours)
                                                 .AddMinutes(barberWorkTime.IndividualStartTime.Minutes);

        var endTime = request.SelectedDate.Date.AddHours(barberWorkTime.IndividualEndTime.Hours)
                                               .AddMinutes(barberWorkTime.IndividualEndTime.Minutes);
        
        
        var list = new List<DateTime>();

        
        for (var i = startTime; i <= endTime; i = i.AddMinutes(30))
        {
            list.Add(i);
        }
        
        list.RemoveAll(x => occupiedTimes.Contains(x));
        var list2 = list.Select(time => new FreeAppointmentsCheckDto(request.BarberId, time)).ToList();

        return list2;

    }
}