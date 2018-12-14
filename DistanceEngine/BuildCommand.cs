using Microsoft.Extensions.CommandLineUtils;
using System;
using static Distance.Engine.Program;

namespace Distance.Engine
{
    internal class BuildCommand
    {
        private Options commonOptions;

        public BuildCommand(Options commonOptions)
        {
            this.commonOptions = commonOptions;
        }


        public static string Name => "build";
        public void Configuration(CommandLineApplication command)
        {
            command.Description = "Build a distance ruleset from the source yaml project.";
            command.HelpOption("-?|-help");

            var sourceProject = command.Option("-source <SourceYamlProject>",
                "A file with the source yaml ruleset project. Multiple values can be specified.",
                CommandOptionType.MultipleValue);

            command.OnExecute(() =>
            {
                throw new NotImplementedException();
                return 0;
            });
        }
    }
}