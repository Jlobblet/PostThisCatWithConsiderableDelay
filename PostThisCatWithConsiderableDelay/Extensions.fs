module PostThisCatWithConsiderableDelay.Extensions

open System
open System.Reflection
open System.Threading.Tasks
open DisCatSharp
open DisCatSharp.ApplicationCommands
open FSharp.Data.UnitSystems.SI.UnitSymbols
open FSharp.Control.Tasks
open Microsoft.EntityFrameworkCore
open PostThisCatWithConsiderableDelay.Settings

[<Measure>]
type ms = s

let isRefNull (a: 'a when 'a: not struct) = obj.ReferenceEquals(a, null)

[<RequireQualifiedAccess>]
module Option =
    let ofRef a =
        match isRefNull a with
        | true -> None
        | false -> Some a
        
type DbContext with
    member this.TryFindAsync<'TEntity when 'TEntity: not struct> ([<ParamArray>] keyValues: obj []) =
        vtask {
            let! result = this.FindAsync<'TEntity> keyValues
            return Option.ofRef result
        }

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
