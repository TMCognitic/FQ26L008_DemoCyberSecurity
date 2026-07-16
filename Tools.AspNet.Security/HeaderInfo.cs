namespace Tools.AspNet.Security
{
    public record HeaderInfo(string Name, Func<string, bool>? Validator = null);
}