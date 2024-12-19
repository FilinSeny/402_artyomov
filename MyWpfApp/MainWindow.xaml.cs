using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using NugetPack;

namespace MyWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GenAlgoViewer Data = new GenAlgoViewer();
        private CancellationTokenSource cancellationTokenSource;
        GenericAlgo gen_alg = new GenericAlgo();
        public MainWindow()
        {
            InitializeComponent();
            MessageBox.Show("Hi1");
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = Data;
            ///MessageBox.Show("Hi");


            Binding bd1 = new Binding();
            bd1.Source = Data;
            bd1.Path = new PropertyPath("N_sqs_1");
            N_sqs_1_input.SetBinding(TextBox.TextProperty, bd1);

            Binding bd2 = new Binding();
            bd2.Source = Data;
            bd2.Path = new PropertyPath("N_sqs_2");
            N_sqs_2_input.SetBinding(TextBox.TextProperty, bd2);

            Binding bd3 = new Binding();
            bd3.Source = Data;
            bd3.Path = new PropertyPath("N_sqs_3");
            N_sqs_3_input.SetBinding(TextBox.TextProperty, bd3);

            /*Binding bd4 = new Binding();
            bd4.Source = Data;
            bd4.Path = new PropertyPath("DisplayedIters");
            bd4.Mode = BindingMode.OneWay;
            N_iterations_out.SetBinding(TextBox.TextProperty, bd4);*/

            /*Binding bd5 = new Binding();
            bd5.Source = Data;
            bd5.Mode = BindingMode.OneWay;
            bd5.Path = new PropertyPath("DispalayedSq");
            Square_out.SetBinding(TextBox.TextProperty, bd5);*/

        }


        private async void Start_Button_Click(object sender, RoutedEventArgs e)
        {

            ////MessageBox.Show($"{Data.n_sq1}, {Data.n_sq2}, {Data.n_sq3}");
            ///Data.Start();
            Start_Button.IsEnabled = false;
            Stop_Button.IsEnabled = true;
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    Create_and_Exec_Gen_Alg(Data, cancellationTokenSource.Token);
                }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
            
        }


        private void Create_and_Exec_Gen_Alg(GenAlgoViewer data, CancellationToken token)
        {
            data.Prepare();
            Solution best_sol = data.Start(token, (iter, best_metric, Sol) =>
            {
                Dispatcher.Invoke(() =>
                {
                    N_iterations_out.Text = iter.ToString();
                    Square_out.Text = best_metric.ToString();
                    Best_sol_out.Text = Sol.ToString();
                    print_all(Sol, iter, false);
                });
            }
            );
            print_all(best_sol);

           

        }

        void print_all(Solution solution, int n = 0, bool is_final = true)
        {
            if (n % 100 != 0) return;

            Dispatcher.Invoke(() =>
            {
                sq_dest.Children.Clear();
            });

            solution.normalaze();
            foreach (var sq in solution.rectangles)
            {
                Dispatcher.Invoke(() =>
                {
                    Draw_Squares(sq, is_final);
                });
            }

           
        }


        private void Draw_Squares(NugetPack.Rectangle square, bool is_final = false, int scale = 30)
        {
            Brush colour;
            switch (square.weight)
            {
                case 1:
                    colour = Brushes.Aqua; break;
                case 2:
                    colour = Brushes.Green; break;
                case 3:
                    colour = Brushes.Pink; break;
                default: colour = Brushes.Red; break;
            }
            System.Windows.Shapes.Rectangle rec = new System.Windows.Shapes.Rectangle
            {
                Width = square.weight * scale,
                Height = square.height * scale,
                Stroke = Brushes.Black,
                Fill = colour
            };

            Canvas.SetLeft(rec, square.x_l * scale);
            Canvas.SetBottom(rec, square.y_b * scale);

            sq_dest.Children.Add(rec);

            if (is_final)
            {
                Stop_Button.IsEnabled = false;
                Start_Button.IsEnabled = true;
            }
        }

        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            Stop_Button.IsEnabled = false;
            Start_Button.IsEnabled = true;
        }

        public string ScrollableText { get; set; }
    }

}


        

