using FluentValidation;
using Hair.Application.Common.Dto.Schedule;
using Hair.Application.Common.Interfaces;
using Hair.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hair.Infrastructure.Services;

public class ScheduleService(IHairDbContext dbContext, 
                            IBarberService barberService, 
                            INotificationService notificationService) : IScheduleService
{
    public async Task<ScheduleAppointmentResponseDto> CreateScheduleAppointmentAsync(ScheduleAppointmentCreateDto schedule,
        CancellationToken cancellationToken)
    {
        bool isWithinWorkHours = await IsWithinBarberWorkHours(schedule, cancellationToken);
        if (!isWithinWorkHours)
        {
            throw new Exception("Barber is not available during the requested time.");
        }
        
        DateTime normalizedTime = new DateTime(
            schedule.time.Year,
            schedule.time.Month,
            schedule.time.Day,
            schedule.time.Hour,
            schedule.time.Minute,
            0, 
            0, 
            schedule.time.Kind 
        );

        var isAppointmentAvailable = await IsAppointmentAvailable(schedule.barberId, normalizedTime, cancellationToken);
        if (isAppointmentAvailable)
        {
            throw new ValidationException("Schedule appointment already exists.");
        }
        
        var haircut = await dbContext.Haircuts.Where(x=> x.Id == schedule.haircutId).FirstOrDefaultAsync(cancellationToken);
        if (haircut == null)
        {
            throw new ValidationException("Haircut not found.");
        }
        decimal haircutDuration = haircut.Duration;
        int requiredSlots = (int) Math.Ceiling(haircutDuration / 30m);
        
        var allFreeAppointments = await GetAllFreeAppointmentsAsync(schedule.time.Date, schedule.barberId, cancellationToken);
        var freeTimes = allFreeAppointments.Select(dto => dto.dateAndTime).ToHashSet();
        List<DateTime> bookedAppointmentsTimes = new List<DateTime>();
        bool foundConsecutiveSlots = false;
        
        DateTime currentCheckTime = normalizedTime;
        if (!freeTimes.Contains(currentCheckTime))
        {
            throw new ValidationException("The requested start time is not available.");
        }
        bookedAppointmentsTimes.Add(currentCheckTime);
        
        /*
        var test = new List<DateTime>();
        bool hasAllSlots = true;
        var requiredTimes = new List<DateTime>();
        */
        for (int i = 1; i < requiredSlots; i++)
        {
            currentCheckTime = normalizedTime.AddMinutes(i * 30);
            var barber = await dbContext.Barbers.FirstOrDefaultAsync(x=> x.BarberId == schedule.barberId, cancellationToken);
            if (barber == null)
            {
                throw new ValidationException("Barber not found for work hour check.");
            }

            if (!freeTimes.Contains(currentCheckTime))
            {
                foundConsecutiveSlots = false;
                break;
            }
            bookedAppointmentsTimes.Add(currentCheckTime);
            foundConsecutiveSlots = true;
            /*
            var checkTime = schedule.time.AddMinutes(i * 30);
            requiredTimes.Add(checkTime);

            if (freeTimes.Contains(checkTime))
            {
                test.Add(checkTime);
            }
            */
        }

        if (!foundConsecutiveSlots || bookedAppointmentsTimes.Count != requiredSlots)
        {
            throw new ValidationException($"Not enough consecutive appointments available for " +
                                          $"a {haircutDuration}-minute haircut starting at {normalizedTime:HH:mm}.");
        }

        /*
        if (!hasAllSlots)
        {
            throw new ValidationException("Schedule appointment doesn't have enough slots.");
        }
        */
        

        try
        {
            AnonymousUser anonymousUser = new AnonymousUser(
                schedule.firstName,
                schedule.lastName,
                schedule.email,
                schedule.phoneNumber
            );
            dbContext.AnonymousUser.Add(anonymousUser);
            
            foreach (var timeSlot in bookedAppointmentsTimes)
            {
                Appointment appointment = new Appointment(timeSlot, schedule.barberId);
                appointment.SetHaircutName(haircut.HaircutType);
                dbContext.Appointments.Add(appointment);
            }
            
            /*
            Appointment appointment = new Appointment(schedule.time,schedule.barberId);
            appointment.SetHaircutName(haircut.HaircutType);

            appointment.SetTime(new DateTime(
                appointment.Time.Year,
                appointment.Time.Month,
                appointment.Time.Day,
                appointment.Time.Hour,
                appointment.Time.Minute,
                0, // Seconds set to 0
                0, // Milliseconds set to 0
                DateTimeKind.Utc
            ));
            */
            
            //dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new ScheduleAppointmentResponseDto(anonymousUser.FirstName, anonymousUser.LastName, anonymousUser.Email,
                anonymousUser.PhoneNumber, bookedAppointmentsTimes[0], schedule.barberId, haircut.HaircutType);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error booking appointment: {ex.Message}");
            throw new Exception(ex.Message);
        }
    }

    


    public async Task<List<GetAllSchedulesByBarberIdDto>> GetAllSchedulesByBarberIdAsync(
        Guid barberId, CancellationToken cancellationToken)
    {
        var appointments = await dbContext.Appointments.Where(x => barberId == x.Barberid).ToListAsync();
        var result = appointments.Select(appointment => new GetAllSchedulesByBarberIdDto
        {
            barberId = appointment.Barberid,
            time = appointment.Time
        }).ToList();
        return result;
    }

    public async Task<List<FreeAppointmentsCheckDto>> GetAllFreeAppointmentsAsync(DateTime selectedDate, Guid barberId, CancellationToken cancellationToken)
    {
        var occupiedAppointments = await dbContext.Appointments
            .Where(x => x.Time.Date == selectedDate.Date && x.Barberid == barberId)
            .ToListAsync(cancellationToken);

        var occupiedTimes = occupiedAppointments
            .Select(app => app.Time) // Čuvamo puni DateTime sa vremenom
            .ToList();
        
        var barberWorkTime = await dbContext.Barbers
            .Where(x=> x.BarberId == barberId)
            .FirstOrDefaultAsync(cancellationToken);

        var startTime = selectedDate.Date.AddHours(barberWorkTime.IndividualStartTime.Hours)
            .AddMinutes(barberWorkTime.IndividualStartTime.Minutes);

        var endTime = selectedDate.Date.AddHours(barberWorkTime.IndividualEndTime.Hours)
            .AddMinutes(barberWorkTime.IndividualEndTime.Minutes);
        
        var list = new List<DateTime>();
        
        for (var i = startTime; i < endTime; i = i.AddMinutes(30))
        {
            list.Add(i);
        }

        list.RemoveAll(x => occupiedTimes.Contains(x));
        var list2 = list.Select(time => new FreeAppointmentsCheckDto(barberId, time)).ToList();

        return list2;
    }

    
    
    
    
    
    
    private async Task<bool> IsWithinBarberWorkHours(ScheduleAppointmentCreateDto schedule, CancellationToken cancellationToken)
    {
        var barber = await dbContext.Barbers.FirstOrDefaultAsync(x => x.BarberId == schedule.barberId, cancellationToken);
        if (barber == null) return false;

        var start = barber.IndividualStartTime;
        var end = barber.IndividualEndTime;
        return schedule.time.TimeOfDay >= start && schedule.time.TimeOfDay < end;
    }

    private async Task<bool> IsAppointmentAvailable(Guid barberId, DateTime time, CancellationToken cancellationToken)
    {
        var occupied = await dbContext.Appointments.Where(x => x.Barberid == barberId && x.Time == time)
            .FirstOrDefaultAsync(cancellationToken);
        return occupied != null;
    }
}