namespace PostThisCatWithConsiderableDelay.Bot.Commands.Misc

open DisCatSharp.ApplicationCommands
open DisCatSharp.Entities
open PostThisCatWithConsiderableDelay.Extensions

type MiscCommands() =
    inherit ApplicationCommandsModule()

    static member Ping(context: InteractionContext) =
        DiscordInteractionResponseBuilder()
            .WithContent("Pong!")
        |> context.ReplyAsync
