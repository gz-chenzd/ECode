using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ECode.Data
{
    public interface ITable<TEntity> : IQuerySet<TEntity>
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity">数据</param>
        /// <returns>true: 成功, false: 失败</returns>
        bool Add(TEntity entity);

        /// <summary>
        /// 当不存在时新增
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="existsCondition">exists的条件</param>
        /// <returns>true: 成功, false: 失败</returns>
        bool AddIfNotExists(TEntity entity, Expression<Func<TEntity, bool>> existsCondition);

        /// <summary>
        /// 批量新增（通过逐条Add）
        /// </summary>
        /// <param name="entities">数据集合</param>
        void AddRange(IEnumerable<TEntity> entities);


        /// <summary>
        /// 局部更新
        /// </summary>
        /// <param name="value">局部数据</param>
        /// <param name="where">更新条件</param>
        /// <returns>返回影响的记录数</returns>
        int Update(object value, Expression<Func<TEntity, bool>> where);

        /// <summary>
        /// 局部更新
        /// </summary>
        /// <param name="value">更新内容</param>
        /// <param name="where">更新条件</param>
        /// <returns>返回影响的记录数</returns>
        int Update(IDictionary value, Expression<Func<TEntity, bool>> where);

        /// <summary>
        /// 局部更新
        /// </summary>
        /// <param name="value">更新内容</param>
        /// <param name="where">更新条件</param>
        /// <returns>返回影响的记录数</returns>
        int Update(Expression<Func<TEntity, object>> value, Expression<Func<TEntity, bool>> where);


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="where">删除条件</param>
        /// <returns>返回删除的记录数</returns>
        int Delete(Expression<Func<TEntity, bool>> where);
    }
}
