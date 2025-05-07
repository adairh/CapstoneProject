using Unity.Netcode;
using UnityEngine;

namespace Manipulator
{
    public class NetworkShapeSpawner : NetworkBehaviour
    {
        public static NetworkShapeSpawner Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log($"[NetworkShapeSpawner] Spawned. IsServer: {IsHost}, IsClient: {IsClient}, IsHost: {IsHost}");
        }

        private void Start()
        {
            if (!IsSpawned && NetworkManager.Singleton.IsHost)
            {
                // Nếu object này được đặt sẵn trong scene, thì cần gọi spawn bằng tay
                var netObj = GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned)
                {
                    netObj.Spawn();
                    Debug.Log("[NetworkShapeSpawner] Manually spawned in Start().");
                }
            }
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
            if (IsHost) return; // Server đã có shape rồi
            var data = JsonUtility.FromJson<ShapeData>(json);
            var shape = ShapeFactory.CreateFromData(data);

            if (shape is Segment segment && data.ConnectedPoints.Count == 2)
            {
                var a = ShapeStorage.GetById(data.ConnectedPoints[0]) as Point;
                var b = ShapeStorage.GetById(data.ConnectedPoints[1]) as Point;
                if (a && b) {
                    segment.SetStartPoint(a);  
                    segment.SetEndPoint(b);  
                }
            }
        }

        public void CreateShapeNetworked(ShapeData data, out Shape shape)
        {
            string json = JsonUtility.ToJson(data);

            Shape temp = null;
            
            if (IsHost)
            {
                temp = ShapeFactory.CreateFromData(data);
                SpawnShapeForAllClientsClientRpc(json);
            }
            else
            {
                RequestCreateShapeServerRpc(json);
            }

            shape = temp;
        }
    }
}
