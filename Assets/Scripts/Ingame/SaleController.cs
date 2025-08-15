using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaleController : MonoBehaviour
{
    public static SaleController Instance { get; private set; }
    public RectTransform SalePanelRectTransform { get; private set; }

    [Header("UI References")]
    public Color normalColor;
    public Color highlightColor;
    public TextMeshProUGUI coinTxt;
    public GameObject saleUIPanel;
    public GameObject saleTextObject;

    private Image highlightImage;
    private int _coin = 0;
    private int _sellBonus = 0;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        InitializeComponents();
    }

    private void OnEnable()
    {
        MagicBookManager.OnBookEffectApplied += HandleBookEffectApplied;
    }

    private void OnDisable()
    {
        MagicBookManager.OnBookEffectApplied -= HandleBookEffectApplied;
    }

    private void InitializeComponents()
    {
        highlightImage = GetComponent<Image>();
        if(highlightImage == null)
        {
            Debug.LogError("SaleController가 붙어있는 GameObject에 Image 컴포넌트가 없습니다! 하이라이트 색상 변경 불가.");
        }

        if (saleUIPanel != null)
        {
            saleUIPanel.SetActive(false);
            SalePanelRectTransform ??= saleUIPanel.GetComponent<RectTransform>();
        }
        if (saleTextObject != null)
        {
            saleTextObject.SetActive(false);
        }
    }

    public void AddCoin(int amount, bool isAddBonus = false)
    {
        _coin += (amount + (isAddBonus ? _sellBonus : 0));
        coinTxt.text = _coin.ToString();
    }

    public bool SpendCoin(int amount)
    {
        if (_coin < amount) return false;
        _coin -= amount;
        return true;
    }

    public void ClearCoin()
    {
        _coin = 0;
        _sellBonus = 0;
    }

    public void SetHighlightColor(bool isHighlight)
    {
        if (highlightImage != null)
        {
            highlightImage.color = isHighlight ? highlightColor : normalColor;
        }
        else
        {
            Debug.LogWarning("SaleController: highlightImage (SaleController가 붙은 Image)가 null이라 하이라이트 색상 변경 불가.");
        }
    }

    public void ShowSaleUI(bool show)
    {
        // saleUIPanel 활성화 / 비활성화
        if (saleUIPanel != null)
        {
            saleUIPanel.SetActive(show);
        }
        else
        {
            Debug.LogWarning("SaleController: saleUIPanel이 할당되지 않았습니다. Inspector를 확인하세요.");
        }

        // saleTextObject 활성화 / 비활성화
        if(saleTextObject != null)
        {
            saleTextObject.SetActive(show);
        }
        else
        {
            Debug.LogWarning("SaleController: saleTextObject가 할당되지 않았습니다. Inspector를 확인하세요.");
        }
    }

    private void HandleBookEffectApplied(EBookEffectType bookEffectType, int value)
    {
        if (bookEffectType != EBookEffectType.SellBonus) return;
        _sellBonus = value;
    }
}
