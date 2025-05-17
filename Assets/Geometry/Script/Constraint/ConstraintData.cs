using System;

namespace Manipulator
{
    /// <summary>
    ///     Dữ liệu cơ bản cho constraint dùng để serialize.
    /// </summary>
    [Serializable]
    public abstract class ConstraintData
    {
        public string Type;
        public string ConstraintId;

        public abstract void Restore();
    }
}