using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hw1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void CalculateButton_Click(object Sender, EventArgs e)
        {
            //if (R > a / 2)
            //{
            //    MessageBox.Show("Радиус R не должен превышать половину ребра куба (a/2).");
            //    return;
            //}

            const double testPI = Math.PI;
            int a = Convert.ToInt32(TextBoxA.Text);
            int R = Convert.ToInt32(TextBoxR.Text);

            double volumeOfCube = Math.Pow(a, 3);
            double volumeOfSphere = (4.0 / 3.0) * testPI * Math.Pow(R, 3);
            double wastePercentage = ((volumeOfCube - volumeOfSphere) / volumeOfCube) * 100;

            double test = Math.Pow(testPI, 3);

            ResultTextBlock.Text = $"Объем куба:  {volumeOfCube}\n" +
                                   $"Объем шара: {volumeOfSphere}\n";
        }
    }
}
