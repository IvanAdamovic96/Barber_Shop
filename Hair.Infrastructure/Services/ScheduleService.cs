﻿using FluentValidation;
using Hair.Application.Common.Dto.Schedule;
using Hair.Application.Common.Interfaces;
using Hair.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hair.Infrastructure.Services;

public class ScheduleService(IHairDbContext dbContext,
                            UserManager<ApplicationUser> userManager,
                            ILogger<ScheduleService> _logger)
    : IScheduleService
{
    public async Task<ScheduleAppointmentResponseDto> CreateScheduleAppointmentAsync(ScheduleAppointmentCreateDto schedule,
        CancellationToken cancellationToken)
    {
        try
        {
            bool isWithinWorkHours = await IsWithinBarberWorkHours(schedule, cancellationToken);
            if (!isWithinWorkHours)
            {
                throw new Exception("Barber is not available during the requested time.");
            }

            var userExists = await userManager.FindByEmailAsync(schedule.email);
            if (userExists is null)
            {
                throw new Exception("Morate biti ulogovani da bi ste zakazali tretman.");
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

            var isAppointmentAvailable =
                await IsAppointmentAvailable(schedule.barberId, normalizedTime, cancellationToken);
            if (isAppointmentAvailable)
            {
                throw new ValidationException("Schedule appointment already exists.");
            }

            var haircut = await dbContext.Haircuts.Where(x => x.Id == schedule.haircutId)
                .FirstOrDefaultAsync(cancellationToken);
            if (haircut == null)
            {
                throw new ValidationException("Haircut not found.");
            }

            decimal haircutDuration = haircut.Duration;
            int requiredSlots = (int)Math.Ceiling(haircutDuration / 30m);

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

            for (int i = 1; i < requiredSlots; i++)
            {
                currentCheckTime = normalizedTime.AddMinutes(i * 30);
                if (!freeTimes.Contains(currentCheckTime))
                {
                    foundConsecutiveSlots = false;
                    break;
                }

                bookedAppointmentsTimes.Add(currentCheckTime);
                foundConsecutiveSlots = true;
            }

            if (!foundConsecutiveSlots && bookedAppointmentsTimes.Count != requiredSlots)
            {
                throw new ValidationException($"Nema dovoljno uzastopnih slobodnih termina za ovaj tretman koji ste " +
                                              $"izabrali: {haircutDuration} minuta, izaberite drugi termin.");
            }

            /*
            AnonymousUser anonymousUser = new AnonymousUser(
                schedule.firstName,
                schedule.lastName,
                schedule.email,
                schedule.phoneNumber
            );
            dbContext.AnonymousUser.Add(anonymousUser);
            */
            
            foreach (var timeSlot in bookedAppointmentsTimes)
            {
                Appointment appointment = new Appointment(timeSlot, schedule.barberId);
                appointment.SetHaircutName(haircut.HaircutType);
                appointment.SetApplicationUserId(userExists.Id);
                dbContext.Appointments.Add(appointment);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return new ScheduleAppointmentResponseDto(userExists.FirstName, userExists.LastName,
                userExists.Email, userExists.PhoneNumber, bookedAppointmentsTimes[0], schedule.barberId, haircut.HaircutType);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Detaljan opis gde i šta se desilo u ScheduleService");
            throw exception;
        }
    }
    

    public async Task<List<GetAllUsedAppointmentsDto>> GetAllSchedulesByBarberIdAsync(
        Guid barberId, CancellationToken cancellationToken)
    {
        
        var appointments = await dbContext.Appointments.Where(x => barberId == x.Barberid)
            .ToListAsync(cancellationToken);
        
        
        var result = appointments.Select(appointment => new GetAllUsedAppointmentsDto(
                AppointmentId: appointment.Id,
                BarberId: appointment.Barberid,
                Time: appointment.Time,
                HaircutName: appointment.HaircutName,
                ApplicationUserId: appointment.ApplicationUserId,
                FirstName: userManager.FindByIdAsync(appointment.ApplicationUserId).Result.FirstName,
                LastName: userManager.FindByIdAsync(appointment.ApplicationUserId).Result.LastName,
                Email: userManager.FindByIdAsync(appointment.ApplicationUserId).Result.Email,
                PhoneNumber: userManager.FindByIdAsync(appointment.ApplicationUserId).Result.PhoneNumber
        )).ToList();
        /*
         * AppointmentId = appointment.Id,
            BarberId = appointment.Barberid,
            Time = appointment.Time,
            HaircutName = appointment.HaircutName,
            ApplicationUserId = appointment.ApplicationUserId,
            FirstName = userManager.FindByIdAsync(appointment.ApplicationUserId).Result.FirstName,
            LastName = userManager.FindByIdAsync(appointment.ApplicationUserId).Result.LastName,
            Email = userManager.FindByIdAsync(appointment.ApplicationUserId).Result.Email,
            PhoneNumber = userManager.FindByIdAsync(appointment.ApplicationUserId).Result.PhoneNumber
         */
        
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