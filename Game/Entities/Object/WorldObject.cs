namespace Game.Entities.Object
{
    public abstract class WorldObject(ObjectGuid guid) : BaseObject(guid)
    {
        public readonly CreateObjectBits CreateObjectBits = new();
        public readonly MovementStatus MovementStatus = new();
    }
}
