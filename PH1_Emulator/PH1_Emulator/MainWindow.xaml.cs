using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace PH1_Emulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        System.Threading.Thread ThreadPH1;

        DateTime DT_ThreadPH1 = new DateTime();

        PH1.UnidadeControle PH1_Emulator;

        
        public MainWindow()
        {
            InitializeComponent();

            PH1_Emulator = new PH1.UnidadeControle();

            PH1_Emulator.logs.PropertyChanged += Logs_PropertyChanged;

            ThreadPH1 = new System.Threading.Thread(CyclicPH1);
            ThreadPH1.Name = "Actualize Screen";
            ThreadPH1.IsBackground = true;
            ThreadPH1.Start();

        }

        private void Logs_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            LB_TESTE.Dispatcher.Invoke(delegate { LB_TESTE.Items.Add(PH1_Emulator.logs.getComponentes);});       
        }

        private void CyclicPH1()
        {
            while (true)
            {
                DT_ThreadPH1 = DateTime.Now;

                //Envia clock para Unidade de controle do PH1.
                PH1_Emulator.Clock = true;

                //Atualiza listbox da com a lista de logs dos componentes do PH1.



                System.Threading.Thread.Sleep(1);

                LB_CyclicTimeThreadPH1.Dispatcher.Invoke(delegate { LB_CyclicTimeThreadPH1.Content = (DateTime.Now - DT_ThreadPH1).ToString(); });

            }

        }
    }
}
