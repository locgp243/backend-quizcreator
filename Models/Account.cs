using System;
using System.Collections.Generic;

namespace backendV1.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }

    public string? Avatar { get; set; }

    public ICollection<AccountQuiz> AccountQuizzes { get; set; }
}
