
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
    public class CustomPointDef
    {
        // Basic type
        public string type;

        // For fields that may be array or string, provide both and use the getter below
        public string[] from;
        public string from_single;

        public float ratio = 0.5f;
        public float distance;
        public string axis;
        public string plane;
        public float[] position;

        public string[] segment;
        public string segment_single;

        public string[] direction;
        public string direction_single;

        // Add extra fields here as your AI output evolves

        // ----- Getter helpers -----
        public string[] GetFrom()
        {
            if (from != null && from.Length > 0)
                return from;
            if (!string.IsNullOrEmpty(from_single))
                return new[] { from_single };
            return null;
        }

        public string[] GetSegment()
        {
            if (segment != null && segment.Length > 0)
                return segment;
            if (!string.IsNullOrEmpty(segment_single))
                return new[] { segment_single };
            return null;
        }

        public string[] GetDirection()
        {
            if (direction != null && direction.Length > 0)
                return direction;
            if (!string.IsNullOrEmpty(direction_single))
                return new[] { direction_single };
            return null;
        }
    }

    [Serializable]
    public class AIShapeResult
    {
        public Dictionary<string, CustomPointDef> CustomPoints;
        public List<string[]> ExtraSegments;
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
    public class ChatChoice
    {
        public ChatMessage message;
    }

    [Serializable]
    public class ChatMessage
    {
        public string content;
    }

    [Serializable]
    public class ChatGPTRawResponse
    {
        public ChatChoice[] choices;
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
                foreach (string line in lines)
                {
                    if (line.StartsWith("OPENAI_KEY="))
                    {
                        apiKey = line.Substring("OPENAI_KEY=".Length).Trim();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[ChatGPTClient] Không thể đọc file .openai: " + ex.Message);
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
                model = "gpt-4o",
                messages = new[]
                {
                    new ChatGPTMessage { role = "system", content = "Bạn là trợ lý hình học không gian." },
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

                ChatGPTRawResponse wrapper = JsonUtility.FromJson<ChatGPTRawResponse>(result);
                if (wrapper?.choices == null || wrapper.choices.Length == 0 || wrapper.choices[0].message == null)
                {
                    Debug.LogError("[ChatGPTClient] Response không hợp lệ hoặc thiếu nội dung: " + result);
                    return null;
                }

                return wrapper.choices[0].message.content;
            }
            catch (Exception ex)
            {
                Debug.LogError("[ChatGPTClient] Lỗi khi gửi request: " + ex.Message);
                return null;
            }
        }
    }
}
