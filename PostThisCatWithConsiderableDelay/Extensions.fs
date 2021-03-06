module PostThisCatWithConsiderableDelay.Extensions

open System
open System.Reflection
open System.Threading.Tasks
open DisCatSharp
open DisCatSharp.ApplicationCommands
open DisCatSharp.Common.Utilities
open DisCatSharp.EventArgs
open FSharp.Control.Tasks
open FSharpPlus
open FsToolkit.ErrorHandling
open Microsoft.EntityFrameworkCore
open PostThisCatWithConsiderableDelay.Settings

[<Measure>]
type ms

let isRefNull (a: 'a when 'a: not struct) = obj.ReferenceEquals(a, null)

[<RequireQualifiedAccess>]
module Option =
    let ofRef a =
        match isRefNull a with
        | true -> None
        | false -> Some a

let inline optionWith f n =
    function
    | Some x -> f x
    | None -> n ()

type IServiceProvider with
    member this.GetService<'a>() = this.GetService typedefof<'a> :?> 'a

type DbContext with
    member this.TryFindAsync<'TEntity when 'TEntity: not struct>([<ParamArray>] keyValues: obj []) =
        this.FindAsync<'TEntity> keyValues
        |> Task.mapV Option.ofRef
        |> Async.AwaitTask

    member this.TryFind<'TEntity when 'TEntity: not struct>([<ParamArray>] keyValues: obj []) =
        Async.RunSynchronously
        <| this.TryFindAsync<'TEntity>(keyValues)

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

    member this.RegisterEvent =
        let event (client: DiscordClient) (args: GuildDownloadCompletedEventArgs) =
            unitTask { this.RegisterAllCommands(Assembly.GetEntryAssembly()) (Array.ofSeq args.Guilds.Keys) }

        AsyncEventHandler<_, _> event

type InteractionContext with
    member this.ReplyAsync builder =
        this.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder)

[<RequireQualifiedAccess>]
module Task =
    let waitUntil (interval: int<ms>) condition =
        unitTask {
            while not <| condition () do
                do! Task.Delay(interval / 1<ms>)
        }

    let runSynchronously (task: Task<'a>) = task.Result

    let start (task: Task) =
        if not task.IsCompleted then
            task.Start()

    let ignoreV<'a> (vt: ValueTask<'a>) = vt.AsTask() |> Task.ignore

[<RequireQualifiedAccess>]
module Async =
    let Return x = async { return x }

    let IgnoreTask: Task -> Async<unit> = Task.ignore >> Async.AwaitTask
    let IgnoreTaskV<'a> : ValueTask<'a> -> Async<unit> = Task.ignoreV >> Async.AwaitTask
