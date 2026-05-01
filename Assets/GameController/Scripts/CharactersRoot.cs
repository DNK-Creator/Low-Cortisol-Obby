using Invector.vCharacterController;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactersRoot : MonoBehaviour, MMEventListener<MMGameEvent>
{
    [SerializeField] private Animator _targetAnimator;
    [SerializeField] private bool _isPlayer;
    [SerializeField] private List<CharacterModel> _characters = new List<CharacterModel>();

    [SerializeField] public CharacterModel _currentCharacter;

    public IReadOnlyList<Transform> GetHands()
    {
        var hands = new Transform[_characters.Count];
        for (int i = 0; i < _characters.Count; i++)
        {
            hands[i] = _characters[i].Model.RightHand.transform;
        }
        return hands;
    }

    private vThirdPersonController _vThirdPersonController;

    private void Start()
    {
        if (_isPlayer)
            _vThirdPersonController = _targetAnimator.GetComponent<vThirdPersonController>();

        //UpdateCharacter();
    }

    public void OnEnable()
    {
        this.MMEventStartListening();
    }

    public void OnDisable()
    {
        this.MMEventStopListening();
    }
    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == "UpdateCharacter")
        {
            UpdateCharacter();
        }
        if (gameEvent.EventName == "CharacterChanged")
        {
            UpdateCharacter();
        }
    }

    public void UpdateCharacter()
    {
        foreach (var character in _characters)
        {
            character.Model.gameObject.SetActive(true);
            Debug.Log("UpdateCharacterRoot");
            _currentCharacter = character;
            if (_targetAnimator)
            {
                _targetAnimator.avatar = character.avatar;
                if (_isPlayer)
                {
                    StartCoroutine("resetCharacterController");
                }
            }
            break;
        }
    }

    IEnumerator resetCharacterController()
    {
        yield return null;
        if (!_vThirdPersonController)
            yield break;
        _vThirdPersonController.enabled = false;
        _vThirdPersonController.enabled = true;
    }
}

[System.Serializable]
public class CharacterModel
{
    public string Name;
    public CustomizableCharacter Model;
    public Avatar avatar;
}
