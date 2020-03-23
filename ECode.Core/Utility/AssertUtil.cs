using System;
using System.Collections;
using System.Net;

namespace ECode.Utility
{
    public static class AssertUtil
    {
        /// <summary>
        /// Checks the specified value and throws an
        /// <see cref="System.ArgumentNullException"/> if it is null.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified value is null.
        /// </exception>
        public static void ArgumentNotNull(object value, string name)
        {
            ArgumentNotNull(value, name, $"Argument '{name}' cannot be null.");
        }

        /// <summary>
        /// Checks the specified value and throws an
        /// <see cref="System.ArgumentNullException"/> if it is null.
        /// </summary>
        /// <param name="value">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentNullException"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified value is null.
        /// </exception>
        public static void ArgumentNotNull(object value, string name, string message)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name, message);
            }
        }

        /// <summary>
        /// Checks the specified string value and throws an
        /// <see cref="System.ArgumentNullException"/> if it is null or
        /// contains only whitespace character(s).
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified string value is null or contains only whitespace character(s).
        /// </exception>
        public static void ArgumentNotEmpty(string value, string name)
        {
            ArgumentNotEmpty(value, name, $"Argument '{name}' cannot be null or empty.");
        }

        /// <summary>
        /// Checks the specified string value and throws an
        /// <see cref="System.ArgumentNullException"/> if it is null or
        /// contains only whitespace character(s).
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentNullException"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified string value is null or contains only whitespace character(s).
        /// </exception>
        public static void ArgumentNotEmpty(string value, string name, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(message, name);
            }
        }

        /// <summary>
        /// Checks the specified <see cref="ICollection"/> value and throws
        /// an <see cref="ArgumentNullException"/> if it is null or contains no elements.
        /// </summary>
        /// <param name="value">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified <paramref name="value"/> is null or contains no elements.
        /// </exception>
        public static void ArgumentNotEmpty(ICollection value, string name)
        {
            ArgumentNotEmpty(value, name, $"Argument '{name}' cannot be null or empty elements.");
        }

        /// <summary>
        /// Checks the specified <see cref="ICollection"/> value and throws
        /// an <see cref="ArgumentNullException"/> if it is null or contains no elements.
        /// </summary>
        /// <param name="value">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown 
        /// <see cref="System.ArgumentNullException"/>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the specified <paramref name="value"/> is null or contains no elements.
        /// </exception>
        public static void ArgumentNotEmpty(ICollection value, string name, string message)
        {
            if (value == null || value.Count == 0)
            {
                throw new ArgumentNullException(name, message);
            }
        }

        /// <summary>
        /// Checks whether the specified value can be cast into the <paramref name="requiredType"/>.
        /// </summary>
        /// <param name="value">The argument to check.</param>
        /// <param name="requiredType">The required type for the argument.</param>
        /// <param name="name">The name of the argument to check.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentException"/>.
        /// </param>
        public static void AssertArgumentType(object value, Type requiredType, string name, string message)
        {
            if (value != null && requiredType != null && !requiredType.IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException(message, name);
            }
        }

        /// <summary>
        /// Checks whether the port is in valid range [0-65535].
        /// </summary>
        /// <param name="port">The argument to check.</param>
        /// <param name="name">The name of the argument to check.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If the specified <paramref name="port"/> is not in valid range.
        /// </exception>
        public static void AssertNetworkPort(int port, string name)
        {
            AssertNetworkPort(port, name, $"Argument '{name}' value must be >= {IPEndPoint.MinPort} and <= {IPEndPoint.MaxPort}.");
        }

        /// <summary>
        /// Checks whether the port is in valid range [0-65535].
        /// </summary>
        /// <param name="port">The argument to check.</param>
        /// <param name="name">The name of the argument to check.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentOutOfRangeException"/>.
        /// </param> 
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If the specified <paramref name="port"/> is not in valid range.
        /// </exception>
        public static void AssertNetworkPort(int port, string name, string message)
        {
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException(name, message);
            }
        }
    }
}