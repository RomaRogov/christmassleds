using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    [SerializeField] private Button playerNameBtn;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Button startBtn;
    [SerializeField] private Button shopBtn;
    [SerializeField] private UIScarfShop scarfShop;
    
    private TouchScreenKeyboard touchScreenKeyboard;
    
    public void Hide()
    {
        startBtn.transform.DOScale(Vector3.zero, .4f).SetEase(Ease.InBack);
        playerNameBtn.transform.DOScale(Vector3.zero, .4f).SetEase(Ease.InBack).SetDelay(.1f);
        shopBtn.transform.DOScale(Vector3.zero, .4f).SetEase(Ease.InBack).SetDelay(.15f);
    }

    private void Start()
    {
        playerNameText.text = "Player" + Random.Range(1000, 9999);
        playerNameBtn.onClick.AddListener(() =>
        {
            touchScreenKeyboard = TouchScreenKeyboard.Open(playerNameText.text);
        });
        shopBtn.onClick.AddListener(scarfShop.Show);
        startBtn.onClick.AddListener(UIController.StartPlaying);
    }

    private void Update()
    {
        if (touchScreenKeyboard != null && touchScreenKeyboard.active)
            playerNameText.text = touchScreenKeyboard.text;
    }
}
