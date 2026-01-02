using System;
using System.Collections.Generic;
using UnityEngine;

public class GameDataInfo
{
    public string Name { get; private set; }
    public string DataType { get; private set; }
    public string DefaultValue { get; private set; }
    public string Range { get; private set; }

    public GameDataInfo(Dictionary<string, object> row)
    {
        Name = row["List"].ToString();
        DataType = row["data type"].ToString();
        DefaultValue = row["default value"].ToString();
        Range = row["Range"].ToString();
    }

    public int GetInt()
    {
        if (int.TryParse(DefaultValue, out int result))
        {
            return result;
        }
        Debug.LogWarning($"GameDataInfo: {Name}의 값 '{DefaultValue}'를 int로 변환할 수 없습니다.");
        return 0;
    }

    public float GetFloat()
    {
        if (float.TryParse(DefaultValue, out float result))
        {
            return result;
        }
        Debug.LogWarning($"GameDataInfo: {Name}의 값 '{DefaultValue}'를 float로 변환할 수 없습니다.");
        return 0f;
    }

    public int[] GetIntArray()
    {
        try
        {
            string trimmed = DefaultValue.Trim('[', ']', '"');
            string[] parts = trimmed.Split(',');
            int[] result = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i].Trim(), out int value))
                {
                    result[i] = value;
                }
            }

            return result;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"GameDataInfo: {Name}의 배열 '{DefaultValue}' 파싱 실패: {e.Message}");
            return new int[0];
        }
    }

    public float[] GetFloatArray()
    {
        try
        {
            string trimmed = DefaultValue.Trim('[', ']', '"');
            string[] parts = trimmed.Split(',');
            float[] result = new float[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (float.TryParse(parts[i].Trim(), out float value))
                {
                    result[i] = value;
                }
            }

            return result;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"GameDataInfo: {Name}의 배열 '{DefaultValue}' 파싱 실패: {e.Message}");
            return new float[0];
        }
    }
}
