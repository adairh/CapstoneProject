public static class ObjectCounter
{
    private static int count = 0; // Global counter

    /// <summary>
    /// Get the next object number in sequence.
    /// </summary>
    public static int Next()
    {
        return ++count;
    }

    /// <summary>
    /// Get the current object number without incrementing.
    /// </summary>
    public static int Current()
    {
        return count;
    }

    /// <summary>
    /// Reset the counter to zero.
    /// </summary>
    public static void Reset()
    {
        count = 0;
    }
}