using System;
using System.Collections.Generic;

namespace ECode.Configuration
{
    public interface IConfigProvider
    {
        event EventHandler Changed;


        ICollection<ConfigItem> GetConfigItems();
    }
}
