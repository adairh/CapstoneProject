namespace Geometry.Script.Network
{
    public interface IShapeSerializable
    {
        string GetShapeType(); // "Circle", "Segment", etc.
        string ToJson(); // Serialize all state
        void FromJson(string json); // Deserialize
    }
}