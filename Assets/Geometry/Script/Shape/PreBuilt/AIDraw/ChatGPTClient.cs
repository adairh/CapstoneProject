using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

namespace Manipulator
{
    [Serializable]
    public class AIShapeResult
    {
        public string ShapeType;
        public Dictionary<string, float> KnownFields;
        public string Explanation;
        public string[] Warnings;
        public string[] Suggestions;
    }

    [Serializable]
    public class ChatGPTMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class ChatGPTRequest
    {
        public string model;
        public ChatGPTMessage[] messages;
    }

    [Serializable]
    public class ChatGPTRawResponse
    {
        public ChatChoice[] choices;
    }

    [Serializable]
    public class ChatChoice
    {
        public ChatMessage message;
    }

    [Serializable]
    public class ChatMessage
    {
        public string content;
    }

    public static class ChatGPTClient
    {
        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

        private static string apiKey;

        static ChatGPTClient()
        {
            try
            {
                string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ".openai");
                string[] lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    if (line.StartsWith("OPENAI_KEY="))
                    {
                        apiKey = line.Substring("OPENAI_KEY=".Length).Trim();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ChatGPTClient] Không đọc được API key từ file .openai: {e.Message}");
            }
        }

        public static async Task<string> Ask(string prompt)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("[ChatGPTClient] API key chưa được cấu hình.");
                return null;
            }

            using var client = new HttpClient();

            var requestBody = new ChatGPTRequest
            {
                model = "gpt-4",
                messages = new[]
                {
                    new ChatGPTMessage
                    {
                        role = "system",
                        content = "Bạn là trợ lý dạy học hình học không gian. Phân tích đề bài và trả kết quả JSON."
                    },
                    new ChatGPTMessage { role = "user", content = prompt }
                }
            };

            string jsonBody = JsonUtility.ToJson(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            try
            {
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string result = await response.Content.ReadAsStringAsync();

                // Parse lần đầu để lấy content thực bên trong
                ChatGPTRawResponse wrapper = JsonUtility.FromJson<ChatGPTRawResponse>(result);
                return wrapper.choices[0].message.content;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ChatGPTClient] Gửi request thất bại: {e.Message}");
                return null;
            }
        }
    }
}
