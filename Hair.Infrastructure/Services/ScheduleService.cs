using Hair.Application.Common.Dto.Schedule;
using Hair.Application.Common.Interfaces;
using Hair.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hair.Infrastructure.Services;

public class ScheduleService(IHairDbContext dbContext, 
                            IBarberService barberService, 
                            INotificationService notificationService) : IScheduleService
{
    public async Task<ScheduleAppointmentCreateDto> CreateScheduleAppointmentAsync(ScheduleAppointmentCreateDto schedule,
        CancellationToken cancellationToken)
    {
        var s = await dbContext.Barbers.Where(x => schedule.barberId == x.BarberId).FirstOrDefaultAsync();
        var start = s.IndividualStartTime.Value;
        var end = s.IndividualEndTime.Value;
        //var usedAppointments = await dbContext.Appointments.Where(x => x.Time == request.Schedule.time).ToListAsync();
        
        
        
        
        DateTime requestedTime = schedule.time;
        var z = requestedTime.TimeOfDay;
        var minutes = requestedTime.Minute;
        DateTime now = DateTime.Now;
        
        if (schedule.time <= now)
        {
            throw new Exception("You cannot schedule an appointment in the past");
        }

        if (z < start || z >= end )
        {
            throw new Exception("You cannot schedule out of barber's work hours");
            
        }

        if (minutes % 30 != 0)
        {
            throw new Exception("You cannot schedule out of 30 minutes appointment");
        }

        if (!barberService.IsValidSerbianPhoneNumber(schedule.phoneNumber))
        {
            throw new Exception("Invalid phone number format!");
        }
        
        var occupiedAppointment = await dbContext.Appointments.Where(x => x.Time == schedule.time).ToListAsync(
            cancellationToken);

        if (occupiedAppointment.Count > 0)
        {
            throw new Exception("Schedule already occupied!!!");
        }
        
        
        try
        {
            Customer customer = new Customer(
                schedule.firstName,
                schedule.lastName,
                schedule.email,
                schedule.phoneNumber);
                
                
            Guid id = Guid.NewGuid();
            Appointment appointment = new Appointment(requestedTime, schedule.barberId);
            appointment.SetHaircutName(schedule.haircut);

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
            
            
            /*
            var occupiedSlots = await dbContext.Appointments
                .Where(x => x.Barberid == schedule.barberId) // Poredi samo datum
                .ToListAsync(cancellationToken);
            */
            
            
            

            //var availableSlots = await GetAvailableSlots(start, end, request.Schedule.barberId, request.Schedule.time ,cancellationToken);
            await notificationService.SendSmsAsync(customer.PhoneNumber, "Zdravo");
            
            dbContext.Customers.Add(customer);
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new ScheduleAppointmentCreateDto(customer.FirstName, customer.LastName, customer.Email,
                customer.PhoneNumber, appointment.Time, schedule.barberId, appointment.HaircutName);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
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
}