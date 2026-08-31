using SchedulerEngine.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SchedulerEngine.Infrastructure.Configuration;

public class RelatedPartyConfiguration : IEntityTypeConfiguration<RelatedParty>
{
    public void Configure(EntityTypeBuilder<RelatedParty> builder)
    {
        builder.Property(r => r.RelatedPartyId).HasColumnName("related_to_party_id");

        builder.HasOne(r => r.Party)
               .WithMany(p => p.RelatedParties)
               .HasForeignKey(r => r.PartyId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Related)
               .WithMany()
               .HasForeignKey(r => r.RelatedPartyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}