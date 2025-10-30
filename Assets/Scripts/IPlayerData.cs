[System.Serializable]
public class PlayerData
{
    public int userID;
    public string userName;

    public InventorySystem inventorySystem;

    public PlayerData()
    {
        inventorySystem = new InventorySystem();
    }
}
