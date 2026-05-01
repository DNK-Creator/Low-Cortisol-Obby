using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoosterButton : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _lockImage;
    [SerializeField] private Sprite _focusedSprite;
    [SerializeField] private Sprite _unfocusedSprite;

    [SerializeField] private Image _background;
    [SerializeField] public Button _button;

    public void Initialize(Sprite iconSprite, bool isOwned)
    {
        _icon.sprite = iconSprite;
        _lockImage.SetActive(!isOwned);
    }

    public void OnSelect()
    {
        _background.sprite = _focusedSprite;
    }

    public void OnDeselect()
    {
        _background.sprite = _unfocusedSprite;
    }    
}
