using System;
using System.Collections.Generic;

namespace DevPulse.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";

        // navs
        public List<ProviderConnection> ProviderConnections { get; set; } = [];
        public List<Repository> Repositories { get; set; } = [];
        public List<Issue> Issues { get; set; } = [];
    }
}