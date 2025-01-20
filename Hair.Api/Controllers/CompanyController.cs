using System.Net;
using Hair.Application.Common.Interfaces;
using Hair.Application.Companies;
using Hair.Application.Companies.Commands;
using Hair.Application.Companies.Queries;
using Hair.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hair.Api.Controllers;


[ApiController]
[Route("company")]
public class CompanyController(IHairDbContext dbContext): ApiBaseController
{
    

    [HttpPost("create")]
    public async Task<ActionResult<Company>> CreateCompanyAsync(CompanyCreateCommand company)
    {
        return Ok(await Mediator.Send(company)); 
    }

    [HttpGet("get")]
    public async Task<ActionResult<Company>> GetCompanyAsync([FromQuery] CompanyDetailsQuery query)
    {
        return Ok(await Mediator.Send(query));
    }
    
    
}