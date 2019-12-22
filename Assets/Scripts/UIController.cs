using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private static UIController instance;
    
    [SerializeField] private Transform coin;
    [SerializeField] private Transform coinContainer;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Button resetBtn;
    [SerializeField] private TextMeshProUGUI FTUEText;
    [SerializeField] private DrawingField drawingField;
    [SerializeField] private Image fade;

    public static void FadeIn(Action callback = null)
    {
        instance.fade.color = new Color(1f, 1f, 1f, 0f);
        instance.fade.DOFade(1f, .3f).OnComplete(() => callback?.Invoke()).SetEase(Ease.OutExpo);
    }
    
    public static void FadeOut(Action callback = null)
    {
        instance.fade.color = Color.white;
        instance.fade.DOFade(0f, .3f).OnComplete(() => callback?.Invoke()).SetEase(Ease.InExpo);
    }

    public static void ResetFtue()
    {
        instance.FTUEText.transform.localScale = Vector3.one;
    }
    
    void Start()
    {
        instance = this;
        
        coinsText.text = GameController.Coins.ToString();
        GameController.CoinsChanged += coins =>
        {
            coinsText.text = coins.ToString();
            coinContainer.DOKill();
            coinContainer.DOPunchScale(Vector3.one * .1f, .5f);
        };
        resetBtn.onClick.AddListener(GameController.Restart);

        FTUEText.transform.DOShakeRotation(1f, Vector3.forward * 10f, 5).SetLoops(-1);
        drawingField.DrawingStarted = () =>
        {
            FTUEText.DOKill();
            FTUEText.transform.DOScale(Vector3.zero, .3f).SetEase(Ease.InBack);
        };
        
        fade.gameObject.SetActive(true);
        FadeOut();
    }
}
