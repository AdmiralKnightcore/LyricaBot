using System;

namespace Lyrica.Data.GenshinImpact
{
    public class AssociatedAccount
    {
        public enum Platform
        {
            Facebook,
            Twitter,
            Google,
            Apple,
            GameCenter
        }

        public Guid Id { get; set; }

        public Platform AccountType { get; set; }

        public string Username { get; set; }
    }
}