using System;

namespace ECode.Core
{
    public class ConfigurationException : Exception
    {
        /// <summary>
        /// The source config which causes this error.
        /// </summary>
        public string ConfigText
        { get; private set; }


        public ConfigurationException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="config">
        /// The source config which causes this error.
        /// </param>
        public ConfigurationException(string message, string config)
            : base(message)
        {
            this.ConfigText = config;
        }
    }
}