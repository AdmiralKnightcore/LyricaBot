using System;
using System.Collections.Generic;
using System.Linq;
using Lyrica.Data.Users;

namespace Lyrica.Data.GenshinImpact
{
    public class GenshinAccount
    {
        public enum LoginType
        {
            Email,
            Username,
            Twitter,
            Facebook,
            PlayStation
        }

        public Guid Id { get; set; }

        public LoginType AccountType { get; set; }

        public string Username { get; set; }

        public Save? ActiveSave => Saves.LastOrDefault();

        public List<AssociatedAccount> AssociatedAccounts { get; set; } = new List<AssociatedAccount>();

        public List<Save> Saves { get; set; } = new List<Save>();
    }
}