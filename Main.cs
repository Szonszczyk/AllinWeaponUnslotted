using AllinWeaponUnslotted.Loaders;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace AllinWeaponUnslotted;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 97223)]
public class AllinWeaponUnslotted(
    ISptLogger<AllinWeaponUnslotted> logger,
    ModHelper modHelper,
    DatabaseService databaseService,
    ItemHelper itemHelper
) : IOnLoad
{
    public Task OnLoad()
    {
        ConfigLoader configLoader = new(logger, modHelper);

        if (!configLoader.Config.ModEnabled) return Task.CompletedTask;

        ChangeItems changeItems = new(logger, databaseService, itemHelper, configLoader);
        changeItems.LoadAttachments();
        changeItems.FckWeapons();
        changeItems.FckMods();
        changeItems.FckMagazines();

        Fixes fixes = new (logger, databaseService);
        fixes.RunFixes();

        var text = "Fcked: ";
        if (configLoader.Config.FckWeapons) text += "weapons, ";
        if (configLoader.Config.FckMods) text += "mods, ";
        if (configLoader.Config.FckChambers) text += "chambers, ";
        if (configLoader.Config.FckCalibers) text += "calibers, ";
        if (configLoader.Config.FckMagazines) text += "magazines."; 
        if (configLoader.Config.FckALL) text = "FCK THEM ALL";
        if (text == "Fcked: ") text = "";
        if (configLoader.Config.RemoveConflictingItems) text += " Removed conflicting items in mod slots.";
        if (configLoader.Config.Experimental) text += " Experimental mode has been enabled!";
        logger.LogWithColor($"[{GetType().Namespace}] Mod finished loading. {text}", LogTextColor.Green);

        return Task.CompletedTask;
    }
}