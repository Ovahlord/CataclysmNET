using Game.Enums;
using System.Numerics;


namespace Game.Entities.Object
{

    public sealed class MovementTransport
    {
        public ObjectGuid Guid { get; set; } = ObjectGuid.Empty;
        public Vector3 Position { get; set; }
        public float Facing { get; set; }
        public byte VehicleSeatIndex { get; set; }
        public uint MoveTime { get; set; }
        public uint? PrevMoveTime { get; set; }
        public int? VehicleRecID { get; set; }
    }

    public sealed class MovementFallVelocity
    {
        public Vector2 Direction { get; set; }
        public float Speed { get; set; }
    }

    public sealed class MovementFallOrLand
    {
        public uint Time { get; set; }
        public float JumpVelocity { get; set; }
        public MovementFallVelocity? Velocity { get; set; }
    }

    public sealed class MovementStatus
    {
        public ObjectGuid MoverGUID { get; set; } = ObjectGuid.Empty;
        public MovementFlags0 MovementFlags0 { get; set; }
        public MovementFlags1 MovementFlags1 { get; set; }
        public uint MoveTime { get; set; }
        public uint MoveIndex { get; set; }
        public float Facing { get; set; }
        public float Pitch { get; set; }
        public float StepUpStartElevation { get; set; }
        public bool HasSpline { get; set; }
        public bool HeightChangeFailed { get; set; }
        public bool RemoteTimeValid { get; set; }
        public Vector3 Position { get; set; }
        public MovementTransport? Transport { get; set; }
        public MovementFallOrLand? Fall {  get; set; }
    }
}
