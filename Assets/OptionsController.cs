using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class OptionsController : MonoBehaviour
{
    [SerializeField]
    AudioManager audioManager;

    [SerializeField]
    TMP_Dropdown screenModeDropdown;

    [SerializeField]
    TMP_Dropdown resolutionDropdown;

    [SerializeField]
    List<Vector2Int> supportedResolutions;

    [SerializeField]
    GameObject content;

    private Dictionary<int, Vector2Int> availableResolutions;

    private void Start()
    {
        Vector2Int currentRes = new(Screen.currentResolution.width, Screen.currentResolution.height);

        this.availableResolutions = new();
        int currentResIndex = -1;
        int i = 0;
        foreach(Resolution res in Screen.resolutions)
        {
            Vector2Int resVector = new(res.width, res.height);
            if (availableResolutions.ContainsValue(resVector))
                continue;
            if (this.supportedResolutions.Contains(resVector))
            {
                if (resVector == currentRes)
                    currentResIndex = i;
                this.availableResolutions.Add(i, resVector);
                i++;
            }
        }
        this.resolutionDropdown.AddOptions(this.availableResolutions.ToList().Select((res) => string.Format("{0} x {1}", res.Value.x, res.Value.y)).ToList());
        this.resolutionDropdown.value = currentResIndex;
    }

    public void ApplySoundChange(float value)
    {
        this.audioManager.SetVolume(value);
    }

    public void ApplyScreenMode(int optionIndex)
    {
        Screen.fullScreenMode = this.OptionIndexToScreenMode(optionIndex);
    }

    private FullScreenMode OptionIndexToScreenMode(int optionIndex)
    {
        string optionAsText = screenModeDropdown.options.Select((optionData) => optionData.text).ToList<string>()[optionIndex];
        FullScreenMode newMode;
        switch (optionAsText)
        {
            case "Fullscreen":
                newMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case "Window":
                newMode = FullScreenMode.Windowed;
                break;
            case "Borderless window":
                newMode = FullScreenMode.FullScreenWindow;
                break;
            default:
                throw new System.Exception("Unhandled screen mode set.");
        }
        return newMode;

    }

    public void ApplyVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }

    public void ApplyResolution(int optionIndex)
    {
        Vector2Int newRes = this.availableResolutions[optionIndex];

        Screen.SetResolution(newRes.x, newRes.y, this.OptionIndexToScreenMode(this.screenModeDropdown.value));
        Debug.LogFormat("Setting res : {0} x {1}", newRes.x, newRes.y);
    }
}
