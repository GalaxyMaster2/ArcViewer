using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeSliderTooltip : MonoBehaviour
{
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI beatText;

    private Canvas parentCanvas;
    private RectTransform rectTransform;
    private RectTransform sliderRectTransform;


    public void UpdateText(float time, float beat)
    {
        ulong currentSeconds = (ulong)time % 60;
        ulong currentMinutes = (ulong)time / 60;

        string secondsString = currentSeconds >= 10 ? $"{currentSeconds}" : $"0{currentSeconds}";

        timeText.text = $"{currentMinutes}:{secondsString}";
        beatText.text = ((ulong)beat).ToString();
    }


    public void UpdatePosition()
    {
        //Need to get the exact width in pixels for mouse position to work properly
        Rect sliderRect = sliderRectTransform.rect;
        float sliderPixelWidth = sliderRect.width * parentCanvas.scaleFactor;

        float midPoint = Camera.main.pixelWidth / 2;
        float leftX = midPoint - sliderPixelWidth / 2;

        //Get mouse position relative to the slider
        float targetPos = Mathf.Clamp(Input.mousePosition.x - leftX, 0, sliderPixelWidth);

        //Calculate the time that's highlighted
        float sliderProgress = targetPos / sliderPixelWidth;
        float targetTime = AudioManager.GetSongLength() * sliderProgress;
        float targetBeat = TimeManager.BeatFromTime(targetTime);

        UpdateText(targetTime, targetBeat);

        //Scale back to canvas scale to set position
        rectTransform.anchoredPosition = new Vector2(sliderProgress * sliderRect.width, rectTransform.anchoredPosition.y);
    }


    private void Update()
    {
        UpdatePosition();
    }


    private void OnEnable()
    {
        if(!parentCanvas)
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }
        if(!rectTransform)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        if(!sliderRectTransform)
        {
            sliderRectTransform = timeSlider.GetComponent<RectTransform>();
        }

        UpdatePosition();
    }
}