namespace Database.LoginDatabase.Tables
{
    public sealed class GameAccounts
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public byte[] Salt { get; set; } = [];
        public byte[] Verifier { get; set; } = [];
        public byte[] SessionKey { get; set; } = [];
        public byte ExpansionLevel { get; set; } = 0;
    }
}
