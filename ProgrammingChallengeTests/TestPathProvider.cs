using ProgrammingChallenge.Helpers;
using System.IO;

namespace ProgrammingChallengeTests
{
    public class TestPathProvider : IPathProvider
    {
        public string MapPath(string path)
        {
            return Path.Combine(@"C:\project\", path);
        }
    }
}
