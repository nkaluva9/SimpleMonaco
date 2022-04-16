using System.Windows;

namespace SimpleMonaco
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static CommandLineArgument Argument { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Argument = CommandLineArgument.FromArgument(e.Args);
        }
    }

    public class CommandLineArgument
    {
        public string FilePath { get; }

        public string Language { get; }

        private CommandLineArgument() : this(string.Empty, string.Empty) { }

        public CommandLineArgument(string filePath, string language)
        {
            FilePath = filePath;
            Language = language;
        }

        public static CommandLineArgument FromArgument(string[] args)
        {
            if (args.Length == 0)
            {
                return new CommandLineArgument();
            }
            else if (args.Length == 1)
            {
                return new CommandLineArgument(args[0], string.Empty);
            }
            else
            {
                return new CommandLineArgument(args[0], args[1]);
            }
        }
    }
}
