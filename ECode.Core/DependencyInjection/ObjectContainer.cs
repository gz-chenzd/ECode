using System;
using System.Collections.Generic;
using System.IO;
using ECode.Logging;

namespace ECode.DependencyInjection
{
    public sealed class ObjectContainer
    {
        static readonly Logger  Log     = LogManager.GetLogger("DependencyInjection");


        readonly Dictionary<string, DefinitionBase>     container   = null;


        public ObjectContainer(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            { throw new ArgumentNullException(nameof(xml)); }

            try
            {
                Log.Debug("Parse config string and initialize container.");

                container = XmlConfigParser.Parse(xml);

                Log.Debug($"Container initialized successfully, total {container.Count} objects loaded.");
            }
            catch (Exception ex)
            {
                Log.Error($"Container initialized error: {ex.Message}.", ex);
                throw ex;
            }
        }

        public ObjectContainer(FileInfo configFile)
        {
            if (!configFile.Exists)
            { throw new FileNotFoundException("File cannot be found.", configFile.FullName); }

            try
            {
                Log.Debug("Parse config file and initialize container.");

                using (Stream stream = configFile.OpenRead())
                {
                    container = XmlConfigParser.Parse(stream);
                }

                Log.Debug($"Container initialized successfully, total {container.Count} objects loaded.");
            }
            catch (Exception ex)
            {
                Log.Error($"Container initialized error: {ex.Message}.", ex);
                throw ex;
            }
        }


        public object Get(string id)
        {
            if (container.TryGetValue(id, out DefinitionBase definition))
            {
                try
                {
                    Log.Debug($"Object '{id}' found.");
                    return definition.GetValue();
                }
                catch (Exception ex)
                {
                    Log.Error($"Object '{id}' initialized error: {ex.Message}.", ex);
                    throw ex;
                }
            }

            Log.Error($"Object '{id}' cannot be found.");
            throw new InvalidOperationException($"Object '{id}' cannot be found.");
        }
    }
}
