using System;
using System.Collections.Generic;

namespace backendV1.Models;

public partial class Quiz
{
    public int QuizId { get; set; }

    public string? QuizTitle { get; set; }

    public string? QuizDesc { get; set; }

    public string? CreateBy { get; set; }

    //public DateOnly? CreateTime { get; set; }

    public virtual ICollection<AccountQuiz> AccountQuizzes { get; set; } = new List<AccountQuiz>();

    public virtual ICollection<QuizDetail> QuizDetails { get; set; } = new List<QuizDetail>();
}
