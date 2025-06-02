using System.Text;

namespace Manipulator
{
    public static class AIHelper
    {
        public static string BuildPrompt(string userInput)
        {
            StringBuilder sb = new();

            sb.AppendLine("Bạn là trợ lý AI cho ứng dụng học hình học không gian.");
            sb.AppendLine("Hãy đọc kỹ đề bài sau và trả lời DUY NHẤT bằng một JSON theo cấu trúc dưới đây, KHÔNG giải thích gì thêm bên ngoài JSON.");
            sb.AppendLine();
            sb.AppendLine(
                "ĐẶC BIỆT LƯU Ý: Không phân tích trên input gốc , cần phải paraphrase phân tích lại trước khi tính toán:");
            sb.AppendLine("- Đây là hình gì");
            sb.AppendLine("- Cần bao nhiêu điểm");
            sb.AppendLine("- Các cạnh là gì");
            sb.AppendLine();
            sb.AppendLine("Rồi từ đó, tính toán tọa độ chính xác của từng điểm dựa trên dữ kiện đề bài");
            sb.AppendLine(
                "Nếu đề không có số đo chính xác như 1, 2, 3,... mà là các ví dụ như \" Độ dài a \" thì mặc định a = 5 ");
                
                
                
                
            sb.AppendLine("Các đề cần phải phân tích cách giải nếu đề có câu hỏi. trong phân tích nếu có cần vẽ thêm thì bao gồm vào nội dung điểm và đường để vẽ luôn ");
            
            
            
            

            sb.AppendLine("⚠️ Chỉ sử dụng các kiểu điểm hợp lệ:");
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
            sb.AppendLine("❌ Không sử dụng:");
            sb.AppendLine("- intersection, of_planes, or shapes khác ngoài điểm và đoạn");
            sb.AppendLine("- Không thêm thuộc tính lạ không có trong ví dụ dưới");

            sb.AppendLine();
            sb.AppendLine("📌 Format JSON bắt buộc (ví dụ):");
            sb.AppendLine("{");
            sb.AppendLine("  \"CustomPoints\": {");
            sb.AppendLine("    \"A\": { \"type\": \"absolute\", \"position\": [0, 0, 0] },");
            sb.AppendLine("    \"B\": { \"type\": \"absolute\", \"position\": [1, 0, 0] },");
            sb.AppendLine("    \"M\": { \"type\": \"midpoint\", \"from\": [\"A\", \"B\"] }");
            sb.AppendLine("  },");
            sb.AppendLine("  \"ExtraSegments\": [ [\"A\", \"B\"], [\"A\", \"M\"] ],");
            sb.AppendLine("  \"Explanation\": \"string mô tả\",");
            sb.AppendLine("  \"Warnings\": [\"string cảnh báo\"],");
            sb.AppendLine("  \"Suggestions\": [\"string gợi ý\"]");
            sb.AppendLine("}");
            
            sb.AppendLine(" ExtraSegments là bao gồm mọi đường có trong hình, kể cả các đường nối của shape hình cơ bản, nên cần phải đầy đủ tuyệt đối");
            sb.AppendLine();
            sb.AppendLine("🛑 QUY TẮC NGHIÊM NGẶT:");
            sb.AppendLine("- Các trường như \"from\", \"to\", \"segment\", \"on_segment\" PHẢI luôn là MẢNG (dù chỉ có 1 phần tử). Ví dụ: [\"A\"]");
            sb.AppendLine("- Tất cả toạ độ trong \"position\" phải là số thực. KHÔNG dùng biểu thức như Math.sqrt.");
            sb.AppendLine("- KHÔNG thêm mô tả nào ngoài JSON. KHÔNG bọc JSON trong markdown hoặc ```json```."); 
            
            sb.AppendLine("- MẶT PHẲNG ĐÁY LÀ OXY, LUÔN LUÔN DỰNG TỌA ĐỘ TÍNH TỪ OXY");

            sb.AppendLine();
            sb.AppendLine("⚠️ ABSOLUTELY DO NOT USE any math expressions (e.g. sqrt, /, *, +, -) inside JSON arrays.");
            sb.AppendLine("⚠️ ALL coordinates (positions, etc.) in the JSON output MUST be real numbers with at least 3 decimal places if needed.");
            sb.AppendLine("Write: [0.5, 0.866, 0]  ✅");
            sb.AppendLine("DO NOT write: [0.5, Math.sqrt(3)/2, 0] ❌");
            sb.AppendLine("DO NOT write: [1/2, 1/2*sqrt(3), 0] ❌");
            sb.AppendLine("Do the calculations yourself and only return the numeric result.");
            sb.AppendLine();
            sb.AppendLine("Nếu bạn không trả về đúng số thực cho mọi toạ độ, kết quả sẽ bị bỏ qua.");
            sb.AppendLine("Không được phép sử dụng sqrt(), /, *, +, - hoặc bất cứ biểu thức toán học nào trong mảng toạ độ.");
            sb.AppendLine("Chỉ được phép trả về số thực, không được phép giải thích hoặc chú thích ngoài JSON.");
            sb.AppendLine();
            sb.AppendLine("Đề bài:");
            sb.AppendLine(userInput);

            return sb.ToString();
        }
    }
}
