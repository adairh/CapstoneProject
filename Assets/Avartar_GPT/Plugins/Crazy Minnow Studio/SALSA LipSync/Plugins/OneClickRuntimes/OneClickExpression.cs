using System.Collections.Generic;

namespace CrazyMinnow.SALSA.OneClicks
{
    public class OneClickExpression
    {
        public List<OneClickComponent> components;
        public string name;

        public OneClickExpression()
        {
        }

        public OneClickExpression(string name, List<OneClickComponent> components)
        {
            this.name = name;
            this.components = components;
        }
    }

    public class OneClickEmoterExpression : OneClickExpression
    {
        public float expressionDynamics = 1.0f;
        public bool isAlwaysEmphasis;
        public bool isEmphasis;
        public bool isPersistent;
        public bool isRandom;
        public bool isRepeater;
        public float repeaterDelay;
        public EmoteRepeater.StartDelay startDelayType = EmoteRepeater.StartDelay.Immediately;

        public OneClickEmoterExpression(string name, List<OneClickComponent> components)
        {
            this.name = name;
            this.components = components;
        }

        public void SetEmoterBools(bool isRand,
            bool isEmph,
            bool isRep,
            float frac,
            bool isAlwaysEmph,
            float delay,
            EmoteRepeater.StartDelay startType,
            bool persistent)
        {
            isRandom = isRand;
            isEmphasis = isEmph;
            isAlwaysEmphasis = isAlwaysEmph;
            isRepeater = isRep;
            expressionDynamics = frac;
            repeaterDelay = delay;
            startDelayType = startType;
            isPersistent = persistent;
        }
    }
}