using Hair.Application.Common.Dto.Schedule;
using Hair.Application.Common.Interfaces;
using Hair.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hair.Application.Schedules.Commands;

public record ScheduleAppointmentCommand(ScheduleAppointmentCreateDto Schedule): IRequest<ScheduleAppointmentCreateDto?>;

public class ScheduleAppointmentCommandHandler(IHairDbContext dbContext)
    : IRequestHandler<ScheduleAppointmentCommand, ScheduleAppointmentCreateDto?>
{
    public async Task<ScheduleAppointmentCreateDto?> Handle(ScheduleAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        
        /*
        var appointments = await dbContext.Appointments.Where(x => request.Schedule.barberId == x.Barberid).ToListAsync();
        var result = appointments.Select(appointment => new GetAllSchedulesByBarberIdDto
        {
            barberId = appointment.Barberid,
            time = appointment.Time
        }).ToList();
        */

        var s = await dbContext.Barbers.Where(x => request.Schedule.barberId == x.BarberId).FirstOrDefaultAsync();
        var start = s.IndividualStartTime.Value;
        var end = s.IndividualEndTime.Value;
        //var usedAppointments = await dbContext.Appointments.Where(x => x.Time == request.Schedule.time).ToListAsync();
     

   
     
        DateTime requestedTime = request.Schedule.time;
        var z = requestedTime.TimeOfDay;
        var minutes = requestedTime.Minute;
        DateTime now = DateTime.Now;
        
        if (request.Schedule.time <= now)
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
        
        try
        {
            Customer customer = new Customer(
                request.Schedule.firstName,
                request.Schedule.lastName,
                request.Schedule.email,
                request.Schedule.phoneNumber);
                
                
            Guid id = Guid.NewGuid();
            Appointment appointment = new Appointment();
            appointment.Id = id;
            appointment.Time = requestedTime;
            appointment.Barberid = request.Schedule.barberId;
                
            var occupiedSlots = await dbContext.Appointments
                .Where(x => x.Barberid == request.Schedule.barberId) // Poredi samo datum
                .ToListAsync(cancellationToken);

            var occupiedAppointment = await dbContext.Appointments.Where(x => x.Time == request.Schedule.time).ToListAsync(
                cancellationToken);

            if (occupiedAppointment.Count > 0)
            {
                throw new Exception("Schedule already occupied!!!");
            }
            
            

            //var availableSlots = await GetAvailableSlots(start, end, request.Schedule.barberId, request.Schedule.time ,cancellationToken);
            Console.WriteLine("Slobodni termini:");
            foreach (var slot in occupiedSlots)
            {
                Console.WriteLine(slot.ToString());
            }
                
            dbContext.Customers.Add(customer);
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new ScheduleAppointmentCreateDto(customer.FirstName, customer.LastName, customer.Email,
                customer.PhoneNumber, requestedTime, request.Schedule.barberId);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
    

    /*
    private async Task<List<DateTime>> GetAvailableSlots(TimeSpan start, TimeSpan end, Guid barberId, DateTime vreme ,CancellationToken cancellationToken)
    {
        // Lista svih mogućih termina u radnom vremenu frizera
        List<DateTime> allSlots = new List<DateTime>();
        DateTime current = vreme; // Počinjemo od početnog vremena radnog vremena
        start = current.TimeOfDay;
        
        while (start < end)
        {
            allSlots.Add(current);

            // Pomeri za 30 minuta
            current = current.AddMinutes(30);
        }

        // Filtriraj zauzete termine
        var occupiedSlots = await dbContext.Appointments
            .Where(x => x.Barberid == barberId && x.Time == vreme)  // Poredi samo datum
            .Select(x => x.Time)
            .ToListAsync(cancellationToken);

        // Slobodni termini su svi koji nisu zauzeti
        var availableSlots = allSlots.Where(slot => !occupiedSlots.Contains(slot)).ToList();

        return availableSlots;
    }
    */



}
