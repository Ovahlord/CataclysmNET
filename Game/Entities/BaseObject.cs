namespace Game.Entities
{
    public abstract class BaseObject(ObjectGuid guid)
    {
        public ObjectGuid Guid { get; } = guid;
    }
}
