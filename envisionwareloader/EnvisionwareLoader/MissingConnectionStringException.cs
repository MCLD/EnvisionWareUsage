using System;

namespace EnvisionwareLoader
{
    internal class MissingConnectionStringException : MissingConfigurationValueException
    {
        private static string FormatParamName(string paramName)
        {
            return $"Missing connection string value: '{paramName}'";
        }

        public MissingConnectionStringException(string paramName)
            : base(FormatParamName(paramName))
        {
        }

        public MissingConnectionStringException(string paramName, Exception innerException)
            : base(FormatParamName(paramName), innerException)
        {
        }
    }
}
