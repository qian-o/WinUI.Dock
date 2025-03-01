using WinUI.Dock.Abstracts;

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

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Document document)
        {
            document.ActualTitle = document.Title.Split("##").LastOrDefault() ?? document.Title;
        }
    }
}
