module PostThisCatWithConsiderableDelay.Bot.BotMain

open System
open System.Reflection
open DisCatSharp
open DisCatSharp.ApplicationCommands
open DisCatSharp.CommandsNext
open FSharp.Control.Tasks
open Microsoft.Extensions.DependencyInjection
open PostThisCatWithConsiderableDelay.Extensions
open PostThisCatWithConsiderableDelay.Bot.Listeners.Cat
open PostThisCatWithConsiderableDelay.Services
open PostThisCatWithConsiderableDelay.Settings

let MainAsync (services: IServiceProvider) =
    let settings = services.GetService<Settings>()

    let discordConfig =
        DiscordConfiguration.FromSettings(settings)

    let client = new DiscordClient(discordConfig)

    client.add_MessageCreated (AwardPoints services)

    let commandsNextConfig =
        let c = CommandsNextConfiguration()
        c.Services <- services
        c.EnableMentionPrefix <- true
        c.StringPrefixes <- [| "!" |]
        c

    let commandsNextExtensions =
        client.UseCommandsNext commandsNextConfig

    commandsNextExtensions.RegisterCommands(Assembly.GetEntryAssembly())

    let appCommandsConfig =
        ApplicationCommandsConfiguration.FromServiceProvider services

    let appCommandsExtension =
        client.UseApplicationCommands(appCommandsConfig)

    client.add_GuildDownloadCompleted appCommandsExtension.RegisterEvent

    let { Token = killSwitch } = services.GetService<KillSwitch>()

    unitTask {
        do! client.ConnectAsync()
        do! Task.waitUntil 1000<ms> (fun () -> killSwitch.IsCancellationRequested)
        do! client.DisconnectAsync()
    }
