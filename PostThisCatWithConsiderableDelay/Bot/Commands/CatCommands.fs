namespace PostThisCatWithConsiderableDelay.Bot.Commands.CatCommands

open DisCatSharp
open DisCatSharp.ApplicationCommands
open DisCatSharp.CommandsNext
open DisCatSharp.CommandsNext.Attributes
open DisCatSharp.Entities
open FSharpPlus
open FSharp.Control.Tasks
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open PostThisCatWithConsiderableDelay.Database
open PostThisCatWithConsiderableDelay.Extensions
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

        unitTask {
            let settings = context.Services.GetService<Settings>()

            // Create a new Guild object or edit an existing one
            let! guild =
                Guild.getOrCreateAsync context.Guild.Id channel.Id
                </ Reader.run /> context.Services

            // Send initial message
            let! channel = context.Client.GetChannelAsync guild.CatChannel
            let! message = channel.SendMessageAsync(settings.CatUrl)
            // Register message
            let! bot =
                User.getOrCreateAsync context.Client.CurrentUser.Id
                </ Reader.run /> context.Services

            let! post =
                Post.createAsync message
                </ Reader.run /> context.Services

            let! points =
                Points.tryUpdateAsync post
                </ Reader.run /> context.Services

            return ()
        }

type CatCommandsOld() =
    inherit BaseCommandModule()

    [<DefaultValue>]
    val mutable settings: Settings

    [<Command("register")>]
    member this.Register(context: CommandContext, channel: DiscordChannel) =
        unitTask {
            let settings = context.Services.GetService<Settings>()
            // Create a new Guild object or edit an existing one
            let! guild =
                Guild.getOrCreateAsync context.Guild.Id channel.Id
                </ Reader.run /> context.Services
            // Send initial message
            let! channel = context.Client.GetChannelAsync guild.CatChannel
            let! message = channel.SendMessageAsync(settings.CatUrl)
            // Register message
            let! bot =
                User.getOrCreateAsync context.Client.CurrentUser.Id
                </ Reader.run /> context.Services

            let! post =
                Post.createAsync message
                </ Reader.run /> context.Services

            let! points =
                Points.tryUpdateAsync post
                </ Reader.run /> context.Services

            do!
                context.Channel.SendMessageAsync "Registered"
                |> Task.ignore
        }
