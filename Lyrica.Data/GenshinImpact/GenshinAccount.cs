using System;
using System.Collections.Generic;

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

        public List<AssociatedAccount> AssociatedAccounts { get; set; } = new List<AssociatedAccount>();

        public List<Save> Saves { get; set; } = new List<Save>();
    }
}