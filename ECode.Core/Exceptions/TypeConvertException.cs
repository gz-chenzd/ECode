using System;

namespace ECode.Core
{
    public class TypeConvertException : Exception
    {
        public TypeConvertException(string message)
            : base(message)
        {

        }

        public TypeConvertException(string message, Exception innerException)
            : base(message, innerException)
        {

        }


        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        /// <param name="sourceValue">
        /// The value which to be converted.
        /// </param>
        /// <param name="targetType">
        /// A <see cref="System.Type"/> that represents what you want to convert to.
        /// </param>
        public TypeConvertException(object sourceValue, Type targetType)
            : this(sourceValue, targetType, null)
        {

        }

        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        /// <param name="sourceValue">
        /// The value that is to be converted.
        /// </param>
        /// <param name="targetType">
        /// A <see cref="System.Type"/> that represents what you want to convert to.
        /// </param>
        /// <param name="innerException">
        /// The root exception that is being wrapped.
        /// </param>
        public TypeConvertException(object sourceValue, Type targetType, Exception innerException)
            : base(BuildMessage(sourceValue, targetType), innerException)
        {

        }


        private static string BuildMessage(object sourceValue, Type targetType)
        {
            return $"Cannot convert source value '{sourceValue}' [type '{sourceValue.GetType().FullName}'] to target type '{targetType.FullName}'.";
        }
    }
}