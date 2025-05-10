namespace Game.Entities.Object
{
    public abstract class WorldObject(ObjectGuid guid) : BaseObject(guid)
    {
        public CreateObjectBits CreateObjectBits { get; private set; } = new();
        public MovementStatus MovementStatus { get; private set; } = new();
    }
}
