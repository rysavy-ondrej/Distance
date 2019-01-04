using Microsoft.Extensions.CommandLineUtils;
using NLog;
using System;

namespace Distance.Engine
{
    class Program
    {
        public class Options
        {
            public CommandOption EnableDebug { get; internal set; }
        }

        static void Main(string[] args)
        {

            var commandLineApplication = new CommandLineApplication
            {
                Name = "distance"
            };
            commandLineApplication.HelpOption("-?|-help");

            var options = new Options
            {
                EnableDebug = commandLineApplication.Option("-debug", "Enable debug output.", CommandOptionType.NoValue)
            };

            commandLineApplication.Command(RunCommand.Name, configuration: new RunCommand(options).Configuration);
            commandLineApplication.Command(BuildCommand.Name, configuration: new BuildCommand(options).Configuration);

            commandLineApplication.OnExecute(() => {
                commandLineApplication.Error.WriteLine("Error: Command not specified!");
                commandLineApplication.ShowHelp();
                return 0;
            });

            try
            {
                commandLineApplication.Execute(args);
            }
            catch (CommandParsingException e)
            {
                commandLineApplication.Error.WriteLine($"Error: {e.Message}");
                commandLineApplication.ShowHelp();
            }
            catch(Exception e)
            {
                commandLineApplication.Error.WriteLine($"Error: {e.Message}");
            }
        }
    }
}
