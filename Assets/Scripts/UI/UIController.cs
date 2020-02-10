using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
    [SerializeField] private GameObject waitForOtherPlayer;
    [SerializeField] private UIMainMenu mainMenu;

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

    public static void StartPlaying()
    {
        instance.StartCoroutine(instance.StartPlayingInternal());
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
        
        waitForOtherPlayer.SetActive(false);
        resetBtn.gameObject.SetActive(false);
    }

    private IEnumerator StartPlayingInternal()
    {
        mainMenu.Hide();
        yield return new WaitForSeconds(.5f);
        waitForOtherPlayer.transform.localScale = Vector3.zero;
        waitForOtherPlayer.SetActive(true);
        waitForOtherPlayer.transform.DOScale(Vector3.one, .4f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(1f);
        GameController.LoadLevel();
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        waitForOtherPlayer.transform.DOScale(Vector3.zero, .4f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(.4f);
        resetBtn.gameObject.SetActive(true);
        resetBtn.transform.localScale = Vector3.zero;
        resetBtn.transform.DOScale(Vector3.one, .4f).SetEase(Ease.OutBack);
        drawingField.Show();
    }
}
