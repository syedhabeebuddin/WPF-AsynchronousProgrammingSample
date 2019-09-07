using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace AsyncProgramming
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource cancellationTokenSource = null;
        public MainWindow()
        {
            InitializeComponent();
            btnCancel.Visibility = Visibility.Collapsed;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            prgBar.Visibility = Visibility.Collapsed;

            if (!chkbox.IsChecked.Value)
            {
                LoadData();
            }
            else
            {
                LoadDataAsync();
                //LoadWithAsyncAndAwait();
                if (chkCancel.IsChecked.Value)
                {
                    btnSearch.Visibility = Visibility.Collapsed;
                    btnCancel.Visibility = Visibility.Visible;

                    LoadWithCancel();
                }

            }

        }
        
        private void LoadWithCancel()
        {
            if(cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();            

            var tsk = Task.Run(() =>
            {
                var lines = File.ReadAllLines(@"C:\Users\Lenovo\Downloads\CSV.csv");                
                return lines;
            });

            var tskGetTasks = tsk.ContinueWith( t =>
            {
                var lines = tsk.Result;
                return GetStocks(lines, cancellationTokenSource.Token);
            }, cancellationTokenSource.Token,TaskContinuationOptions.OnlyOnRanToCompletion,TaskScheduler.Current);

            var displaymsgTask = tsk.ContinueWith(t =>
            {                
                Dispatcher.Invoke(() =>
                {
                    txtComment.AppendText(tsk.Exception.InnerException.Message);

                    lblLoad.Visibility = Visibility.Visible;
                    lblLoad.Content = "Data Loading Failed.";
                    prgBar.Visibility = Visibility.Collapsed;
                });

            }, TaskContinuationOptions.OnlyOnFaulted);

            var updateGridTask = tskGetTasks.ContinueWith(t =>
            {                
                Dispatcher.Invoke(() =>
                {
                    grdData.ItemsSource = tskGetTasks.Result;

                    lblLoad.Visibility = Visibility.Visible;
                    lblLoad.Content = "Data Loaded Successfully !!";
                    prgBar.Visibility = Visibility.Collapsed;
                });
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

        }

        private List<StockPrice> GetStocks(string[] lines, CancellationToken cancellationToken)
        {
            var data = new StockPrice();
            List<StockPrice> lstStock = new List<StockPrice>();

            foreach (var line in lines.Skip(1))
            {                
                if(cancellationToken.IsCancellationRequested)
                {
                    return lstStock;
                }
                
                data = new StockPrice();
                string[] content = line.Split(',');

                data.Date = DateTime.Parse(content[0]);
                data.High = Double.Parse(content[1]);
                data.Low = Double.Parse(content[2]);
                data.Open = Double.Parse(content[3]);
                data.Close = Double.Parse(content[4]);
                data.AdjClose = Double.Parse(content[5]);
                data.Volume = long.Parse(content[6]);

                lstStock.Add(data);
            }

            return lstStock;
        }

        private async Task LoadWithAsyncAndAwait()
        {
            lblLoad.Visibility = Visibility.Visible;
            prgBar.Visibility = Visibility.Visible;
            prgBar.IsIndeterminate = true;

            await Task.Run(() =>
            {
                var lines = File.ReadAllLines(@"C:\Users\Lenovo\Downloads\CSV.csv");

                System.Threading.Thread.Sleep(10000);
                List<StockPrice> lstStock = GetStocks(lines);

                Dispatcher.Invoke(() =>
                {
                    grdData.ItemsSource = lstStock;
                });

            });

            lblLoad.Content = "Data Loaded Successfully !!";
            prgBar.Visibility = Visibility.Collapsed;
        }

        private void LoadDataAsync()
        {
            lblLoad.Visibility = Visibility.Collapsed;
            prgBar.Visibility = Visibility.Visible;
            prgBar.IsIndeterminate = true;

            var tsk = Task.Run(() =>
            {
                var lines = File.ReadAllLines(@"C:\Users\Lenovo\Downloads\CSV.csv");
                System.Threading.Thread.Sleep(1000);
                return lines;
            });

            var tskGetTasks = tsk.ContinueWith(t =>
            {
                var lines=tsk.Result;
                return GetStocks(lines);
            },TaskContinuationOptions.OnlyOnRanToCompletion);

            var displaymsgTask = tsk.ContinueWith(t =>
              {
                  System.Threading.Thread.Sleep(1000);
                  Dispatcher.Invoke(() =>
                  {
                      txtComment.AppendText(tsk.Exception.InnerException.Message);

                      lblLoad.Visibility = Visibility.Visible;
                      lblLoad.Content = "Data Loading Failed.";
                      prgBar.Visibility = Visibility.Collapsed;
                  });

              }, TaskContinuationOptions.OnlyOnFaulted);

            var updateGridTask = tskGetTasks.ContinueWith(t =>
             {
                 System.Threading.Thread.Sleep(5000);
                 Dispatcher.Invoke(() =>
                 {
                     grdData.ItemsSource = tskGetTasks.Result;

                     lblLoad.Visibility = Visibility.Visible;
                     lblLoad.Content = "Data Loaded Successfully !!";
                     prgBar.Visibility = Visibility.Collapsed;
                 });
             },TaskContinuationOptions.OnlyOnRanToCompletion);

            //var lines = File.ReadAllLines(@"C:\Users\Lenovo\Downloads\CSV.csv");

            
        }

        private void LoadData()
        {
            lblLoad.Visibility = Visibility.Visible;
            prgBar.Visibility = Visibility.Visible;
            prgBar.IsIndeterminate = true;

            var lines = File.ReadAllLines(@"C:\Users\Lenovo\Downloads\CSV.csv");
            prgBar.Maximum = lines.Length;
            var data = new StockPrice();

            List<StockPrice> lstStock = new List<StockPrice>();
            int i = 0;
            System.Threading.Thread.Sleep(1000);
            foreach (var line in lines.Skip(1))
            {
                i++;
                //prgBar.Value = i;
                System.Threading.Thread.Sleep(100);

                data = new StockPrice();
                string[] content = line.Split(',');

                data.Date = DateTime.Parse(content[0]);
                data.High = Double.Parse(content[1]);
                data.Low = Double.Parse(content[2]);
                data.Open = Double.Parse(content[3]);
                data.Close = Double.Parse(content[4]);
                data.AdjClose = Double.Parse(content[5]);
                data.Volume = long.Parse(content[6]);

                lstStock.Add(data);
            }

            grdData.ItemsSource = lstStock;
            lblLoad.Content = "Data Loaded Successfully !!";
            prgBar.Visibility = Visibility.Collapsed;
        }

        private async Task<string[]> ReadLinesasync(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            return lines;
        }

        private List<StockPrice> GetStocks(string[] lines)
        {
            var data = new StockPrice();
            List<StockPrice> lstStock = new List<StockPrice>();
            
            foreach (var line in lines.Skip(1))
            {
                data = new StockPrice();
                string[] content = line.Split(',');

                data.Date = DateTime.Parse(content[0]);
                data.High = Double.Parse(content[1]);
                data.Low = Double.Parse(content[2]);
                data.Open = Double.Parse(content[3]);
                data.Close = Double.Parse(content[4]);
                data.AdjClose = Double.Parse(content[5]);
                data.Volume = long.Parse(content[6]);

                lstStock.Add(data);
            }

            return lstStock;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            txtComment.AppendText("Cancellation Token Enabled");
            //System.Threading.Thread.Sleep(10000);

            cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
        }
    }
}
