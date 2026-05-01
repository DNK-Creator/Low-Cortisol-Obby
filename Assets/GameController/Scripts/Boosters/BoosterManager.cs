using MoreMountains.Tools;
using System.Collections;
using UnityEngine;

public class BoosterManager : MonoBehaviour, MMEventListener<MMGameEvent>
{
    public static BoosterManager Instance;

    [SerializeField] private CharacterBooster[] _boosters;
    [SerializeField] private BoosterButton buttonPrefab;
    [SerializeField] private Transform buttonsRoot;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (var booster in _boosters) 
        {
            booster.Button._button.onClick.AddListener(() => OnBoosterSelected(booster));
        }

        CheckBoostersStatus();

    }

    [ContextMenu("SaveBoosters")]
    private void SaveBoosters()
    {
        
    }

    private void OnApplicationPause(bool pause)
    {
        SaveBoosters();
    }

    public void CheckBoostersStatus()
    {
        foreach (var booster in _boosters)
        {
            booster.isOwned = true;
            booster.InitButton();
        }
    }

    private void OnEnable()
    {
        this.MMEventStartListening();
    }

    private void OnDisable()
    {
        this.MMEventStopListening();
    }
    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == "ItemPurchased" || gameEvent.EventName == "PurchasingInitialized")
        {
            CheckBoostersStatus();
        }

        if (gameEvent.EventName == "CharacterChanged")
        {
            StartCoroutine(UpdateBoosterModel());
        }
    }

    public IEnumerator UpdateBoosterModel()
    {
        yield return null;
        foreach (var booster in _boosters)
        {
            if (booster.isSelected)
            {
                booster.OnDeselect();
                yield return null;
                booster.OnSeletct();
            }
        }
    }

    public void OnBoosterSelected(CharacterBooster booster)
    {
        foreach (var characterBooster in _boosters)
        {
            if (characterBooster == booster)
            {

                if (characterBooster.isOwned)
                {
                    if (characterBooster.isSelected)
                    {
                        characterBooster.OnDeselect();
                    }
                    else
                    { 
                        characterBooster.OnSeletct();
                    }
                }
                Debug.Log("Booster Selected " + characterBooster.Button._button.name);
            }
            else
            {
                characterBooster.OnDeselect();
            }
        }
        StartCoroutine(UpdateBoosterModel());
    }

    public void UpdateButtons()
    {
        foreach (var booster in _boosters)
        {
            booster.Button._button.onClick.RemoveAllListeners();
            
        }
    }
    void Update()
    {
        for (int i = 0; i < _boosters.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                // Ensure index is within array bounds
                if (i >= 0 && i < _boosters.Length)
                {
                    OnBoosterSelected(_boosters[i]);
                }
            }
        }
    }

}

[System.Serializable]
public class CharacterBooster
{
    public Booster booster;
    public string ID;
    public BoosterButton Button;
    public Sprite IconSprite;
    public bool isOwned;
    public bool isSelected;

    public void InitButton()
    {
        Button.Initialize(IconSprite, isOwned);
    }
    public void OnSeletct()
    {
        Button.OnSelect();
        booster?.OnActivate();
        isSelected = true;
    }

    public void OnDeselect()
    {
        Button.OnDeselect();
        booster?.OnDeactivate();
        isSelected = false;
    }
}
