using System.Numerics;
using Unity.Netcode;

namespace Manipulator.Data
{
    [System.Serializable]
    public class ShapeData : INetworkSerializable
    {
        public string shapeType; 
        public float[] floats;
        public string id;
        public int ownerId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref shapeType);
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref ownerId); 
            serializer.SerializeValue(ref floats);
        }
    }

}