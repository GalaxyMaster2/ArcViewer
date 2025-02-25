using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TimeSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject timeTooltip;

    private bool Playing;
    private bool clicking;
    private bool mouseOver;


    public void UpdateSlider(float beat)
    {
        slider.value = TimeManager.Progress;
    }


    public void UpdatePlaying(bool newPlaying)
    {
        Playing = newPlaying;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        clicking = true;

        //Force the song to pause if it's playing
        TimeManager.ForcePause = TimeManager.Playing;

        TimeManager.SetPlaying(false);
        TimeManager.Progress = slider.value;

        timeTooltip.SetActive(mouseOver || clicking);
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        if(TimeManager.ForcePause && TimeManager.Progress < 1)
        {
            TimeManager.ForcePause = false;
            TimeManager.SetPlaying(true);
        }
        else
        {
            //Automatically pause if setting to the end of the song
            TimeManager.ForcePause = false;
            TimeManager.SetPlaying(false);
        }
        clicking = false;

        timeTooltip.SetActive(mouseOver || clicking);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
        timeTooltip.SetActive(mouseOver || clicking);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
        timeTooltip.SetActive(mouseOver || clicking);
    }


    private void Update()
    {
        if(clicking)
        {
            TimeManager.Progress = slider.value;
        }
    }


    private void Start()
    {
        TimeManager.OnBeatChanged += UpdateSlider;
        TimeManager.OnPlayingChanged += UpdatePlaying;

        slider = GetComponent<Slider>();
    }
}