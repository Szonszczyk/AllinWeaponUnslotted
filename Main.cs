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
    ItemHelper itemHelper
) : IOnLoad
{
    public Task OnLoad()
    {
        ConfigLoader configLoader = new(logger, modHelper);

        if (!configLoader.Config.ModEnabled) return Task.CompletedTask;

        ChangeItems changeItems = new(logger, databaseService, itemHelper, configLoader);
        changeItems.FckWeapons();
        changeItems.FckMods();

        Fixes fixes = new (logger, databaseService);
        fixes.RunFixes();

        var text = "Fcked: ";
        if (configLoader.Config.FckWeapons) text += "weapons, ";
        if (configLoader.Config.FckMods) text += "mods, ";
        if (configLoader.Config.FckMagazines) text += "magazines.";
        if (text == "Fcked: ") text = "";
        if (configLoader.Config.RemoveConflictingItems) text += " Removed conflicting items in mod slots. ";
        if (configLoader.Config.RemoveRequiredSlots) text += " WARN: Removed required slots!";

        logger.LogWithColor($"[{GetType().Namespace}] Mod finished loading. {text}", LogTextColor.Green);

        return Task.CompletedTask;
    }
}