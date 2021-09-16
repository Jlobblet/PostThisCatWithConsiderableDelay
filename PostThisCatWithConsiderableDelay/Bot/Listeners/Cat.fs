module PostThisCatWithConsiderableDelay.Bot.Listeners.Cat

open FSharp.Control.Tasks
open DisCatSharp
open DisCatSharp.EventArgs
open PostThisCatWithConsiderableDelay.Extensions
open PostThisCatWithConsiderableDelay.Models.CatContext
open PostThisCatWithConsiderableDelay.Models.User

let AwardPoints (db: CatContext) (sender: DiscordClient) (args: MessageCreateEventArgs) =
    unitTask {
        let! guild = db.FindAsync<Guild>(args.Guild.Id)
        let guild = Option.ofRef guild
        return!
            match guild with
            | Some g -> args.Channel.SendMessageAsync "well that was an experience"
            | None -> args.Channel.SendMessageAsync "no equality allowed"
    }
