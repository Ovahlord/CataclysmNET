namespace Game.Entities.Object
{
    public abstract class BaseObject(ObjectGuid guid)
    {
        public ObjectGuid Guid { get; } = guid;
    }
}
