module PostThisCatWithConsiderableDelay.Extensions

open System.Reflection
open System.Threading.Tasks
open DisCatSharp
open DisCatSharp.ApplicationCommands
open FSharp.Data.UnitSystems.SI.UnitSymbols
open FSharp.Control.Tasks
open PostThisCatWithConsiderableDelay.Settings

[<Measure>]
type ms = s

type DiscordConfiguration with
    static member FromSettings settings =
        let c = DiscordConfiguration()
        c.Token <- settings.DiscordToken
        c.TokenType <- TokenType.Bot
        c.Intents <- DiscordIntents.AllUnprivileged
        c.AutoReconnect <- true
        c

type ApplicationCommandsConfiguration with
    static member FromServiceProvider isp =
        let c = ApplicationCommandsConfiguration()
        c.Services <- isp
        c

type ApplicationCommandsExtension with
    member this.RegisterAllCommands (assembly: Assembly) guilds =
        let modules =
            assembly.GetTypes()
            |> Array.filter (fun t -> t.IsSubclassOf typeof<ApplicationCommandsModule>)

        guilds
        |> Array.map System.Nullable
        |> Array.allPairs modules
        |> Array.Parallel.iter this.RegisterCommands

type InteractionContext with
    member this.ReplyAsync builder =
        this.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder)

[<RequireQualifiedAccess>]
module Task =
    let WaitUntil (interval: int<ms>) condition =
        unitTask {
            while not <| condition () do
                do! Task.Delay(interval / 1<ms>)
        }
