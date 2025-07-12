namespace WadMaker.Extensions;

public static class NumberExtensions
{
    public static int NMod(this int value, int modulus)
    {
        if (value < 0)
            return (value % modulus) + modulus;
        else
            return value % modulus;
    }   
}
