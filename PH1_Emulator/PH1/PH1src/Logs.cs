using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PH1.PH1src
{
    class Logs : INotifyPropertyChanged
    {
        string string_Componentes = "";
        string string_UC = "";
        string string_Instrucoes = "";
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

        /// <summary>
        /// Adiciona item de instruções executadas
        /// </summary>
        public string AddInstrucoes
        { 
            set
            {
                string_Instrucoes = value;

                RaisePropertyChanged("Modificou Instrucoes");
            }
        
        }


        public string getComponentes { get => string_Componentes;}
        public string getstring_UC { get => string_UC;}
        public string getstring_Instrucoes { get => string_Instrucoes; }

        // Implementação da interface INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
