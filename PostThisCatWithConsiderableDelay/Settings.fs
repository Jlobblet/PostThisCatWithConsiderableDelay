module PostThisCatWithConsiderableDelay.Settings

open System
open Argu

[<NoComparison>]
type Arguments =
    | [<ExactlyOnce; NoCommandLine>] ConnectionString of ConnectionString: string
    | [<ExactlyOnce>] DiscordToken of DiscordToken: string
    | [<ExactlyOnce>] CatUrl of CatUrl: string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | ConnectionString _ -> ""
            | DiscordToken _ -> ""
            | CatUrl _ -> ""

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
      DiscordToken: string
      CatUrl: string }


[<RequireQualifiedAccess>]
module Settings =
    [<Literal>]
    let private config = "App.Config"

    let fromArgv argv =
        let results =
            Parser.Parse(inputs = argv, configurationReader = ConfigurationReader.FromAppSettingsFile(config))

        { ConnectionString = results.GetResult <@ ConnectionString @>
          DiscordToken = results.GetResult <@ DiscordToken @>
          CatUrl = results.GetResult <@ CatUrl @> }

    let getConnectionString () =
        Parser
            .Parse(ignoreUnrecognized = true, configurationReader = ConfigurationReader.FromAppSettingsFile(config))
            .GetResult <@ ConnectionString @>
