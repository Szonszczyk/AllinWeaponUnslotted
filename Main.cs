using AllinWeaponUnslotted.Loaders;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace AllinWeaponUnslotted;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 97223)]
public class AllinWeaponUnslotted(
    ISptLogger<AllinWeaponUnslotted> logger,
    ModHelper modHelper,
    DatabaseService databaseService,
    ItemHelper itemHelper,
    ConfigLoader _configLoader,
    ChangeItems _changeItems
) : IOnLoad
{
    public ConfigLoader ConfigLoaderF { get; } = _configLoader;
    public ChangeItems ChangeItemsF { get; } = _changeItems;
    

    public Task OnLoad()
    {
        if (!ConfigLoaderF.Config.ModEnabled) return Task.CompletedTask;

        ChangeItemsF.LoadDatabase(databaseService);
        ChangeItemsF.LoadAttachments();
        ChangeItemsF.FckWeapons();
        ChangeItemsF.FckMods();
        ChangeItemsF.FckBullets();

        Fixes fixes = new (logger, databaseService);
        fixes.RunFixes();

        var text = "Fcked: ";
        if (ConfigLoaderF.Config.FckWeapons) text += "weapons, ";
        if (ConfigLoaderF.Config.FckMods) text += "mods, ";
        if (ConfigLoaderF.Config.FckBullets) text += "bullets, ";
        if (ConfigLoaderF.Config.FckMagazines) text += "magazines.";
        if (ConfigLoaderF.Config.FckALL) text = "FCK THEM ALL";
        if (text == "Fcked: ") text = "";
        if (ConfigLoaderF.Config.RemoveConflictingItems) text += " Removed conflicting items in mod slots. ";
        if (ConfigLoaderF.Config.RemoveRequiredSlots) text += " WARN: Removed required slots!";

        logger.LogWithColor($"[{GetType().Namespace}] Mod finished loading. {text}", LogTextColor.Green);

        return Task.CompletedTask;
    }
}
