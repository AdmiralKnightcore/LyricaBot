﻿// <auto-generated />
using System;
using Lyrica.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Lyrica.Data.Migrations
{
    [DbContext(typeof(LyricaContext))]
    [Migration("20200816224448_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0-preview.7.20365.15");

            modelBuilder.Entity("Lyrica.Data.Bless.BlessingResult", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Amount")
                        .HasColumnType("integer");

                    b.Property<Guid?>("StatsId")
                        .HasColumnType("uuid");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("StatsId");

                    b.ToTable("BlessingResult");
                });

            modelBuilder.Entity("Lyrica.Data.Bless.Stats", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Rolls")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Stats");
                });

            modelBuilder.Entity("Lyrica.Data.Guilds.Guild", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("LolaBlessGame")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Owner")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Lyrica.Data.Users.User", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTimeOffset?>("JoinedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("LastSeenAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("StatsId")
                        .HasColumnType("uuid");

                    b.Property<TimeSpan?>("Timezone")
                        .HasColumnType("interval");

                    b.Property<DateTimeOffset>("UserCreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("StatsId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Lyrica.Data.Bless.BlessingResult", b =>
                {
                    b.HasOne("Lyrica.Data.Bless.Stats", null)
                        .WithMany("BlessingResults")
                        .HasForeignKey("StatsId");
                });

            modelBuilder.Entity("Lyrica.Data.Users.User", b =>
                {
                    b.HasOne("Lyrica.Data.Bless.Stats", "Stats")
                        .WithMany()
                        .HasForeignKey("StatsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}