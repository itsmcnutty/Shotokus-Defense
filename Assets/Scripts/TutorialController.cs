using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialController : MonoBehaviour
{
    public enum TutorialSections
    {
        Rock,
        Spike,
        Quicksand,
        Wall,
        Heal
    }

    [Header("UI Elements")]
    public Text tutorialText;
    public Text nextSlideText;
    public Text backSlideText;
    public VideoPlayer tutorialVideo;
    public VideoPlayer controllerVideo;

    [Header("Tutorial Videos")]
    public List<TutorialSlide> rockVideos;
    public List<TutorialSlide> spikeVideos;
    public List<TutorialSlide> wallVideos;
    public List<TutorialSlide> quicksandVideos;
    public List<TutorialSlide> healVideos;

    private static TutorialController instance; 

    private Dictionary<TutorialSections, List<TutorialSlide>> allTutorialVideos = new Dictionary<TutorialSections, List<TutorialSlide>>();

    private List<TutorialSlide> currentSlideSet;
    private int currentSlide;

    // Instance getter and initialization
    public static TutorialController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(TutorialController)) as TutorialController;
            }
            return instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        allTutorialVideos.Add(TutorialSections.Rock, rockVideos);
        allTutorialVideos.Add(TutorialSections.Spike, spikeVideos);
        allTutorialVideos.Add(TutorialSections.Wall, wallVideos);
        allTutorialVideos.Add(TutorialSections.Quicksand, quicksandVideos);
        allTutorialVideos.Add(TutorialSections.Heal, healVideos);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SelectTutorial(TutorialSections slideSet)
    {
        List<TutorialSlide> slides;
        if (allTutorialVideos.TryGetValue(slideSet, out slides))
        {
            currentSlideSet = slides;
            currentSlide = 0;

            SetSlideInfo();
        }
    }

    public void NextSlide()
    {
        if ((currentSlide + 1) == currentSlideSet.Count)
        {
            // Hide tutorial popup
            return;
        }

        currentSlide++;
        SetSlideInfo();
    }

    private void SetSlideInfo()
    {
        TutorialSlide slide = currentSlideSet[currentSlide];
        tutorialVideo.clip = slide.video;
        controllerVideo.clip = slide.controllerInstruction;
        tutorialText.text = slide.slideTitle;

        if ((currentSlide + 1) == currentSlideSet.Count)
        {
            nextSlideText.text = "Practice";
        }
        else
        {
            nextSlideText.text = "Next";
        }

        if (currentSlide == 0)
        {
            backSlideText.text = "Close";
        }
        else
        {
            backSlideText.text = "Back";
        }
    }

    public void PreviousSlide()
    {
        if (currentSlide == 0)
        {
            // Hide tutorial popup
            return;
        }
        currentSlide--;
        SetSlideInfo();
    }
}