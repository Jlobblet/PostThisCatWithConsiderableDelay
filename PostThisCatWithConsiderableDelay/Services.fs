module PostThisCatWithConsiderableDelay.Services

open System.Threading
open Microsoft.EntityFrameworkCore.Design
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open PostThisCatWithConsiderableDelay.Models.CatContext
open PostThisCatWithConsiderableDelay.Settings

type KillSwitch =
    { Token: CancellationToken }
    static member Create() = { Token = CancellationToken(false) }

let ConfigureServices (settings: Settings) (services: IServiceCollection) =
    services
        .AddSingleton<Settings>(settings)
        .AddSingleton<KillSwitch>(KillSwitch.Create())
        .AddDbContext<CatContext>(ServiceLifetime.Transient)
        .BuildServiceProvider()

type CatContextFactory() =
    interface IDesignTimeDbContextFactory<CatContext> with
        member this.CreateDbContext(args) =
            new CatContext(Settings.getConnectionString ())
