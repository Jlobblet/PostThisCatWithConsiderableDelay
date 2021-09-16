module PostThisCatWithConsiderableDelay.Bot.BotMain

open System.Reflection
open DisCatSharp
open DisCatSharp.ApplicationCommands
open DisCatSharp.Common.Utilities
open FSharp.Control.Tasks
open Microsoft.Extensions.DependencyInjection
open PostThisCatWithConsiderableDelay.Extensions
open PostThisCatWithConsiderableDelay.Bot.Listeners.Cat
open PostThisCatWithConsiderableDelay.Models.CatContext
open PostThisCatWithConsiderableDelay.Services
open PostThisCatWithConsiderableDelay.Settings

let MainAsync (services: ServiceProvider) =
    let settings = services.GetService<Settings>()
    let discordConfig =
        DiscordConfiguration.FromSettings(settings)

    let client = new DiscordClient(discordConfig)
    
    let db = new CatContext (settings.ConnectionString)

    client.add_MessageCreated (AsyncEventHandler<_, _>(AwardPoints db))

    let appCommandsConfig =
        ApplicationCommandsConfiguration.FromServiceProvider services

    let appCommandsExtension =
        client.UseApplicationCommands(appCommandsConfig)

    let { Token = killSwitch } = services.GetService<KillSwitch>()

    unitTask {
        do! client.ConnectAsync()
        appCommandsExtension.RegisterAllCommands(Assembly.GetEntryAssembly()) (Array.ofSeq client.Guilds.Keys)
        do! Task.WaitUntil 1000<ms> (fun () -> killSwitch.IsCancellationRequested)
        do! client.DisconnectAsync()
    }
