﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Signum.Engine.Maps;
using Signum.Entities;
using System.Collections;
using System.Threading;
using Signum.Utilities;
using Signum.Engine.Exceptions;
using System.Collections.Concurrent;
using Signum.Utilities.DataStructures;
using Signum.Entities.Reflection;
using Signum.Utilities.Reflection;
using System.Reflection;
using Signum.Entities.Cache;
using Signum.Engine.Authorization;
using System.Drawing;
using Signum.Entities.Basics;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data.SqlTypes;
using Signum.Utilities.ExpressionTrees;
using Signum.Engine.SchemaInfoTables;
using Signum.Engine.Basics;
using Signum.Engine.Linq;
using System.Linq.Expressions;
using System.IO;
using Signum.Engine.Disconnected;

namespace Signum.Engine.Cache
{
    public static class CacheLogic
    {
        public static TextWriter LogWriter;

        public static bool AssertOnStart = true;

        public static void AssertStarted(SchemaBuilder sb)
        {
            sb.AssertDefined(ReflectionTools.GetMethodInfo(() => Start(null)));
        }

        /// <summary>
        /// If you have invalidation problems look at exceptions in: select * from sys.transmission_queue 
        /// If there are exceptions like: 'Could not obtain information about Windows NT group/user'
        ///    Change login to a SqlServer authentication (i.e.: sa)
        ///    Change Server Authentication mode and enable SA: http://msdn.microsoft.com/en-us/library/ms188670.aspx
        ///    Change Database ownership to sa: ALTER AUTHORIZATION ON DATABASE::yourDatabase TO sa
        /// </summary>
        public static void Start(SchemaBuilder sb)
        {
            if (sb.NotDefined(MethodInfo.GetCurrentMethod()))
            {
                PermissionAuthLogic.RegisterTypes(typeof(CachePermission));

                sb.SwitchGlobalLazyManager(new CacheGlobalLazyManager());

                sb.Schema.Synchronizing += Synchronize;
                sb.Schema.Generating += () => Synchronize(null).Try(s => s.ToSimple());
            }
        }

        public static List<T> ToListWithInvalidation<T>(this IQueryable<T> simpleQuery, Type type, string exceptionContext, Action<SqlNotificationEventArgs> invalidation)
        {
            ITranslateResult tr;
            using (ObjectName.OverrideOptions(new ObjectNameOptions { IncludeDboSchema = true, AvoidDatabaseName = true }))
                tr = ((DbQueryProvider)simpleQuery.Provider).GetRawTranslateResult(simpleQuery.Expression);

            OnChangeEventHandler onChange = (object sender, SqlNotificationEventArgs args) =>
            {
                try
                {
                    if (args.Type != SqlNotificationType.Change)
                        throw new InvalidOperationException(
                            "Problems with SqlDependency (Type : {0} Source : {1} Info : {2}) on query: \r\n{3}"
                            .Formato(args.Type, args.Source, args.Info, tr.GetMainPreCommand().PlainSql()));

                    if (args.Info == SqlNotificationInfo.PreviousFire)
                        throw new InvalidOperationException("The same transaction that loaded the data is invalidating it!") { Data = { { "query", tr.GetMainPreCommand().PlainSql() } } };

                    if (CacheLogic.LogWriter != null)
                        CacheLogic.LogWriter.WriteLine("Change ToListWithInvalidations {0} {1}".Formato(typeof(T).TypeName()), exceptionContext);

                    invalidation(args);
                }
                catch (Exception e)
                {
                    e.LogException(c => c.ControllerName = exceptionContext);
                }
            };

            SimpleReader reader = null;

            Expression<Func<IProjectionRow, T>> projectorExpression = (Expression<Func<IProjectionRow, T>>)tr.GetMainProjector();
            Func<IProjectionRow, T> projector = projectorExpression.Compile();

            List<T> list = new List<T>();

            CacheLogic.OnStart();

            Table table = Schema.Current.Table(type);
            DatabaseName db = table.Name.Schema.Try(s => s.Database);

            SqlConnector subConnector = ((SqlConnector)Connector.Current).ForDatabase(db);

            if (CacheLogic.LogWriter != null)
                CacheLogic.LogWriter.WriteLine("Load ToListWithInvalidations {0} {1}".Formato(typeof(T).TypeName()), exceptionContext);

            using (new EntityCache())
                subConnector.ExecuteDataReaderDependency(tr.GetMainPreCommand(), onChange, CacheLogic.ForceOnStart, fr =>
                    {
                        if (reader == null)
                            reader = new SimpleReader(fr, EntityCache.NewRetriever());

                        list.Add(projector(reader));
                    });

            return list;
        }

        class SimpleReader : IProjectionRow
        {
            FieldReader reader;
            IRetriever retriever;

            public SimpleReader(FieldReader reader, IRetriever retriever)
            {
                this.reader = reader;
                this.retriever = retriever;
            }

            public FieldReader Reader
            {
                get { return reader; }
            }

            public IRetriever Retriever
            {
                get { return retriever; }
            }

            public IEnumerable<S> Lookup<K, S>(LookupToken token, K key)
            {
                throw new InvalidOperationException("Subqueries can not be used on simple queries");
            }

            public MList<S> LookupRequest<K, S>(LookupToken token, K key, MList<S> field)
            {
                throw new InvalidOperationException("Subqueries can not be used on simple queries");
            }
        }


        public static SqlPreCommand Synchronize(Replacements replacements)
        {
            if(ExecutionMode.IsSynchronizeSchemaOnly)
                return null;

            SqlConnector connector = (SqlConnector)Connector.Current;
            List<SqlPreCommand> commands = new List<SqlPreCommand>();

            int index = 0; 
            foreach (var database in Schema.Current.DatabaseNames())
            {
                SqlConnector sub = connector.ForDatabase(database);

                using (Connector.Override(sub))
                {
                    string currentUser;

                    if (DisconnectedLogic.OfflineMode == true)
                        currentUser = DisconnectedLogic.LocalDBUser;
                    else
                        currentUser = (string)Executor.ExecuteScalar("select SYSTEM_USER");

                    AssertNonIntegratedSecurity(currentUser);

                    string databaseName = sub.DatabaseName();

                    var serverPrincipalName = (from db in Database.View<SysDatabases>()
                                               where db.name == databaseName
                                               join spl in Database.View<SysServerPrincipals>().DefaultIfEmpty() on db.owner_sid equals spl.sid
                                               select spl.name).Single();


                    if (currentUser != serverPrincipalName)
                        commands.Add(new SqlPreCommandSimple("ALTER AUTHORIZATION ON DATABASE::{0} TO {2}".Formato(databaseName, serverPrincipalName, currentUser)));


                    var databasePrincipalName = (from db in Database.View<SysDatabases>()
                                                 where db.name == databaseName
                                                 join dpl in Database.View<SysDatabasePrincipals>().DefaultIfEmpty() on db.owner_sid equals dpl.sid
                                                 select dpl.name).Single();

                    if (!databasePrincipalName.HasText() || databasePrincipalName != "dbo")
                        commands.Add(new SqlPreCommandSimple("ALTER AUTHORIZATION ON DATABASE::{0} TO {2}".Formato(databaseName, databasePrincipalName.DefaultText("Unknown"), currentUser)));

                    var enabled = Database.View<SysDatabases>().Where(db => db.name == databaseName).Select(a => a.is_broker_enabled).Single();

                    if (!enabled)
                    {
                        commands.Add(SchemaSynchronizer.DisconnectUsers(databaseName, "spid" + (index++)));

                        commands.Add(new SqlPreCommandSimple("ALTER DATABASE {0} SET ENABLE_BROKER".Formato(databaseName)));
                        commands.Add(new SqlPreCommandSimple("--ALTER DATABASE {0} SET NEW_BROKER".Formato(databaseName)));
                    }
                }
            }

            var result = commands.Combine(Spacing.Simple);

            if (result == null)
                return result;

            return SqlPreCommand.Combine(Spacing.Triple,
                new SqlPreCommandSimple("use master -- Start SqlDepencency sync"),
                result,
                new SqlPreCommandSimple("use {0} -- Finish SqlDepencency sync".Formato(connector.DatabaseName())));
        }


        static bool started = false;
        readonly static object startKeyLock = new object();
        internal static void OnStart()
        {
            if (GloballyDisabled)
                return;

            if (started)
                return;

            lock (startKeyLock)
            {
                if (started)
                    return;

                ForceOnStart();
            }
        }

        internal static void ForceOnStart()
        {
            lock (startKeyLock)
            {
                SqlConnector connector = (SqlConnector)Connector.Current;

                if (AssertOnStart)
                {
                    string currentUser = (string)Executor.ExecuteScalar("select SYSTEM_USER");

                    AssertNonIntegratedSecurity(currentUser);
                }

                foreach (var database in Schema.Current.DatabaseNames())
                {
                    try
                    {
                        SqlConnector sub = connector.ForDatabase(database);

                        SqlDependency.Start(sub.ConnectionString);
                    }
                    catch (InvalidOperationException ex)
                    {
                        if (ex.Message.Contains("SQL Server Service Broker"))
                            throw EnableBlocker(database);

                        throw;
                    }
                }

                RegisterOnShutdown();

                started = true;
            }
        }

        static bool registered = false;
        static void RegisterOnShutdown()
        {
            if (registered)
                return;

            SafeConsole.SetConsoleCtrlHandler(ct =>
            {
                Shutdown();
                return true;
            }, true);

            AppDomain.CurrentDomain.ProcessExit += (o, a) => Shutdown();

            registered = true;
        }

        private static void AssertNonIntegratedSecurity(string currentUser)
        {
            var type = Database.View<SysServerPrincipals>().Where(a => a.name == currentUser).Select(a => a.type_desc).Single();

            //if (type != "SQL_LOGIN")
            //    throw new InvalidOperationException("The current login '{0}' is a {1} instead of a SQL_LOGIN. Avoid using Integrated Security with Cache Logic".Formato(currentUser, type));

            //TFS #1163
            if (type != "SQL_LOGIN" && DisconnectedLogic.OfflineMode == false)
            {
                throw new InvalidOperationException("The current login '{0}' is a {1} instead of a SQL_LOGIN. Avoid using Integrated Security with Cache Logic".Formato(currentUser, type));
            }
        }

        private static InvalidOperationException EnableBlocker(DatabaseName database)
        {
            return new InvalidOperationException(@"CacheLogic requires SQL Server Service Broker to be activated. Execute: 
ALTER DATABASE {0} SET ENABLE_BROKER
If you have problems, try first: 
ALTER DATABASE {0} SET NEW_BROKER".Formato(database.TryToString() ?? Connector.Current.DatabaseName()));
        }

        public static void Shutdown()
        {
            if (GloballyDisabled)
                return;

            var connector = ((SqlConnector)Connector.Current);
            foreach (var database in Schema.Current.DatabaseNames())
            {
                SqlConnector sub = connector.ForDatabase(database);

                SqlDependency.Stop(sub.ConnectionString);
            }

        }

        static SqlPreCommandSimple GetDependencyQuery(ITable table)
        {
            return new SqlPreCommandSimple("SELECT {0} FROM {1}".Formato(table.Columns.Keys.ToString(c => c.SqlScape(), ", "), table.Name));
        }

        class CacheController<T> : CacheControllerBase<T>, ICacheLogicController
                where T : IdentifiableEntity
        {
            public CachedTable<T> cachedTable;
            public CachedTableBase CachedTable { get { return cachedTable; } }

            public CacheController(Schema schema)
            {
                var ee = schema.EntityEvents<T>();

                ee.CacheController = this;
                ee.Saving += Saving;
                ee.PreUnsafeDelete += PreUnsafeDelete;
                ee.PreUnsafeUpdate += UnsafeUpdated;
                ee.PreUnsafeInsert += UnsafeInsert;
                ee.PreUnsafeMListDelete += PreUnsafeMListDelete;
            }         

            public void BuildCachedTable()
            {
                cachedTable = new CachedTable<T>(this, new Linq.AliasGenerator(), null, null);
            }

            void UnsafeInsert(IQueryable query, LambdaExpression constructor, IQueryable<T> entityQuery)
            {
                DisableAllConnectedTypesInTransaction(typeof(T));

                Transaction.PostRealCommit -= Transaction_PostRealCommit;
                Transaction.PostRealCommit += Transaction_PostRealCommit;
            }

            void UnsafeUpdated(IUpdateable update, IQueryable<T> entityQuery)
            {
                DisableAllConnectedTypesInTransaction(typeof(T));

                Transaction.PostRealCommit -= Transaction_PostRealCommit;
                Transaction.PostRealCommit += Transaction_PostRealCommit;
            }

            void PreUnsafeMListDelete(IQueryable mlistQuery, IQueryable<T> entityQuery)
            {
                DisableAllConnectedTypesInTransaction(typeof(T));

                Transaction.PostRealCommit -= Transaction_PostRealCommit;
                Transaction.PostRealCommit += Transaction_PostRealCommit;
            }

            void PreUnsafeDelete(IQueryable<T> query)
            {
                DisableTypeInTransaction(typeof(T));

                Transaction.PostRealCommit -= Transaction_PostRealCommit;
                Transaction.PostRealCommit += Transaction_PostRealCommit;
            }

            void Saving(T ident)
            {
                if (ident.IsGraphModified)
                {
                    if (ident.IsNew)
                    {
                        DisableTypeInTransaction(typeof(T));
                    }
                    else
                    {
                        DisableAllConnectedTypesInTransaction(typeof(T));
                    }

                    Transaction.PostRealCommit -= Transaction_PostRealCommit;
                    Transaction.PostRealCommit += Transaction_PostRealCommit;
                }
            }

            void Transaction_PostRealCommit(Dictionary<string, object> obj)
            {
                cachedTable.ResetAll(forceReset: false);
                NotifyInvalidateAllConnectedTypes(typeof(T));
            }

            public override bool Enabled
            {
                get { return !GloballyDisabled && !ExecutionMode.IsCacheDisabled && !IsDisabledInTransaction(typeof(T)); }
            }

            private void AssertEnabled()
            {
                if (!Enabled)
                    throw new InvalidOperationException("Cache for {0} is not enabled".Formato(typeof(T).TypeName()));
            }

            public event EventHandler<CacheEventArgs> Invalidated;

            public void OnChange(object sender, SqlNotificationEventArgs args)
            {
                NotifyInvalidateAllConnectedTypes(typeof(T));
            }

            static object syncLock = new object();

            public override void Load()
            {
                cachedTable.LoadAll();
            }

            public void ForceReset()
            {
                cachedTable.ResetAll(forceReset: true);
            }

            public override IEnumerable<int> GetAllIds()
            {
                AssertEnabled();

                return cachedTable.GetAllIds();
            }

            public override string GetToString(int id)
            {
                AssertEnabled();

                return cachedTable.GetToString(id);
            }

            public override void Complete(T entity, IRetriever retriver)
            {
                AssertEnabled();

                cachedTable.Complete(entity, retriver);
            }

            public void NotifyDisabled()
            {
                if (Invalidated != null)
                    Invalidated(this, CacheEventArgs.Disabled);
            }

            public void NotifyInvalidated()
            {
                if (Invalidated != null)
                    Invalidated(this, CacheEventArgs.Invalidated);
            }
        }


        static Dictionary<Type, ICacheLogicController> controllers = new Dictionary<Type, ICacheLogicController>(); //CachePack

        static DirectedGraph<Type> inverseDependencies = new DirectedGraph<Type>();

        public static bool GloballyDisabled { get; set; }

        const string DisabledCachesKey = "disabledCaches";

        static HashSet<Type> DisabledTypesDuringTransaction()
        {
            var topUserData = Transaction.TopParentUserData();

            var hs = topUserData.TryGetC(DisabledCachesKey) as HashSet<Type>;
            if (hs == null)
            {
                topUserData[DisabledCachesKey] = hs = new HashSet<Type>();
            }

            return hs;
        }

        static bool IsDisabledInTransaction(Type type)
        {
            if (!Transaction.HasTransaction)
                return false;

            HashSet<Type> disabledTypes = Transaction.TopParentUserData().TryGetC(DisabledCachesKey) as HashSet<Type>;

            return disabledTypes != null && disabledTypes.Contains(type);
        }

        static void DisableTypeInTransaction(Type type)
        {
            DisabledTypesDuringTransaction().Add(type);

            controllers[type].NotifyDisabled();
        }

        static void DisableAllConnectedTypesInTransaction(Type type)
        {
            var connected = inverseDependencies.IndirectlyRelatedTo(type, true);

            var hs = DisabledTypesDuringTransaction();

            foreach (var stype in connected)
            {
                hs.Add(stype);
                controllers[stype].TryDo(t => t.NotifyDisabled());
            }
        }

      
        public static Dictionary<Type, EntityData> EntityDataOverrides = new Dictionary<Type, EntityData>();

        public static void OverrideEntityData<T>(EntityData data)
        {
            EntityDataOverrides.AddOrThrow(typeof(T), data, "{0} is already overriden");
        }

        static void TryCacheTable(SchemaBuilder sb, Type type)
        {
            if (!controllers.ContainsKey(type))
                giCacheTable.GetInvoker(type)(sb);
        }

        static GenericInvoker<Action<SchemaBuilder>> giCacheTable = new GenericInvoker<Action<SchemaBuilder>>(sb => CacheTable<IdentifiableEntity>(sb));
        public static void CacheTable<T>(SchemaBuilder sb) where T : IdentifiableEntity
        {
            EntityData data = EntityDataOverrides.TryGetS(typeof(T)) ?? EntityKindCache.GetEntityData(typeof(T));

            if (data == EntityData.Master)
            {
                var cc = new CacheController<T>(sb.Schema);
                controllers.AddOrThrow(typeof(T), cc, "{0} already registered");

                TryCacheSubTables(typeof(T), sb);

                cc.BuildCachedTable();
            }
            else //data == EntityData.Transactional
            {
                controllers.AddOrThrow(typeof(T), null, "{0} already registered");

                TryCacheSubTables(typeof(T), sb);
            }
        }

        static void TryCacheSubTables(Type type, SchemaBuilder sb)
        {
            List<Type> relatedTypes = sb.Schema.Table(type).DependentTables()
                .Where(a => !a.Value.IsEnum)
                .Select(t => t.Key.Type).ToList();

            inverseDependencies.Add(type);

            foreach (var rType in relatedTypes)
            {
                TryCacheTable(sb, rType);

                inverseDependencies.Add(rType, type);
            }
        }

        static ICacheLogicController GetController(Type type)
        {
            var controller = controllers.GetOrThrow(type, "{0} is not registered in CacheLogic");

            if (controller == null)
                throw new InvalidOperationException("{0} is just semi cached".Formato(type.TypeName()));

            return controller;
        }

        static void NotifyInvalidateAllConnectedTypes(Type type)
        {
            var connected = inverseDependencies.IndirectlyRelatedTo(type, includeInitialNode: true);

            foreach (var stype in connected)
            {
                var controller = controllers[stype];
                if (controller != null)
                    controller.NotifyInvalidated();
            }
        }

        public static List<CachedTableBase> Statistics()
        {
            return controllers.Values.NotNull().Select(a => a.CachedTable).OrderByDescending(a => a.Count).ToList();
        }

        public static CacheType GetCacheType(Type type)
        {
            ICacheLogicController controller;
            if (!controllers.TryGetValue(type, out controller))
                return CacheType.None;

            if (controller == null)
                return CacheType.Semi;

            return CacheType.Cached;
        }

        public static void ForceReset()
        {
            foreach (var controller in controllers.Values.NotNull())
            {
                controller.ForceReset();
            }
        }

        public static XDocument SchemaGraph(Func<Type, bool> cacheHint)
        {
            var dgml = Schema.Current.ToDirectedGraph().ToDGML(t =>
                new[]
            {
                new XAttribute("Label", t.Name),
                new XAttribute("Background", GetColor(t.Type, cacheHint).ToHtml())
            }, info => new[]
            {
                info.IsLite ? new XAttribute("StrokeDashArray",  "2 3") : null,
            }.NotNull().ToArray());

            return dgml;
        }

        static Color GetColor(Type type, Func<Type, bool> cacheHint)
        {
            if (type.IsEnumEntity())
                return Color.Red;

            switch (CacheLogic.GetCacheType(type))
            {
                case CacheType.Cached: return Color.Purple;
                case CacheType.Semi: return Color.Pink;
            }

            if (typeof(Symbol).IsAssignableFrom(type))
                return Color.Orange;

            if (cacheHint != null && cacheHint(type))
                return Color.Yellow;

            return Color.Green;
        }

        public class CacheGlobalLazyManager : GlobalLazyManager
        {
            public override void AttachInvalidations(SchemaBuilder sb, InvalidateWith invalidateWith, EventHandler invalidate)
            {
                if (CacheLogic.GloballyDisabled)
                {
                    base.AttachInvalidations(sb, invalidateWith, invalidate);
                }
                else
                {
                    EventHandler<CacheEventArgs> onInvalidation = (sender, args) =>
                    {
                        if (args == CacheEventArgs.Invalidated)
                        {
                            invalidate(sender, args);
                        }
                        else if (args == CacheEventArgs.Disabled)
                        {
                            if (Transaction.InTestTransaction)
                            {
                                invalidate(sender, args);
                                Transaction.Rolledback += dic => invalidate(sender, args);
                            }

                            Transaction.PostRealCommit += dic => invalidate(sender, args);
                        }
                    };

                    foreach (var t in invalidateWith.Types)
                    {
                        CacheLogic.TryCacheTable(sb, t);

                        GetController(t).Invalidated += onInvalidation;
                    }
                }
            }

            public override void OnLoad(SchemaBuilder sb, InvalidateWith invalidateWith)
            {
                if (CacheLogic.GloballyDisabled)
                {
                    base.OnLoad(sb, invalidateWith);
                }
                else
                {
                    foreach (var t in invalidateWith.Types)
                        sb.Schema.CacheController(t).Load();
                }
            }
        }

        public static void LoadAllControllers()
        {
            foreach (var c in controllers.Values.NotNull())
            {
                c.Load();
            }
        }

      
    }

    internal interface ICacheLogicController : ICacheController
    {
        event EventHandler<CacheEventArgs> Invalidated;

        CachedTableBase CachedTable { get; }

        void NotifyDisabled();

        void NotifyInvalidated();

        void OnChange(object sender, SqlNotificationEventArgs args);

        void ForceReset();
    }

    public class CacheEventArgs : EventArgs
    {
        private CacheEventArgs() { }

        public static readonly CacheEventArgs Invalidated = new CacheEventArgs();
        public static readonly CacheEventArgs Disabled = new CacheEventArgs();
    }

    public enum CacheType
    {
        Cached,
        Semi,
        None
    }
}
