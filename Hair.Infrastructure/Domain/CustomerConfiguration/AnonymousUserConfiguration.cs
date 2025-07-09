using Hair.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hair.Infrastructure.Domain.CustomerConfiguration;

public class AnonymousUserConfiguration : IEntityTypeConfiguration<AnonymousUser>
{
    public void Configure(EntityTypeBuilder<AnonymousUser> builder)
    {
        builder.ToTable("AnonymousUser");
        builder.Property(c => c.Id).ValueGeneratedNever();

    }
}