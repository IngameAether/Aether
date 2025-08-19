[System.Serializable]
public struct StatValue
{
    public float baseValue;
    public float reinforceCoef;

    public float CalculateStat(int reinforceLevel)
    {
        return baseValue + (reinforceCoef * reinforceLevel);
    }
}
