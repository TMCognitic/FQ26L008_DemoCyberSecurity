using System.Text.RegularExpressions;

namespace Tools.AspNet.Security
{
    public class CorsOptionsBuilder
    {
        private const string HeaderRegexPattern = "^[!#$%&'*+\\-.^_`|~0-9A-Za-z]+$";

        internal string[] Origins { get; private set; } = [];
        internal Methods AllowedMethods { get; private set; } = Methods.Get | Methods.Post | Methods.Put | Methods.Patch | Methods.Delete;
        internal HeaderInfo[] Headers { get; private set; } = [];

        public CorsOptionsBuilder AddOrigins(params Uri[] origins)
        {
            Origins = origins.Where(uri => uri is { Scheme: "https" }).Select(uri => uri.OriginalString).ToArray();
            return this;
        }

        public CorsOptionsBuilder WithMethods(Methods methods)
        {
            AllowedMethods = methods;
            return this;
        }

        public CorsOptionsBuilder AddHeaders(params HeaderInfo[] headers)
        {
            Regex regex = new Regex(HeaderRegexPattern);

            if (headers.Any(h => !regex.IsMatch(h.Name)))
                throw new ArgumentException($"Invalid headers : {string.Join(", ", headers.Where(h => !regex.IsMatch(h.Name)))}");

            Headers = headers;
            return this;
        }
    }
}