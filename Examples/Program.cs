using BelgianStructuredCommunication;

var a = new Communication(0123456789);
Console.WriteLine(a.ToString()); // +++012/3456/78939+++

var b = Communication.Parse("+++ 012 / 3456 / 78939 +++");
Console.WriteLine(b.Digits); // 123456789
Console.WriteLine(b.CheckSum); // 39
Console.WriteLine(b.ValidChecksum); // True

// Parsing can succeed, but the checksum might be wrong.
// In that case :
// * TryParse will parse the digits and set ValidChecksum to false
// * Parse will throw an InvalidChecksumException
var r = Communication.TryParse("+++012/3456/78900 +++", out var c);
Console.WriteLine(r); // True
Console.WriteLine(c!.ValidChecksum); // False