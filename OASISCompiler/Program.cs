using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

using System.Runtime.InteropServices;

namespace OASISCompiler
{
    
    class Program
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int system(string command);

        static int Main(string[] args)
        {
            int UndefinedSymbols = 0;
            StreamReader inputStream = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
            //string inputStream = "script 203{ bool t; byte a; byte b; byte c; a=3; t=true; t=false; t=a>1; if (t){ a=f1(a); b=f2();} else c=100; while (a!=7) { b=b+1; f3(a,b,c);} lab1: proc(); }";
            //string inputStream = "script 203{ byte a;if(true) a=2; if(true){a=1;} else {a=0;} }";

            //string inputStream = "script 203{ bool t; byte a; if(t) a=0; else a=1; while(t){a=a+1;} t=false; }";

            //StreamReader inputStream = new StreamReader("myscript.txt");

            ErrorListener err = new ErrorListener();
            AntlrInputStream input = new AntlrInputStream(inputStream);//.ReadToEnd());
            OASISGrammarLexer lexer = new OASISGrammarLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            OASISGrammarParser parser = new OASISGrammarParser(tokens);

            parser.AddErrorListener(err);
           // lexer.AddErrorListener(err);

            IParseTree tree = parser.program();

            //Console.Error.WriteLine(tree.ToStringTree(parser));
            MyVisitor visitor = new MyVisitor();
            visitor.Visit(tree);

            for (int i=0; i<visitor.generatedCode.Count(); i++)
            {
                Console.WriteLine(visitor.generatedCode[i]);
            }

            // Check for undefined global labels
            foreach (KeyValuePair<string, bool> entry in visitor.globalCodeLabels)
            {
                if (!entry.Value)
                {
                    Console.Error.WriteLine("Undefined gloabl label " + entry.Key);
                    UndefinedSymbols++;
                }
            }

            Console.Error.WriteLine("=============");
            Console.Error.WriteLine("Syntax Errors: " + err.nErrors);
            Console.Error.WriteLine("Semantic errors: " + visitor.nErrors);
            Console.Error.WriteLine("Undefined symbols:" + UndefinedSymbols);
            Console.Error.WriteLine("Script bytecode size is: " + (visitor.totalSize) + " bytes");

            //system("PAUSE");
            inputStream.Close();

            return (err.nErrors + visitor.nErrors + UndefinedSymbols);
        }
    }
}
