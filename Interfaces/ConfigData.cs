namespace AllinWeaponUnslotted.Interfaces
{
    public class ConfigData
    {
        public bool ModEnabled { get; set; } = true;
        public bool FckWeapons { get; set; } = true;
        public bool FckMods { get; set; } = true;
        public bool FckMagazines { get; set; } = true;
        public bool RemoveConflictingItems { get; set; } = true;
        public bool RemoveRequiredSlots { get; set; } = false;
    }
}