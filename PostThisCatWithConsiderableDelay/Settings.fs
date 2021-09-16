module PostThisCatWithConsiderableDelay.Settings

open System
open Argu

[<NoComparison>]
type Arguments =
    | [<ExactlyOnce; NoCommandLine>] ConnectionString of ConnectionString: string
    | [<ExactlyOnce>] DiscordToken of DiscordToken: string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | ConnectionString _ -> ""
            | DiscordToken _ -> ""

let errorHandler =
    ProcessExiter(
        colorizer =
            function
            | ErrorCode.HelpText -> None
            | _ -> Some ConsoleColor.Red
    )

let Parser =
    ArgumentParser.Create<Arguments>(errorHandler = errorHandler)

[<NoComparison>]
type Settings =
    { ConnectionString: string
      DiscordToken: string }


[<RequireQualifiedAccess>]
module Settings =
    let fromArgv argv =
        let results =
            Parser.Parse(inputs = argv, configurationReader = ConfigurationReader.FromAppSettingsFile("App.Config"))

        { ConnectionString = results.GetResult <@ ConnectionString @>
          DiscordToken = results.GetResult <@ DiscordToken @> }

    let getConnectionString () =
        Parser
            .Parse(
                ignoreUnrecognized = true,
                configurationReader = ConfigurationReader.FromAppSettingsFile("App.Config")
            )
            .GetResult <@ ConnectionString @>
