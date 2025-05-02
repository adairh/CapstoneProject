using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Manipulator
{
    public class CreateShapeBatchAction : IUndoableAction
    {
        private readonly ulong _originalWrapperId;
        private ulong _wrapperId;                      // ID mới khi redo
        private readonly IShapeButton.ShapeType _type;
        private readonly Vector3 _start, _end;
        private readonly List<string> _shapeIds;      // dọn ở Undo
       
        public CreateShapeBatchAction(
            List<string> shapeIds,
            ulong wrapperNetworkObjectId,
            IShapeButton.ShapeType type,
            Vector3 start,
            Vector3 end)
        {
            _shapeIds  = new List<string>(shapeIds);
            _originalWrapperId = wrapperNetworkObjectId;
            _type      = type;
            _start     = start;
            _end       = end;
            _wrapperId = wrapperNetworkObjectId;
        }

        public void Execute()
        {
            if (!UndoManager.Instance.IsServer)
            {
                // client gọi lên server spawn wrapper + shapes
                UndoManager.Instance.SpawnWrapperServerRpc(_type, _start, _end, _originalWrapperId);
                return;
            }
            // trên server: spawn lại wrapper và để ShapeNetworkSync xử lí (như FinishDrawing)
            var go   = Object.Instantiate(PerformDrawing.Instance.GetShapeNetwork());
            var sync = go.GetComponent<ShapeNetworkSync>();
            sync.shapeType.Value    = (ShapeNetworkSync.ShapeType)_type;
            sync.startPoint.Value   = _start;
            sync.currentPoint.Value = _end;
            sync.isDrawing.Value    = false;
            sync.isFinalized.Value  = true;
            go.GetComponent<NetworkObject>().Spawn();
            _wrapperId = go.GetComponent<NetworkObject>().NetworkObjectId;
        }

        public void Undo()
        {
            var csv = string.Join(",", _shapeIds);
            if (NetworkManager.Singleton.IsServer)
            {
                // *We’re* the server, call it directly:
                UndoManager.Instance.ProcessUndoBatch(_wrapperId, csv);
            }
            else
            {
                // a client, so ask the server to do it
                UndoManager.Instance.UndoShapesServerRpc(_wrapperId, csv);
            }
        }

        
        
    }

}