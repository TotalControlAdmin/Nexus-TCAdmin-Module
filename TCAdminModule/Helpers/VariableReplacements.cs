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
            foreach (var keyValuePair in mod)
            {
                Logger.LogMessage(LogLevel.Critical, $"{keyValuePair.Key} should be changed to {keyValuePair.Value}");
                original = original.Replace("{" + keyValuePair.Key + "}", keyValuePair.Value);
            }

            return original;
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
            Logger.LogDebugMessage("Looking up " + propName + " from " + src);
            var propertyInfo = src.GetType().GetProperty(propName);
            if (propertyInfo != null)
            {
                Logger.LogDebugMessage("Found! " + propName);
                return propertyInfo.GetValue(src, null);
            }

            Logger.LogDebugMessage("Could not find: " + propName + " in variables.");

            return "";
        }
    }
}