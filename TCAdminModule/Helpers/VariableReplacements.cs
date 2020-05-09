using System.Collections.Generic;
using System.Linq;
using DSharpPlus;
using Nexus;

namespace TCAdminModule.Helpers
{
    public static class VariableReplacements
    {
        private static readonly Logger Logger = new Logger("VariableReplacements");
        
        public static string ReplaceWithVariables(this string original, Dictionary<string, object> variables)
        {
            var variablesSplit = original.Split('{', '}')
                .Where((item, index) => index % 2 != 0).ToList();

            var mod = ModifyVariables(variables, variablesSplit);

            return mod.Aggregate(original, (current, keyValuePair) => current.Replace("{" + keyValuePair.Key + "}", keyValuePair.Value));
        }

        private static Dictionary<string, string> ModifyVariables(IReadOnlyDictionary<string, object> variablesMap,
            IEnumerable<string> variablesToChange)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var variable in variablesToChange)
            {
                if (variable.Contains("."))
                {
                    var split = variable.Split('.');
                    var prefix = split[0];
                    var lookup = split[1];

                    if (variablesMap.ContainsKey(prefix))
                    {
                        dictionary.Add(variable, GetPropValue(variablesMap[prefix], lookup).ToString());
                    }
                }
                else
                {
                    if (variablesMap.ContainsKey(variable))
                    {
                        dictionary.Add(variable, variablesMap[variable].ToString());
                    }
                }
            }

            return dictionary;
        }

        private static object GetPropValue(object src, string propName)
        {
            var propertyInfo = src.GetType().GetProperty(propName);
            return propertyInfo != null ? propertyInfo.GetValue(src, null) : "";
        }
    }
}