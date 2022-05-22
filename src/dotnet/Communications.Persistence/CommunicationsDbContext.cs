using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Communications.Application.Common.Interfaces;
using Communications.Domain.Common;
using Communications.Domain.Entities;
using Communications.Infrastructure.Identity;
using Communications.Persistence.Configurations;

namespace Communications.Persistence
{
    public class CommunicationsDbContext : IdentityDbContext<ApplicationUser>, ICommunicationsDbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public DbSet<Client> Clients { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public CommunicationsDbContext(DbContextOptions<CommunicationsDbContext> options)
            : base(options)
        {
        }

        public CommunicationsDbContext(DbContextOptions<CommunicationsDbContext> options,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = _currentUserService.UserId;
                        entry.Entity.Created = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = _currentUserService.UserId;
                        entry.Entity.LastModified = DateTime.UtcNow;
                        break;
                }

            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommunicationsDbContext).Assembly);
            base.OnModelCreating(modelBuilder);

            // Customize ASP.NET Identity models and override defaults
            // such as renaming ASP.NET Identity, changing key types etc.
            modelBuilder.ApplyConfiguration(new ApplicationUserConfigurations());
            modelBuilder.ApplyConfiguration(new IdentityUserRoleConfigurations());
            modelBuilder.ApplyConfiguration(new IdentityRoleClaimConfigurations());
            modelBuilder.ApplyConfiguration(new IdentityUserClaimConfigurations());
            modelBuilder.ApplyConfiguration(new IdentityUserLoginConfigurations());
            modelBuilder.ApplyConfiguration(new IdentityUserClaimConfigurations());
            modelBuilder.ApplyConfiguration(new IdentityRoleConfigurations());
            modelBuilder.ApplyConfiguration(new IdentityUserTokenConfigurations());

        }
    }
}