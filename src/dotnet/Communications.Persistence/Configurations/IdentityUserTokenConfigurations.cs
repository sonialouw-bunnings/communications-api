﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Communications.Persistence.Configurations
{
    public class IdentityUserTokenConfigurations : IEntityTypeConfiguration<IdentityUserToken<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
        {
            builder.ToTable(name: "UserTokens");
        }
    }
}