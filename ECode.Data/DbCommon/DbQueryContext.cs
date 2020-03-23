using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using ECode.Utility;

namespace ECode.Data
{
    public class TableInfo
    {
        public string ShortName
        { get; private set; }


        public string TableName
        { get; private set; }

        public Type EntityType
        { get; private set; }

        public EntitySchema EntitySchema
        { get; private set; }


        public DbQueryContext SubQuery
        { get; private set; }


        public TableInfo(DbQueryContext queryContext, string shortName)
        {
            this.SubQuery = queryContext;
            this.ShortName = shortName;
        }

        public TableInfo(string tableName, string shortName, Type entityType, EntitySchema entitySchema)
        {
            this.TableName = tableName;
            this.ShortName = shortName;
            this.EntityType = entityType;
            this.EntitySchema = entitySchema;
        }
    }


    public enum JoinMode
    {
        Join,

        LeftJoin,
    }

    public class JoinTarget
    {
        public JoinMode Mode
        { get; private set; }

        public TableInfo TableInfo
        { get; private set; }

        public LambdaExpression OnExpression
        { get; private set; }


        public JoinTarget(JoinMode mode, TableInfo tableInfo, LambdaExpression onExpression)
        {
            this.Mode = mode;
            this.TableInfo = tableInfo;
            this.OnExpression = onExpression;
        }
    }


    public enum UnionMode
    {
        Union,

        UnionAll,
    }

    public class UnionTarget
    {
        public UnionMode Mode
        { get; private set; }

        public DbQueryContext Query
        { get; private set; }

        public string ShortName
        { get; set; }


        public UnionTarget(UnionMode mode, DbQueryContext queryContext, string shortName)
        {
            this.Mode = mode;
            this.Query = queryContext;
            this.ShortName = shortName;
        }
    }


    public class PagingInfo
    {
        public uint Offset
        { get; private set; }

        public uint Count
        { get; private set; }


        public PagingInfo(uint offset, uint count)
        {
            this.Offset = offset;
            this.Count = count;
        }
    }


    public enum DbQueryAction
    {
        None,

        SetDistinct,

        SetSelect,

        SetFrom,

        SetJoin,

        SetWhere,

        SetGroupBy,

        SetHaving,

        SetOrderBy,

        SetPaging,

        SetUnion,
    }

    public class DbQueryContext
    {
        private DbQueryAction       m_pLastAction  = DbQueryAction.None;


        internal DbQueryContext(DbSession session)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));

            this.Session = session;
        }


        public DbSession Session
        { get; private set; }

        public bool Distinct
        { get; private set; }

        public bool SelectFirst
        { get; internal set; }

        public PagingInfo PagingInfo
        { get; private set; }

        public List<TableInfo> FromTables
        { get; private set; } = new List<TableInfo>();

        public LambdaExpression SelectExpression
        { get; private set; }

        public List<LambdaExpression> WhereExpressions
        { get; private set; } = new List<LambdaExpression>();

        public LambdaExpression GroupByExpression
        { get; private set; }

        public LambdaExpression HavingExpression
        { get; private set; }

        public List<LambdaExpression> OrderByExpressions
        { get; private set; } = new List<LambdaExpression>();

        public List<JoinTarget> JoinTargets
        { get; private set; } = new List<JoinTarget>();

        public List<UnionTarget> UnionTargets
        { get; private set; } = new List<UnionTarget>();


        public void SetDistinct()
        {
            ValidateActionStateChange(DbQueryAction.SetDistinct);

            m_pLastAction = DbQueryAction.SetDistinct;
            this.Distinct = true;
        }

        public void SetPaging(uint offset, uint count)
        {
            ValidateActionStateChange(DbQueryAction.SetPaging);

            m_pLastAction = DbQueryAction.SetPaging;
            this.PagingInfo = new PagingInfo(offset, count);
        }

        public void SetSelect(LambdaExpression selectExpression)
        {
            ValidateActionStateChange(DbQueryAction.SetSelect);

            m_pLastAction = DbQueryAction.SetSelect;
            this.SelectExpression = selectExpression;
        }

        public void SetFrom<TEntity>(DbTable<TEntity> table)
        {
            ValidateActionStateChange(DbQueryAction.SetFrom);

            m_pLastAction = DbQueryAction.SetFrom;
            this.FromTables.Add(new TableInfo(table.TableName, this.Session.CreateTempTableName(), typeof(TEntity), table.Schema));
        }

        public void SetFrom(DbQueryContext queryContext)
        {
            ValidateActionStateChange(DbQueryAction.SetFrom);

            m_pLastAction = DbQueryAction.SetFrom;
            this.FromTables.Add(new TableInfo(queryContext, this.Session.CreateTempTableName()));
        }

        public void AddJoin<TJoin>(DbTable<TJoin> table, LambdaExpression onExpression)
        {
            ValidateActionStateChange(DbQueryAction.SetJoin);

            m_pLastAction = DbQueryAction.SetJoin;
            this.FromTables.Add(new TableInfo(table.TableName, this.Session.CreateTempTableName(), typeof(TJoin), table.Schema));
            this.JoinTargets.Add(new JoinTarget(JoinMode.Join, this.FromTables.Last(), onExpression));
        }

        public void AddJoin<TJoin>(DbQuerySet<TJoin> querySet, LambdaExpression onExpression)
        {
            ValidateActionStateChange(DbQueryAction.SetJoin);

            m_pLastAction = DbQueryAction.SetJoin;
            this.FromTables.Add(new TableInfo(querySet.QueryContext, this.Session.CreateTempTableName()));
            this.JoinTargets.Add(new JoinTarget(JoinMode.Join, this.FromTables.Last(), onExpression));
        }

        public void AddLeftJoin<TJoin>(DbTable<TJoin> table, LambdaExpression onExpression)
        {
            ValidateActionStateChange(DbQueryAction.SetJoin);

            m_pLastAction = DbQueryAction.SetJoin;
            this.FromTables.Add(new TableInfo(table.TableName, this.Session.CreateTempTableName(), typeof(TJoin), table.Schema));
            this.JoinTargets.Add(new JoinTarget(JoinMode.LeftJoin, this.FromTables.Last(), onExpression));
        }

        public void AddLeftJoin<TJoin>(DbQuerySet<TJoin> querySet, LambdaExpression onExpression)
        {
            ValidateActionStateChange(DbQueryAction.SetJoin);

            m_pLastAction = DbQueryAction.SetJoin;
            this.FromTables.Add(new TableInfo(querySet.QueryContext, this.Session.CreateTempTableName()));
            this.JoinTargets.Add(new JoinTarget(JoinMode.LeftJoin, this.FromTables.Last(), onExpression));
        }

        public void SetWhere(LambdaExpression whereExpression)
        {
            ValidateActionStateChange(DbQueryAction.SetWhere);

            m_pLastAction = DbQueryAction.SetWhere;
            this.WhereExpressions.Add(whereExpression);
        }

        public void SetGroupBy(LambdaExpression groupByExpression)
        {
            ValidateActionStateChange(DbQueryAction.SetGroupBy);

            m_pLastAction = DbQueryAction.SetGroupBy;
            this.GroupByExpression = groupByExpression;
        }

        public void SetHaving(LambdaExpression havingExpression)
        {
            ValidateActionStateChange(DbQueryAction.SetHaving);

            m_pLastAction = DbQueryAction.SetHaving;
            this.HavingExpression = havingExpression;
        }

        public void SetOrderBy(LambdaExpression orderByExpression)
        {
            ValidateActionStateChange(DbQueryAction.SetOrderBy);

            m_pLastAction = DbQueryAction.SetOrderBy;
            this.OrderByExpressions.Add(orderByExpression);
        }

        public void AddUnion<TEntity>(DbQuerySet<TEntity> querySet)
        {
            ValidateActionStateChange(DbQueryAction.SetUnion);

            m_pLastAction = DbQueryAction.SetUnion;
            this.UnionTargets.Add(new UnionTarget(UnionMode.Union, querySet.QueryContext, this.Session.CreateTempTableName()));
        }

        public void AddUnionAll<TEntity>(DbQuerySet<TEntity> querySet)
        {
            ValidateActionStateChange(DbQueryAction.SetUnion);

            m_pLastAction = DbQueryAction.SetUnion;
            this.UnionTargets.Add(new UnionTarget(UnionMode.UnionAll, querySet.QueryContext, this.Session.CreateTempTableName()));
        }


        private bool CanChangeActionStateTo(DbQueryAction nextAction)
        {
            if (nextAction == DbQueryAction.None)
            {
                throw new ArgumentException($"参数错误：查询上下文状态错误【{nextAction}】");
            }


            switch (m_pLastAction)
            {
                case DbQueryAction.SetFrom:
                    {
                        if (nextAction == DbQueryAction.SetJoin
                            || nextAction == DbQueryAction.SetWhere
                            || nextAction == DbQueryAction.SetGroupBy
                            || nextAction == DbQueryAction.SetOrderBy
                            || nextAction == DbQueryAction.SetSelect
                            || nextAction == DbQueryAction.SetDistinct
                            || nextAction == DbQueryAction.SetUnion)
                        {
                            return true;
                        }

                        return false;
                    }

                case DbQueryAction.SetJoin:
                    {
                        if (nextAction == DbQueryAction.SetJoin
                            || nextAction == DbQueryAction.SetWhere
                            || nextAction == DbQueryAction.SetGroupBy
                            || nextAction == DbQueryAction.SetOrderBy
                            || nextAction == DbQueryAction.SetSelect
                            || nextAction == DbQueryAction.SetDistinct)
                        {
                            return true;
                        }

                        return false;
                    }

                case DbQueryAction.SetWhere:
                    {
                        if (nextAction == DbQueryAction.SetWhere
                            || nextAction == DbQueryAction.SetGroupBy
                            || nextAction == DbQueryAction.SetOrderBy
                            || nextAction == DbQueryAction.SetSelect
                            || nextAction == DbQueryAction.SetDistinct)
                        {
                            return true;
                        }

                        if (nextAction == DbQueryAction.SetUnion
                            && JoinTargets.Count == 0)
                        {
                            return true;
                        }

                        return false;
                    }

                case DbQueryAction.SetGroupBy:
                    {
                        if (nextAction == DbQueryAction.SetHaving
                            || nextAction == DbQueryAction.SetOrderBy
                            || nextAction == DbQueryAction.SetSelect
                            || nextAction == DbQueryAction.SetDistinct)
                        {
                            return true;
                        }

                        return false;
                    }

                case DbQueryAction.SetHaving:
                    {
                        if (nextAction == DbQueryAction.SetOrderBy
                           || nextAction == DbQueryAction.SetSelect
                           || nextAction == DbQueryAction.SetDistinct)
                        {
                            return true;
                        }

                        return false;
                    }

                case DbQueryAction.SetOrderBy:
                    {
                        if (nextAction == DbQueryAction.SetOrderBy
                            || nextAction == DbQueryAction.SetPaging
                            || nextAction == DbQueryAction.SetSelect
                            || nextAction == DbQueryAction.SetDistinct)
                        {
                            return true;
                        }

                        return false;
                    }

                case DbQueryAction.SetPaging:
                    {
                        if (nextAction == DbQueryAction.SetSelect
                            || nextAction == DbQueryAction.SetDistinct)
                        {
                            return true;
                        }

                        return false;
                    }

                case DbQueryAction.SetSelect:
                    {
                        if (nextAction == DbQueryAction.SetDistinct
                            || nextAction == DbQueryAction.SetUnion)
                        {
                            return true;
                        }

                        return false;
                    }

                case DbQueryAction.SetDistinct:
                    {
                        if (nextAction == DbQueryAction.SetUnion)
                        {
                            return true;
                        }

                        return false;
                    }

                case DbQueryAction.SetUnion:
                    {
                        if (nextAction == DbQueryAction.SetUnion
                            || nextAction == DbQueryAction.SetOrderBy)
                        {
                            return true;
                        }

                        return false;
                    }

                default:
                    {
                        if (nextAction == DbQueryAction.SetFrom)
                        {
                            return true;
                        }

                        return false;
                    }
            }
        }

        private void ValidateActionStateChange(DbQueryAction nextAction)
        {
            if (!CanChangeActionStateTo(nextAction))
            {
                throw new InvalidOperationException($"不能从当前状态【{m_pLastAction}】变更到【{nextAction}】");
            }
        }


        private DbQueryContext CloneContext()
        {
            DbQueryContext newContext = new DbQueryContext(this.Session);
            newContext.m_pLastAction = m_pLastAction;

            newContext.Distinct = this.Distinct;
            newContext.PagingInfo = this.PagingInfo;
            newContext.FromTables.AddRange(this.FromTables);
            newContext.SelectExpression = this.SelectExpression;
            newContext.WhereExpressions.AddRange(this.WhereExpressions);
            newContext.GroupByExpression = this.GroupByExpression;
            newContext.HavingExpression = this.HavingExpression;
            newContext.OrderByExpressions = this.OrderByExpressions;
            newContext.JoinTargets.AddRange(this.JoinTargets);
            newContext.UnionTargets.AddRange(this.UnionTargets);

            return newContext;
        }

        internal DbQueryContext SnapshotForAction(DbQueryAction action)
        {
            if (!this.CanChangeActionStateTo(action))
            {
                var newContext = new DbQueryContext(this.Session);
                newContext.SetFrom(CloneContext());

                return newContext;
            }

            return CloneContext();
        }
    }
}
