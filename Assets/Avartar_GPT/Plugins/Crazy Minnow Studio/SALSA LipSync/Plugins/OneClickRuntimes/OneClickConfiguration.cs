using System.Collections.Generic;

namespace CrazyMinnow.SALSA.OneClicks
{
    public class OneClickConfiguration
    {
        public enum ConfigType
        {
            Salsa,
            Emoter
        }

        public List<OneClickExpression> oneClickExpressions = new();
        public List<string> smrSearches = new();
        public ConfigType type;

        public OneClickConfiguration(ConfigType type)
        {
            this.type = type;
            smrSearches.Clear();
            oneClickExpressions.Clear();
        }
    }
}