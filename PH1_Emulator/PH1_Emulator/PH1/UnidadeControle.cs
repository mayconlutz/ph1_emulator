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

namespace PH1_Emulator.PH1
{
    class UnidadeControle : PH1.Componentes
    {
        int CicloClock = 0;
        typeInstrutions Opcode;

        public UnidadeControle()
        {

        }

        private void Operation()
        {
            switch (CicloClock)
            {
                //Ciclo 0
                //Ação = REM <- PC
                case 0:           
                    PCr = true;
                    REMw = true;
                    CicloClock += 1;
                    logs.AddUC = "0 - REM <- PC";
                    break;
                //Ciclo 1
                //Ação = RDM <- MEM[REM]
                case 1:
                    MEMr = true;
                    CicloClock += 1;
                    logs.AddUC = "1 - RDM <- MEM[" + Valor_REM.ToString("X2")+ "]";
                    break;
                //Ciclo 2
                //Ação = PC <- PC + 1
                case 2:
                    PCmais = true;
                    CicloClock += 1;
                    logs.AddUC = "2 - PC <- PC + 1";
                    break;
                //Ciclo 3
                //Ação = RI <- RDM7..4
                case 3:
                    RDMr = true;
                    RIw = true;
                    Opcode = PH1_Decoder(_BarramentoRI); //RI é atualizado, descodificamos o Opcode.
                    CicloClock += 1;
                    logs.AddUC = "3 - RI <- RDM7..4";
                    break;
                //Ciclo 4 
                //Se RI!='NOP' && RI!='NOT' && RI!='HLT'      Ação = REM <- PC
                //Se RI=='NOP'                                  Ação = Ir p/ 0
                //Se RI=='NOT'                                  Ação = Ir p/ 0
                //Se RI=='HLT'                                  Ação = HALTED <- 1
                case 4:

                    //Se RI!='NOP' && RI!='NOT' && RI!='HLT'              
                    if (Opcode != typeInstrutions.NOP && Opcode != typeInstrutions.NOT && Opcode != typeInstrutions.HLT)
                    {
                        PCr = true;
                        REMw = true;
                        CicloClock += 1;
                        logs.AddUC = "4 - REM <- PC";
                    }
                    else if (Opcode == typeInstrutions.NOP) //Se RI=='NOP' (NOP)
                    {
                        CicloClock = 0;
                        logs.AddUC = "4 - Ir p/ 0";
                    }
                    else if (Opcode == typeInstrutions.NOT) //Se RI=='NOT' (NOT)
                    {
                        ULA(typeULAop.not);
                        ACc = true;
                        CicloClock+=1;
                        logs.AddUC = "4 - AC <- !AC";
                    }
                    else if (Opcode == typeInstrutions.HLT) //Se RI== 'HLT'(HLT)
                    {
                        throw new NotImplementedException();
                    }
                    break;
                //Ciclo 5
                //Se RI!='NOP' && RI!='NOT' && RI!='HLT'   Ação = RDM <- MEM[REM]
                //Se RI=='NOT'                               Ação = Ir p/ 0
                case 5:

                    //Opção 1
                    //Se RI=='NOT' (NOT)
                    if (Opcode == typeInstrutions.NOT)
                    {
                        CicloClock = 0;
                        logs.AddUC = "5 - REM <- PC";
                    }
                    //Opção 2
                    //Se for RI!= 'NOT'(NOT)
                    else
                    {
                        MEMr = true;
                        CicloClock += 1;
                        logs.AddUC = "5 - RDM <- MEM[" + Valor_REM.ToString("X2") + "]";
                    }
                    break;
                //Ciclo 6
                //Ação = PC <- PC + 1
                case 6:

                    PCmais = true;
                    CicloClock += 1;
                    logs.AddUC = "6 - PC <- PC + 1";
                    break;
                //Ciclo 7
                //Se RI=='LDR' || RI=='STR' || RI=='ADD' || RI=='SUB' || RI=='MUL' || RI=='DIV' || RI=='AND' || RI=='OR' || RI=='XOR' Ação = REM <- RDM
                //Se RI=='JEQ' && (AC==0) Ação = PC <- RDM
                //Se RI=='JG' && (AC>0)  Ação = PC <- RDM
                //Se RI=='JL' && (AC<0)  Ação = PC <- RDM
                case 7:

                    //Verifica as condições de JEQ, JG, JL, se não for nenhum dos 3 opcodes, segue a operação REM <- RDM
                    if (Opcode == typeInstrutions.JEQ_end)
                    {
                        if (Valor_AC == 0)
                        {
                            RDMr = true;
                            PCw = true;
                            logs.AddUC = "7 - Se (AC==0) PC <- RDM";
                        }
                    }
                    else if (Opcode == typeInstrutions.JG_end)
                    {
                        if (Valor_AC > 0)
                        {
                            RDMr = true;
                            PCw = true;
                            logs.AddUC = "7 - Se (AC>0) PC <- RDM";
                        }
                    }
                    else if(Opcode == typeInstrutions.JL_end)
                    {
                        if (Valor_AC < 0)
                        {
                            RDMr = true;
                            PCw = true;
                            logs.AddUC = "7 - Se (AC<0) PC <- RDM";
                        }
                    }
                    else
                    {
                        RDMr = true;
                        REMw = true;
                        logs.AddUC = "7 - REM <- RDM";
                    }
                    CicloClock += 1;
                    break;
                //Ciclo 8
                //Se RI=='LDR' || RI=='ADD' || RI=='SUB' || RI=='MUL' || RI=='DIV' || RI=='AND' || RI=='OR' || RI=='XOR' Ação = RDM <- MEM[REM]
                //Se RI=='STR' Ação = RDM <- AC
                //Se RI=='JMP' Ação = PC <- RDM
                //Se RI=='JEQ' || RI=='JG' || RI=='JL' Ação = Ir p/ 0
                case 8:

                    //Verifica as condições de JEQ, JG, JL
                    if (Opcode == typeInstrutions.JEQ_end || Opcode == typeInstrutions.JG_end || Opcode == typeInstrutions.JL_end)
                    {
                        CicloClock = 0;
                        logs.AddUC = "8 - Ir p/ 0";
                        break;
                    }
                    else if(Opcode == typeInstrutions.JMP_end)
                    {
                        RDMr = true;
                        PCw = true;
                        logs.AddUC = "8 - PC <- RDM";
                    }
                    else if (Opcode == typeInstrutions.STR_end)
                    {
                        ACr = true;
                        RDMw = true;
                        logs.AddUC = "8 - RDM <- AC";
                    }
                    else
                    {
                        MEMr = true;
                        logs.AddUC = "8 - RDM <- MEM["+ Valor_REM.ToString("X2") + "]";
                    }

                    CicloClock += 1;
                    break;
                //Ciclo 9
                //RI=='LDR' Ação = AC <- AC <- RDM
                //RI=='STR' Ação = AC <- MEM[REM] <- RDM
                //RI=='ADD' Ação = AC <- AC <- AC + RDM
                //RI=='SUB' Ação = AC <- AC <- AC - RDM
                //RI=='MUL' Ação = AC <- AC <- AC * RDM
                //RI=='DIV' Ação = AC <- AC / RDM
                //RI=='AND' Ação = AC <- AC & RDM
                //RI=='OR'  Ação = AC <- AC | RDM
                //RI=='XOR' Ação = AC <- AC ^ RDM
                //RI=='JMP' Ação = Ir p/ 0
                case 9:
                    if (Opcode == typeInstrutions.LDR_end)
                    {
                        RDMr = true;
                        ACw = true;
                        logs.AddUC = "9 - AC <- RDM";
                        //logs.AddUC = "9 - LDR " + Valor_REM.ToString("X2") + " ; AC <- MEM["+ Valor_REM.ToString("X2") + "]";
                    }
                    else if (Opcode == typeInstrutions.STR_end)
                    {
                        MEMw = true;
                        logs.AddUC = "9 - MEM["+ Valor_REM.ToString("X2") + "] <- RDM";
                    }
                    else if (Opcode == typeInstrutions.ADD_end)
                    {
                        RDMr = true;
                        ULA(typeULAop.add);
                        ACc = true;
                        logs.AddUC = "9 - AC <- AC + RDM";
                    }
                    else if (Opcode == typeInstrutions.SUB_end)
                    {
                        RDMr = true;
                        ULA(typeULAop.sub);
                        ACc = true;
                        logs.AddUC = "9 - AC <- AC - RDM";
                    }
                    else if (Opcode == typeInstrutions.MUL_end)
                    {
                        RDMr = true;
                        ULA(typeULAop.mul);
                        ACc = true;
                        logs.AddUC = "9 - AC <- AC * RDM";
                    }
                    else if (Opcode == typeInstrutions.DIV_end)
                    {
                        RDMr = true;
                        ULA(typeULAop.div);
                        ACc = true;
                        logs.AddUC = "9 - AC <- AC / RDM";
                    }
                    else if (Opcode == typeInstrutions.AND_end)
                    {
                        RDMr = true;
                        ULA(typeULAop.and);
                        ACc = true;
                        logs.AddUC = "9 - AC <- AC & RDM";
                    }
                    else if (Opcode == typeInstrutions.OR_end)
                    {
                        RDMr = true;
                        ULA(typeULAop.or);
                        ACc = true;
                        logs.AddUC = "9 - AC <- AC | RDM";
                    }
                    else if (Opcode == typeInstrutions.XOR_end)
                    {
                        RDMr = true;
                        ULA(typeULAop.xor);
                        ACc = true;
                        logs.AddUC = "9 - AC <- AC * RDM";
                    }
                    else
                    {
                        CicloClock = 0;
                        logs.AddUC = "9 - Ir p/ 0";
                        break;
                    }

                    CicloClock += 1;
                    break;
                //Ciclo 10
                //Ação = Ir p/ 0
                case 10:
                    CicloClock = 0;
                    logs.AddUC = "10 - Ir p/ 0";
                    break;
                default:
                    throw new NotImplementedException();
            }
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
