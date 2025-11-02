using UnityEngine;
using System;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager Instance;

    // Events
    public event Action OnAllPetalsCollected;
    public event Action OnFairyPowerRestored;
    public event Action OnTreeActivationStarted;
    public event Action OnTreeActivated;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AllPetalsCollected()
    {
        OnAllPetalsCollected?.Invoke();
    }

    public void FairyPowerRestored()
    {
        OnFairyPowerRestored?.Invoke();
    }

    public void TreeActivationStarted()
    {
        OnTreeActivationStarted?.Invoke();
    }

    public void TreeActivated()
    {
        OnTreeActivated?.Invoke();
    }
}