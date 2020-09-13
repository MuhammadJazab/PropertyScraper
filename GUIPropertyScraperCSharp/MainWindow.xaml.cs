using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace GUIPropertyScraperCSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string url = "https://www.rightmove.co.uk/";
        string postalCode = "NW3";

        IWebDriver driver;

        public MainWindow()
        {
            InitializeComponent();

            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            driver.Url = url;

            var searchLocation = driver.FindElement(By.Name("searchLocation"));
            searchLocation.SendKeys(postalCode);

            var buyButton = driver.FindElement(By.Id("buy"));
            buyButton.Click();
        }
    }
}
