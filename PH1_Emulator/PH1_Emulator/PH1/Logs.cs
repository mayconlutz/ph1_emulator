using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PH1_Emulator.PH1
{
    class Logs
    {
        static List<string> ListUnidadeControle = new List<string>();

        public List<string> UnidadeControle
        {
            get
            {
                return ListUnidadeControle;
            }
            set
            {
                ListUnidadeControle.Add(ListUnidadeControle.Count.ToString() + value);
                //Implementar um tamanho máximo para a lista ou fazer um auto clean dos valores mais antigos.
            }
        }

    }
}
