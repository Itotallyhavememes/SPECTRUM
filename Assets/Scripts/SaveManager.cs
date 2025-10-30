using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using UnityEngine;
public class SaveManager
{
    public static bool SaveGame(PlayerData playerData)
    {
        string path = Path.Combine(Application.persistentDataPath, ("user" + playerData.userID + ".dat"));

        if (File.Exists(path))
        {
            File.Copy(path, (path + ".bak"), true);
            File.Delete(path);
        }

        try
        {
            File.WriteAllText(path, Encrypt(JsonConvert.SerializeObject(playerData, Formatting.Indented)));
            
        }
        catch (IOException err)
        {
            Debug.LogWarning(err);
            File.Copy((path + ".bak"), path, true);
            return false;
        }

        return true;
    }

    public static PlayerData LoadGame(int UID)
    {
        string path = Path.Combine(Application.persistentDataPath, ("user" + UID + ".dat"));
        
        if (File.Exists(path))
        {
            PlayerData dat = JsonConvert.DeserializeObject<PlayerData>(Decrypt(File.ReadAllText(path)));

            return dat;
        }
        else if (File.Exists(path + ".bak"))
        {
            File.Decrypt(path + ".bak");
            PlayerData dat = JsonConvert.DeserializeObject<PlayerData>(Decrypt(File.ReadAllText(path + ".bak")));
            return dat;
        }

        return null;
    }
    private static string Encrypt(string data)
    {
        byte[] bStr = Encoding.UTF8.GetBytes(data);
        
        for (int i = 0; i < bStr.Length; i++)
        {
            bStr[i] += 15;
        }

        Array.Reverse(bStr);
        return Convert.ToBase64String(bStr); ;
    }

    private static string Decrypt(string data)
    {
        byte[] bStr = Convert.FromBase64String(data);
        Array.Reverse(bStr);

        for (int i = 0; i < bStr.Length; i++)
        {
            bStr[i] -= 15;
        }

        return Encoding.UTF8.GetString(bStr);
    }
}
