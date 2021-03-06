﻿using Lyrica.Data.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Lyrica.Data
{
    public class LyricaContextFactory : IDesignTimeDbContextFactory<LyricaContext>
    {
        public LyricaContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<LyricaConfig>()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<LyricaContext>()
                .UseNpgsql(configuration.GetConnectionString("PostgreSQL"));

            return new LyricaContext(optionsBuilder.Options);
        }
    }
}