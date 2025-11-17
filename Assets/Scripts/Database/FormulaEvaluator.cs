using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.Text.RegularExpressions;
using System;
using System.Globalization;

public static class FormulaEvaluator
{
    // 괄호 매칭
    private static int FindMatchingLeftParen(string s, int rightPos)
    {
        int depth = 0;
        for (int i = rightPos; i >= 0; i--)
        {
            if (s[i] == ')') depth++;
            else if (s[i] == '(')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    // 괄호 매칭
    private static int FindMatchingRightParen(string s, int leftPos)
    {
        int depth = 0;
        for (int i = leftPos; i < s.Length; i++)
        {
            if (s[i] == '(') depth++;
            else if (s[i] == ')')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    // 괄호 한 쌍 찾아 지우기
    private static string TrimOuterParens(string s)
    {
        s = s.Trim();
        while (s.Length >= 2 && s[0] == '(' && s[s.Length - 1] == ')')
        {
            int match = FindMatchingRightParen(s, 0);
            if (match == s.Length - 1)
            {
                s = s.Substring(1, s.Length - 2).Trim();
                continue;
            }
            break;
        }
        return s;
    }

    // % 연산
    private static string EvaluatePercentOperators(string expr)
    {
        while (expr.Contains("%"))
        {
            int pos = expr.IndexOf('%');
            if (pos < 0) break;

            int leftStart = pos - 1;

            while (leftStart >= 0 && char.IsWhiteSpace(expr[leftStart])) leftStart--;
            if (leftStart < 0) throw new Exception("왼쪽 피연산자 없음");

            int leftEnd = leftStart;
            int leftBegin;
            if (expr[leftStart] == ')')
            {
                leftBegin = FindMatchingLeftParen(expr, leftStart);
                if (leftBegin == -1) throw new Exception("일치하지 않은 괄호");
            }
            else
            {
                leftBegin = leftStart;
                while (leftBegin >= 0)
                {
                    char c = expr[leftBegin];
                    if (c == '+' || c == '-' || c == '*' || c == '/' || c == '%' || c == '(' || c == ')')
                    {
                        leftBegin++;
                        break;
                    }
                    leftBegin--;
                }
                if (leftBegin < 0) leftBegin = 0;
                while (leftBegin < leftEnd && char.IsWhiteSpace(expr[leftBegin])) leftBegin++;
            }

            int rightStart = pos + 1;

            while (rightStart < expr.Length && char.IsWhiteSpace(expr[rightStart])) rightStart++;
            if (rightStart >= expr.Length) throw new Exception("오른쪽 피연산자 없음");

            int rightEnd;
            if (expr[rightStart] == '(')
            {
                rightEnd = FindMatchingRightParen(expr, rightStart);
                if (rightEnd == -1) throw new Exception("일치하지 않은 괄호");
            }
            else
            {
                rightEnd = rightStart;
                while (rightEnd < expr.Length)
                {
                    char c = expr[rightEnd];
                    if (c == '+' || c == '-' || c == '*' || c == '/' || c == '%' || c == '(' || c == ')')
                    {
                        rightEnd--;
                        break;
                    }
                    rightEnd++;
                }
                if (rightEnd >= expr.Length) rightEnd = expr.Length - 1;
                while (rightEnd > rightStart && char.IsWhiteSpace(expr[rightEnd])) rightEnd--;
            }

            string leftExpr = expr.Substring(leftBegin, leftEnd - leftBegin + 1).Trim();
            string rightExpr = expr.Substring(rightStart, rightEnd - rightStart + 1).Trim();

            leftExpr = TrimOuterParens(leftExpr);
            rightExpr = TrimOuterParens(rightExpr);

            if (leftExpr.Contains("%") || rightExpr.Contains("%"))
            {
                if (leftExpr.Contains("%"))
                {
                    leftExpr = EvaluatePercentOperators(leftExpr);
                }
                if (rightExpr.Contains("%"))
                {
                    rightExpr = EvaluatePercentOperators(rightExpr);
                }
                expr = expr.Substring(0, leftBegin) + leftExpr + " % " + rightExpr + expr.Substring(rightEnd + 1);
                continue;
            }

            object leftObj = new DataTable().Compute(leftExpr, "");
            object rightObj = new DataTable().Compute(rightExpr, "");

            decimal leftVal = Convert.ToDecimal(leftObj, CultureInfo.InvariantCulture);
            decimal rightVal = Convert.ToDecimal(rightObj, CultureInfo.InvariantCulture);

            if (rightVal == 0) throw new DivideByZeroException("0으로 나눔");

            decimal modResult = leftVal % rightVal;

            string modStr = modResult.ToString(CultureInfo.InvariantCulture);

            expr = expr.Substring(0, leftBegin) + modStr + expr.Substring(rightEnd + 1);
        }

        return expr;
    }

    public static float EvaluateToFloat(string formula, int wave)
    {
        // wave 치환
        string expr = formula.Replace("wave", wave.ToString());
        expr = EvaluatePercentOperators(expr);

        // 정수만 .0 붙이기 (이미 소수면 무시)
        expr = Regex.Replace(expr, @"\b\d+(?!\.\d)\b", "$0.0");

        var result = new DataTable().Compute(expr, "");
        return (float)Math.Round(Convert.ToDouble(result, CultureInfo.InvariantCulture), 1);
    }

    public static int EvaluateToInt(string formula, int wave)
    {
        // wave 치환
        string expr = formula.Replace("wave", wave.ToString());
        expr = EvaluatePercentOperators(expr);

        // 정수만 .0 붙이기 (이미 소수면 무시)
        expr = Regex.Replace(expr, @"\b\d+(?!\.\d)\b", "$0.0");

        var result = new DataTable().Compute(expr, "");
        return Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }

    public static float EvaluateTowerData(string formula, int light, int dark)
    {
        // l, d 변수 치환
        string expr = formula;
        expr = expr.Replace("l", light.ToString());
        expr = expr.Replace("d", dark.ToString());
        expr = EvaluatePercentOperators(expr);

        var result = new DataTable().Compute(expr, "");
        return Convert.ToSingle(result, CultureInfo.InvariantCulture);
    }
}
