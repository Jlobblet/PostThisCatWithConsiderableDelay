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

            // Create a new Guild object or edit an existing one
            let! guild =
                Guild.getOrCreateAsync context.Guild.Id channel.Id
                </ Reader.run /> context.Services

            // Send initial message
            let! channel = context.Client.GetChannelAsync guild.CatChannel
            let! message = channel.SendMessageAsync(settings.CatUrl)
            // Register message
            let! bot =
                User.getOrCreateAsync context.Client.CurrentUser.Id guild.GuildId
                </ Reader.run /> context.Services

            let! post =
                Post.createAsync message context.Client.Logger
                </ Reader.run /> context.Services

            do!
                Task.ignore <!> User.addPostAsync post bot
                </ Reader.run /> context.Services
        }

type CatCommandsOld() =
    inherit BaseCommandModule()

    [<DefaultValue>]
    val mutable settings: Settings

    [<Command("register")>]
    member this.Register(ctx: CommandContext, channel: DiscordChannel) =
        unitTask {
            // Create a new Guild object or edit an existing one
            let! guild =
                Guild.getOrCreateAsync ctx.Guild.Id channel.Id
                </ Reader.run /> ctx.Services

            // Send initial message
            let! channel = ctx.Client.GetChannelAsync guild.CatChannel
            let! message = channel.SendMessageAsync(this.settings.CatUrl)
            // Register message
            let! bot =
                User.getOrCreateAsync ctx.Client.CurrentUser.Id guild.GuildId
                </ Reader.run /> ctx.Services

            let! post =
                Post.createAsync message ctx.Client.Logger
                </ Reader.run /> ctx.Services

            do!
                Task.ignore <!> User.addPostAsync post bot
                </ Reader.run /> ctx.Services

            do!
                ctx.RespondAsync $"Set {channel.Mention} as the cat channel"
                |> Task.ignore
        }
