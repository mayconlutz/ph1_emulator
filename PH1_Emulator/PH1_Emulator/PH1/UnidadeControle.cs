using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PH1_Emulator.PH1
{
    class UnidadeControle : PH1.Componentes
    {
        int CicloClock = 0;

        public UnidadeControle()
        {

        }

        private void Operation()
        {
            switch (CicloClock)
            {
                case 0:
                    PCr = true;
                    REMw = true;
                    break;
                case 1:
                    MEMr = true;
                    break;
                case 2:
                    PCmais = true;
                    break;
                case 3:
                    RDMr = true;
                    RIw = true;
                    break;
                case 4:
                    CicloClock = 0;
                    break;
                default:
                    break;
            }

            CicloClock += 1;
        }

        public bool Clock
        {
            set
            {             
                Operation();
            }
        }

    }
}
