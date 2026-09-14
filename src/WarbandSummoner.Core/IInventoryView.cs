namespace WarbandSummoner.Core
{
    /// <summary>
    /// The only thing the resolver needs to know about an inventory. The
    /// plugin adapts <c>Player.m_inventory</c>; tests use a dictionary.
    /// Consumption is deliberately not here — the resolver never spends.
    /// </summary>
    public interface IInventoryView
    {
        /// <summary>How many of the item with this prefab name the player holds. Never negative.</summary>
        int CountOf(string itemPrefab);
    }
}
