﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTKTest.Bytecode.Shared;

namespace OpenTKTest.Bytecode.Compiler
{
    class ByteWriter : BinaryWriter
    {
        public ByteWriter(Stream output) : base(output) { }

        public void Write(Instruction value)
        {
            Console.WriteLine("Wrote " + value);
            base.Write((byte)value);
        }

        public override void Write(byte value)
        {
            Console.WriteLine("Wrote " + value);
            base.Write(value);
        }

        public override void Write(int value)
        {
            Console.WriteLine("Wrote " + value);
            base.Write(value);
        }

        public override void Write(string value)
        {
            Console.WriteLine("Wrote " + value);
            base.Write(value);
        }
    }


    class WriteQueue
    {
        public List<byte> byteQueue = new List<byte>();

        public void Write(Instruction i)
        {
            Insert(byteQueue.Count, i);
        }

        public void Write(byte b)
        {
            Insert(byteQueue.Count, b);
        }

        public void Write(string s)
        {
            Insert(byteQueue.Count, s);
        }

        public void Write(int i)
        {
            Insert(byteQueue.Count, i);
        }

        public void Insert(int pos, byte b)
        {
            byteQueue.Insert(pos, b);
        }

        public void Insert(int pos, string s)
        {
            List<byte> bytes = new List<byte>();
            foreach (char c in s)
            {
                bytes.Add(Encoding.ASCII.GetBytes(new[] { c })[0]);
            }
            bytes.Insert(0, (byte)s.Length);
            for (int i_ = 0; i_ < bytes.Count; ++i_)
                Insert(pos + i_, bytes[i_]);
        }

        public void Insert(int pos, Instruction i)
        {
            Insert(pos, (byte)i);
        }

        public void Insert(int pos, int i)
        {
            var bytes = BitConverter.GetBytes(i);
            for (int i_ = 0; i_ < bytes.Length; ++i_)
                Insert(pos + i_, bytes[i_]);
        }
    }


    class Function
    {
        public string name;
        public int location;
        public int parameters;
        public bool builtIn;

        public EventHandler<Tuple<Lexer, int>> builtInFunc;
    }

    class Program
    {
        public static Lexer lexer = new Lexer();
        static void Main()
        {
            Console.WriteLine("Simple bytecode compiler");
            Console.Write("Enter the path of a file or directory of files to compile: ");
            string path = Console.ReadLine();
            if (Directory.Exists(path))
                foreach (string file in Directory.GetFiles(path))
                {
                    if (file.EndsWith(".asc")) // agthrs source code ;)
                        CompileFile(file); 
                }
            else if (File.Exists(path))
                CompileFile(path);
            else
                Console.WriteLine("Could not find the directory or file " + path);

            Console.ReadLine();
        }

        static void CompileFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var writer = new ByteWriter(new FileStream(filePath.Remove(filePath.LastIndexOf(".")) + ".abc", FileMode.Create)))
            {
                WriteQueue writeQueue = new WriteQueue();
                bool asmMode = false;
                bool awaitingAsmMode = false;

                List<Function> functions = new List<Function>()
                {
                    new Function()
                    {
                        name = "ConsoleLog",
                        builtIn = true,
                        parameters = 1,
                        location = -1,
                        builtInFunc = new EventHandler<Tuple<Lexer, int>>((sender, lexerIndexPair) =>
                        {
                            var parameter1 = lexerIndexPair.Item1.tokens[lexerIndexPair.Item2 + 2];
                            if (parameter1.type != TokenType.Literal) Console.WriteLine("Expected parameter");
                            writeQueue.Write(Instruction.OUTPUT);
                        })
                    },
                    new Function()
                    {
                        name = "PushToStack",
                        builtIn = true,
                        parameters = 1,
                        location = -1,
                        builtInFunc = new EventHandler<Tuple<Lexer, int>>((sender, lexerIndexPair) =>
                        {
                            var parameter1 = lexerIndexPair.Item1.tokens[lexerIndexPair.Item2 + 2];
                            if (parameter1.type != TokenType.Literal) Console.WriteLine("Expected parameter");
                            writeQueue.Write(Instruction.OUTPUT);
                        })
                    },
                };
                lexer.ParseFile(reader.ReadToEnd());
                for (int i = 0; i < lexer.tokens.Count; ++i)
                {
                    var token = lexer.tokens[i];
                    bool isFunc = false;
                    if (lexer.tokens.Count > i + 1)
                        isFunc = (lexer.tokens[i + 1].type == TokenType.Separator && lexer.tokens[i + 1].value == "(");
                    switch (token.type)
                    {
                        case TokenType.Identifier:
                            if (asmMode)
                            {
                                // use a variant of the old assembly-style interpreter
                                foreach (var instruction in Enum.GetValues(typeof(Instruction)))
                                    if (token.value == instruction.ToString())
                                        writeQueue.Write((byte)instruction);
                            }
                            else
                            {
                                if (isFunc)
                                {
                                    foreach (var function in functions)
                                    {
                                        if (token.value == function.name)
                                        {
                                            List<string> parameters = new List<string>();
                                            int paramCount = 0, p = 2; // +2 for other tokens
                                            while (lexer.tokens[i + p].type != TokenType.Separator)
                                            {
                                                ++paramCount;
                                                ++p;
                                            }
                                            if (paramCount < function.parameters) Console.WriteLine("Not enough parameters to satisfy func");
                                            if (function.builtIn)
                                            {
                                                function.builtInFunc(null, new Tuple<Lexer, int>(lexer, i));
                                            }
                                            else
                                            {
                                                writeQueue.Write(Instruction.INTEGER_LITERAL);
                                                writeQueue.Write(function.location);
                                                writeQueue.Write(Instruction.GOTO);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case TokenType.Literal:
                            var pos = writeQueue.byteQueue.Count;
                            if (pos < 0) pos = 0;
                            if (IsLiteralString(token.value))
                            {
                                writeQueue.Insert(pos, Instruction.STRING_LITERAL);
                                writeQueue.Insert(pos + 1, token.value.Remove(0, 1).Remove(token.value.Length - 2));
                            }
                            else
                            {
                                writeQueue.Insert(pos, Instruction.INTEGER_LITERAL);
                                writeQueue.Insert(pos + 1, token.value);
                            }
                            break;
                        case TokenType.Keyword:
                            if (token.value == "ASM")
                            {
                                awaitingAsmMode = true;
                            }
                            else if (token.value == "func")
                            {
                                // function declaration:
                                // [scope] func [returntype] [functionname]([parameters]) {
                                // keyword keyw keyword??    identifier    s identifier s s
                                // -1        0      1           2          3      4     5 6
                                // check signature:
                                TokenType[] expectedTypesPreArgs = { TokenType.Keyword, TokenType.Keyword, TokenType.Keyword, TokenType.Identifier, TokenType.Separator };
                                bool correctTypes = true;
                                int paramCount = 0;
                                for (int t = 0; t < expectedTypesPreArgs.Length; ++t)
                                    if (expectedTypesPreArgs[t] != lexer.tokens[i - 1 + t].type) correctTypes = false;

                                int p = i - 1 + expectedTypesPreArgs.Length;
                                while (lexer.tokens[p].type != TokenType.Separator)
                                {
                                    ++paramCount;
                                    ++p;
                                }


                                // TODO: check end
                                // , TokenType.Identifier, TokenType.Separator, TokenType.Separator

                                if (correctTypes)
                                    Console.WriteLine("Function declared:\n\tScope: " + lexer.tokens[i - 1].value + "\n\tName: " + lexer.tokens[i + 2].value + "\n\tPosition: " + i + "\n\tParameters: " + paramCount);

                                functions.Add(new Function()
                                {
                                    location = i + 1,
                                    name = lexer.tokens[i + 2].value,
                                    parameters = paramCount
                                });

                                i += expectedTypesPreArgs.Length + paramCount;
                            }
                            break;
                        case TokenType.Separator:
                            if (token.value == "{")
                            {
                                if (awaitingAsmMode)
                                {
                                    asmMode = true;
                                    awaitingAsmMode = false;
                                }
                            }
                            else
                            {
                                if (awaitingAsmMode) awaitingAsmMode = false;
                            }
                            if (token.value == "}")
                            {
                                if (asmMode) asmMode = false;
                            }
                            break;
                        case TokenType.Comment:
                            break;
                    }
                }

                foreach (var b in writeQueue.byteQueue)
                {
                    writer.Write(b);
                }
            }
        }

        static bool IsLiteralString(string literalValue)
        {
            return (literalValue.StartsWith("\""));
        }
    }
}