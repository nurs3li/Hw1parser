// Student: [Adını Buraya Yaz]
// ID: [Öğrenci Numaran]
// Homework 1 – LR Parser Implementation
// Açıklama: Bu ödevde verilen grammar, action ve goto tablolarına göre bir LR parser tasarlanmıştır.
// Kodun amacı: Verilen input ifadeleri stack üzerinden shift-reduce yöntemiyle parse etmek ve parse tree üretmektir.

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
            // Grammar, ActionTable ve GotoTable dosyalarını oku
            List<string> grammar = ReadFile("Grammar.txt");
            var actionTable = LoadActionTable("ActionTable.txt");
            var gotoTable = LoadGotoTable("GotoTable.txt");

            Console.WriteLine("\nDosyalar başarıyla okundu!");
            Console.WriteLine("\nBir ifade giriniz (örnek: id + id * id):");
            string inputLine = Console.ReadLine();

            // Kullanıcıdan alınan ifadeyi boşluklardan ayır ve sonuna $ ekle
            List<string> inputTokens = new List<string>(inputLine.Trim().Split(' '));
            inputTokens.Add("$");

            Stack<string> stack = new Stack<string>();
            stack.Push("0"); // Başlangıç state

            List<string> parseTree = new List<string>();
            bool accepted = false;

            // Başlık satırı (Stage, Options, Options)
            Console.WriteLine("\n{0,-15}{1,-25}{2,-25}", "Stage", "Options", "Options");
            Console.WriteLine(new string('-', 65));

            while (!accepted)
            {
                // Stack'in tepesindeki state alınır
                if (!int.TryParse(stack.Peek(), out int currentState))
                {
                    Console.WriteLine("[HATA] Stack Peek bir state değil: " + stack.Peek());
                    break;
                }

                string currentToken = inputTokens.Count > 0 ? inputTokens[0] : "$";
                string action = FindAction(actionTable, currentState, currentToken);

                // Anlık durum tablosu satırı bastırılır
                string stackStr = string.Join("", stack.Reverse());
                string inputStr = string.Join(" ", inputTokens);
                string actionStr = FormatActionDescription(action, currentState, currentToken, grammar, gotoTable, stack);

                Console.WriteLine("{0,-15}{1,-25}{2,-25}", stackStr, inputStr, actionStr);

                if (action == "acc" || action == "accept")
                {
                    Console.WriteLine("\nParsing başarıyla tamamlandı!");
                    accepted = true;
                }
                else if (action.StartsWith("s") || action.StartsWith("S"))
                {
                    // Shift işlemi yapılır: token ve yeni state stack'e eklenir
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
                    string leftSide = parts[0].Trim(); // "F"
                    string rightSide = parts[1].Trim(); // "id"

                    // Stack'ten çıkarılacak eleman sayısı = 2 * sağ taraf token sayısı
                    string[] rightSymbols = rightSide.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (stack.Count < rightSymbols.Length * 2)
                    {
                        Console.WriteLine("[HATA] Stack'te yeterli eleman yok!");
                        break;
                    }

                    for (int i = 0; i < rightSymbols.Length; i++)
                    {
                        stack.Pop(); // State
                        stack.Pop(); // Symbol
                    }

                    int topState = int.Parse(stack.Peek());
                    int gotoState = FindGoto(gotoTable, topState, leftSide); // GOTO[0, F] aranacak

                    if (gotoState == -1)
                    {
                        Console.WriteLine($"\nHATA: GOTO[{topState}, {leftSide}] bulunamadı!");
                        Console.WriteLine($"Grammar Kuralı: {production}");
                        Console.WriteLine($"Stack: {string.Join(" ", stack.Reverse())}");
                        break;
                    }

                    stack.Push(leftSide);
                    stack.Push(gotoState.ToString());
                    parseTree.Add("/" + leftSide);
                }
                else
                {
                    // Tanımsız action → syntax hatası
                    Console.WriteLine("\nSyntax Error! Parsing başarısız oldu.");
                    break;
                }
            }

            Console.WriteLine("\n---");
            Console.WriteLine("\nParse tree:");
            foreach (var node in parseTree)
                Console.WriteLine(node);
        }

        // Action açıklaması biçimlendirici fonksiyon
        static string FormatActionDescription(string action, int state, string token, List<string> grammar, Dictionary<(int, string), int> gotoTable, Stack<string> stack)
        {
            if (action.StartsWith("s")) return "Shift " + action.Substring(1);
            else if (action.StartsWith("r"))
            {
                int prodNum = int.Parse(action.Substring(1));
                string production = grammar[prodNum - 1];
                string[] parts = production.Split("->");
                string left = parts[0].Trim();
                int topState = int.Parse(stack.Peek()); // DÜZELTME: stack.Peek() kullan
                return $"Reduce {prodNum} (GOTO [{topState}, {left}])"; // 6 F yerine F yaz
            }
            else if (action == "acc") return "Accept";
            else return "error";
        }

        // Action tablosunu dosyadan okur
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

        // Belirli bir state ve token için action'ı döndürür
        static string FindAction(Dictionary<(int, string), string> actionTable, int state, string token)
        {
            return actionTable.TryGetValue((state, token), out string action) ? action : "error";
        }

        // Goto tablosunu dosyadan okur
        static Dictionary<(int, string), int> LoadGotoTable(string filename)
        {
            var gotoTable = new Dictionary<(int, string), int>();
            var lines = File.ReadAllLines(filename);

            // Başlık satırını işle
            var headers = lines[0].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (headers[0] == "State") headers.RemoveAt(0);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                // State numarasını al
                if (!int.TryParse(parts[0], out int state))
                    continue; // Geçersiz state satırını atla

                for (int j = 1; j < parts.Length && j - 1 < headers.Count; j++)
                {
                    // "-" değerlerini atla, sadece sayısal değerleri işle
                    if (parts[j] != "-" && int.TryParse(parts[j], out int nextState))
                    {
                        gotoTable[(state, headers[j - 1])] = nextState;
                    }
                }
            }
            return gotoTable;
        }

        // Belirli bir state ve non-terminal için goto değerini döndürür
        static int FindGoto(Dictionary<(int, string), int> gotoTable, int state, string nonTerminal)
        {
            if (gotoTable.TryGetValue((state, nonTerminal), out int nextState))
                return nextState;
            return -1; // Bulunamazsa -1 döndür
        }

        // Dosya okuma işlemi (satır satır)
        static List<string> ReadFile(string filename)
        {
            return File.ReadLines(filename).Select(line => line.Trim()).ToList();
        }
    }
}