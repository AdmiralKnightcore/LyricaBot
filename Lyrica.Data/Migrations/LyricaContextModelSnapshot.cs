﻿// <auto-generated />
using System;
using Lyrica.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Lyrica.Data.Migrations
{
    [DbContext(typeof(LyricaContext))]
    partial class LyricaContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0-rc.1.20451.13");

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

            modelBuilder.Entity("Lyrica.Data.GenshinImpact.AssociatedAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AccountType")
                        .HasColumnType("integer");

                    b.Property<Guid?>("GenshinAccountId")
                        .HasColumnType("uuid");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GenshinAccountId");

                    b.ToTable("AssociatedAccount");
                });

            modelBuilder.Entity("Lyrica.Data.GenshinImpact.GenshinAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AccountType")
                        .HasColumnType("integer");

                    b.Property<decimal?>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("GenshinAccount");
                });

            modelBuilder.Entity("Lyrica.Data.GenshinImpact.Save", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<Guid?>("GenshinAccountId")
                        .HasColumnType("uuid");

                    b.Property<int>("Region")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GenshinAccountId");

                    b.ToTable("Save");
                });

            modelBuilder.Entity("Lyrica.Data.Guilds.Guild", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<Guid?>("KaraokeId")
                        .HasColumnType("uuid");

                    b.Property<decimal?>("LolaBlessGame")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Owner")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("KaraokeId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Lyrica.Data.Karaoke.KaraokeEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("KaraokeSettingId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("KaraokeSettingId1")
                        .HasColumnType("uuid");

                    b.Property<string>("Song")
                        .HasColumnType("text");

                    b.Property<decimal?>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("KaraokeSettingId");

                    b.HasIndex("KaraokeSettingId1");

                    b.HasIndex("UserId");

                    b.ToTable("KaraokeEntry");
                });

            modelBuilder.Entity("Lyrica.Data.Karaoke.KaraokeSetting", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("Intermission")
                        .HasColumnType("boolean");

                    b.Property<decimal>("KaraokeChannel")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("KaraokeMessage")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("KaraokeVc")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("SingingRole")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("KaraokeSetting");
                });

            modelBuilder.Entity("Lyrica.Data.Users.User", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<Guid?>("ActiveGenshinAccountId")
                        .HasColumnType("uuid");

                    b.Property<decimal?>("ActiveSaveId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTimeOffset?>("JoinedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("KaraokeSettingId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("LastSeenAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("StatsId")
                        .HasColumnType("uuid");

                    b.Property<TimeSpan?>("Timezone")
                        .HasColumnType("interval");

                    b.Property<DateTimeOffset>("UserCreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ActiveGenshinAccountId");

                    b.HasIndex("ActiveSaveId");

                    b.HasIndex("KaraokeSettingId");

                    b.HasIndex("StatsId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Lyrica.Data.Bless.BlessingResult", b =>
                {
                    b.HasOne("Lyrica.Data.Bless.Stats", null)
                        .WithMany("BlessingResults")
                        .HasForeignKey("StatsId");
                });

            modelBuilder.Entity("Lyrica.Data.GenshinImpact.AssociatedAccount", b =>
                {
                    b.HasOne("Lyrica.Data.GenshinImpact.GenshinAccount", null)
                        .WithMany("AssociatedAccounts")
                        .HasForeignKey("GenshinAccountId");
                });

            modelBuilder.Entity("Lyrica.Data.GenshinImpact.GenshinAccount", b =>
                {
                    b.HasOne("Lyrica.Data.Users.User", null)
                        .WithMany("GenshinAccounts")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Lyrica.Data.GenshinImpact.Save", b =>
                {
                    b.HasOne("Lyrica.Data.GenshinImpact.GenshinAccount", null)
                        .WithMany("Saves")
                        .HasForeignKey("GenshinAccountId");
                });

            modelBuilder.Entity("Lyrica.Data.Guilds.Guild", b =>
                {
                    b.HasOne("Lyrica.Data.Karaoke.KaraokeSetting", "Karaoke")
                        .WithMany()
                        .HasForeignKey("KaraokeId");

                    b.Navigation("Karaoke");
                });

            modelBuilder.Entity("Lyrica.Data.Karaoke.KaraokeEntry", b =>
                {
                    b.HasOne("Lyrica.Data.Karaoke.KaraokeSetting", null)
                        .WithMany("NextUp")
                        .HasForeignKey("KaraokeSettingId");

                    b.HasOne("Lyrica.Data.Karaoke.KaraokeSetting", null)
                        .WithMany("Queue")
                        .HasForeignKey("KaraokeSettingId1");

                    b.HasOne("Lyrica.Data.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Lyrica.Data.Users.User", b =>
                {
                    b.HasOne("Lyrica.Data.GenshinImpact.GenshinAccount", "ActiveGenshinAccount")
                        .WithMany()
                        .HasForeignKey("ActiveGenshinAccountId");

                    b.HasOne("Lyrica.Data.GenshinImpact.Save", "ActiveSave")
                        .WithMany()
                        .HasForeignKey("ActiveSaveId");

                    b.HasOne("Lyrica.Data.Karaoke.KaraokeSetting", null)
                        .WithMany("VoteSkippedUsers")
                        .HasForeignKey("KaraokeSettingId");

                    b.HasOne("Lyrica.Data.Bless.Stats", "Stats")
                        .WithMany()
                        .HasForeignKey("StatsId");

                    b.Navigation("ActiveGenshinAccount");

                    b.Navigation("ActiveSave");

                    b.Navigation("Stats");
                });

            modelBuilder.Entity("Lyrica.Data.Bless.Stats", b =>
                {
                    b.Navigation("BlessingResults");
                });

            modelBuilder.Entity("Lyrica.Data.GenshinImpact.GenshinAccount", b =>
                {
                    b.Navigation("AssociatedAccounts");

                    b.Navigation("Saves");
                });

            modelBuilder.Entity("Lyrica.Data.Karaoke.KaraokeSetting", b =>
                {
                    b.Navigation("NextUp");

                    b.Navigation("Queue");

                    b.Navigation("VoteSkippedUsers");
                });

            modelBuilder.Entity("Lyrica.Data.Users.User", b =>
                {
                    b.Navigation("GenshinAccounts");
                });
#pragma warning restore 612, 618
        }
    }
}
