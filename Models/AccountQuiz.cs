using System;
using System.Collections.Generic;

namespace backendV1.Models;

public partial class AccountQuiz
{
    public int? AccountId { get; set; }

    public int? QuizId { get; set; }

    public int? Point { get; set; }

    public int AccountQuizId { get; set; }

    public virtual Account? Account { get; set; }

    public virtual Quiz? Quiz { get; set; }
}
