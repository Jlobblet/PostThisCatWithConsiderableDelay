module PostThisCatWithConsiderableDelay.Bot.Listeners.Cat

open System
open System.Threading.Tasks
open DisCatSharp
open DisCatSharp.Common.Utilities
open DisCatSharp.EventArgs
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open PostThisCatWithConsiderableDelay.Database
open PostThisCatWithConsiderableDelay.Extensions
open PostThisCatWithConsiderableDelay.Models.Models
open PostThisCatWithConsiderableDelay.Settings

let private IsValid (args: MessageCreateEventArgs) =
    let inner (services: IServiceProvider) =
        fun (g: Guild) ->
            if not args.Author.IsBot
               && g.CatChannel = args.Channel.Id
               && args.Message.Content = services.GetService<Settings>().CatUrl then
                Some g
            else
                None

    Reader inner

let AwardPoints (services: IServiceProvider) =
    let event (sender: DiscordClient) (args: MessageCreateEventArgs) =
        // Wrap in async because taskOption likes to execute on the same thread
        async {
            do!
                taskOption {
                    let! guild =
                        Reader.map2 (map << bind) (IsValid args) (Guild.tryFindAsync args.Guild.Id)
                        </ Reader.run /> services

                    let! user =
                        User.getOrCreateAsync args.Author.Id
                        </ Reader.run /> services

                    let! post =
                        Post.createAsync args.Message
                        </ Reader.run /> services

                    do!
                        User.addPostAsync post user
                        </ Reader.run /> services
                        |>> ignore
                }
                |> Task.ignore
                |> Async.AwaitTask
        }
        // Don't wait
        |> Async.Start

        Task.CompletedTask

    AsyncEventHandler<_, _> event
