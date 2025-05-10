using Core.Packets;
using Core.Packets.Opcodes;
using Game.Entities;
using Game.Enums;
using System;

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

        }

        private void WriteUpdateFields(BaseObject baseObject) { }
    }
}
