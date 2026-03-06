using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace AllinWeaponUnslotted
{
    internal class Fixes(
        ISptLogger<AllinWeaponUnslotted> logger,
        DatabaseService databaseService
    )
    {
        private readonly HandbookBase handbook = databaseService.GetHandbook();

        private readonly List<StaticAmmoDetails> caliber127x108details =
        [
            new StaticAmmoDetails
            {
                Tpl = "5cde8864d7f00c0010373be1",
                RelativeProbability = 1
            },
            new StaticAmmoDetails
            {
                Tpl = "5d2f2ab648f03550091993ca",
                RelativeProbability = 1
            }
        ];

        private readonly List<StaticAmmoDetails> caliber30x29details =
        [
            new StaticAmmoDetails
            {
                Tpl = "5d70e500a4b9364de70d38ce",
                RelativeProbability = 1
            }
        ];

        public void RunFixes()
        {
            var locations = databaseService.GetLocations().GetDictionary();
            foreach (var (_, location) in locations)
            {
                if (location.StaticAmmo != null)
                {
                    if (!location.StaticAmmo.TryGetValue("Caliber127x108", out _)) location.StaticAmmo.Add("Caliber127x108", caliber127x108details);
                    if (!location.StaticAmmo.TryGetValue("Caliber30x29", out _)) location.StaticAmmo.Add("Caliber30x29", caliber30x29details);
                }
            }
            if (handbook.Items.Find(t => t.Id == "5d70e500a4b9364de70d38ce") is null)
            {
                handbook.Items.Add(new()
                {
                    Id = "5d70e500a4b9364de70d38ce",
                    ParentId = "5b47574386f77428ca22b33b",
                    Price = 6006
                });
            }
        }
    }
}
