using Game.Entities.Object;

namespace Game.Entities.Unit.Player
{
    /// <summary>
    /// The primary class of the player character entity. All player specific methods and logic goes in here.
    /// </summary>
    /// <param name="guid"></param>
    public sealed class Player(ObjectGuid guid) : Unit(guid)
    {
    }
}
