using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaleController : MonoBehaviour
{
    Image image;
    public Color normalColor;
    public Color highlightColor;
    public static int coin = 0;
    public TextMeshProUGUI coinTxt;
    public GameObject saleUIPanel;
    public RectTransform RectTransform => GetComponent<RectTransform>();

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        coinTxt.text = coin.ToString();
    }

    public void SetHighlightColor(bool isHighlight)
    {
        image.color = isHighlight ? highlightColor : normalColor;
    }

    public void ShowSaleUI(bool show)
    {
        if (saleUIPanel != null)
        {
            saleUIPanel.SetActive(show);
        }
        else
        {
            Debug.LogWarning("SaleController: saleUIPanel이 할당되지 않았습니다. Inspector를 확인하세요.");
        }
    }
}