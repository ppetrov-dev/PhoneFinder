namespace PhoneFinder.Services;

internal class PathService : IPathService
{
    public string GetFullPathToResource(string resourceName)
    {
        return "Resources/" + resourceName;
    }
}
