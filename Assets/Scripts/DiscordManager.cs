using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;
using System;

public class DiscordManager : MonoBehaviour
{
    Discord.Discord discord;
    Activity activity;

    // Start is called before the first frame update
    void Start()
    {
        discord = new Discord.Discord(1431113037359878207, (ulong)Discord.CreateFlags.NoRequireDiscord);
        ChangeActivity();
    }

    // Update is called once per frame
    void Update()
    {
        discord.RunCallbacks();
    }

    void ChangeActivity()
    {

        ActivityManager activityManager = discord.GetActivityManager();
        activity = new Discord.Activity
        {
            State = "Playing as TBD"
            //Assets = new ActivityAssets
            //{
            //    LargeImage = "spectrum_icon",
            //    LargeText = "SPECTRUM",
            //    SmallImage = "spectrum_icon",
            //    SmallText = "SPECTRUM"
            //}
        };
        activityManager.UpdateActivity(activity, (result) =>
        {
            if (result == Discord.Result.Ok)
                Debug.Log("Activity Updated.");
        });
    }

    private void OnApplicationQuit()
    {
        discord.Dispose();
    }
}
