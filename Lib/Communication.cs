using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BelgianStructuredCommunication;

public partial class Communication : IParsable<Communication>
{
    public uint Digits { get; }
    public byte CheckSum { get; }
    public bool ValidChecksum { get; }

    public Communication(uint digits) : this(digits.ToString("D10"))
    {
    }

    public Communication(string digits, string? checksum = null)
    {
        if (digits.Length != 10)
            throw new ArgumentException("Digits must be 10 characters", nameof(digits));

        Digits = uint.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture);

        var computedCheckSum = ComputeCheckSum(Digits);
        CheckSum = checksum is not null
            ? byte.Parse(checksum, NumberStyles.None, CultureInfo.InvariantCulture)
            : computedCheckSum;
        
        ValidChecksum = CheckSum == computedCheckSum;
    }

    public static Communication Parse(string s, IFormatProvider? provider = null)
    {
        if (!TryParse(s, provider, out var result))
            throw new FormatException("Invalid communication format");
        
        if (!result.ValidChecksum)
            throw new InvalidChecksumException(ComputeCheckSum(result.Digits), result.CheckSum, result.Digits);
        
        return result;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out Communication result)
    {
        return TryParse(s, null, out result);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider,
        [MaybeNullWhen(false)] out Communication result)
    {
        result = null;
        ArgumentNullException.ThrowIfNull(s);

        var match = CommunicationRegex().Match(s);
        if (!match.Success)
            return false;

        var value = string.Join(null, match.Groups.Values.Skip(1).Select(v => v.Value));
        var digits = value[..10];
        var checkSum = value[10..];

        result = new Communication(digits, checkSum);

        return true;
    }

    private static byte ComputeCheckSum(uint digits)
    {
        return (byte)(digits % 97);
    }
    
    public override string ToString()
    {
        var strDigits = Digits.ToString("D10", CultureInfo.InvariantCulture);
        return $"+++{strDigits[..3]}/{strDigits[3..7]}/{strDigits[7..]}{CheckSum:D2}+++";
    }

    [GeneratedRegex(@"^(?:\+\+\+)?\s?(\d{3})\s?/?\s?(\d{4})\s?/?\s?(\d{5})\s?(?:\+\+\+)?$")]
    private static partial Regex CommunicationRegex();
}

public class InvalidChecksumException(byte expected, byte actual, uint digits)
    : Exception($"Expected {expected:D2} but got {actual:D2} for {digits:D10}");