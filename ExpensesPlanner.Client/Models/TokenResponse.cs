﻿namespace ExpensesPlanner.Client.Models
{
    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; } = DateTime.MinValue;
    }
}
