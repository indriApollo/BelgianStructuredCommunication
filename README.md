# Belgian Structured Communication

In Belgium, banking transactions can use a structured communication, where a checksum guarantees the validity of the input.  
The structure looks like this : +++ 085 / 1927 / 54115 +++  
There are 10 digits, followed by a 2-digit checksum being the modulo 97 of the previous 10 digits.  
Ex: 0851927541 % 97 = 15

## Usage

```csharp
using BelgianStructuredCommunication;

var c = new Communication(0123456789)
c.ToString(); // +++012/3456/78939+++
```
