module PostThisCatWithConsiderableDelay.Database

open System
open DisCatSharp.Entities
open FSharp.Control.Tasks
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open PostThisCatWithConsiderableDelay.Extensions
open PostThisCatWithConsiderableDelay.Models.CatContext
open PostThisCatWithConsiderableDelay.Models.Models

[<RequireQualifiedAccess>]
module rec User =
    let createAsync user guild =
        let inner (db: CatContext) =
            task {
                let user =
                    { UserId = user
                      GuildId = guild
                      Points = 0L
                      Posts = ResizeArray<_>()
                      Guild = db.Find<Guild>(guild) }

                do! db.AddAsync<User> user |> Task.ignoreV
                do! db.SaveChangesAsync() |> Task.ignore
                return user
            }

        Reader inner

    let getOrCreateAsync user guild =
        let inner (db: CatContext) =
            db.TryFindAsync<User>(user, guild).AsTask()
            >>= (optionWith
                     Task.singleton
                     (thunk <!> (User.createAsync user guild)
                      </ Reader.run /> db))

        Reader inner

    let addPostAsync (post: Post) (user: User) =
        let inner (db: CatContext) =
            task {
                user.Posts.Add post

                let lastPost =
                    query {
                        for post in db.Posts do
                            where (post.User.Guild = user.Guild)
                            sortByDescending post.Timestamp
                            exactlyOne
                    }

                let delta = post.Timestamp - lastPost.Timestamp

                let user = { user with Points = user.Points + 1L }

                do! db.AddAsync<User> user |> Task.ignoreV

                do! db.SaveChangesAsync() |> Task.ignore
                return user
            }

        Reader inner

[<RequireQualifiedAccess>]
module Guild =
    let createAsync guild channel =
        let inner (db: CatContext) =
            task {
                let g =
                    { GuildId = guild
                      CatChannel = channel
                      Users = ResizeArray<_>()
                      Posts = ResizeArray<_>() }

                do! db.AddAsync<Guild> g |> Task.ignoreV
                do! db.SaveChangesAsync() |> Task.ignore
                return g
            }

        Reader inner

    let setChannel channel guild = { guild with CatChannel = channel }

[<RequireQualifiedAccess>]
module Post =
    let createAsync (message: DiscordMessage) =
        let inner (db: CatContext) =
            taskOption {
                let! user =
                    db.TryFindAsync<User>(message.Author.Id, message.Channel.Guild.Id)
                    |> Task.bindV (
                        optionWith
                            Task.singleton
                            (User.createAsync message.Author.Id message.Channel.Guild.Id
                             |> Reader.run
                             <| db
                             |> thunk)
                    )

                let post =
                    { PostId = Guid.NewGuid()
                      User = user
                      Timestamp = message.Timestamp.UtcDateTime }

                do! db.AddAsync<Post> post |> Task.ignoreV

                user.Posts.Add post

                do!
                    { user with Points = 0L }
                    |> db.AddAsync<User>
                    |> Task.ignoreV

                do! db.SaveChangesAsync() |> Task.ignore

                return post
            }
            |>> Option.get

        Reader inner
