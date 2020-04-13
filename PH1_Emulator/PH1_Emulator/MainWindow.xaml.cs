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




namespace PH1_Emulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        System.Threading.Thread ThreadPH1;

        DateTime DT_ThreadPH1 = new DateTime();

        public MainWindow()
        {
            InitializeComponent();


            ThreadPH1 = new System.Threading.Thread(CyclicPH1);
            ThreadPH1.Name = "Actualize Screen";
            ThreadPH1.IsBackground = true;
            ThreadPH1.Start();
        }



        private void CyclicPH1()
        {
            while (true)
            {
                DT_ThreadPH1 = DateTime.Now;



                System.Threading.Thread.Sleep(1000);

                LB_CyclicTimeThreadPH1.Dispatcher.Invoke(delegate { LB_CyclicTimeThreadPH1.Content = (DateTime.Now - DT_ThreadPH1).ToString(); });

            }

        }

    }
}
