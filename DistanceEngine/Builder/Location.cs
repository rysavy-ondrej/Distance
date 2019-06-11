using YamlDotNet.Core;

namespace Distance.Engine.Builder
{
    public class Location
    {
        internal static Location Empty => new Location(string.Empty, Mark.Empty, Mark.Empty);

        public string FilePath { get; }
        public Mark Start { get; }
        public Mark End { get; }

        public Location(string filePath, Mark start, Mark end)
        {
            FilePath = filePath;
            Start = start;
            End = end;
        }
    }

}