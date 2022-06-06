using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Example.Scripts;
using UnityEngine;

public class FilterController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private LegacyPlayerInput _playerInput;

    [Header("UI")] 
    [SerializeField] private GameObject uiParent;
    
    
    private void Awake()
    {
        _playerInput.InputModeChanged += OnToggleFilter;
    }

    private void OnToggleFilter(InputMode inputMode)
    {
        uiParent.SetActive(inputMode == InputMode.UserInterface);
    }

    /// <summary>
    /// Called by ui
    /// </summary>
    public void OnInputChanged(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            MonitoringUI.ResetFilter();
        }
        else
        {
            MonitoringUI.Filter(input);
        }
    }
}
