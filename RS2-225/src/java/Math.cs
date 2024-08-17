namespace RS2_225.java;

public class Math
{
    public static Random _random = new();

    public static double random()
    {
        return _random.NextDouble();
    }
}