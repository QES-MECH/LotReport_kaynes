using System.Windows;

public interface IWindowService
{
    void Show<T>(object dataContext)
        where T : Window, new();

    bool? ShowDialog<T>(object dataContext)
        where T : Window, new();
}
