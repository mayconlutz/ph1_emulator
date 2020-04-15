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
        int countItem = 0;
        /// <summary>
        /// Adiciona item 
        /// </summary>
        public string AddComponentes
        {
            set
            {
                string_Componentes = countItem.ToString() + " - " + value;
                countItem += 1;

                RaisePropertyChanged("Adicionou Lista Componentes");
            }
        }

        public string getComponentes { get => string_Componentes;}

        // Implementação da interface INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
