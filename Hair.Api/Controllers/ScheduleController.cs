﻿using Hair.Application.Common.Interfaces;
using Hair.Application.Schedules.Commands;
using Hair.Application.Schedules.Queries;
using Hair.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Hair.Api.Controllers;
[ApiController]
[Route("schedule")]
public class ScheduleController: ApiBaseController
{
    [HttpPost ("CreateAppointment")]
    public async Task<ActionResult<Appointment>> CreateBarberAsync(ScheduleAppointmentCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
    
    [HttpGet ("GetAllUsedAppointments")]
    public async Task<ActionResult<Appointment>> CreateBarbersAsync([FromQuery]GetAllScheduledAppointments query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet ("GetAllFreeAppointments")]
    public async Task<ActionResult<Appointment>> GetAllFreeAppointmentsAsync([FromQuery]GetAllFreeAppointmentsQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    
}