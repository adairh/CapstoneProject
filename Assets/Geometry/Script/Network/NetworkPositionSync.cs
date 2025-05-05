using Unity.Netcode;
using UnityEngine;

namespace Manipulator
{
    public class NetworkPositionSync : NetworkBehaviour
    {
        public NetworkVariable<Vector3> syncedPosition = new(writePerm: NetworkVariableWritePermission.Server);

        private Transform target;

        private void Awake()
        {
            target = transform;
        }

        private void Update()
        {
            if (IsServer)
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