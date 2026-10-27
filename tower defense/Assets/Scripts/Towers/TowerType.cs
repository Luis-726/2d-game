namespace TowerDefense.Towers
{
    public enum TowerType
    {
        Archer,  // Medium range, single target, fast firing
        Warrior, // Small range, omnidirectional / 360 melee cleave
        Mage     // Medium range, area-of-effect splash damage
    }

    public enum TargetPriority
    {
        First,     // Furthest along the path (lowest remaining distance)
        Last,      // Closest to the start of the path
        Strongest, // Enemy with the highest current HP
        Closest    // Enemy physically closest to the tower
    }
}
