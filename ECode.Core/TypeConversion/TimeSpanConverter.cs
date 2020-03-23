using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using ECode.Core;

namespace ECode.TypeConversion
{
    using TimeSpanNullable = Nullable<TimeSpan>;


    /// <summary>
    /// Base parser for <see cref="TimeSpanConverter"/> custom specifiers.
    /// </summary>
    abstract class SpecifierParser
    {
        const RegexOptions      REGEX_OPTIONS   = RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.IgnoreCase;


        /// <summary>
        /// Specifier
        /// </summary>
        public abstract string Specifier { get; }

        /// <summary>
        /// Convert int value to a Timespan based on the specifier
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract TimeSpan Parse(int value);


        /// <summary>
        /// Check if the string contains the specifier and 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TimeSpanNullable Match(string value)
        {
            string regex = @"^(\d+)" + Specifier + "$";

            Match match = Regex.Match(value, regex, REGEX_OPTIONS);
            if (!match.Success)
            { return new TimeSpanNullable(); }

            return new TimeSpanNullable(Parse(int.Parse(match.Groups[1].Value)));
        }
    }

    /// <summary>
    /// Recognize 10d as ten days
    /// </summary>
    class DaySpecifier : SpecifierParser
    {
        /// <summary>
        /// Day specifier: d
        /// </summary>
        public override string Specifier
        {
            get { return "d"; }
        }

        /// <summary>
        /// Parse value as days
        /// </summary>
        /// <param name="value">Timespan in days</param>
        /// <returns></returns>
        public override TimeSpan Parse(int value)
        {
            return TimeSpan.FromDays(value);
        }
    }

    /// <summary>
    /// Recognize 10h as ten hours
    /// </summary>
    class HourSpecifier : SpecifierParser
    {
        /// <summary>
        /// Hour specifier: h
        /// </summary>
        public override string Specifier
        {
            get { return "h"; }
        }

        /// <summary>
        /// Parse value as hours
        /// </summary>
        /// <param name="value">Timespan in hours</param>
        /// <returns></returns>
        public override TimeSpan Parse(int value)
        {
            return TimeSpan.FromHours(value);
        }
    }

    /// <summary>
    /// Recognize 10m as ten minutes
    /// </summary>
    class MinuteSpecifier : SpecifierParser
    {
        /// <summary>
        /// Minute specifier: m
        /// </summary>
        public override string Specifier
        {
            get { return "m"; }
        }

        /// <summary>
        /// Parse value as minutes
        /// </summary>
        /// <param name="value">Timespan in minutes</param>
        /// <returns></returns>
        public override TimeSpan Parse(int value)
        {
            return TimeSpan.FromMinutes(value);
        }
    }

    /// <summary>
    /// Recognize 10s as ten seconds
    /// </summary>
    class SecondSpecifier : SpecifierParser
    {
        /// <summary>
        /// Second specifier: s
        /// </summary>
        public override string Specifier
        {
            get { return "s"; }
        }

        /// <summary>
        /// Parse value as seconds
        /// </summary>
        /// <param name="value">Timespan in seconds</param>
        /// <returns></returns>
        public override TimeSpan Parse(int value)
        {
            return TimeSpan.FromSeconds(value);
        }
    }

    /// <summary>
    /// Recognize 10ms as ten milliseconds
    /// </summary>
    class MillisecondSpecifier : SpecifierParser
    {
        /// <summary>
        /// Millisecond specifier: ms
        /// </summary>
        public override string Specifier
        {
            get { return "ms"; }
        }

        /// <summary>
        /// Parse value as milliseconds
        /// </summary>
        /// <param name="value">Timespan in milliseconds</param>
        /// <returns></returns>
        public override TimeSpan Parse(int value)
        {
            return TimeSpan.FromMilliseconds(value);
        }
    }


    /// <summary>
    /// Converts string representation into an instance of <see cref="System.TimeSpan"/>.
    /// </summary>
    public class TimeSpanConverter : System.ComponentModel.TimeSpanConverter
    {
        static readonly SpecifierParser[] Specifiers = {
                                                  new DaySpecifier(),
                                                  new HourSpecifier(),
                                                  new MinuteSpecifier(),
                                                  new SecondSpecifier(),
                                                  new MillisecondSpecifier()
                                              };


        /// <summary>
        /// Can we convert from the sourceType to a <see cref="System.TimeSpan"/> instance?
        /// </summary>
        /// <remarks>
        /// <p>
        /// Currently only supports conversion from a <see cref="System.String"/> value.
        /// </p>
        /// </remarks>
        /// <param name="context">
        /// A <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="sourceType">
        /// A <see cref="System.Type"/> that represents what you want to convert from.
        /// </param>
        /// <returns>True if the conversion is possible.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Convert from a <see cref="System.String"/> value to a <see cref="System.TimeSpan"/> instance.
        /// </summary>
        /// <param name="context">
        /// A <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> to use as the current culture. 
        /// </param>
        /// <param name="value">
        /// The value that is to be converted.
        /// </param>
        /// <returns>
        /// A <see cref="System.TimeSpan"/> if successful. 
        /// </returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string strValue = value as string;
            if (!string.IsNullOrWhiteSpace(strValue))
            {
                try
                {
                    strValue = strValue.Trim();
                    foreach (var specifierParser in Specifiers)
                    {
                        var res = specifierParser.Match(strValue);
                        if (res.HasValue)
                        {
                            return res.Value;
                        }
                    }

                    throw new ArgumentException($"Cannot parse '{value}' to a valid TimeSpan.");
                }
                catch (Exception ex)
                { throw new TypeConvertException(value, typeof(TimeSpan), ex); }
            }
            else
            { throw new TypeConvertException(value, typeof(TimeSpan)); }
        }
    }
}