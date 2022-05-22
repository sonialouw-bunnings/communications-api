using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Communications.Infrastructure.Identity;

namespace Communications.Persistence.Configurations
{
    public class ApplicationUserConfigurations : IEntityTypeConfiguration<ApplicationUser>
    {

        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable(name: "Users");
            builder.Property("Email").HasColumnName("EmailAddress");
        }
    }
}