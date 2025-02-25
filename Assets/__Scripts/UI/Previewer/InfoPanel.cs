using UnityEngine;
using TMPro;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI authorText;
    [SerializeField] private TextMeshProUGUI songText;
    [SerializeField] private TextMeshProUGUI mapperText;

    private BeatmapInfo info;


    public void UpdateText(BeatmapInfo newInfo)
    {
        info = newInfo;

        authorText.text = info._songAuthorName;
        songText.text = $"{info._songName} <i><size=70%>{info._songSubName}";
        mapperText.text = $"[{info._levelAuthorName}]";
    }


    private void OnEnable()
    {

        BeatmapManager.OnBeatmapInfoChanged += UpdateText;

        UpdateText(BeatmapManager.Info ?? new BeatmapInfo());
    }


    private void OnDisable()
    {
        BeatmapManager.OnBeatmapInfoChanged -= UpdateText;
    }
}