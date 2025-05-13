using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Manipulator
{ 

    public static class ConstraintFactory {
        public static void CreateConstraintNetworked(ConstraintData data)
        {
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateConstraintAction(data));
        }
        
        // Tạo constraint từ dữ liệu đã có
        public static Constraint CreateFromData(ConstraintData data)
        {
            data.Restore(); // Tự tạo
            return null;
        }

// Xoá constraint hiện có
        public static void Delete(Constraint constraint)
        {
            constraint.Cleanup();
            Object.Destroy(constraint);
        }
 

        
    }


}