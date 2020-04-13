using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*========================================================================================|
|  Conjunto de Instruções                                                                 |
|=========================================================================================|
| Opcode2 | Opcode16 | Instrução | Comentário                   | Operação(RTL)           |
|=========================================================================================|
|  0000   |     0    |   NOP     | Nenhuma operação             |                         |
|  0001   |     1    |   LDR end | Carrega valor no AC          | AC<- MEM[end]           |
|  0010   |     2    |   STR end | Armazena AC na memória       | MEM[end] <- AC          |
|  0011   |     3    |   ADD end | Soma                         | AC<- AC + MEM[end]      |
|  0100   |     4    |   SUB end | Subtração                    | AC<- AC - MEM[end]      |
|  0101   |     5    |   MUL end | Multiplicação                | AC<- AC* MEM[end]       |
|  0110   |     6    |   DIV end | Divisão                      | AC<- AC / MEM[end]      |
|  0111   |     7    |   NOT     | Negação lógica bit-a-bit     | AC<- !AC                |
|  1000   |     8    |   AND end | E lógico bit-a-bit           | AC<- AC & MEM[end]      |
|  1001   |     9    |   OR  end | OU lógico bit-a-bit          | AC<- AC | MEM[end]      |
|  1010   |     A    |   XOR end | OU exclusivo bit-a-bit       | AC<- AC ^ MEM[end]      |
|  1011   |     B    |   JMP end | Desvio incondicional         | PC<- end                |
|  1100   |     C    |   JEQ end | Desvio se AC igual a zero    | Se AC==0 então PC<- end |
|  1101   |     D    |   JG  end | Desvio se AC maior que zero  | Se AC>0 então PC<- end  |
|  1110   |     E    |   JL  end | Desvio se AC menor que zero  | Se AC<0 então PC<- end  |
|  1111   |     F    |   HLT     | Término da execução          |                         |
|========================================================================================*/

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

        //Falta testar as operações!
        /// <summary>
        /// ULA com oito operações 
        /// </summary>
        /// <param name="ULAop">add, sub, mul, div, not, and, or, xor</param>
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
                    
                    BarramentoC = (byte)~BarramentoB;

                    break;
                case typeULAop.and:

                    BarramentoC = (byte)(BarramentoB & BarramentoA);

                    break;
                case typeULAop.or:

                    BarramentoC = (byte)(BarramentoB | BarramentoA);

                    break;
                case typeULAop.xor:

                    BarramentoC = (byte)(BarramentoB ^ BarramentoA);

                    break;
                default:

                    break;
            }

        }

        #endregion

        ///Decodifica o OPCODE que são os 4 bits mais significantes do byte no barramento RI - (0000 0000)
        ///----------------------------------------------------------------------------------- (OPCODE 0000)
        #region Decodificador

        public typeInstrutions PH1_Decoder(byte InByte)
        {
            switch (InByte)
            {
                case 0x00:
                    return typeInstrutions.NOP;
                case 0x10:
                    return typeInstrutions.LDR_end;
                case 0x20:
                    return typeInstrutions.STR_end;
                case 0x30:
                    return typeInstrutions.ADD_end;
                case 0x40:
                    return typeInstrutions.SUB_end;
                case 0x50:
                    return typeInstrutions.MUL_end;
                case 0x60:
                    return typeInstrutions.DIV_end;
                case 0x70:
                    return typeInstrutions.NOT;
                case 0x80:
                    return typeInstrutions.AND_end;
                case 0x90:
                    return typeInstrutions.OR_end;
                case 0xA0:
                    return typeInstrutions.XOR_end;
                case 0xB0:
                    return typeInstrutions.JMP_end;
                case 0xC0:
                    return typeInstrutions.JEQ_end;
                case 0xD0:
                    return typeInstrutions.JG_end;
                case 0xE0:
                    return typeInstrutions.JL_end;
                case 0xF0:
                    return typeInstrutions.HLT;
                default:
                    return typeInstrutions.NOT;
            }

        }


        #endregion

        public enum typeULAop { add, sub, mul, div, not, and, or, xor};
        public enum typeInstrutions { NOP, LDR_end, STR_end, ADD_end, SUB_end, MUL_end, DIV_end, NOT, AND_end, OR_end, XOR_end, JMP_end, JEQ_end, JG_end, JL_end, HLT };
    }
}
