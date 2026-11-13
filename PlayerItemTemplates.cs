using AllinWeaponUnslotted.Loaders;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace AllinWeaponUnslotted;

[Injectable(InjectionType.Singleton)]
public class PlayerItemTemplates(
    TemplateTable templateTable,
    ConfigLoader configLoader,
    ICloner cloner,
    HttpResponseUtil httpResponseUtil
)
{
    // Null when the mod is disabled; the router then returns SPT's normal item data.
    public string? Response { get; private set; }

    // Keep an existing response when disabled, or serialize normal server items if no router supplied one.
    public string GetResponse(string? originalResponse)
    {
        if (Response is not null) return Response;
        if (!string.IsNullOrEmpty(originalResponse)) return originalResponse;

        // Use the same serializer as Prepare, without depending on DataCallbacks method signatures.
        return httpResponseUtil.GetUnclearedBody(templateTable.Items);
    }

    // Copy all nested item data, relax only that copy, and cache the client response.
    public void Prepare()
    {
        if (!configLoader.Config.ModEnabled) return;

        var playerItems = cloner.Clone(templateTable.Items)
            ?? throw new InvalidOperationException("Could not copy item templates for All in Weapon.");

        new ChangeItems(playerItems, configLoader.Config).ApplyChanges();
        Response = httpResponseUtil.GetUnclearedBody(playerItems);
    }
}
