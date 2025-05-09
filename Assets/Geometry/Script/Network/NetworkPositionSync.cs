using Unity.Netcode;
using UnityEngine;

namespace Manipulator
{
    public class NetworkPositionSync : NetworkBehaviour
    {
        public NetworkVariable<Vector3> syncedPosition = new(writePerm: NetworkVariableWritePermission.Server);

        private Transform target;
        private bool isInitialized = false;

        private void Awake()
        {
            target = transform;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost)
            {
                syncedPosition.Value = target.position; // gửi giá trị ban đầu
            }
            else
            {
                target.position = syncedPosition.Value; // client nhận vị trí
            }

            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized) return;

            if (IsHost)
            {
                syncedPosition.Value = target.position;
            }
            else
            {
                target.position = syncedPosition.Value;
            }
        }
    }
}