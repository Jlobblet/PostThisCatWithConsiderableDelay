module PostThisCatWithConsiderableDelay.Database

open System
open System.Threading.Tasks
open DisCatSharp.Entities
open FSharp.Control.Tasks
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open PostThisCatWithConsiderableDelay.Extensions
open PostThisCatWithConsiderableDelay.Models.CatContext
open PostThisCatWithConsiderableDelay.Models.Models

let inline private orDefaultWith found create =
    found >>= (optionWith Async.Return create)


[<RequireQualifiedAccess>]
module rec User =
    let createAsync userId =
        let inner (services: IServiceProvider) =
            async {
                use db = services.GetService<CatContext>()

                let user = { UserId = userId }

                do!
                    db.AddAsync<User> user
                    |> Task.ignoreV
                    |> Async.AwaitTask

                do!
                    db.SaveChangesAsync()
                    |> Task.ignore
                    |> Async.AwaitTask

                return user
            }

        Reader inner

    let tryFindAsync (userId: uint64) =
        let inner (services: IServiceProvider) =
            use db = services.GetService<CatContext>()
            db.TryFindAsync<User>(userId)

        Reader inner

    let getOrCreateAsync (userId: uint64) =
        Reader.map2 orDefaultWith (tryFindAsync userId) (konst <!> createAsync userId)

    let addPostAsync (post: Post) (user: User) =
        let inner (services: IServiceProvider) =
            async {
                use db = services.GetService<CatContext>()

                let lastPost =
                    query {
                        for p in db.Posts do
                            where (p.Guild = post.Guild)
                            sortByDescending p.Timestamp
                            head
                    }

                do!
                    db.SaveChangesAsync()
                    |> Task.ignore
                    |> Async.AwaitTask

                return user
            }

        Reader inner

[<RequireQualifiedAccess>]
module rec Guild =
    let createAsync guildId channelId =
        let inner (services: IServiceProvider) =
            async {
                use db = services.GetService<CatContext>()

                let g =
                    { GuildId = guildId
                      CatChannel = channelId }

                do!
                    db.AddAsync<Guild> g
                    |> Task.ignoreV
                    |> Async.AwaitTask

                do!
                    db.SaveChangesAsync()
                    |> Task.ignore
                    |> Async.AwaitTask

                return g
            }

        Reader inner

    let tryFindAsync (guildId: uint64) =
        let inner (services: IServiceProvider) =
            use db = services.GetService<CatContext>()
            db.TryFindAsync<Guild>(guildId)

        Reader inner

    let getOrCreateAsync (guildId: uint64) channelId =
        Reader.map2 orDefaultWith (tryFindAsync guildId) (konst <!> createAsync guildId channelId)

    let setChannel channel guild = { guild with CatChannel = channel }

[<RequireQualifiedAccess>]
module Post =
    let createAsync (message: DiscordMessage) =
        let inner (services: IServiceProvider) =
            asyncOption {
                let! guild =
                    Guild.tryFindAsync message.Channel.Guild.Id
                    </ Reader.run /> services

                let! user =
                    User.tryFindAsync message.Author.Id
                    </ Reader.run /> services

                let post =
                    { PostId = Guid.NewGuid()
                      UserId = user.UserId
                      User = user
                      GuildId = guild.GuildId
                      Guild = guild
                      Timestamp = message.Timestamp.UtcDateTime }

                use db = services.GetService<CatContext>()
                do! db.AddAsync<Post> post |> Task.ignoreV
                do! db.SaveChangesAsync() |> Task.ignore

                return post
            }
            |>> Option.get

        Reader inner

[<RequireQualifiedAccess>]
module rec Points =
    let createAsync (post: Post) =
        let inner (services: IServiceProvider) =
            async {
                use db = services.GetService<CatContext>()
                let user = post.User
                let guild = post.Guild

                let points =
                    { PointsId = Guid.NewGuid()
                      UserId = user.UserId
                      User = user
                      GuildId = guild.GuildId
                      Guild = guild
                      Points = 0L
                      Posts = ResizeArray<_>() }

                do! db.AddAsync<Points> points |> Async.IgnoreTaskV
                do! db.SaveChangesAsync() |> Async.IgnoreTask
                return points
            }

        Reader inner

    let tryFindAsync (post: Post) =
        let inner (services: IServiceProvider) =
            use db = services.GetService<CatContext>()
            db.TryFindAsync<Points>(post.UserId, post.GuildId)

        Reader inner

    let getOrCreateAsync (post: Post) =
        Reader.map2 orDefaultWith (tryFindAsync post) (konst <!> createAsync post)

    let tryUpdateAsync (post: Post) =
        let inner (services: IServiceProvider) =
            taskOption {
                use db = services.GetService<CatContext>()
                let! points = getOrCreateAsync post </ Reader.run /> services

                points.Posts.Add post

                { points with
                      Points = points.Points + 1L }
                |> db.Entry<Points>(points).CurrentValues.SetValues

                do! db.SaveChangesAsync() |> Task.ignore

                return points
            }

        Reader inner
