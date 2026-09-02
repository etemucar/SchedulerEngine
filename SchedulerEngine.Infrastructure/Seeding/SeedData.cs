using Microsoft.EntityFrameworkCore;
using SchedulerEngine.Core.Seeding;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Data.Seeding
{
    public static class DataSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Language>().HasData(InitialSeedData.Languages);
            modelBuilder.Entity<LocalizableFields>().HasData(InitialSeedData.LocalizableFields);
            modelBuilder.Entity<PartyRoleType>().HasData(InitialSeedData.PartyRoleTypes);
        }
    }
}