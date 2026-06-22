namespace backend.Services;

public static class SeedParser
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static bool TryParse(string? seed, out ulong value)
    {
        value = 0;
        if (string.IsNullOrEmpty(seed)) return false;
        foreach (var character in seed)
        {
            var digit = Alphabet.IndexOf(character);
            if (digit < 0) return false;
            if (value > (ulong.MaxValue - (ulong)digit) / 62) return false;
            value = value * 62 + (ulong)digit;
        }
        return true;
    }
}
