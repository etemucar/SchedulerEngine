using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Infrastructure.Configurations;

public class DigitalIdentityConfiguration : IEntityTypeConfiguration<DigitalIdentity>
{
    public void Configure(EntityTypeBuilder<DigitalIdentity> builder)
    {
        builder.HasOne(x => x.PartyRole)
            .WithOne(x => x.DigitalIdentity)
            .HasForeignKey<DigitalIdentity>(x => x.PartyRoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}