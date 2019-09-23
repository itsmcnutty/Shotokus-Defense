using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Valve.VR;

public class TutorialController : MonoBehaviour
{
    public enum TutorialSections
    {
        Rock,
        Spike,
        Quicksand,
        Wall,
        None
    }

    [Header("Steam VR")]
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean grabAction;

    [Header("UI Elements")]
    public Text tutorialText;
    public Text nextSlideText;
    public Text backSlideText;
    public VideoPlayer tutorialVideo;
    public VideoPlayer controllerVideo;

    
    [Header("Game Objects")]
    public GameObject tutorialSlideWall;
    public GameObject showTutorialPillar;
    public GameObject startWavePillar;

    [Header("Tutorial Videos")]
    public List<TutorialSlide> rockVideos;
    public List<TutorialSlide> spikeVideos;
    public List<TutorialSlide> wallVideos;
    public List<TutorialSlide> quicksandVideos;
    public List<TutorialSlide> healVideos;

    private static TutorialController instance; 

    private Dictionary<TutorialSections, List<TutorialSlide>> allTutorialVideos = new Dictionary<TutorialSections, List<TutorialSlide>>();

    private TutorialSections currentSlideType;
    private List<TutorialSlide> currentSlideSet;
    private int currentSlide;
    private bool tutorialWaveInProgress;
    private AudioSource audioSource;

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
        audioSource = GetComponent<AudioSource>();
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
            currentSlideType = slideSet;
            currentSlideSet = slides;
            currentSlide = 0;

            SetSlideInfo();
        }

        switch(slideSet)
        {
            case TutorialSections.Rock:
                PlayerAbility.ToggleRockAbility();
                break;
            case TutorialSections.Spike:
                PlayerAbility.ToggleSpikeAbility();
                break;
            case TutorialSections.Quicksand:
                PlayerAbility.ToggleQuicksandAbility();
                break;
            case TutorialSections.Wall:
                PlayerAbility.ToggleWallAbility();
                break;
        }
    }

    public void NextSlide()
    {
        if ((currentSlide + 1) == currentSlideSet.Count)
        {
            ToggleTutorialOptions();
            return;
        }

        currentSlide++;
        SetSlideInfo();
    }

    public void PreviousSlide()
    {
        if (currentSlide == 0)
        {
            ToggleTutorialOptions();
            return;
        }
        currentSlide--;
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

    public void ShowTutorial()
    {
        ToggleTutorialOptions();
    }

    public void StartWave()
    {
        // Play sound
        audioSource.Play();
        
        // Begin wave
        GameController.Instance.TogglePauseWaveSystem();
        startWavePillar.SetActive(!startWavePillar.activeSelf);
        tutorialWaveInProgress = true;
    }

    public bool TutorialWaveInProgress()
    {
        return tutorialWaveInProgress;
    }

    public void SetNextTutorial()
    {
        switch(currentSlideType)
        {
            case TutorialSections.Rock:
                SelectTutorial(TutorialSections.Spike);
                break;
            case TutorialSections.Spike:
                SelectTutorial(TutorialSections.Quicksand);
                break;
            case TutorialSections.Quicksand:
                SelectTutorial(TutorialSections.Wall);
                break;
        }
        tutorialWaveInProgress = false;
        startWavePillar.SetActive(!startWavePillar.activeSelf);
        ToggleTutorialOptions();
    }

    public void EndTutorial()
    {
        tutorialWaveInProgress = false;
        tutorialSlideWall.SetActive(false);
        showTutorialPillar.SetActive(false);
        startWavePillar.SetActive(false);
    }

    private void ToggleTutorialOptions()
    {
        tutorialSlideWall.SetActive(!tutorialSlideWall.activeSelf);
        showTutorialPillar.SetActive(!showTutorialPillar.activeSelf);
        if(!tutorialWaveInProgress)
        {
            startWavePillar.SetActive(!startWavePillar.activeSelf);
        }
        else
        {
            Time.timeScale = (Time.timeScale + 1) % 2;
        }
        MenuUIController.Instance.ToggleLaser();
        currentSlide = 0;
        SetSlideInfo();
    }
}