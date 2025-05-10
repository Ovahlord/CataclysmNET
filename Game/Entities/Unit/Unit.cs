using Game.Entities.Object;
using Game.Enums;

namespace Game.Entities.Unit
{
    public abstract class Unit(ObjectGuid guid) : WorldObject(guid)
    {
        public readonly Stats Stats = new();
    }
}
