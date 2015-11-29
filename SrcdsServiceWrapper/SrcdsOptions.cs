using CommandLine;
using CommandLine.Text;

namespace SrcdsServiceWrapper
{
    public class SrcdsOptions
    {
        [Option('u', "update", Required = false, DefaultValue = true, HelpText = "Whether or not to check for updates.")]
        public bool CheckForUpdates { get; set; }

        [Option('r', "restart", Required = false, DefaultValue = true, HelpText = "Whether or not to restart on SRCDS crash.")]
        public bool RestartOnCrash { get; set; }

        [Option('p', "params", Required = true, HelpText = "The parameters used to execute SRCDS.")]
        public string SrcdsParameters { get; set; }

        [Option('s', "srcds", Required = true, HelpText = "The path to the SRCDS file.")]
        public string SrcdsPath { get; set; }

        [Option('c', "steamcmd", Required = false, HelpText = "The path to steamcmd.exe.")]
        public string SteamCmdPath { get; set; }

        [Option('t', "updatescript", Required = false, HelpText = "The script used by the updater.")]
        public string UpdateScriptPath { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}