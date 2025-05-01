
//NURSELİ YILDIZ B221202040 
//ABDULKADİR KILIÇ B221202015


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LRParser
{
    public class Rule
    {
        public string Lhs { get; }
        public List<string> Rhs { get; }
        public Rule(string lhs, List<string> rhs)   // burası genel gramer kuralıdır hocam.
        {
            Lhs = lhs;
            Rhs = rhs;
        }
    }
    public class TreeNode
    {
        public string Value { get; }
        public List<TreeNode> Children { get; } //güğüm mantıgı burada oluşturuluyorr..
        public TreeNode(string value)
        {
            Value = value;
            Children = new List<TreeNode>();
        }
    }
    public class Parser
    {
        private List<object> _stack;
        private List<TreeNode> _treeStack;
        private List<string> _input;
        private int _pos;
        public List<string> Trace { get; }
        public List<string> Log { get; }    // bu kısım temel lr parser uygulaması için bir algoritma
        private List<Rule> _grammar;
        private Dictionary<(int, string), string> _actionTable;
        private Dictionary<(int, string), int> _gotoTable;
        public TreeNode ParseTree { get; private set; }

        public Parser(string input, List<Rule> grammar,
                      Dictionary<(int, string), string> actionTable,
                      Dictionary<(int, string), int> gotoTable)
        {
            _stack = new List<object> { 0 };
            _treeStack = new List<TreeNode>();
            _input = input.Split(' ').ToList();
            _pos = 0;
            _grammar = grammar;
            _actionTable = actionTable;   
            _gotoTable = gotoTable;

            Trace = new List<string>
            {
                "Stack                                    Input                                    Action",
                new string('-', 104)
            };
            Log = new List<string>();
        }

        private string StackToString()
        {
            var sb = new StringBuilder();
            foreach (var item in _stack)      //amacı zaten method asından belli
                sb.Append(item);
            return sb.ToString().PadRight(40);
        }

        private string InputToString()
        {
            var remaining = _pos < _input.Count ? _input.Skip(_pos) : Enumerable.Empty<string>();
            return string.Join(" ", remaining).PadRight(40);
        }

        private void Shift(int nextState)
        {
            var token = _input[_pos];
            _stack.Add(token);
            _stack.Add(nextState);
            _treeStack.Add(new TreeNode(token));
            _pos++;
            Trace.Add($"{StackToString()}{InputToString()}Shift {nextState}");
            Log.Add($"Shift: Moved to state {nextState} with token {token}");
        }

        private void Reduce(int ruleNum)
        {
            var rule = _grammar[ruleNum];
            int rhsLen = rule.Rhs.Count * 2;
            if (rhsLen > 0)
                _stack.RemoveRange(_stack.Count - rhsLen, rhsLen);

            int state = (int)_stack.Last();
            if (!_gotoTable.TryGetValue((state, rule.Lhs), out int nextState))
            {
                Log.Add($"Error: No GOTO state for state {state} and non-terminal {rule.Lhs}");
                return;
            }

            _stack.Add(rule.Lhs);
            _stack.Add(nextState);

            var parent = new TreeNode(rule.Lhs);
            int numChildren = rule.Rhs.Count;
            if (numChildren > 0 && _treeStack.Count >= numChildren)
            {
                var children = _treeStack.GetRange(_treeStack.Count - numChildren, numChildren);
                _treeStack.RemoveRange(_treeStack.Count - numChildren, numChildren);
                parent.Children.AddRange(children);
            }
            _treeStack.Add(parent);

            if (_stack.Count == 3 && (int)_stack[0] == 0 && _stack[1].ToString() == "E" && rule.Lhs == "E")
            {
                ParseTree = parent;
            }

            Trace.Add($"{StackToString()}{InputToString()}Reduce {ruleNum} (GOTO [{state}, {rule.Lhs}])");
            Log.Add($"Reduce: Applied rule {ruleNum} ({rule.Lhs} → {string.Join(" ", rule.Rhs)}), GOTO state {nextState}");
        }

        public bool Parse()
        {
            while (true)
            {
                int state = (int)_stack.Last();
                string token = _pos < _input.Count ? _input[_pos] : "$";

                if (!_actionTable.TryGetValue((state, token), out string action) || string.IsNullOrEmpty(action))
                {
                    Trace.Add($"{StackToString()}{InputToString()}Syntax error");
                    Log.Add($"Syntax error: No action for state {state} and token {token}");
                    throw new Exception($"syntax error at token {token}");
                }

                if (action == "accept")
                {
                    Trace.Add($"{StackToString()}{InputToString()}Accept");
                    Log.Add("Parsing successful");
                    return true;
                }

                if (action.StartsWith("s"))
                {
                    int nextState = int.Parse(action.Substring(1));
                    Shift(nextState);
                }
                else if (action.StartsWith("r"))
                {
                    int ruleNum = int.Parse(action.Substring(1));
                    Reduce(ruleNum);
                }
            }
        }

        public void PrintTree(TreeNode node, string prefix, bool isLast, StringBuilder builder, HashSet<TreeNode> visited)
        {
            if (node == null || visited.Contains(node))
            {
                builder.AppendLine(prefix + "[Cycle detected]");
                return;
            }
            visited.Add(node);

            builder.AppendLine(string.IsNullOrEmpty(prefix) ? "/" + node.Value : prefix + "/" + node.Value);
            foreach (var child in node.Children)
            {
                string newPrefix = string.IsNullOrEmpty(prefix) ? "/" + node.Value : prefix + "/" + node.Value;
                PrintTree(child, newPrefix, false, builder, visited);
            }
            visited.Remove(node);
        }
        public static List<Rule> ReadGrammar(string inputDir)
        {
            var lines = File.ReadAllLines(Path.Combine(inputDir, "Grammar.txt"));
            var grammar = new Rule[lines.Length + 1];
            grammar[0] = new Rule("", new List<string>());
            foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;
                int ruleNum = int.Parse(parts[0]);
                string lhs = parts[1];
                var rhs = parts.Skip(3).ToList();
                grammar[ruleNum] = new Rule(lhs, rhs);
            }
            return grammar.ToList();
        }
        public static Dictionary<(int, string), string> ReadActionTable(string inputDir)
        {
            var lines = File.ReadAllLines(Path.Combine(inputDir, "ActionTable.txt"));
            var headers = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
            var table = new Dictionary<(int, string), string>();
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int state = int.Parse(parts[0]);
                for (int j = 1; j < parts.Length; j++)
                {
                    string action = parts[j] == "-" ? "" : parts[j];
                    table[(state, headers[j - 1])] = action;
                }
            }
            return table;
        }
        public static Dictionary<(int, string), int> ReadGotoTable(string inputDir)
        {
            var lines = File.ReadAllLines(Path.Combine(inputDir, "GotoTable.txt"));
            var headers = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
            var table = new Dictionary<(int, string), int>();
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int state = int.Parse(parts[0]);
                for (int j = 1; j < parts.Length; j++)
                {
                    if (parts[j] != "-")
                        table[(state, headers[j - 1])] = int.Parse(parts[j]);
                }
            }
            return table;
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            string inputDir = "inputs";
            string outputDir = "outputs";
            Directory.CreateDirectory(outputDir);

            var grammar = Parser.ReadGrammar(inputDir);
            var actionTable = Parser.ReadActionTable(inputDir);
            var gotoTable = Parser.ReadGotoTable(inputDir);

            for (int i = 1; i <= 9; i++)
            {
                string inputFile = Path.Combine(inputDir, $"input{i}.txt");
                string outputFile = Path.Combine(outputDir, $"output{i}.txt");
                string logFile = Path.Combine(outputDir, $"log{i}.txt");

                if (!File.Exists(inputFile)) continue;
                string inputText = File.ReadAllText(inputFile).Trim();

                Parser parser = new Parser(inputText, grammar, actionTable, gotoTable);
                bool success = false; Exception ex = null;
                try { success = parser.Parse(); } catch (Exception e) { ex = e; }

                var sb = new StringBuilder();
                foreach (var line in parser.Trace)
                    sb.AppendLine(line);
                sb.AppendLine(new string('-', 104));
                sb.AppendLine("Parse tree:");
                parser.PrintTree(parser.ParseTree, "", true, sb, new HashSet<TreeNode>());

                File.WriteAllText(outputFile, sb.ToString());
                File.WriteAllText(logFile, string.Join(Environment.NewLine, parser.Log));

                if (!success)
                    Console.WriteLine($"Parsing failed for {inputFile}: {ex?.Message}");
                else
                    Console.WriteLine($"Parsing successful for {inputFile}");
            }
        }
    }
}
