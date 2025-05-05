using Unity.Netcode;
using UnityEngine; 

namespace Manipulator
{
    public class NetworkShapeSpawner : NetworkBehaviour
    {
        public static NetworkShapeSpawner Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestCreateShapeServerRpc(string jsonShapeData)
        {
            ShapeData data = JsonUtility.FromJson<ShapeData>(jsonShapeData);
            SpawnShapeForAllClients(data);
        }

        [ClientRpc]
        private void SpawnShapeForAllClients(ShapeData data)
        {
            if (IsServer) return; // server tự tạo shape rồi

            var shape = ShapeFactory.CreateFromData(data);

            if (shape is Segment segment && data.ConnectedPointIds.Count == 2)
            {
                var a = ShapeStorage.GetPointById(data.ConnectedPointIds[0]);
                var b = ShapeStorage.GetPointById(data.ConnectedPointIds[1]);
                if (a && b) segment.SetEndpoints(a, b);
            }
        }

        public void CreateShapeNetworked(ShapeData data)
        {
            if (IsServer)
            {
                ShapeFactory.CreateFromData(data);
                SpawnShapeForAllClients(data);
            }
            else
            {
                string json = JsonUtility.ToJson(data);
                RequestCreateShapeServerRpc(json);
            }
        }
    }
}