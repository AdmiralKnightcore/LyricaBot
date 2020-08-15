﻿// <auto-generated />
using System;
using Lyrica.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lyrica.Data.Migrations
{
    [DbContext(typeof(LyricaContext))]
    [Migration("20200816211914_AddUserTimezone")]
    partial class AddUserTimezone
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0-preview.7.20365.15");

            modelBuilder.Entity("Lyrica.Data.Bless.BlessingResult", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("Amount")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("StatsId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("StatsId");

                    b.ToTable("BlessingResult");
                });

            modelBuilder.Entity("Lyrica.Data.Bless.Stats", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("Rolls")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Stats");
                });

            modelBuilder.Entity("Lyrica.Data.Guilds.Guild", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("LolaBlessGame")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Owner")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Lyrica.Data.Users.User", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("JoinedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("LastSeenAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("StatsId")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("Timezone")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("UserCreatedAt")
                        .HasColumnType("TEXT");

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
