using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ECode.Utility;

namespace ECode.Configuration
{
    public class XmlConfigFile : IConfigProvider
    {
        private FileSystemWatcher   watcher                 = null;
        private bool                waitLoading             = false;
        private DateTime            lastChangedTime         = DateTime.MaxValue;
        private TimeSpan            delayTriggerTime        = new TimeSpan(0, 0, 1);


        public event EventHandler Changed;

        public FileInfo ConfigFile { get; }


        public XmlConfigFile(string fileName, bool watchChanged = true)
        {
            this.ConfigFile = new FileInfo(UtilFunctions.PathFix(fileName));
            if (!this.ConfigFile.Exists)
            { throw new FileNotFoundException("File cannot be found.", this.ConfigFile.FullName); }

            if (watchChanged)
            {
                this.watcher = new FileSystemWatcher(this.ConfigFile.DirectoryName, this.ConfigFile.Name);
                this.watcher.Changed += ConfigFileWatcher_Changed;
                this.watcher.EnableRaisingEvents = true;
            }
        }


        public ICollection<ConfigItem> GetConfigItems()
        {
            using (var reader = new StreamReader(this.ConfigFile.FullName, Encoding.UTF8))
            {
                return XmlConfigParser.Parse(reader);
            }
        }

        private void ConfigFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            lastChangedTime = DateTime.Now;

            if (waitLoading)
            { return; }

            lock (this)
            {
                if (waitLoading)
                { return; }

                waitLoading = true;
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    while ((DateTime.Now - lastChangedTime) < delayTriggerTime)
                    { Task.Delay(200).Wait(); }

                    if (this.Changed != null)
                    {
                        this.Changed(this, EventArgs.Empty);
                    }
                }
                catch (Exception ex)
                { string dummy = ex.Message; }
                finally
                { waitLoading = false; }
            });
        }
    }
}
