module PostThisCatWithConsiderableDelay.Models.Models

open System
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema

[<CLIMutable; NoComparison>]
type User =
    { [<Key; DatabaseGenerated(DatabaseGeneratedOption.None)>]
      UserId: uint64 }

[<CLIMutable; NoComparison>]
type Guild =
    { [<Key; DatabaseGenerated(DatabaseGeneratedOption.None)>]
      GuildId: uint64
      CatChannel: uint64 }

[<CLIMutable; NoComparison>]
type Post =
    { [<Key; DatabaseGenerated(DatabaseGeneratedOption.None)>]
      PostId: Guid
      [<Required>]
      Timestamp: DateTime
      [<ForeignKey("User"); DatabaseGenerated(DatabaseGeneratedOption.None)>]
      UserId: uint64
      User: User
      [<ForeignKey("Guild"); DatabaseGenerated(DatabaseGeneratedOption.None)>]
      GuildId: uint64
      Guild: Guild }

and [<CLIMutable; NoComparison>] Points =
    { [<Key; DatabaseGenerated(DatabaseGeneratedOption.None)>]
      PointsId: Guid
      [<ForeignKey("User"); DatabaseGenerated(DatabaseGeneratedOption.None)>]
      UserId: uint64
      User: User
      [<ForeignKey("Guild"); DatabaseGenerated(DatabaseGeneratedOption.None)>]
      GuildId: uint64
      Guild: Guild
      [<Required>]
      Points: int64
      Posts: ResizeArray<Post> }
