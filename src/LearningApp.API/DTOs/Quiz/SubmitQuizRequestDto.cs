namespace LearningApp.API.DTOs.Quiz;

public class SubmitQuizRequestDto
{
    public List<SubmitQuizAnswerDto> Answers { get; set; } = new();
}
