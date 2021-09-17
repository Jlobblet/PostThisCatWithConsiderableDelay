module PostThisCatWithConsiderableDelay.Bot.Commands.CatCommands

open DisCatSharp
open DisCatSharp.ApplicationCommands
open DisCatSharp.Entities
open FSharpPlus
open FSharp.Control.Tasks
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open PostThisCatWithConsiderableDelay.Database
open PostThisCatWithConsiderableDelay.Extensions
open PostThisCatWithConsiderableDelay.Models.CatContext
open PostThisCatWithConsiderableDelay.Models.Models
open PostThisCatWithConsiderableDelay.Settings

type CatCommands() =
    inherit ApplicationCommandsModule()

    [<SlashCommand("register",
                   "Register the channel in this server to use for posting this cat with considerable delay.")>]
    static member Register(context: InteractionContext, channel: DiscordChannel) =
        // Since this command involves database work, acknowledge immediately
        context
            .CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource)
            .Start()

        task {
            let settings = context.Services.GetService<Settings>()

            let db =
                context.Services.GetService<CatContext>()
            // Create a new Guild object or edit an existing one
            let! existing = db.TryFindAsync<Guild>(context.Guild.Id)

            let! guild =
                existing
                |> optionWith
                    (Task.singleton <!> (Guild.setChannel channel.Id))
                    ((Guild.createAsync context.Guild.Id channel.Id)
                     |>> thunk
                     </ Reader.run /> db)

            // Send initial message
            let! channel = context.Client.GetChannelAsync guild.CatChannel
            let! message = channel.SendMessageAsync(settings.CatUrl)
            // Register message
            let! bot =
                User.getOrCreateAsync context.Client.CurrentUser.Id guild.GuildId
                </ Reader.run /> db

            let! post = Post.createAsync message </ Reader.run /> db

            do!
                Task.ignore <!> User.addPostAsync post bot
                </ Reader.run /> db
        }
