using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace PH1.PH1src
{
    /// <summary>
    /// Esssa classe tem como função criar todos os omponentes que a UC controla, como também todos os seus meios de interligação.
    /// </summary>
    class Componentes
    {

        PH1src.Logs _logs = new Logs();

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
                _logs.AddComponentes = "RIw Executado - Barramento RI <- Barramento A";
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
                _logs.AddComponentes = "PCr Executado - Barramento A <- Valor PC";
            }
        }
        //Passa valor do Barramento A para o PC
        public bool PCw
        {
            set
            {
                _Valor_PC = BarramentoA;
                _logs.AddComponentes = "PCw Executado - Valor PC <- Barramento A";
            }
        }
        //Conta mais uma operação no PC
        public bool PCmais
        {
            set
            {
                _Valor_PC += 1;
                _logs.AddComponentes = "PC+ Executado - Valor PC <- Valor PC + 1";
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
                _logs.AddComponentes = "ACr Executado - Barramento A <- Valor AC";
            }
        }

        //Passa valor do Barramento A para o acumulador
        public bool ACw
        {
            set
            {
                _Valor_AC = BarramentoA;
                _logs.AddComponentes = "ACw Executado - Valor AC <- Barramento A";
                BarramentoB = _Valor_AC;
                _logs.AddComponentes = "ACw Executado - Barramento B <- Valor AC";
            }
        }

        //Passa valor do Barramento C para o acumulador
        public bool ACc
        {
            set
            {
                _Valor_AC = BarramentoC;
                _logs.AddComponentes = "ACc Executado - Valor AC <- Barramento C";
                BarramentoB = _Valor_AC;
                _logs.AddComponentes = "ACc Executado - Barramento B <- Valor AC";
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
                _logs.AddComponentes = "RDMr Executado - Barramento A <- Valor RDM";
            }
        }

        //Passa valor do Barramento A para o RDM
        public bool RDMw
        {
            set
            {
                _Valor_RDM = BarramentoA;
                _logs.AddComponentes = "RDMw Executado - Valor RDM <- Barramento A";
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
                _logs.AddComponentes = "REMr Executado - Barramento A <- Valor REM";
            }
        }

        //Passa valor do Barramento A para o REM
        public bool REMw
        {
            set
            {
                _Valor_REM = BarramentoA;
                _logs.AddComponentes = "REMw Executado - Valor REM <- Barramento A";
            }
        }

        public byte Valor_REM { get => _Valor_REM; }

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
                _logs.AddComponentes = "MEMr Executado - Barramento REM <- Valor REM";
                //O controle da MEM escreve o valor que esta na memória de acordo com o endereço do barramento de dados da memória REM.
                BarramentoRDM = MEM[BarramentoREM];
                _logs.AddComponentes = "MEMr Executado - Barramento REM <- MEM[Barramento REM]";
                //RDM recebe o valor do barramento de dados da memória RDM.
                _Valor_RDM = BarramentoRDM;
                _logs.AddComponentes = "MEMr Executado - Valor RDM <- Barramento RDM";
            }
        }

        //Passa valor do Barramento A para o RDM
        public bool MEMw
        {
            set
            {
                //O controle da MEM recebe o endereço de memória através do barramento de dados da memória REM.
                BarramentoREM = _Valor_REM;
                _logs.AddComponentes = "MEMw Executado - Barramento REM <- Valor REM";
                //barramento de dados da memória RDM recebe o valor do RDM.
                BarramentoRDM = _Valor_RDM;
                _logs.AddComponentes = "MEMw Executado - Barramento RDM <- Valor RDM";
                //O controle da MEM escreve o valor que esta no barramento de memória RDM na memória de acordo com o endereço do barramento de dados da memória REM.
                MEM[BarramentoREM] = BarramentoRDM;
                _logs.AddComponentes = "MEMw Executado - MEM[BarramentoREM] <- Barramento RDM";
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
                    _logs.AddComponentes = "ULA Operação ADD Executado - Barramento C <- Barramento B + Barramento A";
                    break;
                case typeULAop.sub:

                    BarramentoC = (byte)(BarramentoB - BarramentoA);
                    _logs.AddComponentes = "ULA Operação SUB Executado - Barramento C <- Barramento B - Barramento A";
                    break;
                case typeULAop.mul:

                    BarramentoC = (byte)(BarramentoB * BarramentoA);
                    _logs.AddComponentes = "ULA Operação MUL Executado - Barramento C <- Barramento B * Barramento A";
                    break;
                case typeULAop.div:

                    BarramentoC = (byte)(BarramentoB / BarramentoA);
                    _logs.AddComponentes = "ULA Operação DIV Executado - Barramento C <- Barramento B / Barramento A";
                    break;
                case typeULAop.not:
                    
                    BarramentoC = (byte)~BarramentoB;
                    _logs.AddComponentes = "ULA Operação NOT Executado - Barramento C <- NOT Barramento B";
                    break;
                case typeULAop.and:

                    BarramentoC = (byte)(BarramentoB & BarramentoA);
                    _logs.AddComponentes = "ULA Operação AND Executado - Barramento C <- Barramento B AND Barramento A";
                    break;
                case typeULAop.or:

                    BarramentoC = (byte)(BarramentoB | BarramentoA);
                    _logs.AddComponentes = "ULA Operação AND Executado - Barramento C <- Barramento B OR Barramento A";
                    break;
                case typeULAop.xor:

                    BarramentoC = (byte)(BarramentoB ^ BarramentoA);
                    _logs.AddComponentes = "ULA Operação AND Executado - Barramento C <- Barramento B XOR Barramento A";
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
                    _logs.AddComponentes = "OPCODE NOP Decodificado";
                    return typeInstrutions.NOP;
                case 0x10:
                    _logs.AddComponentes = "OPCODE LDR end Decodificado";
                    return typeInstrutions.LDR_end;
                case 0x20:
                    _logs.AddComponentes = "OPCODE STR end Decodificado";
                    return typeInstrutions.STR_end;
                case 0x30:
                    _logs.AddComponentes = "OPCODE ADD end Decodificado";
                    return typeInstrutions.ADD_end;
                case 0x40:
                    _logs.AddComponentes = "OPCODE SUB end Decodificado";
                    return typeInstrutions.SUB_end;
                case 0x50:
                    _logs.AddComponentes = "OPCODE MUL end Decodificado";
                    return typeInstrutions.MUL_end;
                case 0x60:
                    _logs.AddComponentes = "OPCODE DIV end Decodificado";
                    return typeInstrutions.DIV_end;
                case 0x70:
                    _logs.AddComponentes = "OPCODE NOT Decodificado";
                    return typeInstrutions.NOT;
                case 0x80:
                    _logs.AddComponentes = "OPCODE AND end Decodificado";
                    return typeInstrutions.AND_end;
                case 0x90:
                    _logs.AddComponentes = "OPCODE OR end Decodificado";
                    return typeInstrutions.OR_end;
                case 0xA0:
                    _logs.AddComponentes = "OPCODE XOR end Decodificado";
                    return typeInstrutions.XOR_end;
                case 0xB0:
                    _logs.AddComponentes = "OPCODE JMP end Decodificado";
                    return typeInstrutions.JMP_end;
                case 0xC0:
                    _logs.AddComponentes = "OPCODE JEQ end Decodificado";
                    return typeInstrutions.JEQ_end;
                case 0xD0:
                    _logs.AddComponentes = "OPCODE JG end Decodificado";
                    return typeInstrutions.JG_end;
                case 0xE0:
                    _logs.AddComponentes = "OPCODE JL end Decodificado";
                    return typeInstrutions.JL_end;
                case 0xF0:
                    _logs.AddComponentes = "OPCODE HLT Decodificado";
                    return typeInstrutions.HLT;
                default:
                    return typeInstrutions.NOT;
            }

        }


        #endregion

        public enum typeULAop { add, sub, mul, div, not, and, or, xor};
        public enum typeInstrutions { NOP, LDR_end, STR_end, ADD_end, SUB_end, MUL_end, DIV_end, NOT, AND_end, OR_end, XOR_end, JMP_end, JEQ_end, JG_end, JL_end, HLT };

        internal Logs logs { get => _logs; set => _logs = value; }

        public byte _BarramentoRI { get => BarramentoRI;}
        public byte[] _MEM { get => MEM; set => MEM = value; }

    }
}
