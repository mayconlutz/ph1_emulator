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
using System.Windows.Shapes;
using System.Data;

namespace PH1_Emulator.PH1
{
    /// <summary>
    /// Interaction logic for TabelaMemoria.xaml
    /// </summary>
    public partial class TabelaMemoria : Window
    {
        public TabelaMemoria(byte[] MEM)
        {
            InitializeComponent();

            DataTable dt = new DataTable();

            for (int i = 1; i < 9; i++)
            {
                dt.Columns.Add("Addr "+ i, typeof(byte));
                dt.Columns.Add("Valor " + i, typeof(byte));
            }

            for (int i = 0; i < 31; i++)
            {
                DataRow row = dt.NewRow();
                row["Addr 1"] = i;
                row["Valor 1"] = MEM[i];
                dt.Rows.Add(row);
            }

            DT_Memoria.ItemsSource = dt.DefaultView;
        }




    }
}
