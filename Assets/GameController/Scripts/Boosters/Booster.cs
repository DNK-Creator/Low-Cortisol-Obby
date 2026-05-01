using UnityEngine;
using UnityEngine.Events;

public class Booster : MonoBehaviour
{
    public GameObject BoosterModel;
    public BoosterHolder boosterRoot;
    public string AnimationState = "HoldingGear";
    public CharactersRoot characterRoot;
    public Animator _animator;

    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;
    public bool isSelected;

    private void Start()
    {
        if(!_animator)
            _animator = GetComponent<Animator>();
    }
    public virtual void OnActivate()
    {
        OnActivated.Invoke();
        ShowBoosterModel();
        isSelected = true;
    }

    public void ShowBoosterModel()
    {
        BoosterModel.SetActive(true);
        switch (boosterRoot)
        {
            case BoosterHolder.Chest:
                if (!characterRoot._currentCharacter.Model.ChestBone)
                    return;
                BoosterModel.transform.parent = characterRoot._currentCharacter.Model.ChestBone.transform;
                break;
            case BoosterHolder.RightHand:
                if (!characterRoot._currentCharacter.Model.RightHand)
                    return;
                BoosterModel.transform.parent = characterRoot._currentCharacter.Model.RightHand.transform;
                break;
        }
        BoosterModel.transform.localPosition = Vector3.zero;
        BoosterModel.transform.localRotation = Quaternion.Euler(Vector3.zero);
        if(!string.IsNullOrEmpty(AnimationState))
            _animator.SetBool(AnimationState, true);
        Debug.Log("Animator.SetBool " + AnimationState + " true");
    }
    public void HideBoosterModel()
    {
        BoosterModel.SetActive(false);
        BoosterModel.transform.parent = transform;
        _animator.SetBool(AnimationState, false);
    }
    public virtual void OnDeactivate()
    {
        OnDeactivated.Invoke();
        HideBoosterModel();
        isSelected = false;
    }
}

public enum BoosterHolder
{ 
    RightHand,
    Chest
}
