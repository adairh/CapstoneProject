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
        private void RequestCreateShapeServerRpc(string json)
        {
            var data = JsonUtility.FromJson<ShapeData>(json);
            ShapeFactory.CreateFromData(data);
            SpawnShapeForAllClientsClientRpc(json);
        }


        [ClientRpc]
        private void SpawnShapeForAllClientsClientRpc(string json)
        {
            if (IsServer) return; // Server đã tự tạo shape rồi

            var data = JsonUtility.FromJson<ShapeData>(json);
            var shape = ShapeFactory.CreateFromData(data);

            if (shape is Segment segment && data.ConnectedPoints.Count == 2)
            {
                var a = ShapeStorage.GetById(data.ConnectedPoints[0]) as Point;
                var b = ShapeStorage.GetById(data.ConnectedPoints[1]) as Point;
                if (a && b) segment.SetEndpoints(a, b);
            }
        }


        public void CreateShapeNetworked(ShapeData data)
        {
            string json = JsonUtility.ToJson(data);

            if (IsServer)
            {
                ShapeFactory.CreateFromData(data);
                SpawnShapeForAllClientsClientRpc(json);
            }
            else
            {
                RequestCreateShapeServerRpc(json);
            }
        }

    }
}