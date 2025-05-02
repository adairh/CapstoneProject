using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Manipulator
{
    public class CreateShapeBatchAction : IUndoableAction
    {
        private readonly List<string> _shapeIds;
        private readonly ulong        _wrapperId;

        public CreateShapeBatchAction(List<string> shapeIds, ulong wrapperNetworkObjectId)
        {
            // Clone để tránh bị thay đổi bên ngoài
            _shapeIds  = new List<string>(shapeIds);
            _wrapperId = wrapperNetworkObjectId;
        }

        public void Execute()
        {
            // Với create action thì shapes đã được tạo sẵn,
            // nên không cần làm gì thêm khi "redo"
        }

        public void Undo()
        {
            // Gọi ServerRpc, gói mảng ID thành CSV
            var csv = string.Join(",", _shapeIds);
            if (UndoManager.Instance.IsServer)
            {
                // Trên host/server, gọi trực tiếp
                UndoManager.Instance.UndoShapesServerRpc(_wrapperId, csv);
            }
            else
            {
                // Trên client, yêu cầu server gọi
                UndoManager.Instance.UndoShapesServerRpc(_wrapperId, csv);
            }
        }
    }
}