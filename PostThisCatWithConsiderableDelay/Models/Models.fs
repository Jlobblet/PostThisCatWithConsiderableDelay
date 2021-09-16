module PostThisCatWithConsiderableDelay.Models.User

open System
open System.ComponentModel.DataAnnotations

[<CLIMutable; NoComparison>]
type User =
    { [<Key>]
      UserId: uint64
      [<Key>]
      GuildId: uint64
      [<Required>]
      Points: int64
      Posts: ResizeArray<Post> }

and [<CLIMutable; NoComparison>] Post =
    { [<Key>]
      PostId: Guid
      User: User
      [<Required>]
      Timestamp: DateTime }
