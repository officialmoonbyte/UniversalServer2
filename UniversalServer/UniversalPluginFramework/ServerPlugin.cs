using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonbyte.UniversalServer.PluginFramework
{
    public interface UniversalPluginFramework
    {
        string Name { get; }
        void OnLoad(string PluginSettingsDirectory);
        void Invoke(ClientContext Client, string[] RawCommand);
    }
}
