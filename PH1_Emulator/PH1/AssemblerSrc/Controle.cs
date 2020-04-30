using System;
using System.Collections.Generic;
using System.Text;

namespace PH1.AssemblerSrc
{
    class Controle
    {

        //Define memória do controle.
        static byte[] MEM;

        //Define Tabela de simbolos
        static string[] _TS;

        static int auxAdress = 0;         //auxiliar para definir qual o endereço a ser associado o valor na Memoria da linguagem de máquina.
        static bool auxAssembler = false; //auxiliar para definir qual parte do código esta sendo montada a TEXT ou DATA.
        static bool auxMnemonic = false;     //auxiliar para montagem da linguagem de maquina, definindo se a string é mnemonico ou não.
        /// <summary>
        /// Montador do programa, a partir dos mnemonicos e os rótulos monta a linguagem de máquina
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tamanho"></param>
        public static void Assembler(ref string text, int tamanho)
        {
            //Declara memória de controle e tabela de simbolos
            MEM = new byte[256];
            _TS = new string[256];


            getWordFromTextEditor(ref text, tamanho, false);
            getWordFromTextEditor(ref text, tamanho, true);


        }

        /// <summary>
        /// Monta somente a tabela de simbolos
        /// </summary>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        private static int AssemblerTS(string keyWord)
        {
            keyWord = keyWord.ToUpper();

            if (keyWord.Equals("TEXT"))
            {
                auxAdress = 0;
            }
            else if (keyWord.Equals("DATA"))
            {
                auxAdress = 128;
            }
            else if ((!keyWord[keyWord.Length - 1].ToString().Equals(":") && !keyWord.Equals("BYTE")))
            {
                auxAdress += 1;
            }
            else if (!keyWord.Equals("BYTE"))
            {
                keyWord = keyWord.Trim(':');
                _TS[auxAdress] = keyWord;
            }

            return 1;
        }

        /// <summary>
        /// Monta somente o código, ou seja a linguagem de máquina
        /// </summary>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        private static int AssemblerCode(string keyWord)
        {
            keyWord = keyWord.ToUpper();


            if (keyWord.Equals("TEXT"))
            {
                auxAdress = 0;
                auxAssembler = false; //aux para definir qual parte do código esta sendo montada
            }
            else if (keyWord.Equals("DATA"))
            {
                auxAdress = 128;
                auxAssembler = true;  //aux para definir qual parte do código esta sendo montada
            }
            else if(auxAssembler)
            {
                //DATA
                if (!keyWord[keyWord.Length - 1].ToString().Equals(":") && !keyWord.Equals("BYTE"))
                {
                    byte.TryParse(keyWord, out MEM[auxAdress]);
                    auxAdress += 1;
                }
            }
            else
            {
                //TEXT
                if (!keyWord[keyWord.Length - 1].ToString().Equals(":"))
                {
                    if (!auxMnemonic)
                    {
                        MEM[auxAdress] = (byte)Decoder(keyWord);
                    }
                    else
                    {
                        MEM[auxAdress] = (byte)getAdressTS(keyWord);                     
                    }
                    auxAdress += 1;
                    auxMnemonic = !auxMnemonic;
                }
            }

            return 1;
        }

        /// <summary>
        /// Decodifica os mnemonicos
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static int Decoder(string s)
        {
            s = s.ToUpper();
            switch (s)
            {
                case "NOP":
                    return 0x00;
                case "LDR":
                    return 0x10;
                case "STR":
                    return 0x20;
                case "ADD":
                    return 0x30;
                case "SUB":
                    return 0x40;
                case "MUL":
                    return 0x50;
                case "DIV":
                    return 0x60;
                case "NOT":
                    return 0x70;
                case "AND":
                    return 0x80;
                case "OR":
                    return 0x90;
                case "XOR":
                    return 0xA0;
                case "JMP":
                    return 0xB0;
                case "JEQ":
                    return 0xC0;
                case "JG":
                    return 0xD0;
                case "JL":
                    return 0xE0;
                case "HLT":
                    return 0xF0;
                default:
                    return-1;
            }

        }

        /// <summary>
        /// Retorna o endereço do rotulo da tabela de simbolos
        /// </summary>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        private static int getAdressTS(string keyWord)
        {
            keyWord = keyWord.ToUpper();

            for (int i = 0; i < _TS.Length; i++)
            {
                if (_TS[i] != null && _TS[i].ToUpper() == keyWord)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Verifica a sintaxe do programa para evitar uma linguagem de máquina montada errada
        /// </summary>
        private static void verifySyntax()
        {
            // 1 - Verifica se a primeira palavra é text
            // 2 - Verifica se cada linha contem o necessário, uma instrução seguido de um rótulo que esta na tabela de simbolos
            // 3 - Verifica se existe a palabra data
            // 4 - Verifica se cada rotulo contem o tipo de dado e o valor



        }

        /// <summary>
        /// Encontra cada palavra recebida do editor de texto, concatena char para encontrar palavra, desconsidera " ", "\n\r" e "\t"
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tamanho"></param>            
        /// <param name="typeAssembler">Se verdadeiro executa o Montador de código, se não executa o montador da tabela de simbolos</param>               
        public static void getWordFromTextEditor(ref string text, int size, bool typeAssembler)
        {

            string s = "";
            string lastWord = "";
            int linhas = 1;
            int tabs = 0;
            for (int i = 0; i < size; i++)
            {
                
                //Se não tiver espaço " " AND
                //Se não tiver quebra linha "\r\n" 
                //Se não tiver tab "\t" 
                //concatena na string até encontrar um " " ou "\r\n" ou "\t"
                if (!text[i].ToString().Equals(" ") && !text[i].ToString().Equals("\t") && !text[i].ToString().Equals("\r") && !text[i].ToString().Equals("\n"))
                {
                    s += text[i];
                }

                if (i + 1 < size && s != "")
                {
                    if (text[i + 1].ToString().Equals(" "))
                    {
                        lastWord = s;  
                        s = "";
                        i += 1;
                        _ = typeAssembler == true ? AssemblerCode(lastWord) : AssemblerTS(lastWord);
                    }

                    if (i + 2 < size)
                    {
                        if (text[i + 1].ToString().Equals("\r"))
                        {
                            if (text[i + 2].ToString().Equals("\n"))
                            {
                                linhas += 1;
                                lastWord = s;
                                s = "";
                                i += 2;
                                _ = typeAssembler == true ? AssemblerCode(lastWord) : AssemblerTS(lastWord);
                            }
                        }
                    }

                    if (i + 1 < size)
                    {

                        if (text[i + 1].ToString().Equals("\t"))
                        {
                            tabs += 1;
                            lastWord = s;
                            s = "";
                            i += 1;
                            _ = typeAssembler == true ? AssemblerCode(lastWord) : AssemblerTS(lastWord);
                        }
                    }

                }
                else if(s != "")
                {
                    lastWord = s; //vai passar a ultima palavra e sair do laço
                    _ = typeAssembler == true ? AssemblerCode(lastWord) : AssemblerTS(lastWord);
                }

            }

        }

    }
}
