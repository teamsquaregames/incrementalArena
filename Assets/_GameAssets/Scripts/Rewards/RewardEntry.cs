using System;

[Serializable]
public class RewardEntry
{
    public RewardObject rewardObject;
    public double value;

    public RewardEntry(RewardObject rewardObject, double value)
    {
        this.rewardObject = rewardObject;
        this.value = value;
    }
}
