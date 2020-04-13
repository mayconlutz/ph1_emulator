using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PH1_Emulator.PH1
{
    /// <summary>
    /// Esssa classe tem como função criar todos os componentes que a UC controla, como também todos os seus meios de interligação.
    /// </summary>
    class Componentes
    {
        /// Barramentos de interligação
        #region Barramentos

        //Definindo os barramentos do circuito PH1
        //Os dados são trafegados através dos barramentos conforme os comandos que UC envia a cada ciclo de clock.
        //Os barramentos são os meios de interligação entre os componentes que tem funções específicas.
        byte BarramentoA = 0;   //Barramento que está "conectado" diretamente no RI, PC, AC, RDM, REM, ULA. Barramento interno de dados/endereços
        byte BarramentoB = 0;   //Barramento que "conecta" AC e ULA. Barramento interno de dados/endereços
        byte BarramentoC = 0;   //Barramento que "conecta" AC e ULA. Barramento interno de dados/endereços
        byte BarramentoREM = 0; //Barramente que "conecta" REM com MEM. Barramento de DADOS
        byte BarramentoRDM = 0; //Barramente que "conecta" RDM com MEM. Barramento de DADOS
        byte BarramentoRI = 0;  //Barramente que "conecta" REM com MEM. Barramento interno de dados/endereços

        #endregion

        //Declarando memória do PH1.
        byte[] MEM = new byte[256];

        ///Guarda o código binário (opcode) dainstrução sendo executada
        #region RI - Registrador de Instrução)

        //Passa valor do Barramento A para o Barramento RI
        public bool RIw
        {
            set
            {
                BarramentoRI = BarramentoA;
            }
        }


        #endregion

        ///Armazena o endereço da próxima instruçãoa ser executada pela CPU
        #region PC - Program Counter

        //Declarando PC
        byte _Valor_PC;

        //Passa valor do PC para o Barramento A
        public bool PCr
        {
            set
            {
                BarramentoA = _Valor_PC;

            }
        }
        //Passa valor do Barramento A para o PC
        public bool PCw
        {
            set
            {
                _Valor_PC = BarramentoA;
            }
        }
        //Conta mais uma operação no PC
        public bool PCmais
        {
            set
            {
                _Valor_PC += 1;
            }
        }
        //Disponibiliza o Valor de PC
        public byte Valor_PC { get => _Valor_PC; }

        #endregion

        ///Um único registrador disponível para o usuário
        #region AC - Registrador
        //Valor do Barramento B recebe valor do acumulador sempre que o valor é atualizado nos dois encapsulamentos ACw e ACc

        //Declarando AC
        byte _Valor_AC;

        //Passa valor do acumulador para o Barramento A
        public bool ACr
        {
            set
            {
                BarramentoA = _Valor_AC;

            }
        }

        //Passa valor do Barramento A para o acumulador
        public bool ACw
        {
            set
            {
                _Valor_AC = BarramentoA;
                BarramentoB = _Valor_AC;
            }
        }

        //Passa valor do Barramento C para o acumulador
        public bool ACc
        {
            set
            {
                _Valor_AC = BarramentoC;
                BarramentoB = _Valor_AC;
            }
        }

        //Disponibiliza o Valor de AC
        public byte Valor_AC { get => _Valor_AC;}

        #endregion

        ///Armazena os dados que estão sendo transferidos de/para a memória
        #region RDM - Registrador de Dados da Memória

        //Declarando RDM
        byte _Valor_RDM;

        //Passa valor do RDM para o Barramento A
        public bool RDMr
        {
            set
            {
                BarramentoA = _Valor_RDM;
            }
        }

        //Passa valor do Barramento A para o RDM
        public bool RDMw
        {
            set
            {
                _Valor_RDM = BarramentoA;
            }
        }

        #endregion

        ///Armazena o endereço onde deve ser feita apróxima operação de leitura ou escrita na memória
        #region REM - Registrador de Endereços da Memória
        //Declarando REM
        byte _Valor_REM;

        //Passa valor do REM para o Barramento A - Não utilizado por nenhum sinal de controle.
        public bool REMr
        {
            set
            {
                BarramentoA = _Valor_REM;
            }
        }

        //Passa valor do Barramento A para o REM
        public bool REMw
        {
            set
            {
                _Valor_REM = BarramentoA;
            }
        }

        #endregion

        ///Operações que envolvem controle de 3 componentes.
        #region MEMr e MEMw - Atua no RDM, REM e MEM

        //Passa valor da MEM[REM] para o RDM
        public bool MEMr
        {
            set
            {
                //O controle da MEM recebe o endereço de memória através do barramento de dados da memória REM.
                BarramentoREM = _Valor_REM;

                //O controle da MEM escreve o valor que esta na memória de acordo com o endereço do barramento de dados da memória REM.
                BarramentoRDM = MEM[BarramentoREM];

                //RDM recebe o valor do barramento de dados da memória RDM.
                _Valor_RDM = BarramentoRDM;
            }
        }

        //Passa valor do Barramento A para o RDM
        public bool MEMw
        {
            set
            {
                //O controle da MEM recebe o endereço de memória através do barramento de dados da memória REM.
                BarramentoREM = _Valor_REM;

                //barramento de dados da memória RDM recebe o valor do RDM.
                BarramentoRDM = _Valor_RDM;

                //O controle da MEM escreve o valor que esta no barramento de memória RDM na memória de acordo com o endereço do barramento de dados da memória REM.
                MEM[BarramentoREM] = BarramentoRDM;
            }
        }


        #endregion

        #region ULA

        public void ULA(typeULAop ULAop)
        {
            switch (ULAop)
            {
                case typeULAop.add:

                    BarramentoC = (byte)(BarramentoB + BarramentoA);

                    break;
                case typeULAop.sub:

                    BarramentoC = (byte)(BarramentoB - BarramentoA);

                    break;
                case typeULAop.mul:

                    BarramentoC = (byte)(BarramentoB * BarramentoA);

                    break;
                case typeULAop.div:

                    BarramentoC = (byte)(BarramentoB / BarramentoA);

                    break;
                case typeULAop.not:

                    //BarramentoC = not BarramentoB;

                    break;
                case typeULAop.and:

                    break;
                case typeULAop.or:

                    break;
                case typeULAop.xor:

                    break;
                default:

                    break;
            }

        }

        #endregion
        public enum typeULAop { add, sub, mul, div, not, and, or, xor};
    }
}
