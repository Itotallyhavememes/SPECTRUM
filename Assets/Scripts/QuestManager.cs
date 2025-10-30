using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]

public struct questObjective
{
    public string objectiveDesc;
    public object item;
    public int currCount;
    public int reqCount;
}

public struct rewardItem
{
    public string name;
    public object item;
    public int amount;
}

public struct Quest
{
    public string questName;
    public List<rewardItem> rewards;
    public Dictionary<int, questObjective> questObjectives;

    delegate void OnQuestObjectiveFinished();
    delegate void OnQuestCompleted();

    delegate void OnQuestUpdated();

    /*
        C# delegate declaration:
        delegate (returnType) DelegateDeclarationName(params);
        DelegateDeclarationName DelegateName;

        Add/Remove function from Delegate:
        DelegateName += FunctionName; <- Does not require parenthesis
        
        Call delegate:
        DelegateName?.Invoke(params); // ? symbol acts as an "if !null check"

        _ Symbol allows for discarding return values from methods:
        
        _ = FunctionReturningVariable(); // Discards return value
     */

    OnQuestObjectiveFinished onQuestObjectiveFinished;
    OnQuestCompleted onQuestCompleted;
    OnQuestUpdated onQuestUpdated;
    public string GetQuestInfo()
    {
        string res = $"\t{questName} Objectives:\n";

        for (int i = 0; i < questObjectives.Count; i++)
        {
            var obj = questObjectives[i];
            res += $"\t\t{obj.objectiveDesc}: {obj.currCount}/{obj.reqCount}\n";
        }

        res += "\tRewards:\n";

        for (int i = 0; i < rewards.Count; i++)
        {
            res += $"\t\t{rewards[i].amount} {rewards[i].name}\n";
        }

        return res;
    }

    public Dictionary<int, questObjective> GetQuestObjectives() => questObjectives;

    public void UpdateObjective(int objectiveID, questObjective newData)
    {
        if (questObjectives.ContainsKey(objectiveID))
        {
            questObjectives[objectiveID] = newData;

            if (newData.currCount >= newData.reqCount)
            {
                questObjectives.Remove(objectiveID);
                onQuestObjectiveFinished?.Invoke();
            }

            onQuestUpdated?.Invoke();
        }
    }
}

public class QuestManager
{
    [JsonProperty]
    private List<Quest> quests;

    public QuestManager()
    {

    }

    public List<Quest> GetQuests() => quests;

    public void AddQuest(Quest quest)
    {
        quests.Add(quest);
    }

    public string GetAllQuestInfo()
    {
        string res = "All Quests:\n";

        foreach (var item in quests)
        {
            res += item.GetQuestInfo();
        }

        return res;
    }


}