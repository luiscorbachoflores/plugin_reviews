using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.JellyAsk;

public class Plugin : BasePlugin<PluginConfiguration>
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public override string Name => "JellyAsk";

    public override string Description => "Permite a los usuarios pedir películas o series desde el menú, avisando en el registro de actividad.";

    public override Guid Id => Guid.Parse("c3f4a9e2-8d1b-4f6a-9e3c-7a2b5d6e8f10");

    public static Plugin? Instance { get; private set; }
}
