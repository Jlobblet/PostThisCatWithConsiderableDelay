module PostThisCatWithConsiderableDelay.Bot.Listeners.Cat

open System.Threading.Tasks
open DisCatSharp
open DisCatSharp.Common.Utilities
open DisCatSharp.EventArgs
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open Microsoft.Extensions.DependencyInjection
open PostThisCatWithConsiderableDelay.Database
open PostThisCatWithConsiderableDelay.Extensions
open PostThisCatWithConsiderableDelay.Models.CatContext
open PostThisCatWithConsiderableDelay.Models.Models
open PostThisCatWithConsiderableDelay.Settings

let private IsValid (services: ServiceProvider) (args: MessageCreateEventArgs) (g: Guild) =
    if not args.Author.IsBot
       && g.CatChannel = args.Channel.Id
       && args.Message.Content = services.GetService<Settings>().CatUrl then
        Some g
    else
        None

let AwardPoints (services: ServiceProvider) =
    let event (sender: DiscordClient) (args: MessageCreateEventArgs) =
        taskOption {
            let db = services.GetService<CatContext>()

            let! guild =
                db.TryFindAsync<Guild>(args.Guild.Id).AsTask()
                |>> bind (IsValid services args)

            let! user =
                User.getOrCreateAsync args.Author.Id args.Guild.Id
                </ Reader.run /> db

            let! post = Post.createAsync args.Message </ Reader.run /> db

            do!
                User.addPostAsync post user </ Reader.run /> db
                |>> ignore

            return ()
        }
        |> Task.start

        Task.CompletedTask

    AsyncEventHandler<_, _> event
