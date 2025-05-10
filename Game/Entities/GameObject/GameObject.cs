using Game.Entities.Object;
using System.Numerics;

namespace Game.Entities.GameObject
{
    public class GameObject(ObjectGuid guid) : WorldObject(guid)
    {
        private Quaternion _localRotation;
        public Quaternion LocalRotation
        {
            get { return _localRotation; }
            private set
            {
                _localRotation = value;
                UpdatePackedLocalRotation();
            }
        }

        public long PackedLocalRotation { get; private set; }

        private void UpdatePackedLocalRotation()
        {
            int PACK_YZ = 1 << 20;
            int PACK_X = PACK_YZ << 1;
            int PACK_YZ_MASK = (PACK_YZ << 1) - 1;
            int PACK_X_MASK = (PACK_X << 1) - 1;

            sbyte w_sign = (sbyte)(LocalRotation.W >= 0f ? 1 : -1);
            long x = (int)(LocalRotation.X * PACK_X) * w_sign & PACK_X_MASK;
            long y = (int)(LocalRotation.X * PACK_YZ) * w_sign & PACK_YZ_MASK;
            long z = (int)(LocalRotation.X * PACK_YZ) * w_sign & PACK_YZ_MASK;

            PackedLocalRotation = z | (y << 21) | (x << 42);
        }
    }
}
