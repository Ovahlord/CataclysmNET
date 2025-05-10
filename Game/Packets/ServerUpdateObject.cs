using Core.Packets;
using Core.Packets.Opcodes;
using Game.Entities.GameObject;
using Game.Entities.Object;
using Game.Entities.Unit;
using Game.Enums;

namespace Game.Packets
{
    public sealed class ServerUpdateObject : ServerPacket
    {
        public ServerUpdateObject() : base(2048, (int)ServerOpcode.SMSG_UPDATE_OBJECT)
        {
        }

        public sealed class ObjectToUpdate(BaseObject objectToUpdate, UpdateType updateType)
        {
            public BaseObject Object { get; private set; } = objectToUpdate;
            public UpdateType UpdateType { get; private set; } = updateType;
        }

        public ushort MapRecId { get; set; }
        public List<ObjectToUpdate> ObjectsToUpdate { get; private set; } = [];
        public List<ObjectGuid> ObjectsOutOfRange { get; private set; } = [];

        public override ServerPacket Write()
        {
            WriteUInt16(MapRecId);
            WriteUInt32((uint)(ObjectsToUpdate.Count + (ObjectsOutOfRange.Count != 0 ? 1 : 0)));

            // Out of range guids - all entities which have gone out of visibility distance. DestroyObject is handled via extra packet.
            if (ObjectsOutOfRange.Count != 0)
            {
                WriteByte((byte)UpdateType.OutOfRangeObjects);
                WriteUInt32((uint)ObjectsOutOfRange.Count);

                foreach (ObjectGuid guid in ObjectsOutOfRange)
                {
                    WritePackGUID(guid);
                }
            }

            // Updating/Creating of objects
            foreach (ObjectToUpdate objectToUpdate in ObjectsToUpdate)
            {
                WriteByte((byte)objectToUpdate.UpdateType);
                WritePackGUID(objectToUpdate.Object.Guid);
                WriteByte((byte)objectToUpdate.Object.Guid.GetTypeId());

                if (objectToUpdate.Object is WorldObject worldObject)
                    WriteMovement(worldObject);

                WriteUpdateFields(objectToUpdate.Object);
            }

            return this;
        }

        private void WriteMovement(WorldObject worldObject)
        {
            List<uint> pauseTimes = []; // @todo

            WriteBit(worldObject.CreateObjectBits.PlayHoverAnim);
            WriteBit(worldObject.CreateObjectBits.SupressedGreetings);
            WriteBit(worldObject.CreateObjectBits.Rotation);
            WriteBit(worldObject.CreateObjectBits.AnimKit);
            WriteBit(worldObject.CreateObjectBits.CombatVictim);
            WriteBit(worldObject.CreateObjectBits.ThisIsYou);
            WriteBit(worldObject.CreateObjectBits.Vehicle);
            WriteBit(worldObject.CreateObjectBits.MovementUpdate);
            WriteBits((uint)pauseTimes.Count, 24);
            WriteBit(worldObject.CreateObjectBits.NoBirthAnim);
            WriteBit(worldObject.CreateObjectBits.MovementTransport);
            WriteBit(worldObject.CreateObjectBits.Stationary);
            WriteBit(worldObject.CreateObjectBits.AreaTrigger);
            WriteBit(worldObject.CreateObjectBits.EnablePortals);
            WriteBit(worldObject.CreateObjectBits.ServerTime);

            if (worldObject.CreateObjectBits.MovementUpdate)
            {
                WriteBit(worldObject.MovementStatus.MovementFlags0 == MovementFlags0.None);
                WriteBit(worldObject.MovementStatus.Facing != 0f);
                WriteBit(worldObject.Guid[7]);
                WriteBit(worldObject.Guid[3]);
                WriteBit(worldObject.Guid[2]);

                if (worldObject.MovementStatus.MovementFlags0 != MovementFlags0.None)
                    WriteBits((uint)worldObject.MovementStatus.MovementFlags0, 30);

                WriteBit(false); // hasSpline && !self->IsPlayer() - !Has player spline data
                WriteBit(true); // !Has pitch
                WriteBit(false); // Has spline data (independent)
                WriteBit(worldObject.MovementStatus.Fall != null);
                WriteBit(true); // !Has spline elevation
                WriteBit(worldObject.Guid[5]);
                WriteBit(worldObject.MovementStatus.Transport != null);
                WriteBit(worldObject.MovementStatus.MoveTime == 0);

                if (worldObject.MovementStatus.Transport != null)
                {
                    WriteBit(worldObject.MovementStatus.Transport.Guid[1]);
                    WriteBit(worldObject.MovementStatus.Transport.PrevMoveTime.HasValue);
                    WriteBit(worldObject.MovementStatus.Transport.Guid[4]);
                    WriteBit(worldObject.MovementStatus.Transport.Guid[0]);
                    WriteBit(worldObject.MovementStatus.Transport.Guid[6]);
                    WriteBit(worldObject.MovementStatus.Transport.VehicleRecID.HasValue);
                    WriteBit(worldObject.MovementStatus.Transport.Guid[7]);
                    WriteBit(worldObject.MovementStatus.Transport.Guid[5]);
                    WriteBit(worldObject.MovementStatus.Transport.Guid[3]);
                    WriteBit(worldObject.MovementStatus.Transport.Guid[2]);
                }

                WriteBit(worldObject.Guid[4]);

                //if (hasSpline)
                //    Movement::PacketBuilder::WriteCreateBits(*self->movespline, *data);

                WriteBit(worldObject.Guid[6]);

                if (worldObject.MovementStatus.Fall != null)
                    WriteBit(worldObject.MovementStatus.Fall.Velocity != null);

                WriteBit(worldObject.Guid[0]);
                WriteBit(worldObject.Guid[1]);
                WriteBit(worldObject.MovementStatus.HeightChangeFailed);
                WriteBit(worldObject.MovementStatus.MovementFlags1 == MovementFlags1.None);

                if (worldObject.MovementStatus.MovementFlags1 != MovementFlags1.None)
                    WriteBits((uint)worldObject.MovementStatus.MovementFlags1, 12);
            }

            if (worldObject.CreateObjectBits.MovementTransport)
            {
                if (worldObject.MovementStatus.Transport == null)
                    throw new NullReferenceException($"WorldObject (GUID: {worldObject.Guid})" +$" was flagged for sending transport data in SMSG_UPDATE_OBJECT but it had no transport reference!");

                WriteBit(worldObject.MovementStatus.Transport.Guid[5]);
                WriteBit(worldObject.MovementStatus.Transport.VehicleRecID.HasValue);
                WriteBit(worldObject.MovementStatus.Transport.Guid[0]);
                WriteBit(worldObject.MovementStatus.Transport.Guid[3]);
                WriteBit(worldObject.MovementStatus.Transport.Guid[6]);
                WriteBit(worldObject.MovementStatus.Transport.Guid[1]);
                WriteBit(worldObject.MovementStatus.Transport.Guid[4]);
                WriteBit(worldObject.MovementStatus.Transport.Guid[2]);
                WriteBit(worldObject.MovementStatus.Transport.PrevMoveTime.HasValue);
                WriteBit(worldObject.MovementStatus.Transport.Guid[7]);
            }

            if (worldObject.CreateObjectBits.CombatVictim)
            {
                // @todo
                //ObjectGuid guid = ObjectGuid.Empty;
                //WriteBit(guid[2]);
                //WriteBit(guid[7]);
                //WriteBit(guid[0]);
                //WriteBit(guid[4]);
                //WriteBit(guid[5]);
                //WriteBit(guid[6]);
                //WriteBit(guid[1]);
                //WriteBit(guid[3]);
            }

            if (worldObject.CreateObjectBits.AnimKit)
            {
                WriteBit(true); // !hasAIAnimKit
                WriteBit(true); // !hasMovementAnimKit
                WriteBit(true); // !hasMeleeAnimKit
            }

            FlushBits();

            foreach (uint pauseTime in pauseTimes)
            {
                WriteUInt32(pauseTime);
            }

            if (worldObject.CreateObjectBits.MovementUpdate)
            {
                if (worldObject is not Unit unit)
                    throw new Exception($"CreateObjectBits.MovementUpdate can only be used for Units but has been used incorrectly for WorldObject (GUID: {worldObject.Guid})");

                WriteByteSeq(worldObject.Guid[4]);
                WriteFloat(unit.Stats.RunBackSpeed);

                if (worldObject.MovementStatus.Fall != null)
                {
                    if (worldObject.MovementStatus.Fall.Velocity != null)
                    {
                        WriteFloat(worldObject.MovementStatus.Fall.Velocity.Speed);
                        WriteFloat(worldObject.MovementStatus.Fall.Velocity.Direction.X);
                        WriteFloat(worldObject.MovementStatus.Fall.Velocity.Direction.Y);
                    }

                    WriteUInt32(worldObject.MovementStatus.Fall.Time);
                    WriteFloat(worldObject.MovementStatus.Fall.JumpVelocity);
                }

                WriteFloat(unit.Stats.SwimBackSpeed);

                // if (hasSplineElevation)
                //     WriteFloat(worldObject.MovementStatus.StepUpStartElevation);

                //  if (hasSpline)
                //      Movement::PacketBuilder::WriteCreateData(*self->movespline, *data);

                WriteFloat(worldObject.MovementStatus.Position.Z);
                WriteByteSeq(worldObject.Guid[5]);

                if (worldObject.MovementStatus.Transport != null)
                {
                    WriteByteSeq(worldObject.MovementStatus.Transport.Guid[5]);
                    WriteByteSeq(worldObject.MovementStatus.Transport.Guid[7]);
                    WriteUInt32(worldObject.MovementStatus.Transport.MoveTime);
                    WriteFloat(worldObject.MovementStatus.Transport.Facing);

                    if (worldObject.MovementStatus.Transport.PrevMoveTime.HasValue)
                        WriteUInt32(worldObject.MovementStatus.Transport.PrevMoveTime.Value);

                    WriteFloat(worldObject.MovementStatus.Transport.Position.Y);
                    WriteFloat(worldObject.MovementStatus.Transport.Position.X);
                    WriteByteSeq(worldObject.MovementStatus.Transport.Guid[3]);
                    WriteFloat(worldObject.MovementStatus.Transport.Position.Z);
                    WriteByteSeq(worldObject.MovementStatus.Transport.Guid[0]);

                    if (worldObject.MovementStatus.Transport.VehicleRecID.HasValue)
                        WriteInt32(worldObject.MovementStatus.Transport.VehicleRecID.Value);

                    WriteByte(worldObject.MovementStatus.Transport.VehicleSeatIndex);
                    WriteByteSeq(worldObject.MovementStatus.Transport.Guid[1]);
                    WriteByteSeq(worldObject.MovementStatus.Transport.Guid[6]);
                    WriteByteSeq(worldObject.MovementStatus.Transport.Guid[2]);
                    WriteByteSeq(worldObject.MovementStatus.Transport.Guid[4]);
                }

                WriteFloat(worldObject.MovementStatus.Position.X);
                WriteFloat(unit.Stats.PitchRate);
                WriteByteSeq(worldObject.Guid[3]);
                WriteByteSeq(worldObject.Guid[0]);
                WriteFloat(unit.Stats.SwimSpeed);
                WriteFloat(worldObject.MovementStatus.Position.Y);
                WriteByteSeq(worldObject.Guid[7]);
                WriteByteSeq(worldObject.Guid[1]);
                WriteByteSeq(worldObject.Guid[2]);
                WriteFloat(unit.Stats.WalkSpeed);

                if (worldObject.MovementStatus.MoveTime != 0)
                    WriteUInt32(worldObject.MovementStatus.MoveTime);

                WriteFloat(unit.Stats.TurnRate);
                WriteByteSeq(worldObject.Guid[6]);
                WriteFloat(unit.Stats.FlightSpeed);

                if (worldObject.MovementStatus.Facing != 0f)
                    WriteFloat(worldObject.MovementStatus.Facing);

                WriteFloat(unit.Stats.RunSpeed);

                // if (Has pitch)
                //WriteFloat(worldObject.MovementStatus.Pitch);

                WriteFloat(unit.Stats.FlightBackSpeed);
            }

            if (worldObject.CreateObjectBits.Vehicle)
            {
                if (worldObject.MovementStatus.Transport != null)
                    WriteFloat(worldObject.MovementStatus.Transport.Facing);
                else
                    WriteFloat(worldObject.MovementStatus.Facing);

                WriteUInt32(0); // GetVehicleKit()->GetVehicleInfo()->ID
            }

            if (worldObject.CreateObjectBits.MovementTransport)
            {
                if (worldObject.MovementStatus.Transport == null)
                    throw new NullReferenceException($"WorldObject (GUID: {worldObject.Guid})" + $" was flagged for sending transport data in SMSG_UPDATE_OBJECT but it had no transport reference!");

                WriteByteSeq(worldObject.MovementStatus.Transport.Guid[0]);
                WriteByteSeq(worldObject.MovementStatus.Transport.Guid[5]);

                if (worldObject.MovementStatus.Transport.VehicleRecID.HasValue)
                    WriteInt32(worldObject.MovementStatus.Transport.VehicleRecID.Value);

                WriteByteSeq(worldObject.MovementStatus.Transport.Guid[3]);
                WriteFloat(worldObject.MovementStatus.Transport.Position.X);
                WriteByteSeq(worldObject.MovementStatus.Transport.Guid[4]);
                WriteByteSeq(worldObject.MovementStatus.Transport.Guid[6]);
                WriteByteSeq(worldObject.MovementStatus.Transport.Guid[1]);
                WriteUInt32(worldObject.MovementStatus.Transport.MoveTime);
                WriteFloat(worldObject.MovementStatus.Transport.Position.Y);
                WriteByteSeq(worldObject.MovementStatus.Transport.Guid[2]);
                WriteByteSeq(worldObject.MovementStatus.Transport.Guid[7]);
                WriteFloat(worldObject.MovementStatus.Transport.Position.Z);
                WriteByte(worldObject.MovementStatus.Transport.VehicleSeatIndex);
                WriteFloat(worldObject.MovementStatus.Transport.Facing);

                if (worldObject.MovementStatus.Transport.PrevMoveTime.HasValue)
                    WriteUInt32(worldObject.MovementStatus.Transport.PrevMoveTime.Value);
            }

            if (worldObject.CreateObjectBits.Rotation)
            {
                if (worldObject is not GameObject gameObject)
                    throw new Exception($"WorldObject (GUID: {worldObject.Guid}) has CreateObjectBits.Rotation but is not a gameObject!");

                WriteInt64(gameObject.PackedLocalRotation);
            }

            if (worldObject.CreateObjectBits.AreaTrigger)
            {
                // Unused by the client (wasn't ready in 4.3.4)
                WriteFloat(0f);
                WriteFloat(0f);
                WriteFloat(0f);
                WriteFloat(0f);
                WriteByte(0);
                WriteFloat(0f);
                WriteFloat(0f);
                WriteFloat(0f);
                WriteFloat(0f);
                WriteFloat(0f);
                WriteFloat(0f);
                WriteFloat(0f);
                WriteFloat(0f);
                WriteFloat(0f);
                WriteFloat(0f);
                WriteFloat(0f);
                WriteFloat(0f);
            }

            if (worldObject.CreateObjectBits.Stationary)
            {
                WriteFloat(worldObject.MovementStatus.Facing);
                WriteFloat(worldObject.MovementStatus.Position.X);
                WriteFloat(worldObject.MovementStatus.Position.Y);
                WriteFloat(worldObject.MovementStatus.Position.Z);
            }

            if (worldObject.CreateObjectBits.CombatVictim)
            {
                // @todo
                //ObjectGuid guid = ObjectGuid.Empty;
                //WriteByteSeq(guid[4]);
                //WriteByteSeq(guid[0]);
                //WriteByteSeq(guid[3]);
                //WriteByteSeq(guid[5]);
                //WriteByteSeq(guid[7]);
                //WriteByteSeq(guid[6]);
                //WriteByteSeq(guid[2]);
                //WriteByteSeq(guid[1]);
            }

            if (worldObject.CreateObjectBits.AnimKit)
            {
                // @todo
                // if (hasAIAnimKit)
                //    WriteUInt16(self->GetAIAnimKitId());

                //if (hasMovementAnimKit)
                //    WriteUInt16(self->GetMovementAnimKitId());

                //if (hasMeleeAnimKit)
                //    WriteUInt16(self->GetMeleeAnimKitId());
            }

            if (worldObject.CreateObjectBits.ServerTime)
                WriteUInt32(0); // GameTime::GetGameTimeMS() @todo
        }

        private void WriteUpdateFields(BaseObject baseObject)
        {
        }
    }
}
