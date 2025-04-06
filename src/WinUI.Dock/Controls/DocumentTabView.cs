namespace WinUI.Dock.Controls;

public sealed partial class DocumentTabView : TabView
{
    private bool isTabContainerVisible = true;

    private Grid? tabContainerGrid;
    private ContentPresenter? tabContentPresenter;

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        tabContainerGrid = GetTemplateChild("TabContainerGrid") as Grid;
        tabContentPresenter = GetTemplateChild("TabContentPresenter") as ContentPresenter;

        if (isTabContainerVisible)
        {
            ShowTabContainer();
        }
        else
        {
            HideTabContainer();
        }
    }

    public void ShowTabContainer()
    {
        isTabContainerVisible = true;

        if (tabContainerGrid is not null)
        {
            tabContainerGrid.Opacity = 1.0;
            tabContainerGrid.IsHitTestVisible = true;

            BorderThickness = new Thickness(0, 0, 0, 0);
        }

        if (tabContentPresenter is not null)
        {
            Grid.SetRow(tabContentPresenter, 1);
            Grid.SetRowSpan(tabContentPresenter, 1);
        }
    }

    public void HideTabContainer()
    {
        isTabContainerVisible = false;

        if (tabContainerGrid is not null)
        {
            tabContainerGrid.Opacity = 0.0;
            tabContainerGrid.IsHitTestVisible = false;

            BorderThickness = new Thickness(0, 1, 0, 0);
        }

        if (tabContentPresenter is not null)
        {
            Grid.SetRow(tabContentPresenter, 0);
            Grid.SetRowSpan(tabContentPresenter, 2);
        }
    }
}
