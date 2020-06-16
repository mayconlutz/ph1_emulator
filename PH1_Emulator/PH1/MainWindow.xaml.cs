using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using Path = System.IO.Path;
using System.Data;

namespace PH1
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
        PH1src.UnidadeControle _PH1_Emulator;

        //ManualResetEvent serve para colocar estados de sinalização nos Threads, ou seja 
        ManualResetEvent mrse = new ManualResetEvent(false);

        //Variáveis auxiliares
        bool HabilitaDebugClock;
        int InstrucoesExecutadas = 0;
        int TimeSleepThread = 0;
        bool MemoryAdressDecOrHex = false; //false = HEX, true = DEC

        DataGridRow row;

        public MainWindow()
        {
            InitializeComponent();

            //Instânciando o controle do PH1.
            _PH1_Emulator = new PH1src.UnidadeControle();
            //Criando evento que verifica quando tem algum log do controle do PH1.
            _PH1_Emulator.logs.PropertyChanged += Logs_PropertyChanged;


            //Instânciando o Thread e passando alguns parâmetros necessários;
            ThreadPH1 = new System.Threading.Thread(CyclicPH1);
            ThreadPH1.Name = "Clock PH1";
            ThreadPH1.IsBackground = true;
            ThreadPH1.Start();
            
            //Desativa os barramentos
            DesactiveLines();

            TB_PC_VALUE_DEC.Dispatcher.Invoke(delegate { TB_PC_VALUE_DEC.Text = _PH1_Emulator.Valor_PC.ToString(); });
            TB_PC_VALUE_HEX.Dispatcher.Invoke(delegate { TB_PC_VALUE_HEX.Text = "0x" + _PH1_Emulator.Valor_PC.ToString("X2"); });
            TB_PC_VALUE_BIN.Dispatcher.Invoke(delegate { TB_PC_VALUE_BIN.Text = Convert.ToString(_PH1_Emulator.Valor_PC, 2).PadLeft(8, '0'); });

            TB_AC_VALUE_DEC.Dispatcher.Invoke(delegate { TB_AC_VALUE_DEC.Text = _PH1_Emulator.Valor_AC.ToString(); });
            TB_AC_VALUE_HEX.Dispatcher.Invoke(delegate { TB_AC_VALUE_HEX.Text = "0x" + _PH1_Emulator.Valor_AC.ToString("X2"); });
            TB_AC_VALUE_BIN.Dispatcher.Invoke(delegate { TB_AC_VALUE_BIN.Text = Convert.ToString(_PH1_Emulator.Valor_AC, 2).PadLeft(8, '0'); });

            TB_RI_VALUE_DEC.Dispatcher.Invoke(delegate { TB_RI_VALUE_DEC.Text = _PH1_Emulator._BarramentoRI.ToString(); });
            TB_RI_VALUE_HEX.Dispatcher.Invoke(delegate { TB_RI_VALUE_HEX.Text = "0x" + _PH1_Emulator._BarramentoRI.ToString("X2"); });
            TB_RI_VALUE_BIN.Dispatcher.Invoke(delegate { TB_RI_VALUE_BIN.Text = Convert.ToString(_PH1_Emulator._BarramentoRI, 2).PadLeft(8, '0'); });

            TB_RDM_VALUE_DEC.Dispatcher.Invoke(delegate { TB_RDM_VALUE_DEC.Text = _PH1_Emulator.Valor_RDM.ToString(); });
            TB_RDM_VALUE_HEX.Dispatcher.Invoke(delegate { TB_RDM_VALUE_HEX.Text = "0x" + _PH1_Emulator.Valor_RDM.ToString("X2"); });
            TB_RDM_VALUE_BIN.Dispatcher.Invoke(delegate { TB_RDM_VALUE_BIN.Text = Convert.ToString(_PH1_Emulator.Valor_RDM, 2).PadLeft(8, '0'); });

            TB_REM_VALUE_DEC.Dispatcher.Invoke(delegate { TB_REM_VALUE_DEC.Text = _PH1_Emulator.Valor_REM.ToString(); });
            TB_REM_VALUE_HEX.Dispatcher.Invoke(delegate { TB_REM_VALUE_HEX.Text = "0x" + _PH1_Emulator.Valor_REM.ToString("X2"); });
            TB_REM_VALUE_BIN.Dispatcher.Invoke(delegate { TB_REM_VALUE_BIN.Text = Convert.ToString(_PH1_Emulator.Valor_REM, 2).PadLeft(8, '0'); });

        }
        
        /// <summary>
        /// Evento disparado quando ocorre uma mudança no valor da string de logs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Logs_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Adiciona a string no listbox de log dos componentes.

            if (e.PropertyName.Equals("Modificou Log Componentes"))
            {
                LB_logComponentes.Dispatcher.Invoke(delegate { LB_logComponentes.Items.Add(_PH1_Emulator.logs.getComponentes); });

                #region Memória Atualiza

                if (_PH1_Emulator.logs.getComponentes.Contains("MEMw Executado - MEM[BarramentoREM] <- Barramento RDM"))
                {
                    atualizaMemoria();
                }

                #endregion


                #region Atualiza valores do PC e do AC na tela

                if (_PH1_Emulator.logs.getComponentes.Contains("PCw Executado - Valor PC <- Barramento A") ||
                    _PH1_Emulator.logs.getComponentes.Contains("PC+ Executado - Valor PC <- Valor PC + 1") ||
                     _PH1_Emulator.logs.getComponentes.Contains("Clear")
                    )
                {
                    TB_PC_VALUE_DEC.Dispatcher.Invoke(delegate { TB_PC_VALUE_DEC.Text = _PH1_Emulator.Valor_PC.ToString(); });
                    TB_PC_VALUE_HEX.Dispatcher.Invoke(delegate { TB_PC_VALUE_HEX.Text = "0x"+_PH1_Emulator.Valor_PC.ToString("X2"); });
                    TB_PC_VALUE_BIN.Dispatcher.Invoke(delegate { TB_PC_VALUE_BIN.Text =  Convert.ToString(_PH1_Emulator.Valor_PC,2).PadLeft(8,'0'); });

                    atualizaRowMemoria();

                }

                if (_PH1_Emulator.logs.getComponentes.Contains("ACw Executado - Valor AC <- Barramento A") ||
                    _PH1_Emulator.logs.getComponentes.Contains("ACc Executado - Valor AC <- Barramento C") ||
                    _PH1_Emulator.logs.getComponentes.Contains("Clear")
                    )
                {
                    TB_AC_VALUE_DEC.Dispatcher.Invoke(delegate { TB_AC_VALUE_DEC.Text = _PH1_Emulator.Valor_AC.ToString(); });
                    TB_AC_VALUE_HEX.Dispatcher.Invoke(delegate { TB_AC_VALUE_HEX.Text = "0x" + _PH1_Emulator.Valor_AC.ToString("X2"); });
                    TB_AC_VALUE_BIN.Dispatcher.Invoke(delegate { TB_AC_VALUE_BIN.Text = Convert.ToString(_PH1_Emulator.Valor_AC, 2).PadLeft(8, '0'); });
                }


                if (_PH1_Emulator.logs.getComponentes.Contains("ACw Executado - Valor AC <- Barramento A") ||
                    _PH1_Emulator.logs.getComponentes.Contains("ACc Executado - Valor AC <- Barramento C") ||
                    _PH1_Emulator.logs.getComponentes.Contains("Clear")
                    )
                {
                    TB_AC_VALUE_DEC.Dispatcher.Invoke(delegate { TB_AC_VALUE_DEC.Text = _PH1_Emulator.Valor_AC.ToString(); });
                    TB_AC_VALUE_HEX.Dispatcher.Invoke(delegate { TB_AC_VALUE_HEX.Text = "0x" + _PH1_Emulator.Valor_AC.ToString("X2"); });
                    TB_AC_VALUE_BIN.Dispatcher.Invoke(delegate { TB_AC_VALUE_BIN.Text = Convert.ToString(_PH1_Emulator.Valor_AC, 2).PadLeft(8, '0'); });
                }


                if (_PH1_Emulator.logs.getComponentes.Contains("RIw Executado - Barramento RI <- Barramento A"))
                {
                    TB_RI_VALUE_DEC.Dispatcher.Invoke(delegate { TB_RI_VALUE_DEC.Text = _PH1_Emulator._BarramentoRI.ToString(); });
                    TB_RI_VALUE_HEX.Dispatcher.Invoke(delegate { TB_RI_VALUE_HEX.Text = "0x" + _PH1_Emulator._BarramentoRI.ToString("X2"); });
                    TB_RI_VALUE_BIN.Dispatcher.Invoke(delegate { TB_RI_VALUE_BIN.Text = Convert.ToString(_PH1_Emulator._BarramentoRI, 2).PadLeft(8, '0'); });
                }

                if (_PH1_Emulator.logs.getComponentes.Contains("RDMw Executado - Valor RDM <- Barramento A"))
                {
                    TB_RDM_VALUE_DEC.Dispatcher.Invoke(delegate { TB_RDM_VALUE_DEC.Text = _PH1_Emulator.Valor_RDM.ToString(); });
                    TB_RDM_VALUE_HEX.Dispatcher.Invoke(delegate { TB_RDM_VALUE_HEX.Text = "0x" + _PH1_Emulator.Valor_RDM.ToString("X2"); });
                    TB_RDM_VALUE_BIN.Dispatcher.Invoke(delegate { TB_RDM_VALUE_BIN.Text = Convert.ToString(_PH1_Emulator.Valor_RDM, 2).PadLeft(8, '0'); });
                }

                if (_PH1_Emulator.logs.getComponentes.Contains("REMw Executado - Valor REM <- Barramento A"))
                {
                    TB_REM_VALUE_DEC.Dispatcher.Invoke(delegate { TB_REM_VALUE_DEC.Text = _PH1_Emulator.Valor_REM.ToString(); });
                    TB_REM_VALUE_HEX.Dispatcher.Invoke(delegate { TB_REM_VALUE_HEX.Text = "0x" + _PH1_Emulator.Valor_REM.ToString("X2"); });
                    TB_REM_VALUE_BIN.Dispatcher.Invoke(delegate { TB_REM_VALUE_BIN.Text = Convert.ToString(_PH1_Emulator.Valor_REM, 2).PadLeft(8, '0'); });
                }

                #endregion

            }

            if (e.PropertyName.Equals("Modificou Log UC"))
            {
                LB_logUnidadeControle.Dispatcher.Invoke(delegate { LB_logUnidadeControle.Items.Add(_PH1_Emulator.logs.getstring_UC); });

                //Ativa e desativa as linhas conforme o log 
                DesactiveLines();


                if (_PH1_Emulator.logs.getstring_UC.Contains("REM <- PC"))
                {
                    ActiveBarramentoA();
                    Line_PCr.Dispatcher.Invoke(delegate { Line_PCr.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    Line_REMw.Dispatcher.Invoke(delegate { Line_REMw.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                }

                if (_PH1_Emulator.logs.getstring_UC.Contains("RDM <- MEM"))
                {
                    Line_MEMr.Dispatcher.Invoke(delegate { Line_MEMr.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    Line_MEMr_1.Dispatcher.Invoke(delegate { Line_MEMr_1.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    Line_RDM_1.Dispatcher.Invoke(delegate { Line_RDM_1.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                }

                if (_PH1_Emulator.logs.getstring_UC.Contains("PC <- PC + 1"))
                {
                    Line_PCmais.Dispatcher.Invoke(delegate { Line_PCmais.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    Line_PCmais_1.Dispatcher.Invoke(delegate { Line_PCmais_1.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    Line_PCmais_2.Dispatcher.Invoke(delegate { Line_PCmais_2.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    Line_PCmais_3.Dispatcher.Invoke(delegate { Line_PCmais_3.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    Line_PCmais_4.Dispatcher.Invoke(delegate { Line_PCmais_4.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    Line_PCmais_5.Dispatcher.Invoke(delegate { Line_PCmais_5.Foreground = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                }

                if (_PH1_Emulator.logs.getstring_UC.Contains("RI <- RDM7..4"))
                {
                    Line_RDMr.Dispatcher.Invoke(delegate { Line_RDMr.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    ActiveBarramentoA();
                    Line_RIw.Dispatcher.Invoke(delegate { Line_RIw.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                }

                if (_PH1_Emulator.logs.getstring_UC.Contains("REM <- RDM"))
                {
                    Line_RDMr.Dispatcher.Invoke(delegate { Line_RDMr.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    ActiveBarramentoA();
                    Line_REMw.Dispatcher.Invoke(delegate { Line_REMw.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });

                }

                if (_PH1_Emulator.logs.getstring_UC.Contains("AC <- RDM"))
                {
                    Line_RDMr.Dispatcher.Invoke(delegate { Line_RDMr.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    ActiveBarramentoA();
                    Line_ACw.Dispatcher.Invoke(delegate { Line_ACw.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                }

                if (_PH1_Emulator.logs.getstring_UC.Contains("RDM <- AC"))
                {
                    Line_ACr.Dispatcher.Invoke(delegate { Line_ACr.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    ActiveBarramentoA();
                    Line_RDMw.Dispatcher.Invoke(delegate { Line_RDMw.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                }

                if (_PH1_Emulator.logs.getstring_UC.Contains("9 - MEM["))
                {
                    Line_MEMw.Dispatcher.Invoke(delegate { Line_MEMw.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    Line_REMw1.Dispatcher.Invoke(delegate { Line_REMw1.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });

                }

                if (_PH1_Emulator.logs.getstring_UC.Contains("AC <- AC + ") ||
                    _PH1_Emulator.logs.getstring_UC.Contains("AC <- AC - ") ||
                    _PH1_Emulator.logs.getstring_UC.Contains("AC <- AC * ") ||
                    _PH1_Emulator.logs.getstring_UC.Contains("AC <- AC / ") ||
                    _PH1_Emulator.logs.getstring_UC.Contains("AC <- AC & ") ||
                    _PH1_Emulator.logs.getstring_UC.Contains("AC <- AC | ") ||
                    _PH1_Emulator.logs.getstring_UC.Contains("AC <- AC ^ ")
                    )
                {
                    Line_RDMr.Dispatcher.Invoke(delegate { Line_RDMr.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    ActiveBarramentoA();
                    ActiveBarramentoB();
                    Line_ACc1.Dispatcher.Invoke(delegate { Line_ACc1.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    ActiveBarramentoC();
                }

                if (_PH1_Emulator.logs.getstring_UC.Contains("AC <- !AC"))
                {
                    ActiveBarramentoB();
                    Line_ACc1.Dispatcher.Invoke(delegate { Line_ACc1.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    ActiveBarramentoC();
                }

                if (_PH1_Emulator.logs.getstring_UC.Contains("PC <- RDM"))
                {
                    Line_RDMr.Dispatcher.Invoke(delegate { Line_RDMr.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                    ActiveBarramentoA();
                    Line_PCw.Dispatcher.Invoke(delegate { Line_PCw.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
                }

                if (_PH1_Emulator.logs.getstring_UC.Contains("4 - Ir p/ 0 e PAUSE"))
                {
                    mrse.Reset();
                }

            }

            if (e.PropertyName.Equals("Modificou Instrucoes"))
            {
                LB_logInstructions.Dispatcher.Invoke(delegate { LB_logInstructions.Items.Add(_PH1_Emulator.logs.getstring_Instrucoes); });


                InstrucoesExecutadas += 1;
                LB_LogInsutrucoes.Dispatcher.Invoke(delegate { LB_LogInsutrucoes.Content = "Total de " + InstrucoesExecutadas + " instruções executadas."; });
            }


            //Seleciona o ultimo item da lista e mostra no scroll o item, para poder rolar automaticamente para baixo a cada atualização
            LB_logComponentes.Dispatcher.Invoke(delegate { LB_logComponentes.SelectedIndex = LB_logComponentes.Items.Count - 1; });
            LB_logComponentes.Dispatcher.Invoke(delegate { LB_logComponentes.ScrollIntoView(LB_logComponentes.SelectedItem); });

            //Seleciona o ultimo item da lista e mostra no scroll o item, para poder rolar automaticamente para baixo a cada atualização
            LB_logUnidadeControle.Dispatcher.Invoke(delegate { LB_logUnidadeControle.SelectedIndex = LB_logUnidadeControle.Items.Count - 1; });
            LB_logUnidadeControle.Dispatcher.Invoke(delegate { LB_logUnidadeControle.ScrollIntoView(LB_logUnidadeControle.SelectedItem); });

            //Seleciona o ultimo item da lista e mostra no scroll o item, para poder rolar automaticamente para baixo a cada atualização
            LB_logInstructions.Dispatcher.Invoke(delegate { LB_logInstructions.SelectedIndex = LB_logInstructions.Items.Count - 1; });
            LB_logInstructions.Dispatcher.Invoke(delegate { LB_logInstructions.ScrollIntoView(LB_logInstructions.SelectedItem); });
        }

        #region Funções auxiliares color

        private void ActiveBarramentoA()
        {
            BarramentoA_1.Dispatcher.Invoke(delegate { BarramentoA_1.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoA_2.Dispatcher.Invoke(delegate { BarramentoA_2.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoA_3.Dispatcher.Invoke(delegate { BarramentoA_3.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoA_4.Dispatcher.Invoke(delegate { BarramentoA_4.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoA_5.Dispatcher.Invoke(delegate { BarramentoA_5.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoA_6.Dispatcher.Invoke(delegate { BarramentoA_6.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoA_7.Dispatcher.Invoke(delegate { BarramentoA_7.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoA_8.Dispatcher.Invoke(delegate { BarramentoA_8.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoA_9.Dispatcher.Invoke(delegate { BarramentoA_9.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
        }

        private void ActiveBarramentoB()
        {
            BarramentoB_1.Dispatcher.Invoke(delegate { BarramentoB_1.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoB_2.Dispatcher.Invoke(delegate { BarramentoB_2.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoB_3.Dispatcher.Invoke(delegate { BarramentoB_3.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoB_4.Dispatcher.Invoke(delegate { BarramentoB_4.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoB_5.Dispatcher.Invoke(delegate { BarramentoB_5.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoB_6.Dispatcher.Invoke(delegate { BarramentoB_6.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
        }

        private void ActiveBarramentoC()
        {
            BarramentoC_1.Dispatcher.Invoke(delegate { BarramentoC_1.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoC_2.Dispatcher.Invoke(delegate { BarramentoC_2.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            BarramentoC_3.Dispatcher.Invoke(delegate { BarramentoC_3.Stroke = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
        }

        private void DesactiveLines()
        {
            //Barramento A
            BarramentoA_1.Dispatcher.Invoke(delegate { BarramentoA_1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoA_2.Dispatcher.Invoke(delegate { BarramentoA_2.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoA_3.Dispatcher.Invoke(delegate { BarramentoA_3.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoA_4.Dispatcher.Invoke(delegate { BarramentoA_4.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoA_5.Dispatcher.Invoke(delegate { BarramentoA_5.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoA_6.Dispatcher.Invoke(delegate { BarramentoA_6.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoA_7.Dispatcher.Invoke(delegate { BarramentoA_7.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoA_8.Dispatcher.Invoke(delegate { BarramentoA_8.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoA_9.Dispatcher.Invoke(delegate { BarramentoA_9.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });

            BarramentoB_1.Dispatcher.Invoke(delegate { BarramentoB_1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoB_2.Dispatcher.Invoke(delegate { BarramentoB_2.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoB_3.Dispatcher.Invoke(delegate { BarramentoB_3.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoB_4.Dispatcher.Invoke(delegate { BarramentoB_4.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoB_5.Dispatcher.Invoke(delegate { BarramentoB_5.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoB_6.Dispatcher.Invoke(delegate { BarramentoB_6.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });

            BarramentoC_1.Dispatcher.Invoke(delegate { BarramentoC_1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoC_2.Dispatcher.Invoke(delegate { BarramentoC_2.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            BarramentoC_3.Dispatcher.Invoke(delegate { BarramentoC_3.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });

            Line_ACr.Dispatcher.Invoke(delegate { Line_ACr.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_ACw.Dispatcher.Invoke(delegate { Line_ACw.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_ACc1.Dispatcher.Invoke(delegate { Line_ACc1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });

            Line_PCr.Dispatcher.Invoke(delegate { Line_PCr.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_PCw.Dispatcher.Invoke(delegate { Line_PCw.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_PCmais.Dispatcher.Invoke(delegate { Line_PCmais.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_PCmais_1.Dispatcher.Invoke(delegate { Line_PCmais_1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_PCmais_2.Dispatcher.Invoke(delegate { Line_PCmais_2.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_PCmais_3.Dispatcher.Invoke(delegate { Line_PCmais_3.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_PCmais_4.Dispatcher.Invoke(delegate { Line_PCmais_4.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_PCmais_5.Dispatcher.Invoke(delegate { Line_PCmais_5.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });

            Line_MEMr.Dispatcher.Invoke(delegate { Line_MEMr.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_MEMw.Dispatcher.Invoke(delegate { Line_MEMw.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_MEMr_1.Dispatcher.Invoke(delegate { Line_MEMr_1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_MEMw_1.Dispatcher.Invoke(delegate { Line_MEMw_1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });

            Line_REMr.Dispatcher.Invoke(delegate { Line_REMr.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_REMw.Dispatcher.Invoke(delegate { Line_REMw.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });

            Line_RDMr.Dispatcher.Invoke(delegate { Line_RDMr.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_RDMw.Dispatcher.Invoke(delegate { Line_RDMw.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });

            Line_RI.Dispatcher.Invoke(delegate { Line_RI.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_RIw.Dispatcher.Invoke(delegate { Line_RIw.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });

            Line_RDM_1.Dispatcher.Invoke(delegate { Line_RDM_1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });
            Line_REMw1.Dispatcher.Invoke(delegate { Line_REMw1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)); });

        }

        #endregion


        /// <summary>
        /// Atualiza o datagrid da mémoria de acordo com a memória
        /// </summary>
        private void atualizaMemoria()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("Endereço", typeof(string));
            dt.Columns.Add("Valor16", typeof(string));
            dt.Columns.Add("Valor10", typeof(string));
            dt.Columns.Add("Valor2", typeof(string));
            //dt.Columns.Add("Instrução", typeof(string));
            for (int i = 0; i <= 255; i++)
            {
                DataRow row = dt.NewRow();
                if (!MemoryAdressDecOrHex)
                {
                    row["Endereço"] = i.ToString("X2");
                }
                else
                {
                    row["Endereço"] = i;
                }
                
                row["Valor16"] = _PH1_Emulator._MEM[i].ToString("X2");
                row["Valor10"] = _PH1_Emulator._MEM[i].ToString();
                row["Valor2"] = Convert.ToString(_PH1_Emulator._MEM[i], 2).PadLeft(8, '0');
                //row["Instrução"] =  AssemblerSrc.Controle.Memory[i].ToString("X2");
                dt.Rows.Add(row);
            }

            DT_Memory.Dispatcher.Invoke(delegate { DT_Memory.ItemsSource = ""; });
            DT_Memory.Dispatcher.Invoke(delegate { DT_Memory.ItemsSource = dt.DefaultView; });
            atualizaRowMemoria();
        }

        private void atualizaRowMemoria()
        {
            if (row != null)
            {
                row.Dispatcher.Invoke(delegate { row.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)); });
            }

            row = (DataGridRow)DT_Memory.ItemContainerGenerator.ContainerFromIndex(_PH1_Emulator.Valor_PC);

            if (row != null)
            {

                DT_Memory.Dispatcher.Invoke(delegate { DT_Memory.SelectedItem = row; });
                //DT_Memory.Dispatcher.Invoke(delegate { DT_Memory.ScrollIntoView(row); });
                //row.Dispatcher.Invoke(delegate { row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)); });
                row.Dispatcher.Invoke(delegate { row.Background = new SolidColorBrush(Color.FromRgb(0, 220, 0)); });
            }
        }

        /// <summary>
        /// Thread que roda ciclico, simulando o PH1
        /// </summary>
        private void CyclicPH1()
        {
            while (true)
            {
                mrse.WaitOne();

                //Armazena o tempo atual
                DT_ThreadPH1 = DateTime.Now;

                //Envia clock para Unidade de controle do PH1.
                _PH1_Emulator.Clock = true;

                //Coloca o Thread para dormir, enviando o argumento do tempo em milessegundos.
                System.Threading.Thread.Sleep(TimeSleepThread);

                //Subtrai o tempo atual do tempo armazenado no inicio do laço, assim sabemos o tempo que levou para percorrer um ciclo do loop.
                LB_CyclicTimeThreadPH1.Dispatcher.Invoke(delegate { LB_CyclicTimeThreadPH1.Content = "Último Ciclo: " + (DateTime.Now - DT_ThreadPH1).TotalSeconds.ToString() + "ms"; });


                CB_DebugClock.Dispatcher.Invoke(delegate { HabilitaDebugClock = (bool)CB_DebugClock.IsChecked; });
                if (HabilitaDebugClock)
                {
                    mrse.Reset();
                }
            }

        }

        #region Eventos da tela Simulador 

        private void BT_PausePH1_Click(object sender, RoutedEventArgs e)
        {
            mrse.Reset();
        }

        private void BT_PlayPH1_Click(object sender, RoutedEventArgs e)
        {
            mrse.Set();
        }

        private void BT_StopPH1_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            mrse.Reset();
        }

        private void BT_LoadArchive_Click(object sender, RoutedEventArgs e)
        {
            Clear();

            OpenFileDialog Ofp = new OpenFileDialog();
            Ofp.Title = "Abra o Arquivo de Texto";
            Ofp.Filter = "Arquivos TXT (*.txt)|*.txt|All files (*.*)|*.*";

            if (Ofp.ShowDialog() == true)
            {
                var sr = new StreamReader(Ofp.FileName);

                var st1 = sr.ReadToEnd();

                //Comentários sobre caracteres especiais e sistemas operacionais.
                // \r = CR(Carriage Return) // Usado como quebra de linha no Mac OS anterior à versão X
                // \n = LF(Line Feed) // Usado como quebra de linha Unix/Mac OS superior à versão X
                // \r\n = CR + LF // Usado como quebra de linha no Windows


                //Melhorar o código, só esta em funcionamento....
                bool auxread = false;
                byte endereco = 0;
                byte valor = 0;
                string stringdummy = "";
                int i = 0;
                foreach (var item in st1)
                {
                    if (!item.Equals('\n'))
                    {
                        if (item == ' ' || item.Equals('\r'))
                        {
                            if (auxread)
                            {
                                valor = byte.Parse(stringdummy, System.Globalization.NumberStyles.HexNumber);
                                stringdummy = "";
                                auxread = !auxread;
                                _PH1_Emulator._MEM[endereco] = valor;
                            }
                            else
                            {
                                endereco = byte.Parse(stringdummy, System.Globalization.NumberStyles.HexNumber);
                                stringdummy = "";
                                auxread = !auxread;
                            }
                        }
                        else
                        {
                            stringdummy += item;
                        }
                    }

                    i += 1;
                    if (i == st1.Length)
                    {
                        valor = byte.Parse(stringdummy, System.Globalization.NumberStyles.HexNumber);
                        _PH1_Emulator._MEM[endereco] = valor;
                        atualizaMemoria();
                    }

                }
            }
        }

        #endregion

        #region Eventos na tela Assembler

        void propertyGridComboBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (propertyGrid == null)
                return;
            switch (propertyGridComboBox.SelectedIndex)
            {
                case 0:
                    propertyGrid.SelectedObject = textEditor;
                    break;
                case 1:
                    propertyGrid.SelectedObject = textEditor.TextArea;
                    break;
                case 2:
                    propertyGrid.SelectedObject = textEditor.Options;
                    break;
            }
        }

        string currentFileName;

        void openFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            if (dlg.ShowDialog() ?? false)
            {
                currentFileName = dlg.FileName;
                textEditor.Load(currentFileName);
                textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(currentFileName));
            }
        }

        void saveFileClick(object sender, EventArgs e)
        {
            if (currentFileName == null)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = ".txt";
                if (dlg.ShowDialog() ?? false)
                {
                    currentFileName = dlg.FileName;
                }
                else
                {
                    return;
                }
            }
            textEditor.Save(currentFileName);
        }

        #region Folding

        FoldingManager foldingManager;
        object foldingStrategy;

        void HighlightingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (textEditor.SyntaxHighlighting == null)
            {
                foldingStrategy = null;
            }
            else
            {
                switch (textEditor.SyntaxHighlighting.Name)
                {
                    case "XML":
                        foldingStrategy = new XmlFoldingStrategy();
                        textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                        break;
                    case "C#":
                    case "C++":
                    case "PHP":
                    case "Java":
                        textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(textEditor.Options);
                        //foldingStrategy = new BraceFoldingStrategy();
                        break;
                    default:
                        textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                        foldingStrategy = null;
                        break;
                }
            }
            if (foldingStrategy != null)
            {
                if (foldingManager == null)
                    foldingManager = FoldingManager.Install(textEditor.TextArea);
                UpdateFoldings();
            }
            else
            {
                if (foldingManager != null)
                {
                    FoldingManager.Uninstall(foldingManager);
                    foldingManager = null;
                }
            }
        }

        void UpdateFoldings()
        {
            // if (foldingStrategy is BraceFoldingStrategy)
            // {
            //     ((BraceFoldingStrategy)foldingStrategy).UpdateFoldings(foldingManager, textEditor.Document);
            // }
            if (foldingStrategy is XmlFoldingStrategy)
            {
                ((XmlFoldingStrategy)foldingStrategy).UpdateFoldings(foldingManager, textEditor.Document);
            }
        }

        #endregion

        private void BT_Assemble_Click(object sender, RoutedEventArgs e)
        {
            string s = textEditor.Text;


            if (AssemblerSrc.Controle.Assembler(ref s, textEditor.Text.Length) == 0)
            {
                DataTable dt = new DataTable();

                dt.Columns.Add("Endereço", typeof(string));
                dt.Columns.Add("Valor16", typeof(string));
                dt.Columns.Add("Valor10", typeof(string));
                dt.Columns.Add("Valor2", typeof(string));
                dt.Columns.Add("Instrução", typeof(string));
                for (int i = 0; i <= 255; i++)
                {
                    DataRow row = dt.NewRow();
                    row["Endereço"] = i;
                    row["Valor16"] = AssemblerSrc.Controle.Memory[i].ToString("X2");
                    row["Valor10"] = AssemblerSrc.Controle.Memory[i].ToString();
                    row["Valor2"] = Convert.ToString(AssemblerSrc.Controle.Memory[i], 2).PadLeft(8, '0');
                    row["Instrução"] = AssemblerSrc.Controle.Memory[i].ToString("X2");
                    dt.Rows.Add(row);
                }

                DT_Code.ItemsSource = dt.DefaultView;

                dt = new DataTable();

                dt.Columns.Add("Endereço", typeof(string));
                dt.Columns.Add("Rótulo", typeof(string));

                for (int i = 0; i <= 255; i++)
                {
                    DataRow row = dt.NewRow();
                    row["Endereço"] = i;
                    row["Rótulo"] = AssemblerSrc.Controle.SymbolTable[i];
                    dt.Rows.Add(row);
                }
                DT_TS.ItemsSource = dt.DefaultView;
            }
            else
            {
                //mostrar mensagem de erro e atualizar a error list
                DataTable dt = new DataTable();

                DT_Code.ItemsSource = dt.DefaultView;
                DT_TS.ItemsSource = dt.DefaultView;
            }





        }

        private void BT_LoadToSim_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            _PH1_Emulator._MEM = AssemblerSrc.Controle.Memory;
            atualizaMemoria();
        }

        #endregion

        /// <summary>
        /// Limpa memória do PH1, limpa logs, reseta instruções executadas.
        /// </summary>
        private void Clear()
        {
            _PH1_Emulator.ClearUC();
            LB_logComponentes.Items.Clear();
            LB_logInstructions.Items.Clear();
            LB_logUnidadeControle.Items.Clear();

            InstrucoesExecutadas = 0;
            LB_LogInsutrucoes.Dispatcher.Invoke(delegate { LB_LogInsutrucoes.Content = "Total de " + InstrucoesExecutadas + " instruções executadas."; });

        }

        private void SliderTimeSleep_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSleepThread = (int)SliderTimeSleep.Value;

            TB_ActualValueSleep.Text = TimeSleepThread.ToString() + "ms";

        }

        private void BT_Hex_Dec_Memory_Click(object sender, RoutedEventArgs e)
        {
            MemoryAdressDecOrHex = !MemoryAdressDecOrHex;
            if (MemoryAdressDecOrHex)
            {
                BT_Hex_Dec_Memory.Content = "DEC";
            }
            else
            {
                BT_Hex_Dec_Memory.Content = "HEX";
            }


            atualizaMemoria();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadPH1.Interrupt();
        }
    }
}
