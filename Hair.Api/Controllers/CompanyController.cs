﻿using System.Net;
using Hair.Application.Barbers.Queries;
using Hair.Application.Common.Dto.Company;
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

    [HttpGet("getCompanyById")]
    public async Task<ActionResult<Company>> GetCompanyAsync([FromQuery] CompanyDetailsQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("getAllCompanies")]
    public async Task<ActionResult<List<Company>>> GetAllCompaniesAsync()
    {
        return Ok(await Mediator.Send(new GetAllCompaniesQuery(CompanyDetailsDto: new CompanyDetailsDto())));
    }
}