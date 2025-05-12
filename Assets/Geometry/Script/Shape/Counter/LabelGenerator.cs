using System.Collections.Generic;

namespace Manipulator
{
    public static class LabelGenerator
    {
        private static int counter = 0;

        public static void Reset() => counter = 0;

        public static string Next()
        {
            int n = counter++;
            List<char> chars = new();
            do
            {
                chars.Insert(0, (char)('A' + (n % 26)));
                n = n / 26 - 1;
            } while (n >= 0);
            return new string(chars.ToArray());
        }
    }
}