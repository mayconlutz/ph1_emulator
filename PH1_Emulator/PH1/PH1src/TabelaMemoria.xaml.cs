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


namespace PH1.PH1src
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


            dt.Columns.Add("Endereço", typeof(string));
            dt.Columns.Add("Valor" , typeof(string));


            for (int i = 0; i <= 255; i++)
            {
                DataRow row = dt.NewRow();
                row["Endereço"] = i;
                row["Valor"] = MEM[i].ToString("X2");
                dt.Rows.Add(row);
            }

            DT_Memoria.ItemsSource = dt.DefaultView;
        }




    }
}
