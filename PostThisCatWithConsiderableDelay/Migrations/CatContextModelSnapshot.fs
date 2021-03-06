// <auto-generated />
namespace PostThisCatWithConsiderableDelay.Migrations

open System
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Infrastructure
open Microsoft.EntityFrameworkCore.Metadata
open Microsoft.EntityFrameworkCore.Migrations
open Microsoft.EntityFrameworkCore.Storage.ValueConversion
open PostThisCatWithConsiderableDelay.Models

[<DbContext(typeof<CatContext.CatContext>)>]
type CatContextModelSnapshot() =
    inherit ModelSnapshot()

    override this.BuildModel(modelBuilder: ModelBuilder) =
        modelBuilder.HasAnnotation("ProductVersion", "5.0.10")
        |> ignore

        modelBuilder.Entity(
            "PostThisCatWithConsiderableDelay.Models.Models+Guild",
            (fun b ->

                b
                    .Property<UInt64>("GuildId")
                    .IsRequired(true)
                    .HasColumnType("INTEGER")
                |> ignore

                b
                    .Property<UInt64>("CatChannel")
                    .IsRequired(true)
                    .HasColumnType("INTEGER")
                |> ignore

                b.HasKey("GuildId") |> ignore

                b.ToTable("Guilds") |> ignore

                )
        )
        |> ignore

        modelBuilder.Entity(
            "PostThisCatWithConsiderableDelay.Models.Models+Points",
            (fun b ->

                b
                    .Property<Guid>("PointsId")
                    .IsRequired(true)
                    .HasColumnType("TEXT")
                |> ignore

                b
                    .Property<UInt64>("GuildId")
                    .IsRequired(true)
                    .HasColumnType("INTEGER")
                |> ignore

                b
                    .Property<Int64>("Points")
                    .IsRequired(true)
                    .HasColumnType("INTEGER")
                |> ignore

                b
                    .Property<UInt64>("UserId")
                    .IsRequired(true)
                    .HasColumnType("INTEGER")
                |> ignore

                b.HasKey("PointsId") |> ignore


                b.HasIndex("GuildId") |> ignore


                b.HasIndex("UserId") |> ignore

                b.ToTable("Points") |> ignore

                )
        )
        |> ignore

        modelBuilder.Entity(
            "PostThisCatWithConsiderableDelay.Models.Models+Post",
            (fun b ->

                b
                    .Property<Guid>("PostId")
                    .IsRequired(true)
                    .HasColumnType("TEXT")
                |> ignore

                b
                    .Property<UInt64>("GuildId")
                    .IsRequired(true)
                    .HasColumnType("INTEGER")
                |> ignore

                b
                    .Property<Nullable<Guid>>("PointsId")
                    .IsRequired(true)
                    .HasColumnType("TEXT")
                |> ignore

                b
                    .Property<DateTime>("Timestamp")
                    .IsRequired(true)
                    .HasColumnType("TEXT")
                |> ignore

                b
                    .Property<UInt64>("UserId")
                    .IsRequired(true)
                    .HasColumnType("INTEGER")
                |> ignore

                b.HasKey("PostId") |> ignore


                b.HasIndex("GuildId") |> ignore


                b.HasIndex("PointsId") |> ignore


                b.HasIndex("UserId") |> ignore

                b.ToTable("Posts") |> ignore

                )
        )
        |> ignore

        modelBuilder.Entity(
            "PostThisCatWithConsiderableDelay.Models.Models+User",
            (fun b ->

                b
                    .Property<UInt64>("UserId")
                    .IsRequired(true)
                    .HasColumnType("INTEGER")
                |> ignore

                b.HasKey("UserId") |> ignore

                b.ToTable("Users") |> ignore

                )
        )
        |> ignore

        modelBuilder.Entity(
            "PostThisCatWithConsiderableDelay.Models.Models+Points",
            (fun b ->
                b
                    .HasOne("PostThisCatWithConsiderableDelay.Models.Models+Guild", "Guild")
                    .WithMany()
                    .HasForeignKey("GuildId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired()
                |> ignore

                b
                    .HasOne("PostThisCatWithConsiderableDelay.Models.Models+User", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired()
                |> ignore)
        )
        |> ignore

        modelBuilder.Entity(
            "PostThisCatWithConsiderableDelay.Models.Models+Post",
            (fun b ->
                b
                    .HasOne("PostThisCatWithConsiderableDelay.Models.Models+Guild", "Guild")
                    .WithMany()
                    .HasForeignKey("GuildId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired()
                |> ignore

                b
                    .HasOne("PostThisCatWithConsiderableDelay.Models.Models+Points", null)
                    .WithMany("Posts")
                    .HasForeignKey("PointsId")
                |> ignore

                b
                    .HasOne("PostThisCatWithConsiderableDelay.Models.Models+User", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired()
                |> ignore)
        )
        |> ignore
