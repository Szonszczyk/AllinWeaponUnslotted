using AllinWeaponUnslotted.Interfaces;
using AllinWeaponUnslotted.Loaders;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

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

        private readonly HashSet<string> attachmentCategories = [
            "5447b5fc4bdc2d87278b4567",
            "55818a594bdc2db9688b456a",
            "555ef6e44bdc2de9068b457e",
            "5448bc234bdc2d3c308b4569",
            "55818a304bdc2db5418b457d",
            "550aa4cd4bdc2dd8348b456c",
            "55818a684bdc2ddd698b456d",
            "55818a6f4bdc2db9688b456b",
            "55818ad54bdc2ddc698b4569",
            "55818ae44bdc2dde698b456c",
            "55818aeb4bdc2ddc698b456a",
            "55818b224bdc2dde698b456f",
            "55818a104bdc2db9688b4569",
            "56ea9461d2720b67698b456f",
            "55818ac54bdc2d5b648b456e",
            "55818af64bdc2d5b648b4570",
            "550aa4bf4bdc2dd6348b456b",
            "550aa4dd4bdc2dc9348b4569",
            "55818b014bdc2ddc698b456b",
            "5447b5f14bdc2d61278b4567",
            "55818add4bdc2d5b648b456f",
            "55818acf4bdc2dde698b456b",
            "55818b164bdc2ddc698b456c",
            "55802f3e4bdc2de7118b4584",
            "5447b6254bdc2dc3278b4568",
            "5a74651486f7744e73386dd1",
            "5447b6194bdc2d67278b4567",
            "5447bed64bdc2d97278b4568",
            "55818afb4bdc2dde698b456d",
            "5447b5e04bdc2d62278b4567",
            "5447b5cf4bdc2d65278b4567",
            "5447b6094bdc2dc3278b4567",
            "617f1ef5e8b54b0998387733",
            "610720f290b75a49ff2e5e25",
            "627a137bf21bc425b06ab944",
            "5447bedf4bdc2d87278b4568"
        ];

        public void LoadAttachments()
        {
            foreach (var categoryId in attachmentCategories)
            {
                LoadFromCache(categoryId);
            }
        }

        public void FckWeapons()
        {
            foreach (var categoryId in weaponCategories)
            {
                var weaponsInCategory = LoadFromCache(categoryId);
                foreach (var id in weaponsInCategory)
                {
                    var item = items[id];
                    if (item?.Properties?.Slots is null) continue;

                    foreach (var slot in item.Properties.Slots)
                    {
                        // Compatibility with Definitive Weapon Variants
                        if (slot.Name == "mod_core") continue;

                        // Don't touch magazine slot if that option is disabled
                        if (slot.Name == "mod_magazine" && !modConfig.FckCalibers) continue;

                        if (slot.Name != "mod_magazine" && !modConfig.FckWeapons) continue;

                        var filtersEnumerable = slot.Properties?.Filters;
                        if (filtersEnumerable == null) continue;

                        var filters = filtersEnumerable.ToList();
                        if (filters.Count == 0) continue;

                        var categories = DeterminateSlotCategory([.. filters[0].Filter]);

                        foreach (var category in categories)
                        {
                            if (modConfig.FckALL)
                            {
                                filters[0]?.Filter?.UnionWith([.. attachmentCategories]);
                                continue;
                            }
                            if (modConfig.Experimental)
                            {
                                filters[0]?.Filter?.UnionWith([category]);
                                LoadFromCache(category);
                            }
                            else
                            {
                                filters[0]?.Filter?.UnionWith(LoadFromCache(category));
                            }
                        }

                        if (slot?.Properties?.Filters is null) continue;
                        slot.Properties.Filters = filters;

                    }
                    if (modConfig.FckChambers && item?.Properties?.Chambers is not null)
                    {
                        foreach (var chamber in item.Properties.Chambers)
                        {
                            var filtersEnumerable = chamber.Properties?.Filters;
                            if (filtersEnumerable == null) continue;

                            var filters = filtersEnumerable.ToList();
                            if (filters.Count == 0) continue;

                            if (modConfig.Experimental)
                            {
                                filters[0]?.Filter?.Add("5485a8684bdc2da71d8b4567");
                            }
                            else
                            {
                                filters[0].Filter = LoadFromCache("5485a8684bdc2da71d8b4567");
                            }

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

                            foreach (var category in categories)
                            {
                                if (modConfig.FckALL)
                                {
                                    filters[0]?.Filter?.UnionWith([.. attachmentCategories]);
                                    continue;
                                }
                                if (modConfig.Experimental)
                                {
                                    filters[0]?.Filter?.UnionWith([category]);
                                }
                                else
                                {
                                    filters[0]?.Filter?.UnionWith(LoadFromCache(category));
                                }
                            }

                            if (slot?.Properties?.Filters is null) continue;
                            slot.Properties.Filters = filters;
                        }
                    }
                }
            }
        }

        public void FckMagazines()
        {
            if (!modConfig.FckMagazines) return;
            var magList = LoadFromCache("5448bc234bdc2d3c308b4569");

            foreach (var id in magList)
            {
                var item = items[id];
                if (item?.Properties?.Cartridges is not null)
                {
                    foreach (var cartridge in item.Properties.Cartridges)
                    {
                        if (cartridge?.Properties?.Filters is null) continue;

                        var filtersEnumerable = cartridge.Properties.Filters;
                        if (filtersEnumerable == null) continue;
                        var filters = filtersEnumerable.ToList();

                        if (filters.Count == 0) continue;

                        if (modConfig.Experimental)
                        {
                            filters[0]?.Filter?.Add("5485a8684bdc2da71d8b4567");
                        }
                        else
                        {
                            filters[0].Filter = LoadFromCache("5485a8684bdc2da71d8b4567");
                        }

                        cartridge.Properties.Filters = filters;
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
