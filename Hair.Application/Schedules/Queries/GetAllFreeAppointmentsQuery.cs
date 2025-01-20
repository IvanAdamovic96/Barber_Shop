using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using Hair.Application.Common.Dto.Schedule;
using Hair.Application.Common.Interfaces;
using Hair.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hair.Application.Schedules.Queries;

public record GetAllFreeAppointmentsQuery(TimeSpan selectedDate, Guid barberId): IRequest<List<FreeAppointmentsCheckDto>>;

public class GetAllFreAppointmentsHandler(IHairDbContext dbContext) : IRequestHandler<GetAllFreeAppointmentsQuery, List<FreeAppointmentsCheckDto>>
{
    public async Task<List<FreeAppointmentsCheckDto>> Handle(GetAllFreeAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var occupiedAppointment = await dbContext.Appointments.
            Where(x => x.Time.TimeOfDay == request.selectedDate && x.Barberid == request.barberId).ToListAsync(cancellationToken);

        var occupiedTimes = occupiedAppointment
            .Select(app => app.Time.TimeOfDay)
            .ToList();
        
        
        
        var barberWorkTime = await dbContext.Barbers
            .Where(x=> x.BarberId == request.barberId)
            .FirstOrDefaultAsync(cancellationToken);
        
        var startTime = barberWorkTime.IndividualStartTime.Value;
        var endTime = barberWorkTime.IndividualEndTime.Value;
        
        var list = new List<TimeSpan>();

        for (var i = startTime; i <= endTime; i = i + new TimeSpan(0, 30, 0))
        {
            foreach (var time in occupiedTimes)
            {
                if (!(time == i))
                {
                    list.Add(i);
                }
            }
        }

        var list2 = list.Select(time => new FreeAppointmentsCheckDto(request.barberId, time)).ToList();


        return list2;
        

    }
}