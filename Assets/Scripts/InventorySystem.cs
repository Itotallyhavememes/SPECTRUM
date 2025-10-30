using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[System.Serializable]

public class InventorySystem
{
    [JsonProperty]
    private Dictionary<string, object> inventory;

    public InventorySystem()
    {
        inventory = new Dictionary<string, object>();
    }
    public void Set(string key, object value)
    {
            inventory[key] = value;
    }

    /// <summary>
    /// Retrieves the value associated with the specified key and converts it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the value should be converted.</typeparam>
    /// <param name="key">The key whose associated value is to be retrieved. The key cannot be <see langword="null"/>.</param>
    /// <returns>The value associated with the specified key, converted to the specified type <typeparamref name="T"/>. If the
    /// key does not exist in the inventory, returns the default value for the type <typeparamref name="T"/>.</returns>
    public T Get<T>(string key)
    {
        return (T)Convert.ChangeType((inventory.ContainsKey(key)) ? inventory[key] : null, typeof(T));
    }

    public Dictionary<string, object> GetInventory() { return inventory; }

}
