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
        src.UnidadeControle _PH1_Emulator;
        

        bool HabilitaLogComponentes;
        bool HabilitaLogUC;
        bool HabilitaDebugClock;

        src.TabelaMemoria WindowTabelaMemoria;

        public MainWindow()
        {
            //codeBox.InitializeComponent();

            //codeBox.CurrentHighlighter = HighlighterManager.Instance.Highlighters["VHDL"];

            InitializeComponent();

            //Instânciando o controle do PH1.
            _PH1_Emulator = new src.UnidadeControle();
            //Criando evento que verifica quando tem algum log do controle do PH1.
            _PH1_Emulator.logs.PropertyChanged += Logs_PropertyChanged;

            //teste
            byte[] MEM = new byte[256];
            MEM[0x0] = 0x10;
            MEM[0x1] = 0x81;
            MEM[0x2] = 0x30;
            MEM[0x3] = 0x82;
            MEM[0x4] = 0x20;
            MEM[0x5] = 0x80;
            MEM[0x6] = 0xF0;
            MEM[0x80] = 0x00;
            MEM[0x81] = 0x05;
            MEM[0x82] = 0x02;

            _PH1_Emulator._MEM = MEM;

            //Instânciando o Thread e passando alguns parâmetros necessários;
            ThreadPH1 = new System.Threading.Thread(CyclicPH1);
            ThreadPH1.Name = "Clock PH1";
            ThreadPH1.IsBackground = true;
            ThreadPH1.Start();


        }

        //Evento disparado quando ocorre uma mudança no valor da string de logs dos componentes.
        private void Logs_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Adiciona a string no listbox de log dos componentes.

            CB_AtivaDesativaLogComponentes.Dispatcher.Invoke(delegate { HabilitaLogComponentes = (bool)CB_AtivaDesativaLogComponentes.IsChecked; });

            if (HabilitaLogComponentes && e.PropertyName.Equals("Modificou Log Componentes"))
            {
                LB_logComponentes.Dispatcher.Invoke(delegate { LB_logComponentes.Items.Add(_PH1_Emulator.logs.getComponentes); });

            }

            CB_AtivaDesativaLogUnidadeControle.Dispatcher.Invoke(delegate { HabilitaLogUC = (bool)CB_AtivaDesativaLogUnidadeControle.IsChecked; });

            if (HabilitaLogUC && e.PropertyName.Equals("Modificou Log UC"))
            {
                LB_logUnidadeControle.Dispatcher.Invoke(delegate { LB_logUnidadeControle.Items.Add(_PH1_Emulator.logs.getstring_UC); });
            }

        }

        //ManualResetEvent serve para colocar estados de sinalização nos Threads, ou seja 
        ManualResetEvent mrse = new ManualResetEvent(false);

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
                System.Threading.Thread.Sleep(500);

                //Subtrai o tempo atual do tempo armazenado no inicio do laço, assim sabemos o tempo que levou para percorrer um ciclo do loop.
                LB_CyclicTimeThreadPH1.Dispatcher.Invoke(delegate { LB_CyclicTimeThreadPH1.Content = (DateTime.Now - DT_ThreadPH1).ToString(); });


                CB_DebugClock.Dispatcher.Invoke(delegate { HabilitaDebugClock = (bool)CB_DebugClock.IsChecked; });
                if (HabilitaDebugClock)
                {
                    mrse.Reset();
                }
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

        private void BT_ComponenteMEM_Click(object sender, RoutedEventArgs e)
        {
            WindowTabelaMemoria = new src.TabelaMemoria(_PH1_Emulator._MEM);
            WindowTabelaMemoria.Show();
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadPH1.Abort();//Encerra o ThreadPH1, só por precaução, mas ele deve fechar, pois roda em background ou seja, quando o Thread principal fechar ele encerra sozinho.          

            WindowTabelaMemoria.Close(); //Fecha a Window que mostra a memória, pois ele esta sempre aberta, somente é escondida enquanto executa o programa.
        }

        private void CB_AtivaDesativaLogUnidadeControle_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((bool)CB_AtivaDesativaLogUnidadeControle.IsChecked)
            {
                MessageBoxResult result = MessageBox.Show("Desativando o Log da UC, o sistema não irá mais gravar os logs!", "Desativar Logs da Unidade de Controle", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    CB_AtivaDesativaLogUnidadeControle.IsChecked = false;
                }

            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Ativando o Log da UC, o sistema irá consumir mais memória RAM!", "Ativar Logs da Unidade de Controle", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    CB_AtivaDesativaLogUnidadeControle.IsChecked = true;
                }
            }
        }

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
            //
            mrse.Reset();
            byte[] MEM = new byte[256];
            _PH1_Emulator._MEM = MEM;
        }

        private void LB_logComponentes_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }

        private void BT_LoadArchive_Click(object sender, RoutedEventArgs e)
        {
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
                    }

                }
            }
        }

        private void TB_Clock_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key) &&
             ((char)e.Key != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (((char)e.Key == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }




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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in textEditor.Text)
            {
                Console.WriteLine(item);
            }
        }
    }
}
