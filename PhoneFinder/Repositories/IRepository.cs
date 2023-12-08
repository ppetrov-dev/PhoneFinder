namespace PhoneFinder.Repositories;

internal interface IRepository<out T>
{
    IEnumerable<T> Items { get; }

    bool Contains(int id);
    T Resolve(int id);
}
