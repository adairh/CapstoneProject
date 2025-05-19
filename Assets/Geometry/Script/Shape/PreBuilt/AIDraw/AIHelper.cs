
using System.Text;

namespace Manipulator
{
    public static class AIHelper
    {
        public static string BuildPrompt(string userInput)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Bạn là trợ lý AI cho ứng dụng học hình học không gian.");
            sb.AppendLine("Hãy đọc kỹ đề bài sau và trả lời DUY NHẤT bằng một JSON có cấu trúc bên dưới");
            sb.AppendLine(" (và số lượng điểm không giới hạn như bên dưới, nó phải follow theo đề bài một cách chuẩn chỉnh, đầy đủ và chi tiết.");
            sb.AppendLine(" Tưởng tượng như đây là lúc yêu cầu bạn vẽ visualize cái hình ra để chạy code python, nhưng bạn không vẽ, không tạo code mà là trả về Json để có thể hình dung và các app khác có thể vẽ)");
            sb.AppendLine(" Nhắc lại, bên dưới đây chỉ là template:");
            sb.AppendLine();
            sb.AppendLine("{");
            sb.AppendLine("  \"CustomPoints\": {");
            sb.AppendLine("    \"A\": { \"type\": \"absolute\", \"position\": [0, 0, 0] },");
            sb.AppendLine("    \"B\": { \"type\": \"absolute\", \"position\": [1, 0, 0] },");
            sb.AppendLine("    \"C\": { \"type\": \"absolute\", \"position\": [0.5, 0.866, 0] },");
            sb.AppendLine("    \"D\": { \"type\": \"absolute\", \"position\": [0.5, 0.2887, 0.8165] },");
            sb.AppendLine("    \"M\": { \"type\": \"midpoint\", \"from\": [\"A\", \"B\"] },");
            sb.AppendLine("    \"N\": { \"type\": \"midpoint\", \"from\": [\"C\", \"D\"] },");
            sb.AppendLine("    \"I\": { \"type\": \"equidistant\", \"from\": [\"A\", \"B\", \"C\", \"D\"] }");
            sb.AppendLine("  },");
            sb.AppendLine("  \"ExtraSegments\": [ [\"A\", \"B\"], [\"B\", \"C\"], [\"C\", \"A\"], [\"A\", \"D\"], [\"B\", \"D\"], [\"C\", \"D\"],");
            sb.AppendLine("                      [\"M\", \"N\"], [\"B\", \"N\"], [\"M\", \"C\"] ],");
            sb.AppendLine("  \"Explanation\": string,");
            sb.AppendLine("  \"Warnings\": [string],");
            sb.AppendLine("  \"Suggestions\": [string]");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("Hãy suy luận và xác định tất cả các điểm, đoạn thẳng cần thiết để dựng toàn bộ cấu trúc hình học trong không gian.");
            sb.AppendLine("Kết quả trả về cần có đầy đủ các điểm (dạng midpoint, centroid, on_segment, absolute...) và các đoạn nối giữa các điểm đó.");
            sb.AppendLine("Hãy đặc biệt chú ý đến các đoạn liên quan đến chứng minh, thiết diện và giá trị lớn nhất/nhỏ nhất.");
            sb.AppendLine("Không thêm bất kỳ giải thích nào bên ngoài JSON.");
            sb.AppendLine("Không cần trả về ShapeType hoặc KnownFields nữa.");
            sb.AppendLine();
            sb.AppendLine("Đề bài:");
            sb.AppendLine(userInput);

            return sb.ToString();
        }
    }
}
