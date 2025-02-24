namespace Dock.Interfaces;

public interface IContainer : IComponent
{
    IComponent this[int index] { get; }

    int Count { get; }

    bool AutoRemove { get; set; }

    void Add(IComponent component);

    void Add(IComponent component, int index);

    void Remove(IComponent component);

    void RemoveAt(int index);

    int IndexOf(IComponent component);

    void Clear();
}
