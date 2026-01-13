using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Speech.Recognition;
using System.Threading;

namespace Game
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("do speak-mode? (yes/no)");
            if (Console.ReadLine() == "yes")
            {
                Util.Out.speak = true;

                Console.WriteLine("Options you should speak for are indicated by a *, and a ¬ for manual selection.");
            }

            Util.Out.Options("erm question", new List<string> { "hello", "test", "this is a sentence" });
            Util.Out.Type("The program, has finished.");
        }
    }
}

namespace Game.Util
{
    static class Voice
    {
        public static string Detect(List<string> toFind)
        {
            using (SpeechRecognitionEngine rec = new SpeechRecognitionEngine())
            {
                GrammarBuilder gb = new GrammarBuilder(new Choices(toFind.ToArray()));
                Grammar g = new Grammar(gb);

                rec.LoadGrammar(g);
                rec.SetInputToDefaultAudioDevice();

                bool found = false;

                while (!found)
                {
                    RecognitionResult result = rec.Recognize();

                    if (result != null && result.Confidence > 0.83)
                    {
                        found = true;
                        return result.Text;
                    }
                }
                return null;
            }
        }
    }
    public static class Out
    {
        private const string punct = ",.!?-";
        private const int base_speed = 69;
        private const int punct_multiplier = 3;

        public static bool speak = false;

        public static void Type(string toType, int delay = base_speed, bool new_line = true)
        {
            // Animate chars
            foreach(char c in toType)
            {
                Console.Write(c);
                Thread.Sleep(punct.Contains(c) ? delay * punct_multiplier : delay);
            } 
            // Newline flag
            if (new_line) Console.WriteLine();
        }

        public static string Options(string text, List<string> options, bool clear = false, bool speakOverride = false, bool doSpeak = false, bool debug_selected = false)
        {
            if (speak && !speakOverride || speakOverride && doSpeak)
            {
                return VoiceOptions(text, options, clear, debug_selected);
            }
            else
            {
                return TextOptions(text, options, clear, debug_selected);
            }
        }

        public static string TextOptions(string text, List<string> options, bool clear = false, bool debug_selected = false)
        {
            Console.CursorVisible = false;

            if (clear) Console.Clear();

            int selected = 0;
            bool first = true;
            bool done = false;

            int y = Console.GetCursorPosition().Top;

            while (!done)
            {
                Console.SetCursorPosition(y, 0);
                for (int i = 0; i < options.Count; i++) { Console.WriteLine(); }
                Console.SetCursorPosition(y, 0);

                if (first) Type(text + " ¬"); else Console.WriteLine(text + " ¬");

                for (int i = 0; i < options.Count; i++)
                {
                    if (first)
                    {
                        Type(i == selected ? " > " + options[i] : "   " + options[i]);
                    }
                    else
                    {
                        Console.WriteLine(i == selected ? " > " + options[i] : "   " + options[i]);
                    }
                }

                if (first) first = false;

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.W:
                        if (selected == 0) selected = options.Count - 1;
                        else selected--;
                        break;
                    case ConsoleKey.S:
                        if (selected == options.Count - 1) selected = 0;
                        else selected++;
                        break;
                    case ConsoleKey.Enter:
                        done = true;
                        break;
                }
            }

            Console.CursorVisible = true;
            if (debug_selected) Console.WriteLine($"Selected: {options[selected]}");
            return options[selected];
        }

        public static string VoiceOptions(string text, List<string> options, bool clear = false, bool debug_selected = false)
        {
            Console.CursorVisible = false;

            if (clear) Console.Clear();

            Type(text + " *");

            for (int i = 0; i < options.Count; i++)
            {
                Type("   " + options[i]);
            }

            Console.CursorVisible = true;
            string selected = Voice.Detect(options);
            if (debug_selected) Console.WriteLine($"Selected: {selected}");
            return selected;
        }
    }
}
