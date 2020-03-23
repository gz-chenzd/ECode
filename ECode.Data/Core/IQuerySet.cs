using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ECode.Data
{
    public interface IQuerySet
    {
        ISession Session { get; }
    }

    public interface IQuerySet<TEntity> : IQuerySet
    {
        /// <summary>
        /// 获取记录数
        /// </summary>
        int Count();

        /// <summary>
        /// 获取第一条
        /// </summary>
        TEntity First();

        /// <summary>
        /// 获取记录列表
        /// </summary>
        IList<TEntity> ToList();


        /// <summary>
        /// in条件过滤
        /// </summary>
        bool Contains(TEntity value);

        /// <summary>
        /// exists条件过滤
        /// </summary>
        bool Exists(Expression<Func<TEntity, bool>> expression);


        /// <summary>
        /// distinct去重
        /// </summary>
        IQuerySet<TEntity> Distinct();

        /// <summary>
        /// select选择
        /// </summary>
        IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TSelect>> selectExpression);

        /// <summary>
        /// where过滤
        /// </summary>
        IQuerySet<TEntity> Where(Expression<Func<TEntity, bool>> whereExpression);

        /// <summary>
        /// group by聚合
        /// </summary>
        IGroupedResult<TEntity> GroupBy(Expression<Func<TEntity, object[]>> groupByExpression);

        /// <summary>
        /// order by排序
        /// </summary>
        ISortedQuerySet<TEntity> OrderBy(Expression<Func<TEntity, object[]>> orderByExpression);


        /// <summary>
        /// join连接
        /// </summary>
        IJoinedResult<TEntity, TJoin1> Join<TJoin1>(Expression<Func<TEntity, TJoin1, bool>> onExpression);

        /// <summary>
        /// join连接
        /// </summary>
        IJoinedResult<TEntity, TJoin1> Join<TJoin1>(object partitionObject, Expression<Func<TEntity, TJoin1, bool>> onExpression);

        /// <summary>
        /// join连接
        /// </summary>
        IJoinedResult<TEntity, TJoin1> Join<TJoin1>(IQuerySet<TJoin1> querySet, Expression<Func<TEntity, TJoin1, bool>> onExpression);

        /// <summary>
        /// left join连接
        /// </summary>
        IJoinedResult<TEntity, TJoin1> LeftJoin<TJoin1>(Expression<Func<TEntity, TJoin1, bool>> onExpression);

        /// <summary>
        /// left join连接
        /// </summary>
        IJoinedResult<TEntity, TJoin1> LeftJoin<TJoin1>(object partitionObject, Expression<Func<TEntity, TJoin1, bool>> onExpression);

        /// <summary>
        /// left join连接
        /// </summary>
        IJoinedResult<TEntity, TJoin1> LeftJoin<TJoin1>(IQuerySet<TJoin1> querySet, Expression<Func<TEntity, TJoin1, bool>> onExpression);


        /// <summary>
        /// union合并
        /// </summary>
        IQuerySet<TEntity> Union(IQuerySet<TEntity> querySet);

        /// <summary>
        /// union all合并
        /// </summary>
        IQuerySet<TEntity> UnionAll(IQuerySet<TEntity> querySet);
    }


    public interface ISortedQuerySet<TEntity> : IQuerySet<TEntity>
    {
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="count">记录数</param>
        IQuerySet<TEntity> Paging(uint offset, uint count);
    }


    public interface IGroupedResult<TEntity>
    {
        /// <summary>
        /// select选择
        /// </summary>
        IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TSelect>> selectExpression);

        /// <summary>
        /// having聚合
        /// </summary>
        IGroupHavingResult<TEntity> Having(Expression<Func<TEntity, bool>> havingExpression);

        /// <summary>
        /// order by排序
        /// </summary>
        IGroupSortedResult<TEntity> OrderBy(Expression<Func<TEntity, object[]>> orderByExpression);
    }


    public interface IGroupHavingResult<TEntity>
    {
        /// <summary>
        /// select选择
        /// </summary>
        IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TSelect>> selectExpression);

        /// <summary>
        /// order by排序
        /// </summary>
        IGroupSortedResult<TEntity> OrderBy(Expression<Func<TEntity, object[]>> orderByExpression);
    }


    public interface IGroupSortedResult<TEntity>
    {
        /// <summary>
        /// select选择
        /// </summary>
        IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TSelect>> selectExpression);
    }
}
