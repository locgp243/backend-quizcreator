using System;
using System.Collections.Generic;

namespace backendV1.Models;

public partial class QuizDetail
{
    public int? QuizId { get; set; }

    public int QuizDetailId { get; set; }

    public string? Question { get; set; }

    public string? Option1 { get; set; }

    public string? Option2 { get; set; }

    public string? Option3 { get; set; }

    public string? Option4 { get; set; }

    public int? CorrectAnswer { get; set; }

    public string? Avatar { get; set; }

    public virtual Quiz? Quiz { get; set; }
}
