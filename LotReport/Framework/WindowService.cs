using System.Windows;

public class WindowService : IWindowService
{
    public void Show<T>(object dataContext)
        where T : Window, new()
    {
        T window = new T();
        window.DataContext = dataContext;
        window.Show();
    }

    public bool? ShowDialog<T>(object dataContext)
        where T : Window, new()
    {
        T window = new T();
        window.DataContext = dataContext;
        return window.ShowDialog();
    }
}
