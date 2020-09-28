using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Syncfusion.Blazor;

namespace StationAssistant.Shared
{
    public class SyncfusionLocalizer: ISyncfusionStringLocalizer
    {
        public ResourceManager ResourceManager
        {
            get
            {
                return StationAssistant.Resources.SfResources.ResourceManager;
            }
        }

        public string GetText(string key)
        {
            return this.ResourceManager.GetString(key);
        }
    }
}
