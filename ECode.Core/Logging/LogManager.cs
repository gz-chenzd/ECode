using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using ECode.Configuration;
using ECode.Json;
using ECode.TypeResolution;

namespace ECode.Logging
{
    public static class LogManager
    {
        static LogManager()
        {
            try
            {
                // Register the AppDomain events, note we have to do this with a
                // method call rather than directly here because the AppDomain
                // makes a LinkDemand which throws the exception during the JIT phase.
                RegisterAppDomainEvents();

                ConfigurationManager.Changed += OnConfigChanged;
                LoadConfig();
            }
            catch (SecurityException ex)
            {
                LogLog.Error("Security Exception (ControlAppDomain LinkDemand) while trying "
                             + "to register Shutdown handler with the AppDomain. LogManager.Shutdown() "
                             + "will not be called automatically when the AppDomain exits. "
                             + "It must be called programmatically.",
                             ex
                );
            }
        }

        /// <summary>
        /// Register for ProcessExit and DomainUnload events on the AppDomain
        /// </summary>
        /// <remarks>
        /// This needs to be in a separate method because the events make
        /// a LinkDemand for the ControlAppDomain SecurityPermission. Because
        /// this is a LinkDemand it is demanded at JIT time. Therefore we cannot
        /// catch the exception in the method itself, we have to catch it in the
        /// caller.
        /// </remarks>
        static void RegisterAppDomainEvents()
        {
            // ProcessExit seems to be fired if we are part of the default domain
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            // Otherwise DomainUnload is fired
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(OnDomainUnload);
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void Shutdown()
        {
            Logger.DisableAllWrites();

            foreach (var appender in appenders.Values)
            {
                appender.Close();
            }
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Shutdown();
        }

        private static void OnDomainUnload(object sender, EventArgs e)
        {
            Shutdown();
        }


        const string                                CONFIG_NAMESPACE_PREFIX     = "logging/";
        const string                                DEFAULT_LOGGER_NAME         = "#";

        static Level                                defaultLevel                = Level.ALL;
        static List<IAppender>                      defaultAppenderRefs         = new List<IAppender>(new[] { new ConsoleAppender() });
        static Dictionary<string, Logger>           loggers                     = new Dictionary<string, Logger>(StringComparer.InvariantCultureIgnoreCase);
        static Dictionary<string, IAppender>        appenders                   = new Dictionary<string, IAppender>(StringComparer.InvariantCultureIgnoreCase);


        public static Logger Default { get; } = GetLogger(DEFAULT_LOGGER_NAME);


        /// <summary>
        /// Shorthand for <see cref="M:LogManager.GetLogger(string)"/>.
        /// </summary>
        public static Logger GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        /// <summary>
        /// Retrieve or create a named logger.
        /// </summary>
        public static Logger GetLogger(string name)
        {
            name = name?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            { name = DEFAULT_LOGGER_NAME; }

            if (loggers.TryGetValue(name, out Logger logger))
            {
                return logger;
            }

            return CreateLogger(name);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static Logger CreateLogger(string name)
        {
            if (loggers.ContainsKey(name))
            {
                return loggers[name];
            }

            var logger = new Logger(name, defaultLevel);
            logger.SetAppenders(defaultAppenderRefs);

            loggers[name] = logger;

            return logger;
        }


        static bool         waitForReload       = false;
        static DateTime     lastChangedTime     = DateTime.MaxValue;
        static TimeSpan     delayTriggerTime    = new TimeSpan(0, 0, 0, 0, 200);

        private static void OnConfigChanged(ChangedEventArgs e)
        {
            if (!e.Key.StartsWith(CONFIG_NAMESPACE_PREFIX))
            { return; }

            lastChangedTime = DateTime.Now;

            if (waitForReload)
            { return; }

            waitForReload = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    while ((DateTime.Now - lastChangedTime) < delayTriggerTime)
                    { Task.Delay(100).Wait(); }

                    LoadConfig();
                }
                catch (Exception ex)
                { string dummy = ex.Message; }
                finally
                { waitForReload = false; }
            });
        }

        private static void LoadConfig()
        {
            try
            {
                var defaultLevel = Level.ALL;
                var defaultLevelStr = ConfigurationManager.Get(CONFIG_NAMESPACE_PREFIX + "level");
                if (string.IsNullOrWhiteSpace(defaultLevelStr))
                { LogLog.Warn("Logging config: 'level' not exists."); }
                else
                {
                    if (Enum.TryParse(typeof(Level), defaultLevelStr, out object resolvedLevel))
                    { defaultLevel = (Level)resolvedLevel; }
                    else
                    { LogLog.Warn($"Logging config: cannot parse '{defaultLevelStr}' to '{typeof(Level)}'."); }
                }

                var defaultAppenderRefs = new string[0];
                var defaultAppenderRefStr = ConfigurationManager.Get(CONFIG_NAMESPACE_PREFIX + "default");
                if (string.IsNullOrWhiteSpace(defaultAppenderRefStr))
                { LogLog.Warn("Logging config: 'default' not exists."); }
                else
                { defaultAppenderRefs = JsonUtil.Deserialize<string[]>(defaultAppenderRefStr); }

                var namedLoggers = new Dictionary<string, LoggerInfo>(StringComparer.InvariantCultureIgnoreCase);
                var namedLoggersStr = ConfigurationManager.Get(CONFIG_NAMESPACE_PREFIX + "loggers");
                if (string.IsNullOrWhiteSpace(namedLoggersStr))
                { LogLog.Warn("Logging config: 'loggers' not exists."); }
                else
                {
                    var loggerInfos = JsonUtil.Deserialize<List<LoggerInfo>>(namedLoggersStr);
                    foreach (var loggerInfo in loggerInfos)
                    {
                        if (string.IsNullOrWhiteSpace(loggerInfo.Name))
                        {
                            LogLog.Warn("Logging config: logger name cannot be null or empty.");
                            continue;
                        }

                        loggerInfo.Name = loggerInfo.Name.Trim();
                        loggerInfo.Level = Level.ALL;

                        if (loggerInfo.Appenders == null)
                        { loggerInfo.Appenders = new List<string>(); }

                        if (Enum.TryParse(typeof(Level), loggerInfo.LevelStr, out object resolvedLevel))
                        { loggerInfo.Level = (Level)resolvedLevel; }
                        else
                        { LogLog.Warn($"Logging config: cannot parse '{loggerInfo.LevelStr}' to '{typeof(Level)}'."); }

                        namedLoggers[loggerInfo.Name] = loggerInfo;
                    }
                }

                var namedAppenders = new Dictionary<string, AppenderInfo>(StringComparer.InvariantCultureIgnoreCase);
                string namedAppendersStr = ConfigurationManager.Get(CONFIG_NAMESPACE_PREFIX + "appenders");
                if (string.IsNullOrWhiteSpace(namedAppendersStr))
                { LogLog.Warn("Logging config: 'appenders' not exists."); }
                else
                {
                    var appenderInfos = JsonUtil.Deserialize<List<dynamic>>(namedAppendersStr);
                    foreach (var appenderInfo in appenderInfos)
                    {
                        string name = string.Empty;
                        string type = string.Empty;
                        var options = new NameValueCollection();

                        foreach (var keyValue in appenderInfo)
                        {
                            if (keyValue.Name == "name")
                            { name = (keyValue.Value ?? string.Empty).ToString(); }
                            else if (keyValue.Name == "type")
                            { type = (keyValue.Value ?? string.Empty).ToString(); }
                            else
                            {
                                options[keyValue.Name] = (keyValue.Value ?? string.Empty).ToString();
                            }
                        }

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            LogLog.Warn("Logging config: appender name cannot be null or empty.");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(type))
                        {
                            LogLog.Warn("Logging config: appender type cannot be null or empty.");
                            continue;
                        }

                        namedAppenders[name] = new AppenderInfo() { Name = name, TypeClass = type, Options = options };
                    }
                }


                var parsedAppenders = new Dictionary<string, IAppender>(StringComparer.InvariantCultureIgnoreCase);
                var requiredAppenders = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                foreach (string appender in defaultAppenderRefs)
                {
                    if (string.IsNullOrWhiteSpace(appender))
                    { continue; }

                    requiredAppenders.Add(appender.Trim());
                }

                foreach (var loggerInfo in namedLoggers.Values)
                {
                    foreach (string appender in loggerInfo.Appenders)
                    {
                        if (string.IsNullOrWhiteSpace(appender))
                        { continue; }

                        requiredAppenders.Add(appender.Trim());
                    }
                }

                foreach (string appenderName in requiredAppenders)
                {
                    if (!namedAppenders.TryGetValue(appenderName, out AppenderInfo appenderInfo))
                    { continue; }

                    var appender = CreateAppender(appenderInfo);
                    if (appender != null)
                    { parsedAppenders[appenderInfo.Name] = appender; }
                }

                LoadConfigSync(defaultLevel, defaultAppenderRefs, namedLoggers, parsedAppenders);
            }
            catch (Exception ex)
            { LogLog.Error($"Load log config error.", ex); }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void LoadConfigSync(Level defaultLevel, string[] defaultAppenderRefs, Dictionary<string, LoggerInfo> namedLoggers, Dictionary<string, IAppender> parsedAppenders)
        {
            LogManager.defaultLevel = defaultLevel;
            LogManager.appenders = parsedAppenders;

            LogManager.defaultAppenderRefs.Clear();
            var distinctAppenders = new HashSet<string>(defaultAppenderRefs, StringComparer.InvariantCultureIgnoreCase);
            foreach (var appenderRef in distinctAppenders)
            {
                var appenderName = appenderRef.Trim();
                if (parsedAppenders.ContainsKey(appenderName))
                {
                    LogManager.defaultAppenderRefs.Add(parsedAppenders[appenderName]);
                }
            }

            var loggers = new Dictionary<string, Logger>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var loggerInfo in namedLoggers.Values)
            {
                if (!LogManager.loggers.TryGetValue(loggerInfo.Name, out Logger logger))
                { logger = new Logger(loggerInfo.Name, loggerInfo.Level); }

                var appenders = new List<IAppender>();
                distinctAppenders = new HashSet<string>(loggerInfo.Appenders, StringComparer.InvariantCultureIgnoreCase);
                foreach (var appenderRef in distinctAppenders)
                {
                    string appenderName = appenderRef.Trim();
                    if (parsedAppenders.ContainsKey(appenderName))
                    {
                        appenders.Add(parsedAppenders[appenderName]);
                    }
                }

                logger.SetAppenders(appenders);

                loggers[logger.Name] = logger;
            }

            foreach (var loggerInfo in LogManager.loggers.Values)
            {
                if (loggers.ContainsKey(loggerInfo.Name))
                { continue; }

                loggerInfo.Level = LogManager.defaultLevel;
                loggerInfo.SetAppenders(LogManager.defaultAppenderRefs);

                loggers[loggerInfo.Name] = loggerInfo;
            }

            LogManager.loggers = loggers;
        }

        private static IAppender CreateAppender(AppenderInfo appenderInfo)
        {
            try
            {
                var appenderType = TypeResolutionUtil.ResolveType(appenderInfo.TypeClass);
                var appender = (IAppender)Activator.CreateInstance(appenderType);
                appender.Initialize(appenderInfo.Options);

                return appender;
            }
            catch (Exception ex)
            {
                LogLog.Error($"Logging config: cannot create appender '{appenderInfo.TypeClass}'.", ex);
                return null;
            }
        }
    }
}