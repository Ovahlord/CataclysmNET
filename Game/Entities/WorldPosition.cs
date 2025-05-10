namespace Game.Entities
{
    public sealed class WorldPosition
    {
        int MapRecId { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float FacingAngle { get; set; }
    }
}
