using System.Text;

namespace rmApplication
{
    /// <summary>
    /// Minimal, dependency-free JSON string helpers. Output is single-line so it can be
    /// sent over the `\r\n`-delimited remote protocol without being split.
    /// </summary>
    public static class Json
    {
        /// <summary>
        /// Escapes a string for embedding inside JSON double quotes. Control characters
        /// (including CR/LF) are escaped, so the result never contains a raw line break.
        /// </summary>
        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var sb = new StringBuilder(value.Length + 8);
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20)
                            sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else
                            sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>Returns a JSON string literal (quoted and escaped) for the value.</summary>
        public static string Str(string value) => "\"" + Escape(value) + "\"";
    }
}
