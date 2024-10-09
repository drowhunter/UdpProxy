using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace UDPProxy.CommandLine
{

    public class CommandLineParser<T> where T:class , new()
    {
        private static BindingFlags _flags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public;
        public string[] Args { get; } = [];


        public static T Parse(string[] args)
        {
           
            try
            {
                var argDic = string.Join(' ', args)
                    .Split("--", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToDictionary(_ => KebabToPascal(_.Split(' ')[0]), _ =>
                    {
                        var x = _.Split(' ', 2,StringSplitOptions.RemoveEmptyEntries);
                        return ParseValue(x.Length > 1 ? x[1] : null);
                    }) ;

                if (argDic.ContainsKey("Help"))
                {
                    Console.WriteLine(Usage());
                    Environment.Exit(0);
                }

                if (!ValidateArgs(argDic, out var errors))
                {
                    Console.WriteLine(Usage(errors.ToArray()));
                    Environment.Exit(0);
                }
                
                string json = JsonConvert.SerializeObject(argDic, Formatting.Indented);

                Console.WriteLine(json);                

                return JsonConvert.DeserializeObject<T>(json) ?? throw new InvalidOperationException("Deserialization returned null");

            }
            catch(Exception x)
            {
               Console.WriteLine(Usage());
                throw;
            }
        }

        private static bool ValidateArgs(Dictionary<string,object?> args, out List<string> invalidArgs)
        {
            var requiredProps = typeof(T).GetProperties(_flags).Where(pi => pi.GetCustomAttribute<RequiredAttribute>() != null).Select(_ => _.Name).ToList();

            var missingProps = requiredProps.Where(_ => !args.ContainsKey(_)).Select((s,i) => PascalToKebab(s)).ToList();

            invalidArgs = new List<string>();

            if (missingProps.Any())
            {
                
                foreach (var prop in missingProps)
                {
                    invalidArgs.Add(prop);
                    Console.WriteLine($"Missing required argument: {PascalToKebab(prop)}");
                }
               
                return false;
            }

            var noValues = args.Where(a => requiredProps.Contains(a.Key) && a.Value == null).Select(_ => _.Key).ToList();

            if (noValues.Any())
            {
                foreach (var prop in noValues)
                {
                    invalidArgs.Add(prop);
                    Console.WriteLine($"Missing required value for argument: {prop}");
                }
                return false;
            }




            return true;

        }

        private static object? ParseValue(string? val) => val switch
        {
            string s when s.Contains(',') => s.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Select(ParseValue).ToArray(),
            string s when bool.TryParse(s, out bool i) => i,
            string s when int.TryParse(s, out int i) => i,
            string s when double.TryParse(s, out double i) => i,            
            string s when s.StartsWith('"') && s.EndsWith('"') => s.Trim('"'),
            null => null,
            _ => val
        };

        public override string ToString() => Usage();

        /// <summary>
        /// Show usage for the arguments
        /// </summary>
        /// <param name="filter">only show those in this list</param>
        /// <returns></returns>
        private static string Usage(params string[] filter)
        {
            var props = typeof(T).GetProperties(_flags);

            var sb = new StringBuilder();

            foreach (var prop in props.Where(p => filter.Any(_ => _ ==  p.Name)))
            {
                sb.Append($"{PascalToKebab(prop.Name)} ");

                switch (prop.PropertyType)
                {
                    case Type t when t == typeof(bool):
                        sb.Append(isRequired(prop, ""));
                        break;
                    case Type t when t == typeof(double):
                        sb.Append(isRequired(prop, "<double>"));
                        break;
                    case Type t when t == typeof(int):
                        sb.Append(isRequired(prop, "<int>"));
                        break;
                    case Type t when t == typeof(int[]):
                        sb.Append(isRequired(prop, "<int,int,...>"));
                        break;
                    case Type t when t == typeof(string):
                        sb.Append(isRequired(prop, "<string>"));
                        break;
                }

                var d = prop.GetCustomAttribute<DescriptionAttribute>()?.Description;
                if (d != null)
                {
                    sb.AppendFormat("\t\t\t{0}", d);
                }
                
                sb.AppendLine();
                
            }

            string isRequired(PropertyInfo prop, string val)=> (prop.GetCustomAttribute<RequiredAttribute>() == null ? val : $"{val} (required)") + " ";
            

            return sb.ToString();
        }

        private static string PascalToKebab(string pascal) => string.Concat(pascal.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLower();


        private static string KebabToPascal(string kebab) => string.Join("", kebab.Split('-').Select(_ => _.Substring(0, 1).ToUpper() + _.Substring(1)));
    }
}
