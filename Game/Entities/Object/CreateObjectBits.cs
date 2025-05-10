using System.Collections;

namespace Game.Entities.Object
{
    public sealed class CreateObjectBits
    {
        private readonly BitArray _bits = new(14);

        public bool PlayHoverAnim       { get { return _bits[0]; } set { _bits[0] = value; } }
        public bool SupressedGreetings  { get { return _bits[1]; } set { _bits[1] = value; } }
        public bool Rotation            { get { return _bits[2]; } set { _bits[2] = value; } }
        public bool AnimKit             { get { return _bits[3]; } set { _bits[3] = value; } }
        public bool CombatVictim        { get { return _bits[4]; } set { _bits[4] = value; } }
        public bool ThisIsYou           { get { return _bits[5]; } set { _bits[5] = value; } }
        public bool Vehicle             { get { return _bits[6]; } set { _bits[6] = value; } }
        public bool MovementUpdate      { get { return _bits[7]; } set { _bits[7] = value; } }
        public bool NoBirthAnim         { get { return _bits[8]; } set { _bits[8] = value; } }
        public bool MovementTransport   { get { return _bits[9]; } set { _bits[9] = value; } }
        public bool Stationary          { get { return _bits[10]; } set { _bits[10] = value; } }
        public bool AreaTrigger         { get { return _bits[11]; } set { _bits[11] = value; } }
        public bool EnablePortals       { get { return _bits[12]; } set { _bits[12] = value; } }
        public bool ServerTime          { get { return _bits[13]; } set { _bits[13] = value; } }
    }
}
