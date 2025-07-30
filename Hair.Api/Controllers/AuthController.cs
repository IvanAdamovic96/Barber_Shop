using Hair.Application.Auth.Commands;
using Hair.Application.Auth.Queries;
using Hair.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hair.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ApiBaseController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] LoginCommand loginCommand)
    {
        return Ok(await Mediator.Send(loginCommand));
    }
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromForm] RegisterCommand registerCommand)
    {
        return Ok(await Mediator.Send(registerCommand));
    }

    [HttpPost("createCompanyOwner")]
    public async Task<IActionResult> CreateCompanyOwner([FromForm] CreateCompanyOwnerCommand createCompanyOwnerCommand)
    {
        var result = await Mediator.Send(createCompanyOwnerCommand);
        return Ok(new { Message = result });
        //return Ok(await Mediator.Send(createCompanyOwnerCommand));
    }

    [HttpPut("update-owner")]
    public async Task<IActionResult> UpdateCompanyOwner([FromForm] UpdateCompanyOwnerCommand updateCompanyOwnerCommand)
    {
        var result = await Mediator.Send(updateCompanyOwnerCommand);
        return Ok(new { Message = result });
    }
    
    [HttpDelete("delete-owner")]
    public async Task<IActionResult> DeleteCompanyByCompanyIdAsync([FromQuery] DeleteCompanyOwnerCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(new { Message = result });
    }

    [HttpGet("checkIfCompanyOwnerExists")]
    public async Task<IActionResult> CheckIfCompanyOwnerExists([FromQuery] CheckOwnerExistsQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(query, cancellationToken));
    }
    
    [HttpGet("get-owners")]
    public async Task<IActionResult> GetOwners([FromQuery] GetAllOwnersQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("get-owner-details")]
    public async Task<IActionResult> GetOwnerDetailsById([FromQuery] GetOwnerDetailsByIdQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(query, cancellationToken));
    }

    [HttpPost("assign-company-owner")]
    public async Task<IActionResult> AssignCompanyOwner([FromForm] AssignCompanyOwnerCommand assignCompanyOwnerCommand)
    {
        return Ok(await Mediator.Send(assignCompanyOwnerCommand));
    }

    [HttpGet("get-companies-by-owner-email")]
    public async Task<IActionResult> GetCompaniesByOwnerEmailAsync([FromQuery] GetCompaniesByOwnerEmailQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("get-appointments-by-user-id")]
    public async Task<IActionResult> GetAllAppointmentsByUserId([FromQuery] GetAllAppointmentsByUserIdQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }
}