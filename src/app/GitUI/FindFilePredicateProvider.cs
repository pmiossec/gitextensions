using System.Text;
using System.Text.RegularExpressions;
using GitCommands;

namespace GitUI
{
    ////public enum FoundResult
    ////{
    ////    NotFound = -1, // -1: not matching
    ////    D
    ////    // 0: matching with case
    ////    // 1: matching without case (?)
    ////    // 2: matching with regex
    ////}

    public interface IFindFilePredicateProvider
    {
        /// <summary>
        /// Returns the names of files that match the specified search pattern.
        /// </summary>
        /// <param name="searchPattern">The search string to match against the paths of files.</param>
        Func<string?, int> Get(string searchPattern, string workingDir);
    }

    public sealed class FindFilePredicateProvider : IFindFilePredicateProvider
    {
        public Func<string?, int> Get(string searchPattern, string workingDir)
        {
            ArgumentNullException.ThrowIfNull(searchPattern);
            ArgumentNullException.ThrowIfNull(workingDir);

            string pattern = searchPattern.ToPosixPath();
            string dir = workingDir.ToPosixPath();
            Regex camelHumpsRegex = BuildRegexCamelHumps(pattern);

            if (pattern.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
            {
                pattern = pattern[dir.Length..].TrimStart('/');
                return fileName => fileName?.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) is true ? 0 : -1;
            }

            // Method Contains have no override with StringComparison parameter
            return fileName =>
            {
                if (fileName == null)
                {
                    return -1;
                }

                if (fileName.IndexOf(pattern, StringComparison.Ordinal) is >= 0)
                {
                    return 0;
                }

                if (fileName.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) is >= 0)
                {
                    return 1;
                }

                return camelHumpsRegex.IsMatch(fileName) ? 2 : -1;
            };
        }

        private static Regex BuildRegexCamelHumps(string searchPattern)
        {
            StringBuilder sb = new();
            foreach (char c in searchPattern)
            {
                if (char.IsUpper(c) || c == '/')
                {
                    sb.Append(".*");
                }

                sb.Append(c);
            }

            return new Regex(sb.ToString(), RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking);
        }
    }
}
