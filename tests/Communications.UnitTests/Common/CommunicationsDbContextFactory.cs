using Microsoft.EntityFrameworkCore;
using Communications.Persistence;
using System;

namespace Communications.UnitTests.Common
{
    public class CommunicationsDbContextFactory
    {
        public static CommunicationsDbContext Create()
        {
            var options = new DbContextOptionsBuilder<CommunicationsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new CommunicationsDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        public static void Destroy(CommunicationsDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}