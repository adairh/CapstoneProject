using Unity.Netcode;
using UnityEngine;

namespace Manipulator
{
    public class NetworkPositionSync : NetworkBehaviour
    {
        public NetworkVariable<Vector3> syncedPosition = new(writePerm: NetworkVariableWritePermission.Server);
        public NetworkVariable<Vector3> syncedScale = new(writePerm: NetworkVariableWritePermission.Server);

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
                syncedScale.Value = target.localScale;
            }
            else
            {
                target.position = syncedPosition.Value; // client nhận vị trí
                target.localScale = syncedScale.Value;
            }

            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized) return;

            if (IsHost)
            {
                syncedPosition.Value = target.position;
                syncedScale.Value = target.localScale;

            }
            else
            {
                target.position = syncedPosition.Value;
                target.localScale = syncedScale.Value;
            }
        }
    }
}