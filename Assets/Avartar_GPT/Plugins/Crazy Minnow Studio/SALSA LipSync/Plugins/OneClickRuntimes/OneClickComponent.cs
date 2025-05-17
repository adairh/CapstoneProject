namespace CrazyMinnow.SALSA.OneClicks
{
    public class OneClickComponent
    {
        public enum ComponentType
        {
            Shape,
            UMA,
            Bone,
            Animator
        }

        public string componentName;
        public float durHold;
        public float durOff;
        public float durOn;
        public ComponentType type;
    }

    public class OneClickShapeComponent : OneClickComponent
    {
        public string[] blendshapeNames;
        public bool isSpecificSmr = false;
        public float maxAmount;
        public string specificSmr = "";
        public bool useRegex;

        public OneClickShapeComponent(string componentName,
            string[] blendshapeNames,
            float maxAmount,
            float durOn,
            float durHold,
            float durOff,
            ComponentType type,
            bool useRegex,
            string specificSmr)
        {
            this.componentName = componentName;
            this.blendshapeNames = blendshapeNames;
            this.maxAmount = maxAmount;
            this.durOn = durOn;
            this.durHold = durHold;
            this.durOff = durOff;
            this.type = type;
            this.useRegex = useRegex;
            this.specificSmr = specificSmr;
        }
    }

    public class OneClickBoneComponent : OneClickComponent
    {
        public string componentSearchName;
        public TformBase max;
        public bool usePos;
        public bool useRot;
        public bool useScl;

        public OneClickBoneComponent(string componentName,
            string boneSearchName,
            TformBase max,
            bool usePos,
            bool useRot,
            bool useScl,
            float durOn,
            float durHold,
            float durOff,
            ComponentType type)
        {
            componentSearchName = boneSearchName;
            this.componentName = componentName;
            this.max = max;
            this.usePos = usePos;
            this.useRot = useRot;
            this.useScl = useScl;
            this.durOn = durOn;
            this.durHold = durHold;
            this.durOff = durOff;
            this.type = type;
        }
    }

    public class OneClickUepComponent : OneClickComponent
    {
        public float maxAmount;
        public string poseName;

        public OneClickUepComponent(string componentName,
            string poseName,
            float maxAmount,
            float durOn,
            float durHold,
            float durOff,
            ComponentType type)
        {
            this.componentName = componentName;
            this.poseName = poseName;
            this.maxAmount = maxAmount;
            this.durOn = durOn;
            this.durHold = durHold;
            this.durOff = durOff;
            this.type = type;
        }
    }

    public class OneClickAnimatorComponent : OneClickComponent
    {
        public int animationParmIndex;
        public string componentSearchName;
        public bool isTriggerParmBiDirectional;

        public OneClickAnimatorComponent(string componentName,
            string animatorSearchName,
            int animationParmIndex,
            bool isTriggerParmBiDirectional,
            float durOn,
            float durHold,
            float durOff,
            ComponentType type)
        {
            this.componentName = componentName;
            componentSearchName = animatorSearchName;
            this.animationParmIndex = animationParmIndex;
            this.isTriggerParmBiDirectional = isTriggerParmBiDirectional;
            this.durOn = durOn;
            this.durHold = durHold;
            this.durOff = durOff;
            this.type = type;
        }
    }
}