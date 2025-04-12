using System.Numerics;
using GLTFast.Schema;
using Manipulator.Data;

namespace Geometry.Script.Network
{
    public interface ISynchronizedShape
    {
        void BeginSketch(Vector3 world, Vector3 screen, Camera cam);
        void UpdateSketch(Vector3 world, Vector3 screen, Camera cam);
        void EndSketch();

        ShapeData Serialize(); // Convert to serializable data
        void Deserialize(ShapeData data); // Rebuild from data
    }

}