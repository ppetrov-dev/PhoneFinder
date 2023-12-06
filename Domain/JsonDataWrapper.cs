namespace PhoneFinder.Domain;

[Serializable]
internal class JsonDataWrapper<T>
{
    public T[] Data { get; set; } = Array.Empty<T>();
}
