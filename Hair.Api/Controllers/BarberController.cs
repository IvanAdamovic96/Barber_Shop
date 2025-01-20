using Hair.Application.Barbers.Commands;
using Hair.Application.Common.Interfaces;
using Hair.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Hair.Api.Controllers;

[ApiController]
[Route("barber")]
public class BarberController(IHairDbContext dbContext) : ApiBaseController
{
    [HttpPost("create")]
    public async Task<ActionResult<Barber>> CreateBarberAsync(BarberCreateCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
    
}