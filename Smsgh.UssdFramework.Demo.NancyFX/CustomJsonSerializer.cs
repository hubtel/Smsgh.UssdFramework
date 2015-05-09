namespace Smsgh.UssdFramework.Demo.NancyFX
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    public class CustomJsonSerializer : JsonSerializer
    {
        public CustomJsonSerializer()
        {
            this.ContractResolver = new CamelCasePropertyNamesContractResolver();  // DefaultContractResolver(); | SnakeCaseContractResolver(); 
            this.NullValueHandling = NullValueHandling.Ignore;
            this.Formatting = Formatting.Indented;
            this.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            this.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            this.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
            this.Converters.Add(new StringEnumConverter());
        }
    }

    public class SnakeCaseContractResolver : DefaultContractResolver
    {
        public SnakeCaseContractResolver() : base(true) { }

        protected override string ResolvePropertyName(string propertyName)
        {
            return GetSnakeCase(propertyName);
        }

        private string GetSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var buffer = "";

            for (var i = 0; i < input.Length; i++)
            {
                var isLast = (i == input.Length - 1);
                var isSecondFromLast = (i == input.Length - 2);

                var curr = input[i];
                var next = !isLast ? input[i + 1] : '\0';
                var afterNext = !isSecondFromLast && !isLast ? input[i + 2] : '\0';

                buffer += char.ToLower(curr);

                if (!char.IsDigit(curr) && char.IsUpper(next))
                {
                    if (char.IsUpper(curr))
                    {
                        if (!isLast && !isSecondFromLast && !char.IsUpper(afterNext))
                            buffer += "_";
                    }
                    else
                        buffer += "_";
                }

                if (!char.IsDigit(curr) && char.IsDigit(next))
                    buffer += "_";
                if (char.IsDigit(curr) && !char.IsDigit(next) && !isLast)
                    buffer += "_";
            }

            return buffer;
        }
    }
}
