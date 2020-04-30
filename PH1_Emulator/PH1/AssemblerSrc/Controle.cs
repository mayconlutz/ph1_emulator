using System;
using System.Collections.Generic;
using System.Text;

namespace PH1.AssemblerSrc
{
    class Controle
    {

        //Declarando memória do controle.
        static byte[] MEM = new byte[256];

        //Declarando Tabela de simbolos
        static string[] _TS = new string[256];

        static int adress = 0;

        private static void AssemblerCode(string keyWord)
        {

        }


        private static void AssemblerTS(string keyWord)
        {
            string s = keyWord.ToUpper();

            if (s.Equals("TEXT"))
            {
                adress = 0;
            }
            else if (s.Equals("DATA"))
            {
                adress = 128;
            }
            else if ((!s[s.Length -1].ToString().Equals(":") && !s.Equals("BYTE")))
            {
                adress += 1;
            }
            else if (!s.Equals("BYTE"))
            {
                _TS[adress] = s;       
            }
        }

        
        /// <summary>
        /// concatena char para encontrar palavra, desconsidera " ", "\n\r" e "\t"
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tamanho"></param>        
        public static void getWordFromTextEditor(string text, int tamanho)
        {

            string s = "";
            string lastWord = "";
            int linhas = 1;
            int tabs = 0;
            for (int i = 0; i < tamanho; i++)
            {
                
                //Se não tiver espaço " " AND
                //Se não tiver quebra linha "\r\n" 
                //Se não tiver tab "\t" 
                //concatena na string até encontrar um " " ou "\r\n" ou "\t"
                if (!text[i].ToString().Equals(" ") && !text[i].ToString().Equals("\t") && !text[i].ToString().Equals("\r") && !text[i].ToString().Equals("\n"))
                {
                    s += text[i];
                }

                if (i + 1 < tamanho && s != "")
                {
                    if (text[i + 1].ToString().Equals(" "))
                    {
                        lastWord = s;
                        Assembler(lastWord);
                        s = "";
                        i += 1;
                    }

                    if (i + 2 < tamanho)
                    {
                        if (text[i + 1].ToString().Equals("\r"))
                        {
                            if (text[i + 2].ToString().Equals("\n"))
                            {
                                linhas += 1;
                                lastWord = s;
                                Assembler(lastWord);
                                s = "";
                                i += 2;
                            }
                        }
                    }

                    if (i + 1 < tamanho)
                    {

                        if (text[i + 1].ToString().Equals("\t"))
                        {
                            tabs += 1;
                            lastWord = s;
                            Assembler(lastWord);
                            s = "";
                            i += 1;
                        }
                    }

                }
                else if(s != "")
                {
                    lastWord = s; //vai passar a ultima palavra e sair do laço
                    Assembler(lastWord);
                }

            }

        }

    }
}
