using System.Text;

namespace Manipulator
{
    public static class AIHelper
    {
        public static string BuildPrompt(string userInput)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Bạn là trợ lý AI cho ứng dụng học hình học không gian.");
            sb.AppendLine("Hãy đọc kỹ đề bài sau và trả lời duy nhất bằng một JSON có cấu trúc sau:");
            sb.AppendLine("\n{");
            sb.AppendLine("  \"ShapeType\": string, // loại hình học cần dựng, ví dụ: 'Cone', 'RegularTetrahedron'");
            sb.AppendLine("  \"KnownFields\": { string: number }, // các trường dữ kiện đã biết như Radius, Height, Side");
            sb.AppendLine("  \"CustomPoints\": { // các điểm dựng thêm như trung điểm, kéo dài, phản xạ... (tùy chọn)");
            sb.AppendLine("    \"M\": { \"type\": \"midpoint\", \"from\": [\"A\", \"B\"] },");
            sb.AppendLine("    \"N\": { \"type\": \"split\", \"from\": [\"A\", \"C\"], \"ratio\": 0.33 },");
            sb.AppendLine("    ...");
            sb.AppendLine("  },");
            sb.AppendLine("  \"ExtraSegments\": [ [\"A\", \"M\"], [\"B\", \"N\"] ], // (tùy chọn) các đoạn phụ cần vẽ thêm");
            sb.AppendLine("  \"Explanation\": string, // (tùy chọn) giải thích cách phân tích");
            sb.AppendLine("  \"Warnings\": [string], // (tùy chọn) cảnh báo như mâu thuẫn dữ kiện");
            sb.AppendLine("  \"Suggestions\": [string] // (tùy chọn) gợi ý dữ kiện còn thiếu hoặc cần xác minh");
            sb.AppendLine("}\n");

            sb.AppendLine("Dưới đây là đề bài:");
            sb.AppendLine(userInput);

            return sb.ToString();
        }
    }
}