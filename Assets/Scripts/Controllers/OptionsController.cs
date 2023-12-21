using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class OptionsController : MonoBehaviour
{
    [SerializeField]
    Slider volumeSlider;

    [SerializeField]
    TMP_Dropdown screenModeDropdown;

    [SerializeField]
    TMP_Dropdown resolutionDropdown;

    [SerializeField]
    Toggle vsyncToggle;

    [SerializeField]
    List<Vector2Int> defaultResolutions;

    [SerializeField]
    GameObject content;

    private SortedList<Vector2Int, Vector2Int> sortedResolutionOptions;

    private void Start()
    {
        this.SetupResolutionOptions();       

        this.InitFieldsToCurrentValues();
    }

    private void SetupResolutionOptions()
    {
        this.sortedResolutionOptions = new(new ResolutionSorter());

        //Add screen supported resolutions
        foreach (Resolution screenSupportedResolution in Screen.resolutions)
        {
            Vector2Int resolutionVector = new(screenSupportedResolution.width, screenSupportedResolution.height);
            //skip duplicates caused by multiple listings for each framerate
            if (this.sortedResolutionOptions.ContainsValue(resolutionVector))
                continue;
            this.sortedResolutionOptions.Add(resolutionVector, resolutionVector);
        }

        //Add game defined supported resolutions
        foreach (Vector2Int res in this.defaultResolutions)
        {
            if (this.sortedResolutionOptions.ContainsValue(res))
                continue;
            this.sortedResolutionOptions.Add(res, res);
        }

        //Add current resolution (might be weird because of display specific scaling and monitor swaps, so assume it is supported...)
        Vector2Int currentResolution = new(Screen.width, Screen.height);
        if (!this.sortedResolutionOptions.ContainsValue(currentResolution))
            this.sortedResolutionOptions.Add(currentResolution, currentResolution);

        List<string> resolutionOptions = this.sortedResolutionOptions.Select((res) => string.Format("{0} x {1}", res.Value.x, res.Value.y)).ToList();
        
        this.resolutionDropdown.ClearOptions();
        this.resolutionDropdown.AddOptions(resolutionOptions);
    }

    private void InitFieldsToCurrentValues()
    {
        this.volumeSlider.value = AudioManager.Singleton.GetEffectsVolume();

        Vector2Int currentResolution = new(Screen.width, Screen.height);
        int currentResIndex = this.sortedResolutionOptions.IndexOfValue(currentResolution);
        //in case resolution changed since we opened menu
        if (currentResIndex == -1)
        {
            Debug.Log("Couldn't find current resolution. This should not happen as it should be added to options when setting those up");
        }
        Debug.LogFormat("Current res : {0}, setting field to option index : {1}", currentResolution, currentResIndex);
        this.resolutionDropdown.value = currentResIndex;

        this.screenModeDropdown.value = ScreenModeToOptionIndex(Screen.fullScreenMode);
        this.vsyncToggle.isOn = QualitySettings.vSyncCount != 0;
    }

    public void ApplyVolumeChange(float value)
    {
        AudioManager.Singleton.SetVolume(value);
    }

    public void ApplyScreenMode(int optionIndex)
    {
        FullScreenMode mode = this.OptionIndexToScreenMode(optionIndex);

        Screen.fullScreenMode = mode;
    }

    private FullScreenMode OptionIndexToScreenMode(int optionIndex)
    {
        string optionAsText = screenModeDropdown.options.Select((optionData) => optionData.text).ToList<string>()[optionIndex];
        FullScreenMode newMode;
        switch (optionAsText)
        {
            case "Fullscreen":
                newMode = FullScreenMode.FullScreenWindow;
                break;
            case "Window":
                newMode = FullScreenMode.Windowed;
                break;
            default:
                throw new System.Exception("Unhandled screen mode set.");
        }
        return newMode;
    }

    private int ScreenModeToOptionIndex(FullScreenMode mode)
    {        
        for(int i = 0; i < screenModeDropdown.options.Count; i++)
        {
            if(mode == OptionIndexToScreenMode(i))
            {
                return i;
            }
        }
        throw new System.Exception("Unsupported screenmode supplied");
    }

    public void ApplyVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }

    public void ApplyResolution(int optionIndex)
    {
        Vector2Int newRes = this.sortedResolutionOptions.ElementAt(optionIndex).Key;

        Screen.SetResolution(newRes.x, newRes.y, Screen.fullScreenMode);
        Debug.LogFormat("Setting resolution : {0} x {1}", newRes.x, newRes.y);
    }
}

class ResolutionSorter : IComparer<Vector2Int>
{
    public int Compare(Vector2Int r1, Vector2Int r2)
    {
        if (r1.x != r2.x) return r1.x.CompareTo(r2.x);
        else return r1.y.CompareTo(r2.y);
    }
}
