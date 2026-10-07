using AllinWeaponUnslotted.Helpers;
using AllinWeaponUnslotted.Loaders;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace AllinWeaponUnslotted;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 97223)]
public class AllinWeaponUnslotted(
    CustomLogger logger,
    PlayerItemTemplates playerItemTemplates,
    ConfigLoader configLoader,
    Fixes fixes
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        if (!configLoader.Config.ModEnabled) return Task.CompletedTask;

        playerItemTemplates.Prepare();

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
        logger.Ok($"Mod finished loading. {text}");

        return Task.CompletedTask;
    }
}
