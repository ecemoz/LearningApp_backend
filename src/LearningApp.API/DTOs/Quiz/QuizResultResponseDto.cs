using System;
using System.Collections.Generic;

namespace LearningApp.API.DTOs.Quiz;

public class QuizResultResponseDto
{
    public Guid QuizId { get; set; }

    public int Score { get; set; }

    public int CorrectCount { get; set; }

    public int TotalQuestionCount { get; set; }

    public int AttemptCount { get; set; }

    public string Message { get; set; } = string.Empty;

    public List<QuestionResultDto> QuestionResults { get; set; } = new();
}

public class QuestionResultDto
{
    public Guid QuestionId { get; set; }
    public bool? IsCorrect { get; set; }
    public Guid? CorrectOptionId { get; set; }
}
