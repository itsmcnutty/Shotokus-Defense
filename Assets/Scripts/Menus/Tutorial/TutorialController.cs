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
    public GameObject backSlideButton;
    public GameObject showTutorialPillar;
    public GameObject startWavePillar;
    public GameObject startTutorialPillar;
    public GameObject targetDummy;

    [Header("Tutorial Videos")]
    public List<TutorialSlide> rockVideos;
    public List<TutorialSlide> spikeVideos;
    public List<TutorialSlide> wallVideos;
    public List<TutorialSlide> quicksandVideos;

    private static TutorialController instance; 

    private Dictionary<TutorialSections, List<TutorialSlide>> allTutorialVideos = new Dictionary<TutorialSections, List<TutorialSlide>>();

    private TutorialSections currentSlideType;
    private List<TutorialSlide> currentSlideSet;
    private int currentSlide;
    public static bool tutorialWaveInProgress;
    private Transform dummyTransform;
    private GameObject currentTargetDummy;
    private AudioSource audioSource;
    private bool tutorialSlideDone;

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
        dummyTransform = targetDummy.transform;
        SpawnNewDummy();
        currentTargetDummy.SetActive(false);
        
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
            tutorialSlideDone = true;
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
            if(tutorialSlideDone)
            {
                backSlideText.text = "Close";
            }
            else
            {
                backSlideButton.SetActive(false);
            }
        }
        else
        {
            backSlideButton.SetActive(true);
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
        currentTargetDummy.SetActive(!currentTargetDummy.activeSelf);
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
        tutorialSlideDone = false;
        tutorialWaveInProgress = false;
        startWavePillar.SetActive(!startWavePillar.activeSelf);
        currentTargetDummy.SetActive(!currentTargetDummy.activeSelf);
        ToggleTutorialOptions();
    }

    public void StartTutorial()
    {
        startTutorialPillar.SetActive(false);
        tutorialSlideWall.SetActive(true);
    }

    public void EndTutorial()
    {
        tutorialWaveInProgress = false;
        tutorialSlideWall.SetActive(false);
        showTutorialPillar.SetActive(false);
        startWavePillar.SetActive(false);
        startTutorialPillar.SetActive(false);
    }
    
    public void SpawnNewDummy()
    {
        currentTargetDummy = Instantiate(targetDummy);
        currentTargetDummy.transform.position = dummyTransform.position;
        currentTargetDummy.transform.rotation = dummyTransform.rotation;
        currentTargetDummy.name = "Target Dummy";
        currentTargetDummy.SetActive(!currentTargetDummy.activeSelf);
    }

    public void ToggleTutorialPowerups()
    {
        switch(currentSlideType)
        {
            case TutorialSections.Wall:
                PlayerAbility.ToggleWallAbility();
                goto case TutorialSections.Quicksand;
            case TutorialSections.Quicksand:
                PlayerAbility.ToggleQuicksandAbility();
                goto case TutorialSections.Spike;
            case TutorialSections.Spike:
                PlayerAbility.ToggleSpikeAbility();
                goto default;
            default:
                PlayerAbility.ToggleRockAbility();
                break;
        }
    }
    
    private void ToggleTutorialOptions()
    {
        GameController.Instance.destroyAll(false);
        tutorialSlideWall.SetActive(!tutorialSlideWall.activeSelf);
        showTutorialPillar.SetActive(!showTutorialPillar.activeSelf);
        currentTargetDummy.SetActive(!currentTargetDummy.activeSelf);
        if(!tutorialWaveInProgress)
        {
            startWavePillar.SetActive(!startWavePillar.activeSelf);
        }
        else
        {
            Time.timeScale = (Time.timeScale + 1) % 2;
        }
        currentSlide = 0;
        SetSlideInfo();
    }
}