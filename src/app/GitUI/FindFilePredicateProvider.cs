using System.Text;
using System.Text.RegularExpressions;
using GitCommands;

namespace GitUI
{
    public interface IFindFilePredicateProvider
    {
        /// <summary>
        /// Returns the names of files that match the specified search pattern.
        /// </summary>
        /// <param name="searchPattern">The search string to match against the paths of files.</param>
        Func<string?, bool> Get(string searchPattern, string workingDir);
    }

    public sealed class FindFilePredicateProvider : IFindFilePredicateProvider
    {
        public Func<string?, bool> Get(string searchPattern, string workingDir)
        {
            ArgumentNullException.ThrowIfNull(searchPattern);
            ArgumentNullException.ThrowIfNull(workingDir);

            string pattern = searchPattern.ToPosixPath();
            string dir = workingDir.ToPosixPath();
            Regex camelHumpsRegex = BuildRegexCamelHumps(pattern);

            if (pattern.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
            {
                pattern = pattern[dir.Length..].TrimStart('/');
                return fileName => fileName?.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) is true;
            }

            // Method Contains have no override with StringComparison parameter
            return fileName => fileName != null && (fileName.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) is >= 0 || camelHumpsRegex.IsMatch(fileName));
        }

        private Regex BuildRegexCamelHumps(string searchPattern)
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
