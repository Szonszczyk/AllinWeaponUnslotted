using AllinWeaponUnslotted.Interfaces;
using AllinWeaponUnslotted.Loaders;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;


namespace AllinWeaponUnslotted
{
    internal class ChangeItems(
        ISptLogger<AllinWeaponUnslotted> logger,
        DatabaseService databaseService,
        ItemHelper itemHelper,
        ConfigLoader configLoader
    )
    {
        private readonly Dictionary<MongoId, TemplateItem> items = databaseService.GetItems();
        private readonly ConfigData modConfig = configLoader.Config;
        private readonly Dictionary<MongoId, HashSet<MongoId>> modCache = [];

        private readonly List<string> weaponCategories = [
            "5447b5fc4bdc2d87278b4567",
            "5447b5f14bdc2d61278b4567",
            "5447b6254bdc2dc3278b4568",
            "5447b6194bdc2d67278b4567",
            "5447bed64bdc2d97278b4568",
            "5447b5e04bdc2d62278b4567",
            "5447b5cf4bdc2d65278b4567",
            "5447b6094bdc2dc3278b4567",
            "617f1ef5e8b54b0998387733",
            "5447bedf4bdc2d87278b4568"
        ];

        public void FckWeapons()
        {
            foreach (var categoryId in weaponCategories)
            {
                var weaponsInCategory = LoadFromCache(categoryId);
                foreach (var id in weaponsInCategory)
                {
                    var item = items[id];
                    if (item?.Properties?.Slots is null) continue;

                    if (modConfig.FckWeapons)
                    {
                        foreach (var slot in item.Properties.Slots)
                        {
                            // Compatibility with Definitive Weapon Variants
                            if (slot.Name == "mod_core") continue;

                            // Don't touch magazine slot if that option is disabled
                            if (slot.Name == "mod_magazine" && !modConfig.FckMagazines) continue;

                            var filtersEnumerable = slot.Properties?.Filters;
                            if (filtersEnumerable == null) continue;

                            var filters = filtersEnumerable.ToList();
                            if (filters.Count == 0) continue;

                            var categories = DeterminateSlotCategory([.. filters[0].Filter]);
                            var newFilter = new HashSet<MongoId>();

                            foreach (var category in categories)
                            {
                                newFilter.UnionWith(LoadFromCache(category));
                            }

                            filters[0].Filter = newFilter;

                            if (slot?.Properties?.Filters is null) continue;
                            slot.Properties.Filters = filters;

                            if (modConfig.RemoveRequiredSlots) slot.Required = false;
                        }
                    }
                    if (modConfig.FckMagazines && item?.Properties?.Chambers is not null)
                    {
                        foreach (var chamber in item.Properties.Chambers)
                        {
                            var filtersEnumerable = chamber.Properties?.Filters;
                            if (filtersEnumerable == null) continue;

                            var filters = filtersEnumerable.ToList();
                            if (filters.Count == 0) continue;

                            filters[0].Filter = LoadFromCache("5485a8684bdc2da71d8b4567");

                            if (chamber?.Properties?.Filters is null) continue;
                            chamber.Properties.Filters = filters;
                        }
                    }
                }
            }
        }
        public void FckMods()
        {
            foreach (var (categoryId, itemList) in modCache.ToList())
            {
                if (weaponCategories.Contains(categoryId)) continue;

                foreach (var id in itemList)
                {
                    var item = items[id];
                    if (modConfig.RemoveConflictingItems && item?.Properties?.ConflictingItems is not null) item.Properties.ConflictingItems = [];

                    if (modConfig.FckMods && item?.Properties?.Slots is not null)
                    {
                        foreach (var slot in item.Properties.Slots)
                        {
                            var filtersEnumerable = slot.Properties?.Filters;
                            if (filtersEnumerable == null) continue;

                            var filters = filtersEnumerable.ToList();
                            if (filters.Count == 0 || filters[0].Filter is null) continue;

                            var categories = DeterminateSlotCategory([.. filters[0].Filter]);
                            var newFilter = new HashSet<MongoId>();

                            foreach (var category in categories)
                            {
                                newFilter.UnionWith(LoadFromCache(category));
                            }

                            filters[0].Filter = newFilter;

                            if (slot?.Properties?.Filters is null) continue;
                            slot.Properties.Filters = filters;

                            if (modConfig.RemoveRequiredSlots) slot.Required = false;
                        }
                    }

                    if (modConfig.FckMagazines && categoryId == "5448bc234bdc2d3c308b4569" && item?.Properties?.Cartridges is not null)
                    {
                        foreach (var cartridge in item.Properties.Cartridges)
                        {
                            if (cartridge?.Properties?.Filters is null) continue;

                            var filtersEnumerable = cartridge.Properties.Filters;
                            if (filtersEnumerable == null) continue;
                            var filters = filtersEnumerable.ToList();

                            if (filters.Count == 0) continue;
                            filters[0].Filter = LoadFromCache("5485a8684bdc2da71d8b4567");

                            cartridge.Properties.Filters = filters;
                        }
                    }
                }
            }
        }

        private HashSet<MongoId> DeterminateSlotCategory(List<MongoId> oldList)
        {
            var hSet = new HashSet<MongoId>();

            foreach (var id in oldList)
            {
                items.TryGetValue(id, out var item);
                if (item is null) continue;
                hSet.Add(item.Parent);
            }

            return hSet;
        }

        private HashSet<MongoId> LoadFromCache(string category)
        {
            if (modCache.TryGetValue(category, out var list)) return list;
            var newList = itemHelper.GetItemTplsOfBaseType(category).ToHashSet();
            modCache.Add(category, newList);
            return newList;
        }
    }
}
