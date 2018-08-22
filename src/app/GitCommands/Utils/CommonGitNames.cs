namespace GitExtUtils
{
    public class CommonGitNames
    {
        public const string Origin = "origin";
        public const string Fork = "fork";
        public const string Upstream = "upstream";

        public static string[] Remotes = [Upstream, Fork, Origin, "remote", "internal"];

        public const string Master = "master";
        public const string Main = "main";
        public const string Dev = "dev";
        public const string Trunk = "trunk";

        public static string[] Locals = [Master, Main, Dev, Trunk];
    }
}
