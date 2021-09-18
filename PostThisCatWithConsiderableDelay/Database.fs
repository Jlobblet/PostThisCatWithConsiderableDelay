module PostThisCatWithConsiderableDelay.Database

open System
open System.Threading.Tasks
open DisCatSharp.Entities
open FSharp.Control.Tasks
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Logging
open PostThisCatWithConsiderableDelay.Database
open PostThisCatWithConsiderableDelay.Extensions
open PostThisCatWithConsiderableDelay.Models
open PostThisCatWithConsiderableDelay.Models.CatContext
open PostThisCatWithConsiderableDelay.Models.Models

[<RequireQualifiedAccess>]
module rec User =
    let createAsync userId =
        let inner (services: IServiceProvider) =
            task {
                use db = services.GetService<CatContext>()

                let user = { UserId = userId }

                do! db.AddAsync<User> user |> Task.ignoreV
                do! db.SaveChangesAsync() |> Task.ignore
                return user
            }

        Reader inner

    let tryFindAsync userId guildId =
        let inner (services: IServiceProvider) =
            use db = services.GetService<CatContext>()

            db.TryFindAsync<User>(userId, guildId)

        Reader inner

    let getOrCreateAsync (userId: uint64) =
        let inner (services: IServiceProvider) =
            use db = services.GetService<CatContext>()

            db.TryFindAsync<User>(userId)
            >>= (fun u ->
                (optionWith
                    Task.singleton
                    (fun () ->
                        (User.createAsync userId)
                        </ Reader.run /> services)
                 <| u))

        Reader inner

    let addPostAsync (post: Post) (user: User) =
        let inner (services: IServiceProvider) =
            task {
                use db = services.GetService<CatContext>()

                let lastPost =
                    query {
                        for p in db.Posts do
                            where (p.Guild = post.Guild)
                            sortByDescending p.Timestamp
                            head
                    }

                do! db.SaveChangesAsync() |> Task.ignore
                return user
            }

        Reader inner

[<RequireQualifiedAccess>]
module rec Guild =
    let createAsync guildId channelId : Reader<IServiceProvider, Task<Guild>> =
        let inner (services: IServiceProvider) =
            task {
                use db = services.GetService<CatContext>()

                let g =
                    { GuildId = guildId
                      CatChannel = channelId }

                do! db.AddAsync<Guild> g |> Task.ignoreV
                do! db.SaveChangesAsync() |> Task.ignore

                return g
            }

        Reader inner

    let tryFindAsync (guildId: uint64) : Reader<IServiceProvider, Task<Guild option>> =
        let inner (services: IServiceProvider) =
            use db = services.GetService<CatContext>()

            db.TryFindAsync<Guild>(guildId)

        Reader inner

    let getOrCreateAsync (guildId: uint64) channelId : Reader<IServiceProvider, Task<Guild>> =
        let inner (services: IServiceProvider) =
            use db = services.GetService<CatContext>()

            db.TryFindAsync<Guild> guildId
            |> Task.bind (
                optionWith
                    Task.singleton
                    (fun () ->
                        Guild.createAsync guildId channelId
                        </ Reader.run /> services)
            )

        Reader inner

    let setChannel channel guild = { guild with CatChannel = channel }

[<RequireQualifiedAccess>]
module Post =
    let createAsync (message: DiscordMessage) (logger: ILogger<_>) =
        let inner (services: IServiceProvider) =
            taskOption {
                use db = services.GetService<CatContext>()

                let! user =
                    User.getOrCreateAsync message.Author.Id
                    </ Reader.run /> services

                let! guild =
                    Guild.tryFindAsync message.Channel.Guild.Id
                    </ Reader.run /> services

                let post =
                    { PostId = Guid.NewGuid()
                      UserId = user.UserId
                      User = user
                      GuildId = guild.GuildId
                      Guild = guild
                      Timestamp = message.Timestamp.UtcDateTime }

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
            task {
                use db = services.GetService<CatContext>()
                let user = post.User
                let guild = post.Guild
                let points =
                    { UserId = user.UserId
                      User = user
                      GuildId = guild.GuildId
                      Guild = guild
                      Points = 0L
                      Posts = ResizeArray<_>() }
                do! db.AddAsync<Points> points |> Task.ignoreV
                do! db.SaveChangesAsync() |> Task.ignore
                return points
            }
            
        Reader inner
        
    let tryFindAsync (post: Post) =
        let inner (services: IServiceProvider) =
            use db = services.GetService<CatContext>()
            db.TryFindAsync<Points>(post.UserId, post.GuildId)
            
        Reader inner
        
    let getOrCreateAsync (post: Post) =
        let inner (services: IServiceProvider) =
            use db = services.GetService<CatContext>()

            db.TryFindAsync<Points>(post.UserId, post.GuildId)
            |> Task.bind (
                optionWith
                    Task.singleton
                    (fun () ->
                        Points.createAsync post
                        </ Reader.run /> services)
            )

        Reader inner
        
    let tryUpdateAsync (post: Post) =
        let inner (services: IServiceProvider) =
            taskOption {
                use db = services.GetService<CatContext>()
                let! points = Points.tryFindAsync post </ Reader.run /> services
                
                points.Posts.Add post
                { points with Points = points.Points + 1L }
                |> db.Entry<Points>(points).CurrentValues.SetValues
                
                do! db.SaveChangesAsync() |> Task.ignore
                
                return points
            }
            
        Reader inner
