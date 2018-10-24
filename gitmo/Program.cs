using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mono.Options;

namespace gitmo
{
    class Program
    {
        static OptionSet options = new OptionSet ();
        static Dictionary<string, string> optionValues = new Dictionary<string, string> ();

        static void AddOption(string name, string description)
        {
            options.Add (name + "=", description, v => optionValues[name] = v);
            optionValues.Add (name, null);
        }

        static async Task Main (string[] args)
        {
            var command = args.FirstOrDefault ();
            if (command == "open-pr") 
            {
                AddOption ("repopath", "Path to the repository");
                AddOption ("repoowner", "The user or org name of the repo where you're opening the PR");
                AddOption ("reponame", "Name of the repository (it doesn't necessarily have to match the path/folder)");
                AddOption ("branch", "The branch that you're merging (in the local repository)");
                AddOption ("name", "Your full name");
                AddOption ("email", "Your email");
                AddOption ("username", "Your username");
                AddOption ("pass", "Your password or personal access token");
                AddOption ("title", "The title of the pull request");
                AddOption ("message", "The message associated with the pull request. This can be markdown");

                try
                {
                    // parse the command line
                    var extra = options.Parse (args.Skip(1));
                }
                catch (OptionException e)
                {
                    // output some error message
                    Console.Write ("error: ");
                    Console.WriteLine (e.Message);
                    return;
                }

                var empties = optionValues.Where (v => string.IsNullOrWhiteSpace (v.Value));
                if (empties.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine ("Required Values (set with `=`): ");
                    foreach(var empty in empties)
                    {
                        Console.WriteLine ($"\t-{empty.Key}: {options[empty.Key].Description}");
                    }
                    Environment.Exit (1);
                }
                else
                {
                    var task = new OpenPR (optionValues);

                    try
                    {
                        await task.Process ();
                    }
                    catch(Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine (ex);
                        Environment.ExitCode = 2;
                    }
                }
            }
            else
            {
                Console.WriteLine ($"unknown command: \"{command}\". Try 'open-pr'");
            }
        }
    }
}
