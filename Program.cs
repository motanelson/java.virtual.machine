
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JVM_Studio
{
    class Instruction
    {
        public string Opcode = "";
        public List<string> Args = new List<string>();
        public int LineNumber;
        public string Raw = "";
    }

    class JVMInterpreter
    {
        public List<Instruction> Instructions = new List<Instruction>();
        public Dictionary<string, int> Labels = new Dictionary<string, int>();
        public Stack<object> Stack = new Stack<object>();
        public Dictionary<int, object> Locals = new Dictionary<int, object>();

        public int PC = 0;
        public bool Halted = true;

        public void Reset()
        {
            PC = 0;
            Stack.Clear();
            Locals.Clear();
            Halted = Instructions.Count == 0;
        }

        public static List<string> Tokenize(string text)
        {
            List<string> result = new List<string>();
            string current = "";
            bool quoted = false;

            foreach (char c in text)
            {
                if (c == '"')
                    quoted = !quoted;

                if (char.IsWhiteSpace(c) && !quoted)
                {
                    if (current.Length > 0)
                    {
                        result.Add(current);
                        current = "";
                    }
                }
                else
                {
                    current += c;
                }
            }

            if (current.Length > 0)
                result.Add(current);

            return result;
        }

        public void Load(string code)
        {
            Instructions.Clear();
            Labels.Clear();
            Stack.Clear();
            Locals.Clear();
            PC = 0;

            string[] lines = code.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                int comment = line.IndexOf(';');

                if (comment >= 0)
                    line = line.Substring(0, comment).Trim();

                if (line.Length == 0)
                    continue;

                // Ignore assembler directives.
                if (line.StartsWith("."))
                    continue;

                // Register labels.
                if (line.EndsWith(":"))
                {
                    string label = line.Substring(0, line.Length - 1);

                    if (Labels.ContainsKey(label))
                        throw new Exception("Duplicate label: " + label);

                    Labels[label] = Instructions.Count;
                    continue;
                }

                List<string> tokens = Tokenize(line);

                if (tokens.Count == 0)
                    continue;

                Instruction instruction = new Instruction();

                instruction.Opcode = tokens[0].ToLowerInvariant();
                instruction.Args = tokens.Skip(1).ToList();
                instruction.LineNumber = i + 1;
                instruction.Raw = line;

                Instructions.Add(instruction);
            }

            Halted = Instructions.Count == 0;
        }

        private object Pop()
        {
            if (Stack.Count == 0)
                throw new Exception("Operand stack underflow");

            return Stack.Pop();
        }

        private int PopInt()
        {
            return Convert.ToInt32(Pop());
        }

        private int GetLocal(int index)
        {
            if (!Locals.ContainsKey(index))
                return 0;

            return Convert.ToInt32(Locals[index]);
        }

        private void SetLocal(int index, object value)
        {
            Locals[index] = value;
        }

        private string GetArgument(Instruction instr, int index)
        {
            if (instr.Args.Count <= index)
                throw new Exception("Missing instruction argument");

            return instr.Args[index];
        }

        private void Jump(string label)
        {
            if (!Labels.ContainsKey(label))
                throw new Exception("Unknown label: " + label);

            PC = Labels[label];
        }

        private void Branch(bool condition, string label)
        {
            if (condition)
                Jump(label);
        }

        public bool Step(Action<object> output)
        {
            if (Halted || PC >= Instructions.Count)
            {
                Halted = true;
                return true;
            }

            Instruction instr = Instructions[PC];
            string op = instr.Opcode;

            PC++;

            try
            {
                switch (op)
                {
                    // Integer constants.

                    case "iconst_m1": Stack.Push(-1); break;
                    case "iconst_0": Stack.Push(0); break;
                    case "iconst_1": Stack.Push(1); break;
                    case "iconst_2": Stack.Push(2); break;
                    case "iconst_3": Stack.Push(3); break;
                    case "iconst_4": Stack.Push(4); break;
                    case "iconst_5": Stack.Push(5); break;

                    case "bipush":
                    case "sipush":
                        Stack.Push(int.Parse(GetArgument(instr, 0)));
                        break;

                    case "ldc":
                        {
                            string value = GetArgument(instr, 0);

                            if (value.StartsWith("\"") &&
                                value.EndsWith("\""))
                            {
                                value = value.Substring(1, value.Length - 2);
                            }
                            else if (int.TryParse(value, out int number))
                            {
                                Stack.Push(number);
                                break;
                            }

                            Stack.Push(value);
                            break;
                        }

                    // Local variable stores.

                    case "istore":
                    case "astore":
                        SetLocal(int.Parse(GetArgument(instr, 0)), Pop());
                        break;

                    case "istore_0":
                    case "astore_0":
                        SetLocal(0, Pop());
                        break;

                    case "istore_1":
                    case "astore_1":
                        SetLocal(1, Pop());
                        break;

                    case "istore_2":
                    case "astore_2":
                        SetLocal(2, Pop());
                        break;

                    case "istore_3":
                    case "astore_3":
                        SetLocal(3, Pop());
                        break;

                    // Local variable loads.

                    case "iload":
                    case "aload":
                        Stack.Push(GetLocal(int.Parse(GetArgument(instr, 0))));
                        break;

                    case "iload_0":
                    case "aload_0":
                        Stack.Push(GetLocal(0));
                        break;

                    case "iload_1":
                    case "aload_1":
                        Stack.Push(GetLocal(1));
                        break;

                    case "iload_2":
                    case "aload_2":
                        Stack.Push(GetLocal(2));
                        break;

                    case "iload_3":
                    case "aload_3":
                        Stack.Push(GetLocal(3));
                        break;

                    // Arithmetic operations.

                    case "iadd":
                        {
                            int b = PopInt();
                            int a = PopInt();
                            Stack.Push(unchecked(a + b));
                            break;
                        }

                    case "isub":
                        {
                            int b = PopInt();
                            int a = PopInt();
                            Stack.Push(unchecked(a - b));
                            break;
                        }

                    case "imul":
                        {
                            int b = PopInt();
                            int a = PopInt();
                            Stack.Push(unchecked(a * b));
                            break;
                        }

                    case "idiv":
                        {
                            int b = PopInt();
                            int a = PopInt();

                            if (b == 0)
                                throw new Exception("ArithmeticException: / by zero");

                            if (a == int.MinValue && b == -1)
                                Stack.Push(int.MinValue);
                            else
                                Stack.Push(a / b);

                            break;
                        }

                    case "irem":
                        {
                            int b = PopInt();
                            int a = PopInt();

                            if (b == 0)
                                throw new Exception("ArithmeticException: / by zero");

                            Stack.Push(a % b);
                            break;
                        }

                    case "iinc":
                        {
                            int index = int.Parse(GetArgument(instr, 0));
                            int value = int.Parse(GetArgument(instr, 1));

                            SetLocal(index, unchecked(GetLocal(index) + value));
                            break;
                        }

                    // Stack operations.

                    case "dup":
                        {
                            object value = Pop();
                            Stack.Push(value);
                            Stack.Push(value);
                            break;
                        }

                    case "pop":
                        Pop();
                        break;

                    // Unconditional jump.

                    case "goto":
                        Jump(GetArgument(instr, 0));
                        break;

                    // Conditional branches.

                    case "ifeq":
                        Branch(PopInt() == 0, GetArgument(instr, 0));
                        break;

                    case "ifne":
                        Branch(PopInt() != 0, GetArgument(instr, 0));
                        break;

                    case "if_icmpeq":
                        {
                            int b = PopInt();
                            int a = PopInt();
                            Branch(a == b, GetArgument(instr, 0));
                            break;
                        }

                    case "if_icmpne":
                        {
                            int b = PopInt();
                            int a = PopInt();
                            Branch(a != b, GetArgument(instr, 0));
                            break;
                        }

                    case "if_icmplt":
                        {
                            int b = PopInt();
                            int a = PopInt();
                            Branch(a < b, GetArgument(instr, 0));
                            break;
                        }

                    case "if_icmpge":
                        {
                            int b = PopInt();
                            int a = PopInt();
                            Branch(a >= b, GetArgument(instr, 0));
                            break;
                        }

                    case "if_icmpgt":
                        {
                            int b = PopInt();
                            int a = PopInt();
                            Branch(a > b, GetArgument(instr, 0));
                            break;
                        }

                    case "if_icmple":
                        {
                            int b = PopInt();
                            int a = PopInt();
                            Branch(a <= b, GetArgument(instr, 0));
                            break;
                        }

                    // System.out simulation.

                    case "getstatic":
                        Stack.Push("[PrintStream:out]");
                        break;

                    case "invokevirtual":
                        {
                            string method = GetArgument(instr, 0);

                            if (method.Contains("println"))
                            {
                                object value = Pop();
                                object reference = Pop();

                                if (!Equals(reference, "[PrintStream:out]"))
                                    throw new Exception("Invalid PrintStream reference");

                                output(value);
                            }

                            break;
                        }

                    // Method termination.

                    case "return":
                    case "ireturn":
                        Halted = true;
                        return true;

                    default:
                        throw new Exception("Unsupported opcode: " + op);
                }

                if (PC >= Instructions.Count)
                    Halted = true;

                return Halted;
            }
            catch (Exception ex)
            {
                Halted = true;

                throw new Exception(
                    "Runtime error at line " +
                    instr.LineNumber +
                    " (" + instr.Raw + "): " +
                    ex.Message
                );
            }
        }

        public void DisplayState()
        {
            Console.WriteLine();
            Console.WriteLine("================================");
            Console.WriteLine("          JVM INTERNAL STATE");
            Console.WriteLine("================================");

            Console.WriteLine("Program Counter: " + PC);

            Console.WriteLine();
            Console.WriteLine("Operand Stack:");

            if (Stack.Count == 0)
                Console.WriteLine("  (empty)");

            int index = 0;

            foreach (object value in Stack.Reverse())
            {
                Console.WriteLine("  [" + index + "] " + value);
                index++;
            }

            Console.WriteLine();
            Console.WriteLine("Local Variables:");

            if (Locals.Count == 0)
                Console.WriteLine("  (none)");

            foreach (var item in Locals.OrderBy(x => x.Key))
            {
                Console.WriteLine(
                    "  local_" + item.Key + " = " + item.Value
                );
            }

            Console.WriteLine();

            if (!Halted && PC < Instructions.Count)
            {
                Instruction next = Instructions[PC];

                Console.WriteLine(
                    "Next instruction [L" +
                    next.LineNumber + "]: " +
                    next.Raw
                );
            }
            else
            {
                Console.WriteLine("VM Halted / Complete");
            }

            Console.WriteLine("================================");
        }
    }

    class Program
    {
        static JVMInterpreter vm = new JVMInterpreter();
        static string sourceFile = "";

        static void PrintOutput(object value)
        {
            //Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(value);
            //Console.ResetColor();
        }

        static void PrintError(string message)
        {
            //Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            //Console.ResetColor();
        }

        static void LoadFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("File not found: " + filename);

            string code = File.ReadAllText(filename);

            vm.Load(code);
            sourceFile = filename;

            Console.WriteLine();
            Console.WriteLine("Loaded file: " + filename);
            Console.WriteLine("Instructions: " + vm.Instructions.Count);

            vm.DisplayState();
        }

        static void ExecuteStep()
        {
            try
            {
                if (vm.Halted)
                {
                    Console.WriteLine("VM is halted. Use Reset first.");
                    return;
                }

                bool done = vm.Step(PrintOutput);

                vm.DisplayState();

                if (done)
                    Console.WriteLine("[Process finished with exit code 0]");
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
            }
        }

        static void ExecuteRun()
        {
            int cycles = 0;
            const int MAX_CYCLES = 50000;

            try
            {
                if (vm.Halted)
                {
                    Console.WriteLine("VM is halted. Use Reset first.");
                    return;
                }

                while (!vm.Halted && cycles < MAX_CYCLES)
                {
                    vm.Step(PrintOutput);
                    cycles++;
                }

                if (cycles >= MAX_CYCLES && !vm.Halted)
                {
                    PrintError(
                        "Maximum cycle limit exceeded. Possible infinite loop."
                    );
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("[Process finished with exit code 0]");
                }

                Console.WriteLine("Executed instructions: " + cycles);
                vm.DisplayState();
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
                vm.DisplayState();
            }
        }

        static void ResetVM()
        {
            vm.Reset();

            Console.WriteLine("--- VM Reset ---");
            vm.DisplayState();
        }

        static void Main(string[] args)
        {
            Console.BackgroundColor= ConsoleColor.White;
            Console.ForegroundColor= ConsoleColor.Black;
            Console.Clear();
            Console.Title = "JVM .jasm Studio - C# Console";

            Console.WriteLine("======================================");
            Console.WriteLine("          JVM .jasm STUDIO");
            Console.WriteLine("          C# Console Edition");
            Console.WriteLine("======================================");

            try
            {
                string filename;

                if (args.Length > 0)
                {
                    filename = args[0];
                }
                else
                {
                    Console.WriteLine();
                    Console.Write("Input .jasm file: ");
                    filename = Console.ReadLine() ?? "";
                }

                if (string.IsNullOrWhiteSpace(filename))
                {
                    Console.WriteLine("No input file specified.");
                    return;
                }

                LoadFile(filename);
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
                return;
            }

            bool exit = false;

            while (!exit)
            {
                Console.WriteLine();
                Console.WriteLine("============== MENU ==============");
                Console.WriteLine("1 - Step (Execute one instruction)");
                Console.WriteLine("2 - Run (Execute all instructions)");
                Console.WriteLine("3 - Reset VM");
                Console.WriteLine("4 - Reload input file");
                Console.WriteLine("5 - Show VM state");
                Console.WriteLine("6 - Exit");
                Console.WriteLine("==================================");

                Console.Write("Option: ");

                string option = Console.ReadLine() ?? "";

                switch (option)
                {
                    case "1":
                        ExecuteStep();
                        break;

                    case "2":
                        ExecuteRun();
                        break;

                    case "3":
                        ResetVM();
                        break;

                    case "4":
                        try
                        {
                            LoadFile(sourceFile);
                        }
                        catch (Exception ex)
                        {
                            PrintError(ex.Message);
                        }
                        break;

                    case "5":
                        vm.DisplayState();
                        break;

                    case "6":
                        exit = true;
                        break;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }

            Console.WriteLine("JVM Studio terminated.");
        }
    }
}
