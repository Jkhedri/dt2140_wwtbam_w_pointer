using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Meta.WitAi.TTS.Utilities;
using OpenAI;
using TMPro;

public class HelpManager : MonoBehaviour
{
    [SerializeField] private TTSSpeaker _speaker;
    public UIManager uiManager;
    

    [Header("UI")] 
    public Button button;
    public Text buttonText;
    public Text outputText;
    public Text timeText;
    public ScrollRect scroll;
    private OpenAIApi openAI = new OpenAIApi("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", "org-tlIiHtsp9GuYJOmwUIeyQMDb");
    private List<ChatMessage> messages = new List<ChatMessage>();

    private int uses = 3;

    private void Awake()
    {    
        button.onClick.AddListener(OnButtonPressed);
    }

private void OnButtonPressed()
{
    buttonText.text = "Helping...";

    if (uses > 0)
    {
        uses--;
        // Change buttontext to "Help (uses left: uses)"
        buttonText.text = "Help (" + uses + " uses)";
    }
    else
    {
        buttonText.text = "No more uses";
        return;
    }
    outputText.text = "Thinking...";

    var question = uiManager.questionText.text;
    var text = "Question: " + question + " Answers: ";
    var first = true;

    foreach (var button in uiManager.answerButtons)
    {
        if (first)
        {
            first = false;
        }
        else
        {
            text += "||ALTERNATIVE_SEPARATOR||";
        }
        text += button.GetComponentInChildren<TextMeshProUGUI>().text + " ";
    }

    
    AskChatGPT(text);
}


    public async void AskChatGPT(string newText)
    {
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = "Hi! You are the friend used in the 'Call a friend' option in 'Who Wants to be a Millionaire?'. Answer the following question, possibly with given alternatives, as such and make sure to be friendly (You are a friend to the contestant). Do NOT repeat the whole question, possibly with alternatives, in your response. You can however motivate your answer if you want. " + newText;
        newMessage.Role = "user";

        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-3.5-turbo";

        var response = await openAI.CreateChatCompletion(request);
        //UnityEngine.Debug.Log(response);
        //Debug.Log(response);

        if(response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            
            messages.Add(chatResponse);

            print(chatResponse.Content);

            outputText.text = chatResponse.Content;

            // split chatResponse.Content into segments of 20 words if it is longer than 20 words
            // if it is shorter than 20 words, just send it to the tts
            string[] words = chatResponse.Content.Split(' ');
            int numSegments = words.Length / 20;
            if(words.Length % 20 != 0)
            {
                numSegments++;
            }
            if(numSegments > 1)
            {
                string[] segments = new string[numSegments];
                for(int i = 0; i < numSegments; i++)
                {
                    string segment = "";
                    for(int j = 0; j < 20; j++)
                    {
                        if(i * 20 + j < words.Length)
                        {
                            segment += words[i * 20 + j] + " ";
                        }
                    }
                    segments[i] = segment;
                }
                bool first = true;
                foreach(string segment in segments)
                {
                    if (first)
                    {
                        _speaker.Speak(segment);
                        first = false;
                    }
                    else
                    {
                        _speaker.SpeakQueued(segment);
                    }
                }
            }
            else
            {
                _speaker.Speak(chatResponse.Content);
            }

            
        }
        else
        {
            outputText.text = "Sorry, I don't know what to say.";
            
            _speaker.Speak("Sorry, I don't know what to say.");
        }
    }

}

