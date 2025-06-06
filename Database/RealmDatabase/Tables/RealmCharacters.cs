﻿using Microsoft.EntityFrameworkCore;

namespace Database.RealmDatabase.Tables
{
    [PrimaryKey(nameof(RealmId), nameof(GameAccountId), nameof(CharacterId))]
    public sealed class RealmCharacters
    {
        public int RealmId { get; set; }
        public int GameAccountId { get; set; }
        public byte ListPosition { get; set; }

        public int CharacterId { get; set; }
        public Characters? Character { get; set; }
    }
}
