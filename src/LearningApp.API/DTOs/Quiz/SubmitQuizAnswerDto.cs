namespace LearningApp.API.DTOs.Quiz;

public class SubmitQuizAnswerDto
{
    public Guid QuestionId { get; set; }

    public Guid SelectedOptionId { get; set; }
}
