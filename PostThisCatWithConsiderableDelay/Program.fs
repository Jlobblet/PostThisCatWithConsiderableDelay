// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open FSharp.Control.Tasks
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling.Operator.Task
open Microsoft.Extensions.DependencyInjection
open PostThisCatWithConsiderableDelay
open PostThisCatWithConsiderableDelay.Bot
open PostThisCatWithConsiderableDelay.Database
open PostThisCatWithConsiderableDelay.Extensions
open PostThisCatWithConsiderableDelay.Models.CatContext
open PostThisCatWithConsiderableDelay.Settings
open PostThisCatWithConsiderableDelay.Services

[<EntryPoint>]
let main argv =
    let services =
        ServiceCollection()
        |> ConfigureServices(Settings.fromArgv argv)

    BotMain
        .MainAsync(services)
        .GetAwaiter()
        .GetResult()

    0 // return an integer exit code
