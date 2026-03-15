namespace AllinWeaponUnslotted.Interfaces
{
    public class ConfigData
    {
        public bool ModEnabled { get; set; } = true;
        public bool FckWeapons { get; set; } = true;
        public bool FckMods { get; set; } = true;
        public bool FckMagazines { get; set; } = true;
        public bool FckChambers { get; set; } = true;
        public bool FckCalibers { get; set; } = true;
        public bool FckALL { get; set; } = false;
        public bool RemoveConflictingItems { get; set; } = true;
        public bool Experimental { get; set; } = false;
    }
}