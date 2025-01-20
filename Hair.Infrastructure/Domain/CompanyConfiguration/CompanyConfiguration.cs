﻿using Hair.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hair.Infrastructure.Domain.CompanyConfiguration;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Company");
        builder.HasIndex(x => x.CompanyName).IsUnique();
        builder.Property(x => x.Id).ValueGeneratedNever();
        
    }
}
