module PostThisCatWithConsiderableDelay.Bot.BotMain

open System.Reflection
open DisCatSharp
open DisCatSharp.ApplicationCommands
open FSharp.Control.Tasks
open Microsoft.Extensions.DependencyInjection
open PostThisCatWithConsiderableDelay.Extensions
open PostThisCatWithConsiderableDelay.Services
open PostThisCatWithConsiderableDelay.Settings




let MainAsync (services: ServiceProvider) =
    let discordConfig =
        DiscordConfiguration.FromSettings(services.GetService<Settings>())

    let client = new DiscordClient(discordConfig)
    
    let appCommandsConfig =
        ApplicationCommandsConfiguration.FromServiceProvider services

    let appCommandsExtension =
        client.UseApplicationCommands(appCommandsConfig)

    let { Token = killSwitch } = services.GetService<KillSwitch>()

    unitTask {
        do! client.ConnectAsync()
        appCommandsExtension.RegisterAllCommands (Assembly.GetEntryAssembly()) client.Guilds.Keys
        do! Task.WaitUntil 1000<ms> (fun () -> killSwitch.IsCancellationRequested)
        do! client.DisconnectAsync()
    }
