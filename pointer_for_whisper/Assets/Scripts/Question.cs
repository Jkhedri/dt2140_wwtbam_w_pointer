[System.Serializable]
public class Question
{
    public string question;
    public string[] content;
    public int correct;

    public bool IsCorrectAnswer(int index)
    {
        return index == correct;
    }
}