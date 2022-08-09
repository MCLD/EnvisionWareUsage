using System;

namespace EnvisionwareLoader
{
    public class MissingConfigurationValueException : Exception
    {
        private static string FormatParamName(string paramName)
        {
            return $"Missing configuration value: '{paramName}'";
        }

        public MissingConfigurationValueException(string paramName)
            : base(FormatParamName(paramName))
        {
        }

        public MissingConfigurationValueException(string paramName, Exception innerException)
            : base(FormatParamName(paramName), innerException)
        {
        }
    }
}
