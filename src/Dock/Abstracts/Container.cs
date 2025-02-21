using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using IContainer = Dock.Interfaces.IContainer;

namespace Dock.Abstracts;

[ContentProperty(Name = nameof(Children))]
public abstract class Container<T> : Control, IContainer
{
    public ObservableCollection<T> Children { get; } = [];
}
