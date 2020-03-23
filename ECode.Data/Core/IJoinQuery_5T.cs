using System;
using System.Linq.Expressions;

namespace ECode.Data
{
    public interface IJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        /// <summary>
        /// select选择
        /// </summary>
        IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression);

        /// <summary>
        /// where过滤
        /// </summary>
        IJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> Where(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, bool>> whereExpression);

        /// <summary>
        /// group by聚合
        /// </summary>
        IJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> GroupBy(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, object[]>> groupByExpression);

        /// <summary>
        /// order by排序
        /// </summary>
        IJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> OrderBy(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, object[]>> orderByExpression);
    }


    public interface IJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        /// <summary>
        /// select选择
        /// </summary>
        IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression);

        /// <summary>
        /// where过滤
        /// </summary>
        IJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> Where(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, bool>> whereExpression);

        /// <summary>
        /// group by聚合
        /// </summary>
        IJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> GroupBy(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, object[]>> groupByExpression);

        /// <summary>
        /// order by排序
        /// </summary>
        IJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> OrderBy(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, object[]>> orderByExpression);
    }


    public interface IJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="count">记录数</param>
        IJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> Paging(uint offset, uint count);

        /// <summary>
        /// select选择
        /// </summary>
        IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression);
    }


    public interface IJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        /// <summary>
        /// select选择
        /// </summary>
        IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression);
    }


    public interface IJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        /// <summary>
        /// select选择
        /// </summary>
        IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression);

        /// <summary>
        /// having过滤
        /// </summary>
        IJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> Having(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, bool>> havingExpression);

        /// <summary>
        /// order by排序
        /// </summary>
        IJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> OrderBy(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, object[]>> orderByExpression);
    }


    public interface IJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        /// <summary>
        /// select选择
        /// </summary>
        IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression);

        /// <summary>
        /// order by排序
        /// </summary>
        IJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> OrderBy(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, object[]>> orderByExpression);
    }


    public interface IJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        /// <summary>
        /// select选择
        /// </summary>
        IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression);
    }
}