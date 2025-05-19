using System.Text;

namespace Manipulator
{
    public static class AIHelper
    {
        public static string BuildPrompt(string userInput)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Bạn là trợ lý AI cho ứng dụng học hình học không gian.");
            sb.AppendLine("Hãy đọc kỹ đề bài sau và trả lời DUY NHẤT bằng một JSON theo cấu trúc dưới đây.");

            sb.AppendLine();
            sb.AppendLine("⚠️ Chỉ sử dụng các kiểu điểm sau:");
            sb.AppendLine("- absolute: chỉ định toạ độ");
            sb.AppendLine("- midpoint: trung điểm của 2 điểm");
            sb.AppendLine("- split: chia đoạn theo tỷ lệ (0 < ratio < 1)");
            sb.AppendLine("- on_segment: tương đương split");
            sb.AppendLine("- centroid: trọng tâm của tam giác");
            sb.AppendLine("- extend: kéo dài đoạn theo vector");
            sb.AppendLine("- equidistant: điểm cách đều nhiều điểm");
            sb.AppendLine("- perpendicularFoot: chân đường vuông góc từ điểm đến đoạn");
            sb.AppendLine("- arbitrary: một điểm bất kỳ trên đoạn");

            sb.AppendLine();
            sb.AppendLine("❌ KHÔNG sử dụng các kiểu phức tạp như:");
            sb.AppendLine("- intersection, of_planes, shapes khác ngoài điểm và đoạn");
            sb.AppendLine("- KHÔNG được trả về các thuộc tính không liên quan đến điểm và đoạn");

            sb.AppendLine();
            sb.AppendLine("Hãy tưởng tượng bạn đang dựng hình bằng code Python, nhưng thay vì code, bạn trả về JSON chứa tất cả thông tin dựng hình:");
            sb.AppendLine();
            sb.AppendLine("{");
            sb.AppendLine("  \"CustomPoints\": {");
            sb.AppendLine("    \"A\": { \"type\": \"absolute\", \"position\": [0, 0, 0] },");
            sb.AppendLine("    \"B\": { \"type\": \"absolute\", \"position\": [1, 0, 0] },");
            sb.AppendLine("    \"M\": { \"type\": \"midpoint\", \"from\": [\"A\", \"B\"] }");
            sb.AppendLine("  },");
            sb.AppendLine("  \"ExtraSegments\": [ [\"A\", \"B\"], [\"A\", \"M\"] ],");
            sb.AppendLine("  \"Explanation\": string,");
            sb.AppendLine("  \"Warnings\": [string],");
            sb.AppendLine("  \"Suggestions\": [string]");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("Hãy phân tích đề bài cẩn thận và trả về đầy đủ các điểm và đoạn liên quan đến hình học không gian.");
            sb.AppendLine("Không thêm bất kỳ dòng giải thích nào ngoài JSON.");
            sb.AppendLine();
            sb.AppendLine("Đề bài:");
            sb.AppendLine(userInput);

            return sb.ToString();
        }
    }
}
