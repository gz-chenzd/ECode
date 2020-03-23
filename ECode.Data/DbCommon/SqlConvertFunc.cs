using System;
using System.Linq.Expressions;

namespace ECode.Data
{
    public static class SqlConvertFunc
    {
        public static T IfNull<T>(object srcValue, T newValue)
        {
            throw new NotImplementedException();
        }

        public static int ToInt(Expression<Func<string>> expression)
        {
            throw new NotImplementedException();
        }

        public static long ToLong(Expression<Func<string>> expression)
        {
            throw new NotImplementedException();
        }

        public static string ToShortDate(Expression<Func<DateTime>> expression)
        {
            throw new NotImplementedException();
        }

        public static string ToShortDate(Expression<Func<DateTime?>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
