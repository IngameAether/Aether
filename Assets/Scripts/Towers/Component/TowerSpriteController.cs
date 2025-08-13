using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TowerSpriteController : MonoBehaviour
{
    protected SpriteRenderer elementRenderer;  // 원소
    protected SpriteRenderer magicCircleRenderer;  // 마법진
    protected SpriteRenderer reinforceMagicCircleRenderer;  // 강화 마법진

    [Header("Sprites Array")]
    [SerializeField] protected Sprite[] elementSprites;
    [SerializeField] protected Sprite[] levelSprites;
    [SerializeField] protected Sprite[] reinforceSprites;

    private void Awake()
    {
        if (elementRenderer == null)
        {
            var element = transform.Find("Element");
            if (element != null) elementRenderer = element.GetComponent<SpriteRenderer>();
        }

        if (magicCircleRenderer == null)
        {
            var magicCircle = transform.Find("MagicCircle");
            if (magicCircle != null) magicCircleRenderer = magicCircle.GetComponent<SpriteRenderer>();
        }

        if (reinforceMagicCircleRenderer == null)
        {
            var reinforceMagicCircle = transform.Find("ReinforceMagicCircle");
            if (reinforceMagicCircle != null) reinforceMagicCircleRenderer = reinforceMagicCircle.GetComponent<SpriteRenderer>();
        }
    }

    // 레벨에 따라 원소, 마법진 스프라이트 변경
    public void SetSpritesByLevel(int level)
    {
        if (elementSprites==null || levelSprites==null)
        {
            Debug.Log($"{gameObject.name}에 스프라이트 설정 안 되어 있음");
            return;
        }
        if (level > levelSprites.Length) return;

        if (elementRenderer != null) elementRenderer.sprite = elementSprites[level-1];
        if (magicCircleRenderer != null) magicCircleRenderer.sprite = levelSprites[level-1];
    }

    // 강화 단계에 따라 마법진 스프라이트 변경
    public void SetSpriteByReinForce(int reinforce)
    {
        if (reinforceSprites == null)
        {
            Debug.Log($"{gameObject.name}에 스프라이트 설정 안 되어 있음");
            return;
        }

        if (reinforceMagicCircleRenderer != null)
            reinforceMagicCircleRenderer.sprite = reinforceSprites[(reinforce / 5) - 1];
    }
}
