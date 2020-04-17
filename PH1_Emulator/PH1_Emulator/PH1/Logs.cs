using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PH1_Emulator.PH1
{
    class Logs : INotifyPropertyChanged
    {
        string string_Componentes = "";
        string string_UC = "";
        int countItem = 0;

        /// <summary>
        /// Adiciona item de controle dos componentes
        /// </summary>
        public string AddComponentes
        {
            set
            {
                string_Componentes = countItem.ToString() + " - " + value;
                countItem += 1;

                RaisePropertyChanged("Modificou Log Componentes");
            }
        }

        /// <summary>
        /// Adiciona item da unidade de controle
        /// </summary>
        public string AddUC
        {
            set
            {
                string_UC = value;


                RaisePropertyChanged("Modificou Log UC");
            }
        }

        public string getComponentes { get => string_Componentes;}
        public string getstring_UC { get => string_UC;}

        // Implementação da interface INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
