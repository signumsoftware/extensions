﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Signum.Entities;
using System.Collections.Concurrent;
using Signum.Engine.Maps;
using Signum.Utilities.Reflection;
using Signum.Engine.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Signum.Utilities;
using Signum.Entities.Cache;
using System.Data.SqlClient;
using Signum.Engine.Basics;
using System.Diagnostics;
using System.Threading;
using Signum.Utilities.ExpressionTrees;
using System.Data;
using Signum.Entities.Reflection;
using Signum.Entities.Internal;

namespace Signum.Engine.Cache
{
    public abstract class CachedTableBase
    {
        public abstract ITable Table { get; }

        public abstract IColumn ParentColumn { get; set; }

        internal List<CachedTableBase> subTables;
        public List<CachedTableBase> SubTables { get { return subTables; } }
        protected SqlPreCommandSimple query;
        internal ICacheLogicController controller;

        internal CachedTableConstructor Constructor;

        internal CachedTableBase(ICacheLogicController controller)
        {
            this.controller = controller;
        }

        protected void OnChange(object sender, SqlNotificationEventArgs args)
        {
            try
            {
                if (args.Info == SqlNotificationInfo.Invalid &&
                    args.Source == SqlNotificationSource.Statement &&
                    args.Type == SqlNotificationType.Subscribe)
                    throw new InvalidOperationException("Invalid query for SqlDependency") { Data = { { "query", query.PlainSql() } } };

                if (args.Info == SqlNotificationInfo.PreviousFire)
                    throw new InvalidOperationException("The same transaction that loaded the data is invalidating it!") { Data = { { "query", query.PlainSql() } } };

                if (CacheLogic.LogWriter != null)
                    CacheLogic.LogWriter.WriteLine("Change {0}".FormatWith(GetType().TypeName())); 

                Reset();

                Interlocked.Increment(ref invalidations);

                controller.OnChange(this, args);
            }
            catch (Exception e)
            {
                e.LogException();
            }
        }

        public void ResetAll(bool forceReset)
        {
            if (CacheLogic.LogWriter != null)
                CacheLogic.LogWriter.WriteLine("ResetAll {0}".FormatWith(GetType().TypeName()));

            Reset();

            if (forceReset)
            {
                invalidations = 0;
                hits = 0;
                loads = 0;
                sumLoadTime = 0;
            }
            else
            {
                Interlocked.Increment(ref invalidations);
            }

            if (subTables != null)
                foreach (var st in subTables)
                    st.ResetAll(forceReset);
        }

        public abstract void SchemaCompleted();

        internal void LoadAll()
        {
            Load();

            if (subTables != null)
                foreach (var st in subTables)
                    st.LoadAll();
        }

        protected abstract void Load();
        protected abstract void Reset();

        public abstract Type Type { get; }

        public abstract int? Count { get; }

        int invalidations;
        public int Invalidations { get { return invalidations; } }

        protected int hits;
        public int Hits { get { return hits; } }

        int loads;
        public int Loads { get { return loads; } }

        long sumLoadTime;
        public TimeSpan SumLoadTime
        {
            get { return TimeSpan.FromMilliseconds(sumLoadTime / PerfCounter.FrequencyMilliseconds); }
        }

        protected IDisposable MeasureLoad()
        {
            long start = PerfCounter.Ticks;

            return new Disposable(() =>
            {
                sumLoadTime += (PerfCounter.Ticks - start);
                Interlocked.Increment(ref loads);
            });
        }


        internal static readonly MethodInfo ToStringMethod = ReflectionTools.GetMethodInfo((object o) => o.ToString());

        internal abstract bool Contains(PrimaryKey primaryKey);
    }

  


    class CachedTable<T> : CachedTableBase where T : Entity
    {
        Table table;

        public Dictionary<PrimaryKey, object> Rows { get { return rows.Value; } }

        ResetLazy<Dictionary<PrimaryKey, object>> rows;

        Func<FieldReader, object> rowReader;
        Action<object, IRetriever, T> completer;
        Expression<Action<object, IRetriever, T>> completerExpression;
        Func<object, PrimaryKey> idGetter;
        Func<PrimaryKey, string> toStrGetter;

        public override IColumn ParentColumn { get; set; }

        SemiCachedController<T> semiCachedController;

        public CachedTable(ICacheLogicController controller, AliasGenerator aliasGenerator, string lastPartialJoin, string remainingJoins)
            : base(controller)
        {
            this.table = Schema.Current.Table(typeof(T));

            CachedTableConstructor ctr = this.Constructor = new CachedTableConstructor(this, aliasGenerator);

            //Query
            using (ObjectName.OverrideOptions(new ObjectNameOptions { AvoidDatabaseName = true }))
            {
                string select = "SELECT\r\n{0}\r\nFROM {1} {2}\r\n".FormatWith(
                    Table.Columns.Values.ToString(c => ctr.currentAlias.Name.SqlEscape() + "." + c.Name.SqlEscape(), ",\r\n"),
                    table.Name.ToString(),
                    ctr.currentAlias.Name.SqlEscape());

                ctr.remainingJoins = lastPartialJoin == null ? null : lastPartialJoin + ctr.currentAlias.Name.SqlEscape() + ".Id\r\n" + remainingJoins;

                if (ctr.remainingJoins != null)
                    select += ctr.remainingJoins;

                query = new SqlPreCommandSimple(select);
            }
            

            //Reader
            {
                rowReader = ctr.GetRowReader();
            }

            //Completer
            {
                ParameterExpression me = Expression.Parameter(typeof(T), "me");

                var block = ctr.MaterializeEntity(me, table);

                completerExpression = Expression.Lambda<Action<object, IRetriever, T>>(block, CachedTableConstructor.originObject, CachedTableConstructor.retriever, me);

                completer = completerExpression.Compile();

                idGetter = ctr.GetPrimaryKeyGetter((IColumn)table.PrimaryKey);
            }

            rows = new ResetLazy<Dictionary<PrimaryKey, object>>(() =>
            {
                CacheLogic.AssertSqlDependencyStarted();

                var connector = (SqlConnector)Connector.Current;
                Table table = connector.Schema.Table(typeof(T));

                var subConnector = connector.ForDatabase(table.Name.Schema.Try(s => s.Database));

                Dictionary<PrimaryKey, object> result = new Dictionary<PrimaryKey, object>();
                using (MeasureLoad())
                using (Connector.Override(subConnector))
                using (Transaction tr = Transaction.ForceNew(IsolationLevel.ReadCommitted))
                {
                    if (CacheLogic.LogWriter != null)
                        CacheLogic.LogWriter.WriteLine("Load {0}".FormatWith(GetType().TypeName()));

                    ((SqlConnector)Connector.Current).ExecuteDataReaderOptionalDependency(query, OnChange, fr =>
                    {
                        object obj = rowReader(fr);
                        result[idGetter(obj)] = obj; //Could be repeated joins
                    });
                    tr.Commit();
                }

                return result;
            }, mode: LazyThreadSafetyMode.ExecutionAndPublication);

            if(!CacheLogic.WithSqlDependency && lastPartialJoin.HasText()) //Is semi
            {
                semiCachedController = new SemiCachedController<T>(this);
            }
        }

        public override void SchemaCompleted()
        {
            toStrGetter = ToStringExpressionVisitor.GetToString<T>(this.Constructor, s => s.ToString());
            if (this.subTables != null)
                foreach (var item in this.subTables)
                    item.SchemaCompleted();

        }

        protected override void Reset()
        {
            if (CacheLogic.LogWriter != null)
                CacheLogic.LogWriter.WriteLine((rows.IsValueCreated ? "RESET {0}" : "Reset {0}").FormatWith(GetType().TypeName()));

            rows.Reset();
        }

        protected override void Load()
        {
            rows.Load();
        }

        public string GetToString(PrimaryKey id)
        {
            return toStrGetter(id);
        }

        public object GetRow(PrimaryKey id)
        {
            Interlocked.Increment(ref hits);
            var origin = Rows.TryGetC(id);
            if (origin == null)
                throw new EntityNotFoundException(typeof(T), id);

            return origin;
        }

        public string TryGetToString(PrimaryKey id)
        {
            Interlocked.Increment(ref hits);
            var origin = Rows.TryGetC(id);
            if (origin == null)
                return null;

            return toStrGetter(id);
        }

        public void Complete(T entity, IRetriever retriever)
        {
            Interlocked.Increment(ref hits);

            var origin = Rows.TryGetC(entity.Id);
            if (origin == null)
                throw new EntityNotFoundException(typeof(T), entity.Id);

            completer(origin, retriever, entity);
        }

        internal IEnumerable<PrimaryKey> GetAllIds()
        {
            Interlocked.Increment(ref hits);
            return Rows.Keys;
        }

        public override int? Count
        {
            get { return rows.IsValueCreated ? rows.Value.Count : (int?)null; }
        }

        public override Type Type
        {
            get { return typeof(T); }
        }

        public override ITable Table
        {
            get { return table; }
        }


        internal override bool Contains(PrimaryKey primaryKey)
        {
            return this.rows.Value.ContainsKey(primaryKey);
        }
    }


    class CachedTableMList<T> : CachedTableBase
    {
        public override IColumn ParentColumn { get; set; }

        TableMList table;

        ResetLazy<Dictionary<PrimaryKey, Dictionary<PrimaryKey, object>>> relationalRows;

        static ParameterExpression result = Expression.Parameter(typeof(T));

        Func<FieldReader, object> rowReader;
        Expression<Func<object, IRetriever, MList<T>.RowIdValue>> activatorExpression;
        Func<object, IRetriever, MList<T>.RowIdValue> activator;
        Func<object, PrimaryKey> parentIdGetter;
        Func<object, PrimaryKey> rowIdGetter;

        public CachedTableMList(ICacheLogicController controller, TableMList table, AliasGenerator aliasGenerator, string lastPartialJoin, string remainingJoins)
            : base(controller)
        {
            this.table = table;

            CachedTableConstructor ctr = this.Constructor= new CachedTableConstructor(this, aliasGenerator);

            //Query
            using (ObjectName.OverrideOptions(new ObjectNameOptions { AvoidDatabaseName = true }))
            {
                string select = "SELECT\r\n{0}\r\nFROM {1} {2}\r\n".FormatWith(
                    ctr.table.Columns.Values.ToString(c => ctr.currentAlias.Name.SqlEscape() + "." + c.Name.SqlEscape(), ",\r\n"),
                    table.Name.ToString(),
                    ctr.currentAlias.Name.SqlEscape());

                ctr.remainingJoins = lastPartialJoin + ctr.currentAlias.Name.SqlEscape() + "." + table.BackReference.Name.SqlEscape() + "\r\n" + remainingJoins;

                query = new SqlPreCommandSimple(select);
            }

            //Reader
            {
                rowReader = ctr.GetRowReader();
            }

            //Completer
            {
                List<Expression> instructions = new List<Expression>();

                instructions.Add(Expression.Assign(ctr.origin, Expression.Convert(CachedTableConstructor.originObject, ctr.tupleType)));
                instructions.Add(Expression.Assign(result, ctr.MaterializeField(table.Field)));

                var ci = typeof(MList<T>.RowIdValue).GetConstructor(new []{typeof(T), typeof(PrimaryKey), typeof(int?)});

                var order = table.Order == null ? Expression.Constant(null, typeof(int?)) : 
                     ctr.GetTupleProperty(table.Order).Nullify();

                instructions.Add(Expression.New(ci, result, CachedTableConstructor.NewPrimaryKey(ctr.GetTupleProperty(table.PrimaryKey)), order));

                var block = Expression.Block(typeof(MList<T>.RowIdValue), new[] { ctr.origin, result }, instructions);

                activatorExpression = Expression.Lambda<Func<object, IRetriever, MList<T>.RowIdValue>>(block, CachedTableConstructor.originObject, CachedTableConstructor.retriever);

                activator = activatorExpression.Compile();

                parentIdGetter = ctr.GetPrimaryKeyGetter(table.BackReference);
                rowIdGetter = ctr.GetPrimaryKeyGetter(table.PrimaryKey);
            }

            relationalRows = new ResetLazy<Dictionary<PrimaryKey, Dictionary<PrimaryKey, object>>>(() =>
            {
                CacheLogic.AssertSqlDependencyStarted();

                var connector = (SqlConnector)Connector.Current;

                var subConnector = connector.ForDatabase(table.Name.Schema.Try(s => s.Database));

                Dictionary<PrimaryKey, Dictionary<PrimaryKey, object>> result = new Dictionary<PrimaryKey, Dictionary<PrimaryKey, object>>();

                using (MeasureLoad())
                using (Connector.Override(subConnector))
                using (Transaction tr = Transaction.ForceNew(IsolationLevel.ReadCommitted))
                {
                    if (CacheLogic.LogWriter != null)
                        CacheLogic.LogWriter.WriteLine("Load {0}".FormatWith(GetType().TypeName()));

                    ((SqlConnector)Connector.Current).ExecuteDataReaderOptionalDependency(query, OnChange, fr =>
                    {
                        object obj = rowReader(fr);
                        PrimaryKey parentId = parentIdGetter(obj);
                        var dic = result.TryGetC(parentId);
                        if (dic == null)
                            result[parentId] = dic = new Dictionary<PrimaryKey, object>();

                        dic[rowIdGetter(obj)] = obj;
                    });
                    tr.Commit();
                }

                return result;
            }, mode: LazyThreadSafetyMode.ExecutionAndPublication);
        }

        protected override void Reset()
        {
            if (CacheLogic.LogWriter != null)
                CacheLogic.LogWriter.WriteLine((relationalRows.IsValueCreated ? "RESET {0}" : "Reset {0}").FormatWith(GetType().TypeName()));


            relationalRows.Reset();
        }

        protected override void Load()
        {
            relationalRows.Load();
        }

        public MList<T> GetMList(PrimaryKey id, IRetriever retriever)
        {
            Interlocked.Increment(ref hits);

            MList<T> result;
            var dic = relationalRows.Value.TryGetC(id);
            if (dic == null)
                result = new MList<T>();
            else
            {
                result = new MList<T>(dic.Count);
                var innerList = ((IMListPrivate<T>)result).InnerList;
                foreach (var obj in dic.Values)
                {
                    innerList.Add(activator(obj, retriever));
                }
                ((IMListPrivate)result).ExecutePostRetrieving();
                
            }

            CachedTableConstructor.resetModifiedAction(retriever, result);

            return result;
        }

        public override int? Count
        {
            get { return relationalRows.IsValueCreated ? relationalRows.Value.Count : (int?)null; }
        }

        public override Type Type
        {
            get { return typeof(MList<T>); }
        }

        public override ITable Table
        {
            get { return table; }
        }

        internal override bool Contains(PrimaryKey primaryKey)
        {
            throw new InvalidOperationException("CacheMListTable does not implements contains");
        }

        public override void SchemaCompleted()
        {
            if (this.subTables != null)
                foreach (var item in this.subTables)
                    item.SchemaCompleted();
        }
    }


    class CachedLiteTable<T> : CachedTableBase where T : Entity
    {
        public override IColumn ParentColumn { get; set; }

        Table table;

        Alias currentAlias;
        string lastPartialJoin;
        string remainingJoins; 

        Func<FieldReader, KeyValuePair<PrimaryKey, string>> rowReader;
        ResetLazy<Dictionary<PrimaryKey, string>> toStrings;

        SemiCachedController<T> semiCachedController;

        public CachedLiteTable(ICacheLogicController controller, AliasGenerator aliasGenerator, string lastPartialJoin, string remainingJoins)
            : base(controller)
        {
            this.table = Schema.Current.Table(typeof(T));
            this.lastPartialJoin = lastPartialJoin;
            this.remainingJoins = remainingJoins;
            this.currentAlias = aliasGenerator.NextTableAlias(table.Name.Name);

            if (!CacheLogic.WithSqlDependency)
            {
                semiCachedController = new SemiCachedController<T>(this);
            }
        }

        public override void SchemaCompleted()
        {
            List<IColumn> columns = new List<IColumn> { table.PrimaryKey };

            ParameterExpression reader = Expression.Parameter(typeof(FieldReader));

            var expression = ToStringExpressionVisitor.GetToString(table, reader, columns);

            //Query
            using (ObjectName.OverrideOptions(new ObjectNameOptions { AvoidDatabaseName = true }))
            {
                string select = "SELECT {0}\r\nFROM {1} {2}\r\n".FormatWith(
                    columns.ToString(c => currentAlias.Name.SqlEscape() + "." + c.Name.SqlEscape(), ", "),
                    table.Name.ToString(),
                    currentAlias.Name.SqlEscape());

                select += this.lastPartialJoin + currentAlias.Name.SqlEscape() + "." + table.PrimaryKey.Name.SqlEscape() + "\r\n" + this.remainingJoins;

                query = new SqlPreCommandSimple(select);
            }

            //Reader
            {
                var kvpConstructor = Expression.New(CachedTableConstructor.ciKVPIntString,
                    CachedTableConstructor.NewPrimaryKey(FieldReader.GetExpression(reader, 0, this.table.PrimaryKey.Type)),
                    expression);

                rowReader = Expression.Lambda<Func<FieldReader, KeyValuePair<PrimaryKey, string>>>(kvpConstructor, reader).Compile();
            }

            toStrings = new ResetLazy<Dictionary<PrimaryKey, string>>(() =>
            {
                CacheLogic.AssertSqlDependencyStarted();

                var connector = (SqlConnector)Connector.Current;

                var subConnector = connector.ForDatabase(table.Name.Schema.Try(s => s.Database));

                Dictionary<PrimaryKey, string> result = new Dictionary<PrimaryKey, string>();

                using (MeasureLoad())
                using (Connector.Override(subConnector))
                using (Transaction tr = Transaction.ForceNew(IsolationLevel.ReadCommitted))
                {
                    if (CacheLogic.LogWriter != null)
                        CacheLogic.LogWriter.WriteLine("Load {0}".FormatWith(GetType().TypeName()));

                    ((SqlConnector)Connector.Current).ExecuteDataReaderOptionalDependency(query, OnChange, fr =>
                    {
                        var kvp = rowReader(fr);
                        result[kvp.Key] = kvp.Value;
                    });
                    tr.Commit();
                }

                return result;
            }, mode: LazyThreadSafetyMode.ExecutionAndPublication);

            if (this.subTables != null)
                foreach (var item in this.subTables)
                    item.SchemaCompleted();
        }

        protected override void Reset()
        {
            if (toStrings == null)
                return;

            if (CacheLogic.LogWriter != null )
                CacheLogic.LogWriter.WriteLine((toStrings.IsValueCreated ? "RESET {0}" : "Reset {0}").FormatWith(GetType().TypeName()));

            toStrings.Reset();
        }

        protected override void Load()
        {
            if (toStrings == null)
                return;

            toStrings.Load();
        }


        public Lite<T> GetLite(PrimaryKey id, IRetriever retriever)
        {
            Interlocked.Increment(ref hits);

            return retriever.ModifiablePostRetrieving((LiteImp<T>)Lite.Create<T>(id,toStrings==null?null: toStrings.Value[id]));
        }

        public override int? Count
        {
            get { return toStrings.IsValueCreated ? toStrings.Value.Count : (int?)null; }
        }

        public override Type Type
        {
            get { return typeof(Lite<T>); }
        }

        public override ITable Table
        {
            get { return table; }
        }

        class ToStringExpressionVisitor : ExpressionVisitor
        {
            ParameterExpression param;
            ParameterExpression reader;

            List<IColumn> columns;

            Table table;

            public static Expression GetToString(Table table, ParameterExpression reader, List<IColumn> columns)
            {
                LambdaExpression lambda = ExpressionCleaner.GetFieldExpansion(table.Type, CachedTableBase.ToStringMethod);

                if (lambda == null)
                {
                    columns.Add(table.ToStrColumn);
                    
                    return FieldReader.GetExpression(reader, columns.Count - 1, typeof(string));
                }

                ToStringExpressionVisitor toStr = new ToStringExpressionVisitor
                {
                    param = lambda.Parameters.SingleEx(),
                    reader = reader,
                    columns = columns,
                    table = table,
                };

                var result = toStr.Visit(lambda.Body);

                return result;
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (node.NodeType == ExpressionType.Convert)
                {
                    var obj = Visit(node.Operand);

                    return Expression.Convert(obj, node.Type);
                }

                return base.VisitUnary(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression == param)
                {
                    var field = table.GetField(node.Member);

                    var column = GetColumn(field);

                    columns.Add(column);

                    return FieldReader.GetExpression(reader, columns.Count - 1, column.Type);
                }

                return base.VisitMember(node);
            }

            private IColumn GetColumn(Field field)
            {
                if (field is FieldPrimaryKey || field is FieldValue || field is FieldTicks)
                    return (IColumn)field;

                throw new InvalidOperationException("{0} not supported when caching the ToString for a Lite of a transacional entity ({1})".FormatWith(field.GetType().TypeName(), this.table.Type.TypeName()));
            }
        }

        internal override bool Contains(PrimaryKey primaryKey)
        {
            return this.toStrings.Value.ContainsKey(primaryKey);
        }
    }

    public class SemiCachedController<T> where T : Entity
    {
        CachedTableBase cachedTable;

        public SemiCachedController(CachedTableBase cachedTable)
        {
            this.cachedTable = cachedTable;

            CacheLogic.semiControllers.GetOrCreate(typeof(T)).Add(cachedTable);

            var ee = Schema.Current.EntityEvents<T>();
            ee.Saving += ident =>
            {
                if (ident.IsGraphModified && !ident.IsNew)
                {
                    cachedTable.LoadAll();

                    if (cachedTable.Contains(ident.Id))
                        DisableAndInvalidate();
                }
            };
            //ee.PreUnsafeDelete += query => DisableAndInvalidate();
            ee.PreUnsafeUpdate += (update, entityQuery) => DisableAndInvalidateMassive();
            ee.PreUnsafeInsert += (query, constructor, entityQuery) =>
            {
                if (constructor.Body.Type.IsInstantiationOf(typeof(MListElement<,>)))
                    DisableAndInvalidateMassive();

                return constructor;
            };
            ee.PreUnsafeMListDelete += (mlistQuery, entityQuery) => DisableAndInvalidateMassive();
            ee.PreBulkInsert += inMListTable =>
            {
                if (inMListTable)
                    DisableAndInvalidateMassive();
            };
        }

        void DisableAndInvalidateMassive()
        {
            if (CacheLogic.IsAssumedMassiveChangeAsInvalidation<T>())
                DisableAndInvalidate();
        }

        void DisableAndInvalidate()
        {
            CacheLogic.DisableAllConnectedTypesInTransaction(this.cachedTable.controller.Type);

            Transaction.PostRealCommit -= Transaction_PostRealCommit;
            Transaction.PostRealCommit += Transaction_PostRealCommit;
        }

        void Transaction_PostRealCommit(Dictionary<string, object> obj)
        {
            cachedTable.ResetAll(forceReset: false);
            CacheLogic.NotifyInvalidateAllConnectedTypes(this.cachedTable.controller.Type);
        }
    }
}
