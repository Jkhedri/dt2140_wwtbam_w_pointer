using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;
using Meta.WitAi.TTS.Utilities;
using OpenAI;

namespace Whisper.Samples
{
    /// <summary>
    /// Record audio clip from microphone and make a transcription.
    /// </summary>
    public class MicrophoneDemo : MonoBehaviour
    {
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public bool streamSegments = true;
        public bool printLanguage = true;
        [SerializeField] private TTSSpeaker _speaker;

        [Header("UI")] 
        public Button button;
        public Text buttonText;
        public Text outputText;
        public Text timeText;
        public Dropdown languageDropdown;
        public Toggle translateToggle;
        public Toggle vadToggle;
        public ScrollRect scroll;
        
        private string _buffer;

        private int uses = 5;
        private OpenAIApi openAI = new OpenAIApi("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", "org-tlIiHtsp9GuYJOmwUIeyQMDb");
        private List<ChatMessage> messages = new List<ChatMessage>();

        private void Awake()
        {
            whisper.OnNewSegment += OnNewSegment;
            whisper.OnProgress += OnProgressHandler;
            
            microphoneRecord.OnRecordStop += OnRecordStop;
            
            button.onClick.AddListener(OnButtonPressed);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            languageDropdown.value = 1;
            OnLanguageChanged(1);

            translateToggle.isOn = whisper.translateToEnglish;
            translateToggle.onValueChanged.AddListener(OnTranslateChanged);

            vadToggle.isOn = microphoneRecord.vadStop;
            vadToggle.onValueChanged.AddListener(OnVadChanged);
            buttonText.text = "Record (" + uses + " uses)";
        }

        private void OnVadChanged(bool vadStop)
        {
            microphoneRecord.vadStop = vadStop;
        }

        private void OnButtonPressed()
        {
            if (!microphoneRecord.IsRecording)
            {
                if (uses > 0)
                {
                    uses--;
                }
                else
                {
                    buttonText.text = "No more uses";
                    return;
                }
                outputText.text = "Listening...";
                microphoneRecord.StartRecord();
                buttonText.text = "Stop";
            }
            else
            {
                microphoneRecord.StopRecord();
                if (uses > 0)
                {
                    buttonText.text = "Record (" + uses + " uses)";
                }
                else
                {
                    buttonText.text = "No more uses";
                    return;
                }

            }
        }
        
        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            buttonText.text = "Record";
            _buffer = "";

            outputText.text = "Processing...";
            var sw = new Stopwatch();
            sw.Start();
            
            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null || !outputText) 
                return;

            var time = sw.ElapsedMilliseconds;
            var rate = recordedAudio.Length / (time * 0.001f);
            timeText.text = $"Time: {time} ms\nRate: {rate:F1}x";

            var text = res.Result;
            AskChatGPT(text);
            if (printLanguage)
                text += $"\n\nLanguage: {res.Language}";
            
            ///outputText.text = chatGPTResponse;
            ///UiUtils.ScrollDown(scroll);
        }
        
        private void OnLanguageChanged(int ind)
        {
            var opt = languageDropdown.options[ind];
            whisper.language = opt.text;
        }
        
        private void OnTranslateChanged(bool translate)
        {
            whisper.translateToEnglish = translate;
        }

        private void OnProgressHandler(int progress)
        {
            if (!timeText)
                return;
            timeText.text = $"Progress: {progress}%";
        }
        
        private void OnNewSegment(WhisperSegment segment)
        {
            if (!streamSegments || !outputText)
                return;

            _buffer += segment.Text;
            outputText.text = _buffer + "...";
            UiUtils.ScrollDown(scroll);
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
                UiUtils.ScrollDown(scroll);

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
                uses++;
                UiUtils.ScrollDown(scroll);
                _speaker.Speak("Sorry, I don't know what to say.");
            }
        }

    }
}