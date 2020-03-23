using System;
using System.Linq.Expressions;

namespace ECode.Data
{
    public static class SqlOrderFunc
    {
        public static T Asc<T>(Expression<Func<T>> expression)
        {
            throw new NotFiniteNumberException();
        }

        public static T Desc<T>(Expression<Func<T>> expression)
        {
            throw new NotFiniteNumberException();
        }
    }
}
