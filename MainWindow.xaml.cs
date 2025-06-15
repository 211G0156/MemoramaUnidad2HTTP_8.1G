using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using MemoramaUnidad2HTTP_8._1G.Models;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MemoramaUnidad2HTTP_8._1G;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ServidorMemorama servidor;

    public MainWindow()
    {
        InitializeComponent();
        servidor = new ServidorMemorama();
    }

    private void btnIniciar_Click(object sender, RoutedEventArgs e)
    {
        servidor.Iniciar();
    }

    private void btnDetener_Click(object sender, RoutedEventArgs e)
    {
        servidor.Detener();
        MessageBox.Show("Servidor detenido.");
    }
}