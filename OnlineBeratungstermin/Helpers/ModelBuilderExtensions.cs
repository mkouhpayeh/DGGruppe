using Microsoft.EntityFrameworkCore;
using OnlineBeratungstermin.Models;

namespace OnlineBeratungstermin.Helpers
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Terminart>().HasData(
                new Terminart { ID = 1, Name = "Type 15", Dauer = 15 },
                new Terminart { ID = 2, Name = "Type 30", Dauer = 30 },
                new Terminart { ID = 3, Name = "Type 45", Dauer = 45 },
                new Terminart { ID = 4, Name = "Type 60", Dauer = 60 },
                new Terminart { ID = 5, Name = "Type 90", Dauer = 90 }
            );
            modelBuilder.Entity<Berater>().HasData(
                new Berater { ID = 1, Name = "Mahboubeh  K" },
                new Berater { ID = 2, Name = "Viktoria G" },
                new Berater { ID = 3, Name = "Micheal R" },
                new Berater { ID = 4, Name = "Andrii M" }
            );
        }
    }
}
