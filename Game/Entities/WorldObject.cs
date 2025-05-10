using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Entities
{
    public abstract class WorldObject(ObjectGuid guid) : BaseObject(guid)
    {
        public WorldPosition Position { get; private set; } = new();

    }
}
