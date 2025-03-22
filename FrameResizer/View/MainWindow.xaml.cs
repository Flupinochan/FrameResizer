using FrameResizer.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FrameResizer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // ViewModelのCommand等の呼び出しを可能にする
        ImageConfig? imageConfig = App.Current.Services.GetService<ImageConfig>();
        if(imageConfig != null)
        {
            this.DataContext = imageConfig;
        }
        else
        {
            MessageBox.Show("ViewModelが取得できませんでした", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Close();
            Application.Current.Shutdown();
        }
    }
}