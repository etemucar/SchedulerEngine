
using SchedulerEngine.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SchedulerEngine.Infrastructure.Configuration;

public class PartyRoleConfiguration : IEntityTypeConfiguration<PartyRole>
{
    public void Configure(EntityTypeBuilder<PartyRole> builder)
    {
        builder.HasOne(x => x.PartyRoleType)
            .WithMany()
            .HasForeignKey(x => x.PartyRoleTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

