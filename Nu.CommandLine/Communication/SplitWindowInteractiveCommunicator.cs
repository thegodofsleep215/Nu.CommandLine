﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Nu.CommandLine.Attributes;

namespace Nu.CommandLine.Communication
{

    public class SplitWindowInteractiveCommunicator : ICommandCommunicator
    {
        #region Fields

        // List of loaded scripts.
        readonly Dictionary<string, List<string>> scripts = new Dictionary<string, List<string>>();

        // Console thread where commands are entered and executed.
        private Thread consoleThread;
        private Thread sizeThread;

        // History of commands entered.
        private readonly List<string> commandHistory = new List<string>();

        // Says if the console thread is running.
        private bool running;

        // Prompt that is printed out.
        readonly string promptName;

        // Length of the prompt
        readonly int promptLength;

        List<string> output = new List<string>();

        private System.Drawing.Size consoleSize;
        private object consoleLock = new object();
        private string inputText;




        // Matches a user entered command with parameters from the prompt. Items are white space delimited,
        // and quoted strings count as one parameter. It also matches incomplete quoted strings as oen parameter.
        readonly Regex userEntryParamsRegex = new Regex(@"^(?<command>\S+)\s+((?<params>(""[^""]*"")|([^\s""\[][^\s\[]*)|(""[^""]*$)|(\[[^\]]*\])|(\[[^\]]*$))\s*)+$");

        readonly Regex arrayValuesRegex = new Regex(@"^\[((?<item>(""[^""]*"")|([^\s""=,][^\s=,]*)|(""[^""]*$))((\s*)|(\s*,\s*)))*\]");

        readonly Regex keyValuePairRegex = new Regex(@"\[((?<pair>[^\s""=]+\s*=\s*((""[^""]*"")|([^\s"",][^\s*,\]]*)|(""[^""]*$)))((\s*)|(\s*,\s*)))+\]");

        // Matches a command with no parameters.
        readonly Regex userEntry = new Regex(@"^(?<command>\S+)$");

        /// <summary>
        /// Breaks up a line into items a ctrl+backspace cares about.
        /// </summary>
        readonly Regex ctrlBckspc = new Regex(@"^(?<parts>(\.)|(\\)|(\s+)|([^\s\.\\]+))*");
        #endregion

        public event Action ShuttingDown;

        /// <summary>
        /// Construcotr that adds default commands.
        /// </summary>
        /// <param name="commInterface"></param>
        /// <param name="pName"></param>
        public SplitWindowInteractiveCommunicator(string pName)
        {
            promptName = pName;
            promptLength = promptName.Length + 2; //2 == '> '
        }

        /// <summary>
        /// Starts the command line interface
        /// </summary>
        /// <param name="script">Script to run on startup.</param>
        public void Start()
        {
            running = true;
            consoleSize = new System.Drawing.Size { Height = Console.WindowHeight, Width = Console.WindowWidth };
            Console.SetBufferSize(consoleSize.Width, consoleSize.Height);

            sizeThread = new Thread(SizeManager);
            sizeThread.Start();

            consoleThread = new Thread(ConsoleRead);
            consoleThread.Start();
        }



        private void SizeManager()
        {
            while (running)
            {
                var size = new System.Drawing.Size { Height = Console.WindowHeight, Width = Console.WindowWidth };
                bool redraw = false;
                lock (consoleLock)
                {
                    if (size.Width != consoleSize.Width || size.Height != consoleSize.Height)
                    {
                        consoleSize = size;
                        Console.SetBufferSize(Console.WindowLeft + Console.WindowWidth, Console.WindowTop + Console.WindowHeight);
                        redraw = true;
                    }
                }
                if (redraw)
                {
                    DrawLayout();
                }
                Thread.Sleep(10);
            }
        }


        private void DrawLayout()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            int height;
            int width;
            lock (consoleLock)
            {
                height = consoleSize.Height;
                width = consoleSize.Width;
            }

            var outputHeight = height - 3;
            if (outputHeight <= 0) outputHeight = height;
            var index = output.Count - outputHeight;
            if (index < 0) index = 0;
            for (; index < output.Count; index++)
            {
                Console.WriteLine(output[index]);
            }

            Console.SetCursorPosition(0, height - 2);
            Console.Write(string.Join("", Enumerable.Repeat('-', width)));

            Console.SetCursorPosition(0, height - 1);


            // Draw input
            Console.Write($"{promptName}> {inputText}");

        }

        /// <summary>
        /// Stops the working thread.
        /// </summary>
        public void Stop()
        {
            ShuttingDown?.Invoke();
            running = false;
        }

        /// <summary>
        /// Executes a command with parameters.
        /// </summary>
        /// <param name="fullCommand"></param>
        private void ExecuteCommand(string fullCommand)
        {
            Match match;
            string command;
            var parameters = new List<object>();
            if ((match = userEntryParamsRegex.Match(fullCommand)).Success)
            {
                command = match.Groups["command"].Value;

                foreach (Capture cap in match.Groups["params"].Captures)
                {
                    string c = cap.Value.Trim('"');
                    Match a;
                    if ((a = arrayValuesRegex.Match(c)).Success)
                    {
                        var array = (from Capture capture in a.Groups["item"].Captures select capture.Value.Trim('"')).ToList();
                        parameters.Add(array);
                    }
                    else if ((a = keyValuePairRegex.Match(c)).Success)
                    {
                        var pairs = new Dictionary<string, string>();
                        foreach (Capture capture in a.Groups["pair"].Captures)
                        {
                            string[] parts = Regex.Split(capture.Value, @"\s*=\s*");
                            pairs[parts[0]] = parts[1].Trim('"');
                        }
                        parameters.Add(pairs);
                    }
                    else
                    {
                        parameters.Add(cap.Value.Trim('"'));
                    }
                }

            }
            else if ((match = userEntry.Match(fullCommand)).Success)
            {
                command = match.Groups["command"].Value;
            }
            else
            {
                WriteToConsole("Syntax error.");
                return;
            }


            WriteToConsole(OnProcessCommand(command, parameters));

        }

        /// <summary>
        /// Worker that listens to the console and exectues commands.
        /// </summary>
        private void ConsoleRead()
        {
            DrawLayout();
            while (running)
            {
                try
                {
                    string c = KeyIntercept();
                    inputText = "";
                    DrawLayout();

                    ExecuteCommand(c);
                }

                catch (Exception ex)
                {
                    WriteToConsole("An Exception occured while executing a command. {0}\n\n{1}", ex.Message, ex.StackTrace);
                }
            }
        }


        /// <summary>
        /// Intercepts a key strokes until they enter key is hit.
        /// </summary>
        /// <returns></returns>
        private string KeyIntercept()
        {
            int historyPos = -1;
            inputText = "";

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                if (key.Key == ConsoleKey.UpArrow)
                {
                    if (historyPos < commandHistory.Count - 1)
                    {
                        historyPos++;
                        ClearLine();
                        Console.Write(commandHistory[historyPos]);
                        inputText = commandHistory[historyPos];
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (historyPos > 0)
                    {
                        historyPos--;
                        ClearLine();
                        Console.Write(commandHistory[historyPos]);
                        inputText = commandHistory[historyPos];
                    }
                    else
                    {
                        historyPos = -1;
                        inputText = "";
                        ClearLine();
                    }
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    int left = Console.CursorLeft;
                    left = left - 2 <= 2 ? 2 : left - 2;
                    Console.CursorLeft = left;
                    if (inputText.Length > 0)
                        inputText = inputText.Substring(0, inputText.Length - 1);
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if ((key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                    {
                        Match m;
                        if ((m = ctrlBckspc.Match(inputText)).Success)
                        {
                            inputText = "";
                            for (int i = 0; i < m.Groups["parts"].Captures.Count - 1; i++)//foreach (Capture c in m.Groups["parts"].Captures)
                            {
                                inputText += m.Groups["parts"].Captures[i];
                            }
                            ClearLine();
                            Console.Write(inputText);
                        }
                    }
                    else
                    {
                        if (promptLength < Console.CursorLeft)
                        {
                            Console.CursorLeft -= 1;
                            Console.Write(" ");
                            Console.CursorLeft -= 1;
                        }
                        if (inputText.Length > 0)
                            inputText = inputText.Substring(0, inputText.Length - 1);
                    }
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    Console.CursorLeft--;
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    inputText = Tab(inputText);
                }
                else if (key.KeyChar >= 0x20 && key.KeyChar <= 0x7E)
                {
                    inputText += key.KeyChar;
                    Console.Out.Write(key.KeyChar);
                }
            }
            commandHistory.Insert(0, inputText);
            return inputText;
        }

        /// <summary>
        /// Writes output to the console and print the prompt.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parmeters"></param>
        private void WriteToConsole(string input, params object[] parmeters)
        {
            if (string.IsNullOrEmpty(input)) return;
            string foo = string.Format(input, parmeters);
            output.Add(foo);
            DrawLayout();
        }

        public void LogMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            output.Add(message);
            DrawLayout();
        }

        /// <summary>
        /// Clears the line to the prompt.
        /// </summary>
        private void ClearLine()
        {
            Console.CursorLeft = promptLength;
            for (int i = promptLength; i < Console.WindowWidth - 1; i++)
                Console.Write(" ");
            Console.CursorLeft = promptLength;
        }

        #region Tab Matching Method

        /// <summary>
        /// Tab completes text based off of internal commands then CommCmd commands.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string Tab(string text)
        {
            Match m = userEntryParamsRegex.Match(text);
            var temp = new List<string> { m.Groups["command"].Value };
            temp.AddRange(from Capture cap in m.Groups["params"].Captures select cap.Value.Trim('"'));
            string[] s = temp.ToArray();

            string result = "";

            if (!text.Contains(" "))
            {
                var matches = from c in GetCommandsCallBack()
                              where c.StartsWith(text)
                              select c;

                result = DoMatch(text, s, matches, true);
            }
            else if ((text.StartsWith("help ") || text.StartsWith("dhelp")) && s.Count() == 2)
            {
                var matches = from c in GetCommandsCallBack()
                              where c.StartsWith(s[1])
                              select c;
                result = DoMatch(text, s, matches, true);
            }
            else if (text.StartsWith("run ") || text.StartsWith("scripts "))
            {
                if (s.Count() == 2)
                {
                    var matches = from x in scripts.Keys
                                  where x.StartsWith(s[1])
                                  select x;

                    result = DoMatch(text, s, matches, true);
                }
            }
            return result;
        }

        /// <summary>
        /// Processes the action taken given the matches to text.
        /// </summary>
        /// <param name="text">The text that was matched.</param>
        /// <param name="s">The parameters from text.</param>
        /// <param name="matches">The matches returned.</param>
        /// <param name="caseSensitive">Mathing on case sensitivity.</param>
        /// <returns>The resulting text from matching.</returns>
        private string DoMatch(string text, string[] s, IEnumerable<string> matches, bool caseSensitive)
        {
            var arrayMatches = matches as string[] ?? matches.ToArray();
            if (arrayMatches.Count() == 1)
            {
                text = WriteMatchToConsole(text, s, arrayMatches.ElementAt(0));
            }
            else if (arrayMatches.Count() > 1 && arrayMatches.Count() <= 20)
            {
                text = WriteMatches(s, arrayMatches, caseSensitive);
            }
            else if (arrayMatches.Count() > 20)
            {
                Console.Write("\nMore than 20 matchs, display them all? ");
                ConsoleKeyInfo k = Console.ReadKey();
                if (k.KeyChar.ToString(CultureInfo.InvariantCulture).ToLower() == "y")
                {
                    text = WriteMatches(s, arrayMatches, caseSensitive);
                }
            }
            return text;
        }

        /// <summary>
        /// Writes a singular match to the console.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="s"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        private string WriteMatchToConsole(string text, string[] s, string match)
        {
            if (text == null) throw new ArgumentNullException("text");
            ClearLine();

            string result = "";

            for (int i = 0; i < s.Count() - 1; i++)
            {
                result += (s[i].Contains(' ') ? "\"" + s[i] + "\"" : s[i]) + " ";
            }
            result += match.Contains(' ') ? "\"" + match + "\"" : match;
            Console.Write(result);
            text = result;

            return text;
        }

        /// <summary>
        /// Writes many matches to the console, and partially completes text if there are partial matches.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="matches"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        private string WriteMatches(string[] s, IEnumerable<string> matches, bool caseSensitive)
        {
            Console.Write("\n\nMatches\n");
            var arrayMatcheds = matches as string[] ?? matches.ToArray();
            for (int i = 0; i < arrayMatcheds.Count(); i++)
            {
                Console.WriteLine(" " + arrayMatcheds.ElementAt(i));
            }
            string partial = PartialComplete(arrayMatcheds, caseSensitive);
            string result = "";

            for (int i = 0; i < s.Count() - 1; i++)
            {
                result += (s[i].Contains(' ') ? "\"" + s[i] + "\"" : s[i]) + " ";
            }
            result += partial;

            Console.Write("\n" + promptName + "> " + result);
            return result;
        }

        /// <summary>
        /// Finds the greatest number of similar characters int the value of matches.
        /// </summary>
        /// <param name="matches"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        private string PartialComplete(IEnumerable<string> matches, bool caseSensitive)
        {
            int minLength = int.MaxValue;
            string text = "";
            var temp = new List<string>();
            bool allHaveSpace = true;

            foreach (string s in matches)
            {
                if
                (s.Length < minLength)
                {
                    minLength = s.Length;
                }
                temp.Add(s.Trim('"'));

                if (!s.Contains('"'))
                {
                    allHaveSpace = false;
                }
            }

            for (int i = 0; i < minLength; i++)
            {
                bool allMatch = true;
                char c = temp.ElementAt(0)[i];
                for (int x = 1; x < temp.Count(); x++)
                {
                    if (!caseSensitive)
                    {
                        if (temp.ElementAt(x)[i].ToString(CultureInfo.InvariantCulture).ToUpper() != c.ToString(CultureInfo.InvariantCulture).ToUpper())
                        {
                            allMatch = false;
                            break;
                        }
                    }
                    else
                    {
                        if (temp.ElementAt(x)[i] != c)
                        {
                            allMatch = false;
                            break;
                        }
                    }
                }
                if (allMatch)
                {
                    text += c;
                }
                else
                {
                    break;
                }
            }

            if (allHaveSpace)
            {
                text = "\"" + text;
            }
            return text;
        }

        #endregion

        #region Client Command Method

        [TypedCommand("loadScript", "loads a script for execution.")]
        public string Load(string filename)
        {
            var r = new Regex(@"\w:\\");

            FileInfo fi = r.IsMatch(filename) ? new FileInfo(filename) : new FileInfo(Environment.CurrentDirectory + "\\" + filename);

            if (!fi.Exists)
            {
                return string.Format("The filename, '{0}', does not extis.", fi.FullName);
            }

            using (var sr = new StreamReader(new FileStream(fi.FullName, FileMode.Open, FileAccess.Read)))
            {
                var script = new List<string>();

                while (!sr.EndOfStream)
                {
                    script.Add(sr.ReadLine());
                }

                scripts[fi.Name] = script;
            }
            return "Done.";
        }

        [TypedCommand("runScript", "Runs a loaded script.")]
        public string Run(string script)
        {
            if (scripts.ContainsKey(script))
            {
                foreach (string c in scripts[script])
                {
                    ExecuteCommand(c);
                }
            }
            else
            {
                return string.Format("Script, '{0}', was not loaded.", script);
            }
            return "";
        }

        [TypedCommand("showScripts", "Shows all scripts.")]
        public string ShowScripts()
        {
            return string.Join("\n", scripts.Keys);
        }

        [TypedCommand("showScript", "Shows the contents of a script.")]
        public string ShowScript(string script)
        {
            if (scripts.ContainsKey(script))
            {
                return string.Join("\n", scripts[script]);
            }
            return string.Format("Script, {0} has not been loaded.", script);
        }

        [TypedCommand("saveHistoryAsScript", "Saves all commands in the history buffer as a script.")]
        public string SaveHistory(string filename)
        {
            var fi = new FileInfo(Environment.CurrentDirectory + "\\" + filename);

            if (fi.Exists)
            {
                return string.Format("File, {0} already exists.", filename);
            }

            using (var sw = new StreamWriter(new FileStream(fi.FullName, FileMode.Create, FileAccess.Write)))
            {
                int count = 0;
                foreach (string c in commandHistory)
                {
                    sw.WriteLine(c);
                    count++;
                }
                return string.Format("{0} commands have been written to {1}", count, filename);
            }
        }

        [TypedCommand("clear", "Clears the screen")]
        public string Clear()
        {
            output.Clear();
            DrawLayout();
            return "";
        }
        #endregion

        #region  ICommandCommunicator
        public event Func<string, List<object>, string> ProcessCommand;

        protected virtual string OnProcessCommand(string arg1, List<object> arg2)
        {
            Func<string, List<object>, string> handler = ProcessCommand;
            if (handler != null) return handler(arg1, arg2);
            return "Process Command is not wired up.";
        }

        public event Func<string, Dictionary<string, object>, string> ProcessCommandNamedArguments;
        public Func<List<string>> GetCommandsCallBack { get; set; }

        protected virtual string OnProcessCommandNamedArguments(string arg1, Dictionary<string, object> arg2)
        {
            Func<string, Dictionary<string, object>, string> handler = ProcessCommandNamedArguments;
            if (handler != null) return handler(arg1, arg2);
            return "Process Command Named arguments is not wired up.";
        }

        #endregion

    }


}
