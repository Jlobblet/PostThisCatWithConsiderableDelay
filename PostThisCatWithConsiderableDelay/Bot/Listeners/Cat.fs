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

let private IsValid (services: IServiceProvider) (args: MessageCreateEventArgs) (g: Guild) =
    if not args.Author.IsBot
       && g.CatChannel = args.Channel.Id
       && args.Message.Content = services.GetService<Settings>().CatUrl then
        Some g
    else
        None

let AwardPoints (services: IServiceProvider) =
    let event (sender: DiscordClient) (args: MessageCreateEventArgs) =
        // Wrap in async because taskOption likes to execute on the same thread
        async {
            do!
                taskOption {
                    let! guild =
                        Guild.tryFindAsync args.Guild.Id
                        </ Reader.run /> services
                        |>> bind (IsValid services args)

                    let! user =
                        User.getOrCreateAsync args.Author.Id args.Guild.Id
                        </ Reader.run /> services

                    let! post =
                        Post.createAsync args.Message sender.Logger
                        </ Reader.run /> services

                    do!
                        User.addPostAsync post user |>> Async.AwaitTask
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
