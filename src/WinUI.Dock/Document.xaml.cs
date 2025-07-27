using System.Text.Json.Nodes;

namespace WinUI.Dock;

[ContentProperty(Name = nameof(Content))]
public partial class Document : DockModule
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title),
                                                                                          typeof(string),
                                                                                          typeof(Document),
                                                                                          new PropertyMetadata(string.Empty, OnTitleChanged));

    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
                                                                                            typeof(object),
                                                                                            typeof(Document),
                                                                                            new PropertyMetadata(null));

    public static readonly DependencyProperty CanPinProperty = DependencyProperty.Register(nameof(CanPin),
                                                                                           typeof(bool),
                                                                                           typeof(Document),
                                                                                           new PropertyMetadata(true));

    public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(nameof(CanClose),
                                                                                             typeof(bool),
                                                                                             typeof(Document),
                                                                                             new PropertyMetadata(true));

    private static readonly DependencyProperty ActualTitleProperty = DependencyProperty.Register(nameof(ActualTitle),
                                                                                                 typeof(string),
                                                                                                 typeof(Document),
                                                                                                 new PropertyMetadata(string.Empty));

    public Document()
    {
        DefaultStyleKey = typeof(Document);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public bool CanPin
    {
        get => (bool)GetValue(CanPinProperty);
        set => SetValue(CanPinProperty, value);
    }

    public bool CanClose
    {
        get => (bool)GetValue(CanCloseProperty);
        set => SetValue(CanCloseProperty, value);
    }

    public string ActualTitle
    {
        get => (string)GetValue(ActualTitleProperty);
        private set => SetValue(ActualTitleProperty, value);
    }

    internal DockSide PreferredSide { get; set; } = (DockSide)(-1);

    internal int PreferredSideIndex { get; set; }

    public void DockTo(Document dest, DockTarget target)
    {
        if (dest.Owner is not DocumentGroup group)
        {
            throw new InvalidOperationException("Destination document must be part of a DocumentGroup.");
        }

        if (dest.Root is not DockManager manager)
        {
            throw new InvalidOperationException("Destination document must be part of a DockManager.");
        }

        Detach();

        switch (target)
        {
            case DockTarget.Center:
            case DockTarget.SplitLeft:
            case DockTarget.SplitTop:
            case DockTarget.SplitRight:
            case DockTarget.SplitBottom:
                group.Dock(this, target);
                break;
            case DockTarget.DockLeft:
            case DockTarget.DockTop:
            case DockTarget.DockRight:
            case DockTarget.DockBottom:
                manager.Dock(this, target);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, "Invalid DockTarget specified.");
        }
    }

    internal override void SaveLayout(JsonObject writer)
    {
        writer.WriteByModuleType(this);
        writer.WriteDockModuleProperties(this);

        writer[nameof(Title)] = Title;
        writer[nameof(CanPin)] = CanPin;
        writer[nameof(CanClose)] = CanClose;
    }

    internal override void LoadLayout(JsonObject reader)
    {
        reader.ReadDockModuleProperties(this);

        Title = reader[nameof(Title)].Deserialize<string>();
        CanPin = reader[nameof(CanPin)].Deserialize<bool>();
        CanClose = reader[nameof(CanClose)].Deserialize<bool>();
    }

    internal void ResetPreferredSide(DockTarget? target)
    {
        PreferredSide = (DockSide)(PreferredSideIndex = -1);

        if (target.HasValue)
        {
            switch (target)
            {
                case DockTarget.DockLeft:
                    PreferredSide = DockSide.Left;
                    break;
                case DockTarget.DockTop:
                    PreferredSide = DockSide.Top;
                    break;
                case DockTarget.DockRight:
                    PreferredSide = DockSide.Right;
                    break;
                case DockTarget.DockBottom:
                    PreferredSide = DockSide.Bottom;
                    break;
            }
        }
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Document document = (Document)d;

        document.ActualTitle = document.Title.Split("##").LastOrDefault() ?? document.Title;
    }
}
