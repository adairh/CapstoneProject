using UnityEngine;

namespace Manipulator
{
    public interface IPrebuiltDrawer
    {
        /// <summary> Called when user first clicks to start drawing </summary>
        void Begin(Vector3 startPos);

        /// <summary> Called every frame while dragging </summary>
        void Working(Vector3 currentPos);

        /// <summary> Called when user releases the mouse </summary>
        void End(Vector3 finalPos);

        /// <summary> Optional: destroy preview </summary>
        void Cancel();
    }
}