module PostThisCatWithConsiderableDelay.Models.CatContext

open EntityFrameworkCore.FSharp.Extensions
open Microsoft.EntityFrameworkCore
open PostThisCatWithConsiderableDelay.Models.Models

type CatContext(connectionString: string) =
    inherit DbContext()

    [<DefaultValue>]
    val mutable users: DbSet<User>

    member this.Users
        with get () = this.users
        and set v = this.users <- v

    [<DefaultValue>]
    val mutable private posts: DbSet<Post>

    member this.Posts
        with get () = this.posts
        and set v = this.posts <- v

    [<DefaultValue>]
    val mutable private guilds: DbSet<Guild>

    member this.Guilds
        with get () = this.guilds
        and set v = this.guilds <- v

    override _.OnModelCreating builder =
        builder.RegisterOptionTypes()
        builder.RegisterSingleUnionCases()

        builder
            .Entity<User>()
            .HasKey(fun u -> (u.UserId, u.GuildId) :> obj)
        |> ignore<Metadata.Builders.KeyBuilder>

    override _.OnConfiguring builder =
        builder.UseSqlite(connectionString)
        |> ignore<DbContextOptionsBuilder>
