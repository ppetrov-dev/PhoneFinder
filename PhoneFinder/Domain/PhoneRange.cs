namespace PhoneFinder.Domain;

[Serializable]
internal record PhoneRange(int Id, int Code, int Begin, int End, int Capacity, string Operator, string Region, string Inn);
