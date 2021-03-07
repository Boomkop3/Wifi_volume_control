using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static not_a_keylogger.UserKeyInfo;
using Boomkop3;
using System.IO;

namespace not_a_keylogger
{
    class Program {
        private static void printUserKeys()
        {
            Enum.GetNames(typeof(UserKeys)).ForEach(key => Console.WriteLine($" -> {key}"));
        }
        private static void printHelp()
        {
            Console.WriteLine(
                @"not_a_keylogger V0.1
    this application listens to keys and performs actions when pressed/released, could be used as a simple keylogger

usage: 

not_a_keylogger.exe --help
    -> shows this screen
not_a_keylogger.exe --list
    -> lists all key names
not_a_keylogger.exe --bind [key] [released/pressed] [action] [arguments]
    -> binds the specific key up/down event to an program to launch
example:
    "
     + "-> not_a_keylogger.exe --bind VOLUME_MUTE DOWN \"C:\\apps\\mute.exe\" \" \"");
        }
        private static (string[][] slices, Action onInvalid) checkArguments(string[] args)
        {
            List<string[]> slices = new List<string[]>();
            if (args.Length == 0) return (null, printHelp);
            if (args.Length == 1 & args[0] == "--list") return (null, printUserKeys);
            if (args.Length < 5 | (args.Length % 5) != 0) return (null, printHelp);
            for (int i = 0; i < args.Length; i++)
            {
                int startSlice = i;
                if (args[i++].ToLower() != "--bind") return (null, printHelp);
                if (!chars.ContainsKey((UserKeys)Enum.Parse(typeof(UserKeys), args[i++].ToUpper()))) return (null, printHelp);
                if (!Enum.GetNames(typeof(KeyState)).Contains(args[i++].ToUpper())) return (null, printHelp);
                // if (!File.Exists(args[i++])) return (false, printHelp); // doesn't search PATH
                while (args.Length > i)
                {
                    if (args[i] == "--bind")
                    {
                        --i;
                        break;
                    }
                    i++;
                }
                slices.Add(args.Skip(startSlice).Take(i + 1).ToArray());
            }
            return (slices.ToArray(), null);
        }
        private static ((Process pressed, Process released) actions, ChangeResponder<UserKeys> change)[] parseParams(string[] _args)
        {
            var input = checkArguments(_args);
            if (input.slices == null)
            {
                input.onInvalid();
                return null;
            }
            var bindings = new Dictionary<UserKeys, ((Process pressed, Process released) actions, ChangeResponder<UserKeys> change)>();
            foreach (string[] args in input.slices)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    i++;
                    UserKeys key = (UserKeys)Enum.Parse(typeof(UserKeys), args[i++]);
                    KeyState state = (KeyState)Enum.Parse(typeof(KeyState), args[i++].ToUpper());
                    string filepath = args[i++];
                    string arguments = args[i++];
                    var binding = bindings.ContainsKey(key) ? bindings[key] : ((null, null), new ChangeResponder<UserKeys>(key));
                    Process process = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = filepath,
                            WorkingDirectory = new FileInfo(filepath).DirectoryName,
                            Arguments = arguments,
                            CreateNoWindow = true,
                            UseShellExecute = false
                        }
                    };
                    switch (state)
                    {
                        case KeyState.RELEASED:
                            binding.actions = (binding.actions.pressed, process);
                            break;
                        case KeyState.PRESSED:
                            binding.actions = (process, binding.actions.released);
                            break;
                    }
                    bindings.Add(key, binding);
                }
            }
            return bindings.ToArray().Select(pair => pair.Value).ToArray();
        }
        static void Main(params string[] _args)
        {
            var args = parseParams(_args);
            if (args != null) run(args);
        }
        private static void PerformAction(KeyState state, UserKeys key, Process process)
        {
            Console.WriteLine($"->  {key.ToString()} {state.ToString()}");
            Console.WriteLine($"    -> running {process.StartInfo.FileName}");
            try
            {
                process.Start();
            } 
            catch (Exception ex)
            {
                Console.WriteLine("\t!!\t\tERROR\t\t!!");
                Console.WriteLine(ex.Message);
            }
        }
        static void run(((Process pressed, Process released) actions, ChangeResponder<UserKeys> change)[] bindings)
        {
            /*
            ParallelQuery<((Action pressedAction, Action releasedAction) actions, ChangeResponder<UserKeys> change)> keys = ((UserKeys[])Enum.GetValues(typeof(UserKeys)))
                .AsParallel()
                .Where(key => !IsKeyPushedDown(key))
                .Select(key => (getKeyActions(key), new ChangeResponder<UserKeys>(key)))
                .ToArray().AsParallel();
            */
            Console.WriteLine($"running with {bindings.Length} keys registered");
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(5);
                    bindings.ForAll(key =>
                    {
                        bool isDown = KeyCheck.IsKeyPushedDown(key.change.getObject());
                        var keyPress = key.change.check(isDown);
                        var check = key.change.check(isDown);
                        if (keyPress.pressed & key.actions.pressed != null) 
                            PerformAction(KeyState.PRESSED, key.change.getObject(), key.actions.pressed);
                        if (keyPress.released & key.actions.released != null) 
                            PerformAction(KeyState.RELEASED, key.change.getObject(), key.actions.released);
                    });
                }
            });
            while (true)
            {
                Thread.Sleep(int.MaxValue);
            }
        }
    }
}
