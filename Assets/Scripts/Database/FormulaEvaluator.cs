using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.Text.RegularExpressions;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

public static class FormulaEvaluator
{
    public static float EvaluateToFloat(string formula, int wave)
    {
        // wave 변수 치환
        string expr = formula.Replace("wave", wave.ToString());
        // float으로 반환하기 위해 float 나눗셈으로 강제 변환
        expr = Regex.Replace(expr, @"/\s*(\d+)", ex => $"/ ({ex.Groups[1].Value} * 1.0)");
        // % 연산 계산
        expr = Regex.Replace(expr, @"\(([^()]+)\%([^()]+)\)", ex =>
        {
            int left = Convert.ToInt32(new DataTable().Compute(ex.Groups[1].Value, ""));
            int right = Convert.ToInt32(new DataTable().Compute(ex.Groups[2].Value, ""));
            return (left % right).ToString();
        });

        // 소수점 1자리로 반올림
        var result = new DataTable().Compute(expr, "");
        return (float)Math.Round(Convert.ToDouble(result), 1);
    }

    public static int EvaluateToInt(string formula, int wave)
    {   // wave 변수 치환
        string expr = formula.Replace("wave", wave.ToString());
        // % 연산 계산
        expr = Regex.Replace(expr, @"\(([^()]+)\%([^()]+)\)", ex =>
        {
            int left = Convert.ToInt32(new DataTable().Compute(ex.Groups[1].Value, ""));
            int right = Convert.ToInt32(new DataTable().Compute(ex.Groups[2].Value, ""));
            return (left % right).ToString();
        });

        // int형으로 반환
        var result = new DataTable().Compute(expr, "");
        return Convert.ToInt32(result);
    }
}
