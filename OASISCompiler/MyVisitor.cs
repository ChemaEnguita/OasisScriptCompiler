﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;

namespace OASISCompiler
{

    class MyVisitor : OASISGrammarBaseVisitor<Symbol.Types>
    {
        /* Local and global symbols, no need for stacks
         * of sybmol tables because there are no scopes further
         * than those two (KISS rule) */
        public SymbolTable localSymbols;
        public SymbolTable globalSymbols;

        /* I'm not using getters and setters or access functions to keep
         * it simple. It is for my own use anyway */

        public int byteOffset;  // Current offset for the byte code
        public int nErrors;     // Number of semantic errors found

        /* Syntax and Lexer errors are kept track by the ErrorListener class
         per requirement of ANTLR*/

        /* The generated code is a list of strings to keep it also simple and being able to generate
         * comments, macros and so on */
        public List<string> generatedCode = new List<string>();

        public int totalSize; // Calculated code size

        /* We also need a dictionary with labels. Only one field is needed: a boolean
         * value indicating if the label has been declared or not. As we will produce asm
         * code we will use the supported label system, but we want to issue errors if a label
         * is not defined somewhere after the first pass */
        public Dictionary<string, bool> codeLabels = new Dictionary<string, bool>();

        /* And another one for exported symbols (used in dialog options inside scripts, for instance) */
        public Dictionary<string, bool> globalCodeLabels = new Dictionary<string, bool>();

        /* We keep lists of functions and commands with the types and number of commands, associated
         * macro label and return value type. I did not put that in the grammar as tokens to simplify
         * it and also being easily expandable/amendable */
        public OASISFunctions theFunctions;
        public OASISCommands theCommands;

        /* We need a data structure to hold all the information while processing dialog options.
         * a list of structures will do the job */
        public struct DlgEntry
        {
            public String sentence;
            public String labelOffset;
            public bool active;
        };

        public List<DlgEntry> theDlgOptions;

        // Constructor
        public MyVisitor() {
            /* Some basic initializaitons. Local symbols start at 200
             * as #defined in the OASIS sources */
            localSymbols = new SymbolTable();
            localSymbols.firstaddrB = 200;
            localSymbols.firstaddrL = 200;

            globalSymbols = new SymbolTable();
            byteOffset = 0;
            nErrors = 0;
            totalSize = 0;

            theFunctions = new OASISFunctions();
            theCommands = new OASISCommands();

            codeLabels = new Dictionary<string, bool>();
            codeLabels.Clear();

            globalCodeLabels = new Dictionary<string, bool>();
            globalCodeLabels.Clear();

            theDlgOptions = new List<DlgEntry>();
            theDlgOptions.Clear();

            // Output a header
            OutputCode(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;");
            OutputCode("; Generated by OASIS compiler");
            OutputCode("; (c) Chema 2016");
            OutputCode(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;");
            OutputCode("");
        }

        /* This method produces a string encoding a constant value
         * which must be preceded by 64 if it is greater than 63 (bit 5 is 1)
         * Also returns the number of bytes needed (either 1 or 2) */
        protected string EncodeNumber(int n, out int bytes)
        {
            if (n >= 64)
                bytes = 2;
            else bytes = 1;

            return ("" + (n < 64 ? "" : "64, ") + n);
        }

        /* Stores the string in the code list increasing the
         byte offset by the number specified as parameter */
        public void OutputCode(string s, int nbytes = 0)
        {
            // generatedCode.Add(byteOffset+": "+s);
            generatedCode.Add(s);
            byteOffset += nbytes;
        }

        /* Errors produced by the semantic checker */
        public void Error(string s, int line, string c = "")
        {
            if (line >= 0)
                Console.Error.WriteLine("Error " + s + " in line: " + line + ": " + c);
            else
                Console.Error.WriteLine("Error " + s + ": " + s);
            nErrors++;
        }

        /* Here come the visitors for the rules in the grammar */
        public override Symbol.Types VisitScriptMain([NotNull] OASISGrammarParser.ScriptMainContext context)
        {
            int id;

            // Try to parse the script number
            if ((!int.TryParse(context.NUMBER().GetText(), out id)) || (id > 255))
                Error("Invalid Script ID", context.Start.Line, context.GetText());

            byteOffset = 0;
            OutputCode("", 0);
            OutputCode("; Script " + id, 0);
            OutputCode(".(", 0);

            if (id >= 200)
                OutputCode(".byt RESOURCE_SCRIPT|$80", 0);
            else
                OutputCode(".byt RESOURCE_SCRIPT", 0);

            OutputCode(".word (res_end-res_start +4)", 0);

            OutputCode(".byt " + id);
            OutputCode("res_start", 0);

            // For exporting symbols
            OutputCode("+script_" + id + "_start", 0);

            // Parse the content of the script
            Visit(context.block());

            OutputCode(".byt SC_STOP_SCRIPT", 1);
            OutputCode("res_end", 0);
            OutputCode(".)", 0);
            totalSize += byteOffset;

            // Check for undefined labels
            foreach (KeyValuePair<string, bool> entry in codeLabels)
            {
                if (!entry.Value)
                {
                    Error("Undefined label " + entry.Key + " in script " + id, -1, context.GetText());
                }
            }

            // CLear things for the next script/objectcode/...
            codeLabels.Clear();
            localSymbols.Empty();
            localSymbols.firstaddrB = 200;
            localSymbols.firstaddrL = 200;
            return Symbol.Types.None;
        }

        public override Symbol.Types VisitGlobalDeclare([NotNull] OASISGrammarParser.GlobalDeclareContext context)
        {
            Symbol sym = new Symbol();
            Symbol sym2;
            int Addr;

            // Check if the symbol is already declared. Only global, as no other
            // kind can be found here
            sym.Name = context.GetChild(1).GetText();
            sym2 = globalSymbols.resolve(sym.Name);

            if (sym2 != null)
            {
                Error("Redefined variable", context.Start.Line, context.GetText());
                return base.VisitGlobalDeclare(context);
            }

            // Check if a fixed address is given (META: CURRENTLY NOT WORKING PROPERLY)
            if (context.NUMBER() != null)
            {
                // A fixed address has been given to this var...
                bool res = int.TryParse(context.NUMBER().GetText(), out Addr);

                // Check if it is valid
                if (res && (Addr < 256))
                {
                    // Check it is not in use
                    if (!globalSymbols.isAddressUsed(Addr))
                    {
                        // Create the symbol
                        switch (context.t.Type)
                        {
                            case OASISGrammarParser.TYPEBOOL:
                                sym.Type = Symbol.Types.Bool;
                                sym.Address = Addr;
                                break;
                            case OASISGrammarParser.TYPEBYTE:
                                sym.Type = Symbol.Types.Byte;
                                sym.Address = Addr;
                                break;
                        }
                    }
                    else
                        Error("Address is in use", context.Start.Line, context.GetText());
                }
                else
                {
                    Error("Invalid address", context.Start.Line, context.GetText());
                }
            }
            else
            {
                // Address was not specified, assign one
                switch (context.t.Type)
                {
                    case OASISGrammarParser.TYPEBOOL:
                        sym.Type = Symbol.Types.Bool;
                        sym.Address = globalSymbols.firstaddrL;
                        globalSymbols.firstaddrL++;
                        break;
                    case OASISGrammarParser.TYPEBYTE:
                        sym.Type = Symbol.Types.Byte;
                        sym.Address = globalSymbols.firstaddrB;
                        globalSymbols.firstaddrB++;
                        break;
                }
            }

            // Add the symbol
            globalSymbols.define(sym);
            return base.VisitGlobalDeclare(context);
        }

        /* Helper to check if a symbol already exists and generate an error if so
         * Checks both in local and global tables */
        protected bool symbolExists(string name, int line)
        {
            Symbol s;

            s = globalSymbols.resolve(name);
            if (s != null)
            {
                Error("A global symbol with the same name already exists", line, name);
                return true;
            }

            s = localSymbols.resolve(name);
            if (s != null)
            {
                Error("Redefined variable", line, name);
                return true;
            }

            return false;
        }

        public override Symbol.Types VisitDeclare([NotNull] OASISGrammarParser.DeclareContext context)
        {

            Symbol sym = new Symbol();

            sym.Name = context.IDENT().GetText();
            switch (context.t.Type)
            {
                case OASISGrammarParser.TYPEBOOL: sym.Type = Symbol.Types.Bool;
                    sym.Address = localSymbols.firstaddrL;
                    localSymbols.firstaddrL++;
                    break;
                case OASISGrammarParser.TYPEBYTE: sym.Type = Symbol.Types.Byte;
                    sym.Address = localSymbols.firstaddrB;
                    localSymbols.firstaddrB++;
                    break;
            }

            if (!symbolExists(sym.Name, context.Start.Line))
                localSymbols.define(sym);

            return base.VisitDeclare(context);
        }

        public override Symbol.Types VisitLabel([NotNull] OASISGrammarParser.LabelContext context)
        {
            bool t;
            string name = context.IDENT().GetText();

            // META: This code is not correct, as labels which are redefined will probably overlap, both if
            // local or one local another global or two globals....
            // Will issue an assembler error, though.
            if (context.e != null)
            {
                if (globalCodeLabels.TryGetValue(name, out t))
                    globalCodeLabels.Remove(name);
                globalCodeLabels.Add(name, true);

                OutputCode("+l_" + name, 0); // Labels will appear with a preceding +l_ to avoid collisions
            }
            else
            {
                if (codeLabels.TryGetValue(name, out t))
                    codeLabels.Remove(name);
                codeLabels.Add(name, true);

                OutputCode("l_" + name, 0); // Labels will appear with a preceding l_ to avoid collisions
            }
            return base.VisitLabel(context);
        }

        public override Symbol.Types VisitNumber([NotNull] OASISGrammarParser.NumberContext context)
        {
            int val; int b;

            if ((int.TryParse(context.GetText(), out val)) && (val <= 256))
                OutputCode(".byt " + EncodeNumber(val, out b), b);
            else
                Error("Invalid constant", context.Start.Line, context.GetText());


            base.VisitNumber(context);
            return Symbol.Types.Byte;
        }

        public override Symbol.Types VisitParens([NotNull] OASISGrammarParser.ParensContext context)
        {
            if (context.expression() != null)
                return (Visit(context.expression()));
            else
                return Symbol.Types.None; // Avoid exception if empty expresion ()
        }

        public override Symbol.Types VisitLParens([NotNull] OASISGrammarParser.LParensContext context)
        {
            if (context.logicalexpression() != null)
                return (Visit(context.logicalexpression()));
            else
                return Symbol.Types.None;
        }

        public override Symbol.Types VisitLConstant([NotNull] OASISGrammarParser.LConstantContext context)
        {
            if (context.tv.Type == OASISGrammarParser.TRUE)
                OutputCode(".byt 1", 1);
            else
                OutputCode(".byt 0", 1);
            base.VisitLConstant(context);
            return Symbol.Types.Bool;
        }

        public override Symbol.Types VisitIdentifier([NotNull] OASISGrammarParser.IdentifierContext context)
        {
            /* An identifier was found, check if local first (would obscure global) */
            Symbol sym = localSymbols.resolve(context.GetText());
            /* If a local one was not found, try to check for a global one */
            if (sym == null)
                sym = globalSymbols.resolve(context.GetText());

            int b;

            if (sym == null)
            {
                Error("Variable not found:" + context.GetText(), context.Start.Line, context.GetText());
                return base.VisitIdentifier(context);
            }
            OutputCode(".byt SF_GETVAL," + EncodeNumber(sym.Address, out b) + "\t; " + context.GetText(), b + 1);
            base.VisitIdentifier(context);
            return sym.Type;
        }

        public override Symbol.Types VisitMulDiv([NotNull] OASISGrammarParser.MulDivContext context)
        {
            if (context.op.Type == OASISGrammarParser.MUL)
                OutputCode(".byt SF_MUL", 1);
            else
                OutputCode(".byt SF_DIV", 1);

            if ((context.expression(0) == null) || (Visit(context.expression(0)) != Symbol.Types.Byte))
                Error("Left side of operation is not a number", context.start.Line, context.GetText());
            if ((context.expression(1) == null) || (Visit(context.expression(1)) != Symbol.Types.Byte))
                Error("Right side of operation is not a number", context.start.Line, context.GetText());
            return Symbol.Types.Byte;
        }

        public override Symbol.Types VisitAddSub([NotNull] OASISGrammarParser.AddSubContext context)
        {
            if (context.op.Type == OASISGrammarParser.ADD)
                OutputCode(".byt SF_ADD", 1);
            else
                OutputCode(".byt SF_SUB", 1);

            if ((context.expression(0) == null) || (Visit(context.expression(0)) != Symbol.Types.Byte))
                Error("Left side of operation is not a number", context.start.Line, context.GetText());
            if ((context.expression(1) == null) || (Visit(context.expression(1)) != Symbol.Types.Byte))
                Error("Right side of operation is not a number", context.start.Line, context.GetText());
            return Symbol.Types.Byte;
        }

        public override Symbol.Types VisitAssignmentStatement([NotNull] OASISGrammarParser.AssignmentStatementContext context)
        {
            int b;
            /* An identifier was found, check if local first (would obscure global) */
            Symbol sym = localSymbols.resolve(context.IDENT().GetText());
            /* If a local one was not found, try to check for a global one */
            if (sym == null)
                sym = globalSymbols.resolve(context.IDENT().GetText());


            if (sym == null)
            {
                Error("Variable not found: " + context.GetText(), context.Start.Line, context.GetText());
                return base.VisitAssignmentStatement(context);
            }

            if (sym.Type == Symbol.Types.Byte)
                OutputCode(".byt SC_ASSIGN, " + EncodeNumber(sym.Address, out b) + "\t; " + context.GetText(), b + 1);
            else
                OutputCode(".byt SC_SETFLAG, " + EncodeNumber(sym.Address, out b) + "\t; " + context.GetText(), b + 1);

            if ((context.expression() == null) && (context.logicalexpression() == null))
            {
                Error("Invalid expression", context.start.Line, context.GetText());
                return Symbol.Types.None;
            }

            Symbol.Types rt;

            if (context.expression() != null)
                rt = Visit(context.expression());
            else
                rt = Visit(context.logicalexpression());

            if (sym.Type != rt)
            {
                Error("Inconsistent types: " + context.GetText(), context.Start.Line, context.GetText());
                return base.VisitAssignmentStatement(context);
            }

            return Symbol.Types.None;
        }


        public override Symbol.Types VisitGotoStatement([NotNull] OASISGrammarParser.GotoStatementContext context)
        {

            bool t;
            string name = context.IDENT().GetText();
            if (!codeLabels.TryGetValue(name, out t))
                codeLabels.Add(name, false);

            OutputCode(".byt SC_JUMP", 1);
            OutputCode(".word (l_" + name + "-res_start)", 2);

            return Symbol.Types.None;
        }

        public override Symbol.Types VisitFunctionCall([NotNull] OASISGrammarParser.FunctionCallContext context)
        {
            OASISFunction f = theFunctions.resolve(context.IDENT().GetText());
            if (f == null)
            {
                Error("Unknown function", context.Start.Line, context.GetText());
                return Symbol.Types.None;
            }

            // A function with that name exists... check parameters...
            int n = 0; int ni = 0;
            if (context.argumentlist() != null)
            {
                while (context.argumentlist().GetChild(ni) != null)
                {
                    if (context.argumentlist().GetChild(ni).GetText() != ",")
                        n++;
                    ni++;
                }
            }
            if (n != f.nArguments)
            {
                Error("Wrong number of arguments", context.Start.Line, context.GetText());
                return f.returnType;
            }

            OutputCode(".byt " + f.opCode, 1);
            n = 0;
            for (int i = 0; i < ni; i++)
            {
                Symbol.Types t;
                if (context.argumentlist().GetChild(i).GetText() == ",") continue;

                t = Visit(context.argumentlist().GetChild(i));

                if (f.paramList.ElementAt(n) != t)
                    Error("Argument at position " + n + " has wrong type", context.Start.Line, context.GetText());
                n++;
            }

            return f.returnType;
        }

        public override Symbol.Types VisitLFunctionCall([NotNull] OASISGrammarParser.LFunctionCallContext context)
        {

            /* This is basically the same as above, but I did not find a way to cast a OASISGrammarParser.LFunctionCallContext 
             * to a OASISGrammarParser.FunctionCallContext, so had to repeat the code :( */

            OASISFunction f = theFunctions.resolve(context.IDENT().GetText());
            if (f == null)
            {
                Error("Unknown function", context.Start.Line, context.GetText());
                return Symbol.Types.None;
            }

            // A function with that name exists... check parameters...

            int n = 0; int ni = 0;
            if (context.argumentlist() != null)
            {
                while (context.argumentlist().GetChild(ni) != null)
                {
                    if (context.argumentlist().GetChild(ni).GetText() != ",")
                        n++;
                    ni++;
                }
            }

            if (n != f.nArguments)
            {
                Error("Wrong number of arguments", context.Start.Line, context.GetText());
                return f.returnType;
            }

            OutputCode(".byt " + f.opCode, 1);

            n = 0;
            for (int i = 0; i < ni; i++)
            {
                Symbol.Types t;
                if (context.argumentlist().GetChild(i).GetText() == ",") continue;

                t = Visit(context.argumentlist().GetChild(i));

                if (f.paramList.ElementAt(n) != t)
                    Error("Argument at position " + n + " has wrong type", context.Start.Line, context.GetText());
                n++;
            }

            return f.returnType;
        }

        public override Symbol.Types VisitCommandCall([NotNull] OASISGrammarParser.CommandCallContext context)
        {
            /* And again very similar code, but could not group it all! */

            OASISCommand c = theCommands.resolve(context.IDENT().GetText());
            if (c == null)
            {
                Error("Unknown command", context.Start.Line, context.GetText());
                return Symbol.Types.None;
            }


            int n = 0; int ni = 0;
            if (context.argumentlist() != null)
            {
                while (context.argumentlist().GetChild(ni) != null)
                {
                    if (context.argumentlist().GetChild(ni).GetText() != ",")
                        n++;
                    ni++;
                }
            }


            if (n != c.nArguments)
            {
                Error("Wrong number of arguments", context.Start.Line, context.GetText());
                return Symbol.Types.None;
            }

            OutputCode(".byt " + c.opCode, 1);
            n = 0;
            for (int i = 0; i < ni; i++)
            {
                Symbol.Types t;
                if (context.argumentlist().GetChild(i).GetText() == ",") continue;


                if (c.paramList.ElementAt(n) == Symbol.Types.Word)
                {
                    bool b;
                    // The command wants a label here
                    string name = context.argumentlist().GetChild(i).GetText();
                    if (!codeLabels.TryGetValue(name, out b))
                        codeLabels.Add(name, false);
                    OutputCode(".word (l_" + name + "-res_start)", 2);
                }
                else
                {
                    t = Visit(context.argumentlist().GetChild(i));

                    if (c.paramList.ElementAt(n) != t)
                        Error("Argument at position " + n + " has wrong type", context.Start.Line, context.GetText());
                }

                n++;
            }

            return Symbol.Types.None;
        }

        public override Symbol.Types VisitRelationalExpression([NotNull] OASISGrammarParser.RelationalExpressionContext context)
        {

            switch (context.op.Type)
            {
                case OASISGrammarParser.LT: OutputCode(".byt SF_LT", 1); break;
                case OASISGrammarParser.LE: OutputCode(".byt SF_LE", 1); break;
                case OASISGrammarParser.GT: OutputCode(".byt SF_GT", 1); break;
                case OASISGrammarParser.GE: OutputCode(".byt SF_GE", 1); break;
                case OASISGrammarParser.EQ: OutputCode(".byt SF_EQ", 1); break;
                case OASISGrammarParser.NE: OutputCode(".byt SF_NOT, SF_EQ", 2); break;
            }

            if ((context.expression(0) == null) || (Visit(context.expression(0)) != Symbol.Types.Byte))
                Error("Left side of operation is not a number", context.start.Line, context.GetText());
            if ((context.expression(1) == null) || (Visit(context.expression(1)) != Symbol.Types.Byte))
                Error("Right side of operation is not a number", context.start.Line, context.GetText());

            return Symbol.Types.Bool;
        }

        public override Symbol.Types VisitAndOrLExpression([NotNull] OASISGrammarParser.AndOrLExpressionContext context)
        {
            switch (context.op.Type)
            {
                case OASISGrammarParser.LAND: OutputCode(".byt SF_AND", 1); break;
                case OASISGrammarParser.LOR: OutputCode(".byt SF_OR", 1); break;
            }

            if ((context.logicalexpression(0) == null) || (Visit(context.logicalexpression(0)) != Symbol.Types.Bool))
                Error("Left side of operation is not a boolean", context.start.Line, context.GetText());
            if ((context.logicalexpression(1) == null) || (Visit(context.logicalexpression(1)) != Symbol.Types.Bool))
                Error("Right side of operation is not a boolean", context.start.Line, context.GetText());

            return Symbol.Types.Bool;
        }

        public override Symbol.Types VisitNotLExpression([NotNull] OASISGrammarParser.NotLExpressionContext context)
        {
            OutputCode(".byt SF_NOT", 1);
            if ((context.logicalexpression() == null) || (Visit(context.logicalexpression()) != Symbol.Types.Bool))
                Error("Operand is not a boolean", context.start.Line, context.GetText());
            return Symbol.Types.Bool;
        }

        public override Symbol.Types VisitLIdentifier([NotNull] OASISGrammarParser.LIdentifierContext context)
        {
            Symbol sym = localSymbols.resolve(context.GetText());

            if (sym == null)
            {
                Error("Variable not found: " + context.GetText(), context.Start.Line, context.GetText());
                return base.VisitLIdentifier(context);
            }

            if (sym.Type != Symbol.Types.Bool)
            {
                Error("Variable is not boolean: " + context.GetText(), context.Start.Line, context.GetText());
                return base.VisitLIdentifier(context);
            }

            int b;
            OutputCode(".byt SF_GETFLAG," + EncodeNumber(sym.Address, out b) + "\t; " + context.GetText(), b + 1);
            base.VisitLIdentifier(context);
            return Symbol.Types.Bool;
        }

        public override Symbol.Types VisitIfStatement([NotNull] OASISGrammarParser.IfStatementContext context)
        {

            int sizethenpart;
            int sizeelsepart;
            int sizeconditional;
            int tempOffset = byteOffset;

            OutputCode("; if", 0);
            OutputCode(".byt SC_JUMP_REL_IF, SF_NOT", 2);
            if (context.logicalexpression() == null)
                Error("No condition found", context.start.Line, context.GetText());
            else
                Visit(context.logicalexpression());

            int pos = generatedCode.Count();
            sizeconditional = byteOffset - tempOffset;

            tempOffset = byteOffset;
            if (context.statement(0) == null)
                Error("No statement found", context.start.Line, context.GetText());
            else
                Visit(context.statement(0));

            sizethenpart = byteOffset - tempOffset;
            generatedCode.RemoveRange(pos, generatedCode.Count() - pos);

            byteOffset = tempOffset;

            if (context.statement(1) != null)
            {
                tempOffset = byteOffset;
                Visit(context.statement(1));
                sizeelsepart = byteOffset - tempOffset;
                generatedCode.RemoveRange(pos, generatedCode.Count() - pos);
                byteOffset = tempOffset;
            }
            else
                sizeelsepart = 0;

            int tojump = 1 + sizethenpart + sizeconditional;
            if (tojump >= 64) tojump++;
            if (sizeelsepart != 0) tojump += 2;

            int tojump2 = 2 + sizeelsepart;
            if (tojump2 >= 64) tojump2++;

            if (tojump2 >= 64) tojump++;

            int b;
            OutputCode(".byt " + EncodeNumber(tojump, out b), b);
            OutputCode("; then part", 0);
            if (context.statement(0) == null)
                Error("No statement found", context.start.Line, context.GetText());
            else
                Visit(context.statement(0));


            if (context.statement(1) != null)
            {
                OutputCode(".byt SC_JUMP_REL, " + EncodeNumber(tojump2, out b), b + 1);
                OutputCode("; else part", 0);
                Visit(context.statement(1));
            }

            return Symbol.Types.None;
        }


        public override Symbol.Types VisitWhileStatement([NotNull] OASISGrammarParser.WhileStatementContext context)
        {

            int tempOffset = byteOffset;
            int hi, lo;

            OutputCode("; while", 0);
            OutputCode(".byt SC_JUMP_IF, SF_NOT", 2);
            Visit(context.logicalexpression());

            int pos = generatedCode.Count();
            OutputCode("00,00", 2);

            Visit(context.statement());

            hi = Math.DivRem(tempOffset, 256, out lo);
            OutputCode("; end while", 0);
            OutputCode(".byt SC_JUMP, " + lo + ", " + hi + "\t; jump to " + tempOffset, 3);


            generatedCode.RemoveAt(pos);
            generatedCode.Insert(pos, ".word " + byteOffset);

            return Symbol.Types.None;
        }

        public override Symbol.Types VisitDoWhileStatement([NotNull] OASISGrammarParser.DoWhileStatementContext context)
        {

            int tempOffset = byteOffset;

            OutputCode("; do", 0);
            Visit(context.statement());
            OutputCode("; while", 0);
            OutputCode(".byt SC_JUMP_IF", 1);
            Visit(context.logicalexpression());
            OutputCode(".word " + tempOffset, 2);
            return Symbol.Types.None;
        }

        public override Symbol.Types VisitForStatement([NotNull] OASISGrammarParser.ForStatementContext context)
        {
            int tempOffset = byteOffset;
            int hi, lo;

            OutputCode("; for", 0);
            OutputCode("; Init part", 0);
            Visit(context.assignment(0));
            OutputCode("; Condition", 0);
            tempOffset = byteOffset;
            OutputCode(".byt SC_JUMP_IF, SF_NOT", 2);
            Visit(context.logicalexpression());

            int pos = generatedCode.Count();
            OutputCode("00,00", 2);

            OutputCode("; for body", 0);
            Visit(context.statement());

            OutputCode("; Increment expression", 0);
            Visit(context.assignment(1));

            hi = Math.DivRem(tempOffset, 256, out lo);
            OutputCode("; end for", 0);
            OutputCode(".byt SC_JUMP, " + lo + ", " + hi + "\t; jump to " + tempOffset, 3);


            hi = Math.DivRem(byteOffset, 256, out lo);
            generatedCode.RemoveAt(pos);
            generatedCode.Insert(pos, ".word " + byteOffset);

            return Symbol.Types.None;
        }


        protected string getVerbCode(string s)
        {
            switch (s)
            {
                case "Give": return "VERB_GIVE";
                case "PickUp": return "VERB_PICKUP";
                case "Use": return "VERB_USE";
                case "Open": return "VERB_OPEN";
                case "LookAt": return "VERB_LOOKAT";
                case "Push": return "VERB_PUSH";
                case "Close": return "VERB_CLOSE";
                case "TalkTo": return "VERB_TALKTO";
                case "Pull": return "VERB_PULL";
                case "WalkTo": return "VERB_WALKTO";
            }
            return null;
        }

        public override Symbol.Types VisitObjectCodeMain([NotNull] OASISGrammarParser.ObjectCodeMainContext context)
        {
            int pos;
            int id;

            if ((!int.TryParse(context.NUMBER().GetText(), out id)) || (id > 255))
                Error("Invalid Object ID", context.Start.Line, context.GetText());

            byteOffset = 0;

            OutputCode("", 0);
            OutputCode("; Object Code " + id, 0);
            OutputCode(".(", 0);
            if (id >= 200)
                OutputCode(".byt RESOURCE_OBJECTCODE|$80", 0);
            else
                OutputCode(".byt RESOURCE_OBJECTCODE", 0);

            OutputCode(".word (res_end-res_start +4)", 0);

            OutputCode(".byt " + id);
            OutputCode("res_start", 0);

            pos = generatedCode.Count();

            Visit(context.block());

            OutputCode(".byt SC_STOP_SCRIPT", 1);
            OutputCode("res_end", 0);
            OutputCode(".)", 0);

            totalSize += byteOffset;

            generatedCode.Insert(pos, "; Response table"); pos++;
            foreach (KeyValuePair<string, bool> entry in codeLabels)
            {
                if (!entry.Value)
                {
                    Error("Undefined label " + entry.Key + " in script " + id, -1, context.GetText());
                }
                else
                {
                    var vc = getVerbCode(entry.Key);
                    if (vc != null)
                    {
                        generatedCode.Insert(pos, ".byt " + vc + Environment.NewLine + ".word (l_" + entry.Key + "-res_start)");
                        pos++;
                        totalSize += 3;
                    }
                }
            }
            generatedCode.Insert(pos, ".byt $ff ; End of response table");
            totalSize++;

            codeLabels.Clear();
            localSymbols.Empty();
            localSymbols.firstaddrB = 200;
            localSymbols.firstaddrL = 200;

            return Symbol.Types.None;
        }


        public override Symbol.Types VisitStringpackMain([NotNull] OASISGrammarParser.StringpackMainContext context)
        {
            int id;

            if ((!int.TryParse(context.NUMBER().GetText(), out id)) || (id > 255))
                Error("Invalid Object ID", context.Start.Line, context.GetText());

            byteOffset = 0;

            OutputCode("", 0);
            OutputCode("; String pack " + id, 0);
            OutputCode(".(", 0);
            if (id >= 200)
                OutputCode(".byt RESOURCE_STRING|$80", 0);
            else
                OutputCode(".byt RESOURCE_STRING", 0);

            OutputCode(".word (res_end-res_start +4)", 0);

            OutputCode(".byt " + id);
            OutputCode("res_start", 0);

            int i = 0;
            while (context.STRING(i) != null)
            {
                OutputCode(".asc " + context.STRING(i).GetText() + ",0 ; String " + i, 0);
                totalSize += context.STRING(i).GetText().Length + 1;
                i++;
            }
            OutputCode("res_end", 0);
            OutputCode(".)", 0);

            return Symbol.Types.None;
        }

        public override Symbol.Types VisitDialogMain([NotNull] OASISGrammarParser.DialogMainContext context)
        {
            int id;
            int pos;
            int stid;
            int scid;

            // Check the id is valid
            if ((!int.TryParse(context.NUMBER(0).GetText(), out id)) || (id > 255))
                Error("Invalid Object ID", context.Start.Line, context.GetText());

            if ((!int.TryParse(context.st.Text, out stid)) || (stid > 255))
                Error("Invalid Stringpack ID", context.Start.Line, context.GetText());

            if ((!int.TryParse(context.s.Text, out scid)) || (scid > 255))
                Error("Invalid Script ID", context.Start.Line, context.GetText());


            // Ouput header
            OutputCode("", 0);
            OutputCode("; Dialog " + id, 0);
            OutputCode(".(", 0);
            if (id >= 200)
                OutputCode(".byt RESOURCE_DIALOG|$80", 0);
            else
                OutputCode(".byt RESOURCE_DIALOG", 0);

            OutputCode(".word (res_end-res_start +4)", 0);

            OutputCode(".byt " + id);
            OutputCode("res_start", 0);

            // Check if rest of ids are valid


            // Stringpack and script id
            OutputCode(".byt " + stid + "\t; Stringpack with options", 1);
            OutputCode(".byt " + scid + "\t; Script with response actions", 1);

            // Save current position and parse options
            pos = generatedCode.Count();

            base.VisitDialogMain(context);

            // Insert the list of active options ending in $ff and the list of word offsets
            // stored in the corresponding lists member variables

            String s = ".byt ";
            foreach (DlgEntry d in theDlgOptions)
            {
                s += (d.active ? "1" : "0") + ",";
            }
            s += "$ff";
            OutputCode(s + "\t; Active flags", theDlgOptions.Count() + 1);

            s = ".word ";
            foreach (DlgEntry d in theDlgOptions)
            {
                s += "(l_" + d.labelOffset + "-script_" + context.s.Text + "_start),";
            }
            OutputCode(s.TrimEnd(',') + "\t; Jump labels", theDlgOptions.Count()*2);

            OutputCode("res_end", 0);
            OutputCode(".)", 0);

            // Generate strtingpack associated resource
            OutputCode("; String pack for dialog " + id, 0);
            OutputCode(".(", 0);
            if (stid >= 200)
                OutputCode(".byt RESOURCE_STRING|$80", 0);
            else
                OutputCode(".byt RESOURCE_STRING", 0);

            OutputCode(".word (res_end-res_start +4)", 0);

            OutputCode(".byt " + stid);
            OutputCode("res_start", 0);

            int i = 0;
            foreach(DlgEntry d in theDlgOptions)
            {
                OutputCode(".asc " + d.sentence + ",0 ; String " + i, 0);
                totalSize += d.sentence.Length + 1;
                i++;
            }
            OutputCode("res_end", 0);
            OutputCode(".)", 0);


            return Symbol.Types.None;

        }

        public override Symbol.Types VisitDialogOption([NotNull] OASISGrammarParser.DialogOptionContext context)
        {
            DlgEntry newd;
            newd.active = (context.a.Text == "active");
            newd.labelOffset = context.l.Text;
            newd.sentence = context.o.Text;
            theDlgOptions.Add(newd);
            
            return Symbol.Types.None;
        }
    }
}

