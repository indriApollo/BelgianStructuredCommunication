# Belgian Structured Communication

In Belgium, banking transactions can use a structured communication, where a checksum guarantees the validity of the input.  
The structure looks like this : +++ 085 / 1927 / 54115 +++  
There are 10 digits, followed by a 2-digit checksum being the modulo 97 of the previous 10 digits.  
Ex: 0851927541 % 97 = 15

## Usage

```csharp
using IndriApollo.BelgianStructuredCommunication;

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
```
