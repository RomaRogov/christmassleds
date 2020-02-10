using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UIScarfItem : MonoBehaviour
{
    private static List<UIScarfItem> instances = new List<UIScarfItem>();

    [SerializeField] private int cost;
    [SerializeField] private string prefsKey;
    [SerializeField] private GameObject lockedBg;
    [SerializeField] private GameObject selectedBg;
    [SerializeField] private GameObject availableBg;
    [SerializeField] private UILineRenderer scarfRenderer;
    [SerializeField] private TextMeshProUGUI priceLabel;
    private Button btn;

    private bool locked => !PlayerPrefs.HasKey(prefsKey);
    private bool canBuy => GameController.Coins >= cost;
    private bool selected => GameController.ScarfColor == scarfRenderer.color;

    void Start()
    {
        if (selected)
            PlayerPrefs.SetString(prefsKey, "yay");
        priceLabel.text = cost.ToString();
        
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (locked && canBuy)
            {
                GameController.SpendCoins(cost);
                PlayerPrefs.SetString(prefsKey, "yay");
            }

            if (locked) return;
            GameController.ScarfColor = scarfRenderer.color;
            instances.ForEach(i => i.UpdateState());
            UIScarfShop.ScarfSelected();
        });
        UpdateState();
        
        instances.Add(this);
    }

    private void UpdateState()
    {
        lockedBg.SetActive(locked);
        btn.interactable = canBuy;
        selectedBg.SetActive(selected);
        availableBg.SetActive(!locked && !selected);
    }

    private void OnDestroy()
    {
        instances.Remove(this);
    }
}
