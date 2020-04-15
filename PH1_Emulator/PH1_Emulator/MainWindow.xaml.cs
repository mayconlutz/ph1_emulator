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
        //Declarando Thread do loop que roda o clock da unidade de controle do PH1.
        System.Threading.Thread ThreadPH1;

        //Instanciando o objeto
        DateTime DT_ThreadPH1 = new DateTime();

        //Declarando o Controle do PH1.
        PH1.UnidadeControle PH1_Emulator;

        bool teste;

        public MainWindow()
        {
            InitializeComponent();

            //Instânciando o controle do PH1.
            PH1_Emulator = new PH1.UnidadeControle();
            //Criando evento que verifica quando tem algum log do controle do PH1.
            PH1_Emulator.logs.PropertyChanged += Logs_PropertyChanged;

            //Instânciando o Thread e passando alguns parâmetros necessários;
            ThreadPH1 = new System.Threading.Thread(CyclicPH1);
            ThreadPH1.Name = "Clock PH1";
            ThreadPH1.IsBackground = true;
            ThreadPH1.Start();
        }

        //Evento disparado quando ocorre uma mudança no valor da string de logs dos componentes.
        private void Logs_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Adiciona a string no listbox de log dos componentes.

            CB_AtivaDesativaLogComponentes.Dispatcher.Invoke(delegate { teste = (bool)CB_AtivaDesativaLogComponentes.IsChecked; });
            if (teste)
            {
                LB_logComponentes.Dispatcher.Invoke(delegate { LB_logComponentes.Items.Add(PH1_Emulator.logs.getComponentes); });
            }
  
        }

        private void CyclicPH1()
        {
            while (true)
            {
                //Armazena o tempo atual
                DT_ThreadPH1 = DateTime.Now;

                //Envia clock para Unidade de controle do PH1.
                PH1_Emulator.Clock = true;



                //Coloca o Thread para dormir, enviando o argumento do tempo em milessegundos.
                System.Threading.Thread.Sleep(1);

                //Subtrai o tempo atual do tempo armazenado no inicio do laço, assim sabemos o tempo que levou para percorrer um ciclo do loop.
                LB_CyclicTimeThreadPH1.Dispatcher.Invoke(delegate { LB_CyclicTimeThreadPH1.Content = (DateTime.Now - DT_ThreadPH1).ToString(); });

            }

        }

        //Evento do CheckBox que ativa ou desativa o log na tela.
        private void CB_AtivaDesativaLogComponentes_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((bool)CB_AtivaDesativaLogComponentes.IsChecked)
            {
                MessageBoxResult result = MessageBox.Show("Desativando o Log dos componentes, o sistema não irá mais gravar os logs!", "Desativar Logs dos Componentes", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    CB_AtivaDesativaLogComponentes.IsChecked = false;
                }

            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Ativando o Log dos componentes, o sistema irá consumir mais memória RAM!", "Ativar Logs dos Componentes", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    CB_AtivaDesativaLogComponentes.IsChecked = true;
                }
            }
        }
    }
}
