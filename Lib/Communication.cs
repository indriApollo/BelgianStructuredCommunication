using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace IndriApollo.BelgianStructuredCommunication;

public partial class Communication : IParsable<Communication>
{
    public uint Digits { get; }
    public byte CheckSum { get; }
    public bool ValidChecksum { get; }

    public Communication(uint digits) : this(digits.ToString("D10"))
    {
    }

    /// <summary>
    /// Create a new <see cref="Communication"/>
    /// </summary>
    /// <param name="digits">The 10 digits</param>
    /// <param name="checksum">Optional. Will be computed if omitted</param>
    /// <exception cref="ArgumentException"> digits must be 10 characters</exception>
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

    /// <summary>
    /// Parse a string and return a <see cref="Communication"/>
    /// </summary>
    /// <param name="s">The source string to parse</param>
    /// <param name="provider">Required by <see cref="IParsable{TSelf}"/> but otherwise unused</param>
    /// <returns>the parsed <see cref="Communication"/></returns>
    /// <exception cref="FormatException"> The source string isn't formatted as expected</exception>
    /// <exception cref="InvalidChecksumException">The source string format is valid, but the checksum is invalid</exception>
    /// <exception cref="ArgumentNullException">The source string can't be null</exception>
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

    /// <summary>
    /// Parse a string and return a <see cref="Communication"/>.
    /// The parsing succeeds as long as the source string is well formatted.
    /// You should check <see cref="ValidChecksum"/> to make sure the parsed value is valid.
    /// </summary>
    /// <param name="s">The source string to parse</param>
    /// <param name="provider">Required by <see cref="IParsable{TSelf}"/> but otherwise unused</param>
    /// <param name="result">The parsed <see cref="Communication"/>, or null when parsing failed</param>
    /// <returns>true if s was converted successfully; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">The source string can't be null</exception>
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