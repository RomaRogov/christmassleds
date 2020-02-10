using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIScarfShop : MonoBehaviour
{
    private static UIScarfShop instance;
    
    [SerializeField] private RectTransform window;
    [SerializeField] private Button closeBtn;
    [SerializeField] private UIScarf selectedScarf;

    public static void ScarfSelected()
    {
        instance.selectedScarf.Color = GameController.ScarfColor;
    }
    
    public void Show()
    {
        window.anchoredPosition = Vector2.down * 3400;
        gameObject.SetActive(true);
        window.DOAnchorPosY(0f, .6f).SetEase(Ease.OutBack);
    }

    private void Hide()
    {
        window.DOAnchorPosY(-3400f, .3f).SetEase(Ease.InBack)
            .OnComplete(() =>gameObject.SetActive(false));
    }

    private void Start()
    {
        instance = this;
        gameObject.SetActive(false);
        closeBtn.onClick.AddListener(Hide);
        selectedScarf.Color = GameController.ScarfColor;
    }
}
