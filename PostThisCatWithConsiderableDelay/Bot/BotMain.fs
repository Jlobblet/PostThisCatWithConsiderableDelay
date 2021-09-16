module PostThisCatWithConsiderableDelay.Bot.BotMain

open System.Threading.Tasks
open DisCatSharp
open DisCatSharp.ApplicationCommands
open DisCatSharp.Entities
open FSharp.Control.Tasks
open Microsoft.Extensions.DependencyInjection
open PostThisCatWithConsiderableDelay.Extensions
open PostThisCatWithConsiderableDelay.Services
open PostThisCatWithConsiderableDelay.Settings


let waitUntil (interval: int) condition =
    unitTask {
        while not <| condition() do
            do! Task.Delay interval
    }

let MainAsync (services: ServiceProvider) =
    let discordConfig =
        DiscordConfiguration.FromSettings(services.GetService<Settings>())

    use client = new DiscordClient(discordConfig)

//    let appCommandsConfig =
//        ApplicationCommandsConfiguration.FromServiceProvider services
//
//    let appCommandsExtension =
//        client.UseApplicationCommands(appCommandsConfig)
//
//    appCommandsExtension.RegisterCommands()

    let { Token = killSwitch } = services.GetService<KillSwitch>()

    unitTask {
        do! client.ConnectAsync()
//        do! client.UpdateStatusAsync(userStatus = UserStatus.Online)
//        do! waitUntil 1000 (fun () -> killSwitch.IsCancellationRequested)
//        do! client.UpdateStatusAsync(userStatus = UserStatus.Offline)
//        do! client.DisconnectAsync()
    }
