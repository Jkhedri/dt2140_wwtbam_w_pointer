using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class QuestionLoader
{

    public static List<Question> LoadQuestions(TextAsset jsonAsset)
    {
        QuestionCollection questionCollection = JsonUtility.FromJson<QuestionCollection>(jsonAsset.text);
        
        List<Question> allQuestions = new List<Question>();

        // Iterate through all games and collect their questions
        foreach (Game game in questionCollection.games)
        {
            allQuestions.AddRange(game.questions);
        }

        return allQuestions;
    }

    [System.Serializable]
    private class QuestionCollection
    {
        public Game[] games;
    }

    [System.Serializable]
    private class Game
    {
        public Question[] questions;
    }
}