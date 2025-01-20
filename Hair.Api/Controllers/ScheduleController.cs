using Hair.Application.Common.Interfaces;
using Hair.Application.Schedules.Commands;
using Hair.Application.Schedules.Queries;
using Hair.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Hair.Api.Controllers;
[ApiController]
[Route("schedule")]
public class ScheduleController: ApiBaseController
{
    [HttpPost]
    public async Task<ActionResult<Appointment>> CreateBarberAsync(ScheduleAppointmentCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
    
    [HttpGet]
    public async Task<ActionResult<Appointment>> CreateBarberAsync([FromQuery]GetAllScheduledAppointments command)
    {
        return Ok(await Mediator.Send(command));
    }

    
}