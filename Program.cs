// Düzenlenmiş hali: Stack | Input | Action sütunları hizalı, hocanın istediği log formatına uygun şekilde
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hw1parser
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> grammar = ReadFile("Grammar.txt");
            var actionTable = LoadActionTable("ActionTable.txt");
            var gotoTable = LoadGotoTable("GotoTable.txt");

            Console.WriteLine("\nDosyalar başarıyla okundu!");
            Console.WriteLine("\nBir ifade giriniz (örnek: id + id * id):");
            string inputLine = Console.ReadLine();

            List<string> inputTokens = new List<string>(inputLine.Trim().Split(' '));
            inputTokens.Add("$");

            Stack<string> stack = new Stack<string>();
            stack.Push("0");

            bool accepted = false;

            Console.WriteLine("\n{0,-40}{1,-40}{2,-20}", "Stack", "Input", "Action");
            Console.WriteLine(new string('-', 100));

            while (!accepted)
            {
                int currentState = int.Parse(stack.Peek());
                string currentToken = inputTokens.Count > 0 ? inputTokens[0] : "$";

                string action = FindAction(actionTable, currentState, currentToken);

                Console.WriteLine(
                    "{0,-40}{1,-40}{2,-20}",
                    string.Join(" ", stack.Reverse()),
                    string.Join(" ", inputTokens),
                    action
                );

                if (action == "acc" || action == "accept")
                {
                    Console.WriteLine("\nParsing başarıyla tamamlandı!");
                    accepted = true;
                }
                else if (action.StartsWith("s") || action.StartsWith("S"))
                {
                    int nextState = int.Parse(action.Substring(1));
                    stack.Push(currentToken);
                    stack.Push(nextState.ToString());
                    inputTokens.RemoveAt(0);
                }
                else if (action.StartsWith("r") || action.StartsWith("R"))
                {
                    int productionNumber = int.Parse(action.Substring(1));
                    string production = grammar[productionNumber - 1];

                    string[] parts = production.Split("->");
                    string leftSide = parts[0].Trim();
                    string rightSide = parts[1].Trim();

                    string[] rightSymbols = rightSide.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < rightSymbols.Length; i++)
                    {
                        stack.Pop(); // Token
                        stack.Pop(); // State
                    }

                    int topState = int.Parse(stack.Peek());
                    int gotoState = FindGoto(gotoTable, topState, leftSide);
                    stack.Push(leftSide);                // non-terminal gibi davranır (örnek: F, T, E)
                    stack.Push(gotoState.ToString());    // ardından gelen yeni state
                    ;


                    Console.WriteLine("{0,-40}{1,-40}{2,-20}",
     string.Join(" ", stack.Reverse()),
     string.Join(" ", inputTokens),
     $"goto {gotoState}");

                }
                else
                {
                    Console.WriteLine("\nSyntax Error! Parsing başarısız oldu.");
                    break;
                }
            }
        }

        static Dictionary<(int, string), string> LoadActionTable(string filename)
        {
            var actionTable = new Dictionary<(int, string), string>();
            var lines = File.ReadAllLines(filename);

            var headers = lines[0].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (headers[0] == "State") headers.RemoveAt(0);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                int state = int.Parse(parts[0]);

                for (int j = 1; j < parts.Length; j++)
                {
                    if (parts[j] != "-" && j - 1 < headers.Count)
                        actionTable[(state, headers[j - 1])] = parts[j];
                }
            }
            return actionTable;
        }

        static string FindAction(Dictionary<(int, string), string> actionTable, int state, string token)
        {
            if (actionTable.TryGetValue((state, token), out string action))
                return action;
            return "error";
        }

        static Dictionary<(int, string), int> LoadGotoTable(string filename)
        {
            var gotoTable = new Dictionary<(int, string), int>();
            var lines = File.ReadAllLines(filename);

            var headers = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (headers[0] == "State") headers.RemoveAt(0);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                int state = int.Parse(parts[0]);
                for (int j = 1; j < parts.Length; j++)
                {
                    if (parts[j] != "-")
                    {
                        string nonTerminal = headers[j - 1];
                        int nextState = int.Parse(parts[j]);
                        gotoTable[(state, nonTerminal)] = nextState;
                    }
                }
            }
            return gotoTable;
        }

        static int FindGoto(Dictionary<(int, string), int> gotoTable, int state, string nonTerminal)
        {
            if (gotoTable.TryGetValue((state, nonTerminal), out int nextState))
                return nextState;
            return -1;
        }

        static List<string> ReadFile(string filename)
        {
            List<string> lines = new List<string>();
            foreach (var line in File.ReadLines(filename))
                lines.Add(line.Trim());
            return lines;
        }
    }
}
