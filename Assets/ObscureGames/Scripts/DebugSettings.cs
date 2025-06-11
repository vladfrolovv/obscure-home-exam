using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DebugSettings : MonoBehaviour
{
    public static DebugSettings instance;

    [Header("Game Parameters")]
    [SerializeField] private ScriptableDebugSettings[] rulesets;
    [SerializeField] private int rulesetIndex = 0;
    private ScriptableDebugSettings currentRuleSet;

    private Vector2Int gridSize;

    [SerializeField] private TMP_InputField inputGameSpeed;
    [SerializeField] private TMP_InputField inputRoundsPerMatch;
    [SerializeField] private TMP_InputField inputMovesPerRound;
    [SerializeField] private TMP_InputField inputTimePerRound;
    [SerializeField] private TMP_InputField inputGridSizeX;
    [SerializeField] private TMP_InputField inputGridSizeY;
    [SerializeField] private TMP_InputField inputGridSeed;
    [SerializeField] private TMP_InputField inputMinimumLinkSize;
    [SerializeField] private Toggle toggleAllowDiagonal;

    [SerializeField] private TMP_InputField inputExecuteTime;
    [SerializeField] private TMP_InputField inputExecuteTimeMultiplier;
    [SerializeField] private TMP_InputField inputExecuteTimeMinimum;

    [SerializeField] private TMP_InputField inputItemDropDelay;
    [SerializeField] private TMP_InputField inputItemDropTime;

    [SerializeField] private TMP_InputField inputPropellerAtLink;
    [SerializeField] private TMP_InputField inputRocketAtLink;
    [SerializeField] private TMP_InputField inputBombAtLink;
    [SerializeField] private TMP_InputField inputDiscoAtLink;

    [Header("UI References")]
    [SerializeField] private Canvas canvasOpen;
    [SerializeField] private Button buttonOpen;
    [SerializeField] private Canvas canvasPopup;
    [SerializeField] private Button buttonOK;
    [SerializeField] private Button buttonClose;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        buttonOpen.onClick.AddListener(Open);
        buttonOK.onClick.AddListener(ConfirmAndRestart);
        buttonClose.onClick.AddListener(Close);

        UpdateSettings();

        Close();
    }

    public void UpdateSettings()
    {
        currentRuleSet = rulesets[rulesetIndex];

        inputGameSpeed.text = currentRuleSet.gameSpeed.ToString();
        inputRoundsPerMatch.text = currentRuleSet.roundsPerMatch.ToString();
        inputMovesPerRound.text = currentRuleSet.movesPerRound.ToString();
        inputTimePerRound.text = currentRuleSet.timePerRound.ToString();
        inputGridSizeX.text = currentRuleSet.gridSize.x.ToString();
        inputGridSizeY.text = currentRuleSet.gridSize.y.ToString();
        inputGridSeed.text = currentRuleSet.gridSeed.ToString();
        inputMinimumLinkSize.text = currentRuleSet.minimumLinkSize.ToString();
        toggleAllowDiagonal.isOn = currentRuleSet.allowDiagonal;
        inputExecuteTime.text = currentRuleSet.executeTime.ToString();
        inputExecuteTimeMultiplier.text = currentRuleSet.executeTimeMultiplier.ToString();
        inputExecuteTimeMinimum.text = currentRuleSet.executeTimeMinimum.ToString();
        inputItemDropDelay.text = currentRuleSet.itemDropDelay.ToString();
        inputItemDropTime.text = currentRuleSet.itemDropTime.ToString();

        //if(inputPropellerAtLink) inputPropellerAtLink.text = currentRuleSet.propellerAtLink.ToString();
        //if(inputRocketAtLink) inputRocketAtLink.text = currentRuleSet.rocketAtLink.ToString();
        if(inputBombAtLink) inputBombAtLink.text = currentRuleSet.bombAtLink.ToString();
        //if(inputDiscoAtLink) inputDiscoAtLink.text = currentRuleSet.discoAtLink.ToString();
    }

    public void AssignSettings()
    {
        currentRuleSet = rulesets[rulesetIndex];

        // TimeController.instance.SetGameSpeed(currentRuleSet.gameSpeed);
        GameManager.instance.SetRoundsPerMatch(currentRuleSet.roundsPerMatch);
        GameManager.instance.SetMovesPerRound(currentRuleSet.movesPerRound);
        GameManager.instance.SetTimePerRound(currentRuleSet.timePerRound);
        GridManager.instance.SetGridSize(currentRuleSet.gridSize);
        GridManager.instance.SetGridSeed(currentRuleSet.gridSeed);
        if(GameManager.instance.playerController) GameManager.instance.playerController.SetMinimumLinkSize(currentRuleSet.minimumLinkSize);
        GridManager.instance.SetAllowDiagonal(currentRuleSet.allowDiagonal);
        if(GameManager.instance.playerController) GameManager.instance.playerController.SetExecuteTime(currentRuleSet.executeTime, currentRuleSet.executeTimeMultiplier, currentRuleSet.executeTimeMinimum);
        GridManager.instance.SetItemDropDelay(currentRuleSet.itemDropDelay);
        GridManager.instance.SetItemDropTime(currentRuleSet.itemDropTime);

        GameManager.instance.SetSpecialLink(0, currentRuleSet.bombAtLink);
    }


    void ConfirmAndRestart()
    {
        currentRuleSet = rulesets[rulesetIndex];

        if (!string.IsNullOrEmpty(inputGameSpeed.text)) currentRuleSet.gameSpeed = float.Parse(inputGameSpeed.text);

        if (!string.IsNullOrEmpty(inputRoundsPerMatch.text)) currentRuleSet.roundsPerMatch = int.Parse(inputRoundsPerMatch.text);

        if (!string.IsNullOrEmpty(inputMovesPerRound.text)) currentRuleSet.movesPerRound = int.Parse(inputMovesPerRound.text);

        if (!string.IsNullOrEmpty(inputTimePerRound.text)) currentRuleSet.timePerRound = float.Parse(inputTimePerRound.text);

        if ( !string.IsNullOrEmpty(inputGridSizeX.text) && !string.IsNullOrEmpty(inputGridSizeY.text) )
        {
            currentRuleSet.gridSize = new Vector2Int(int.Parse(inputGridSizeX.text), int.Parse(inputGridSizeY.text));
        }

        if (!string.IsNullOrEmpty(inputGridSeed.text)) currentRuleSet.gridSeed = int.Parse(inputGridSeed.text);

        if (!string.IsNullOrEmpty(inputMinimumLinkSize.text)) currentRuleSet.minimumLinkSize = int.Parse(inputMinimumLinkSize.text);

        currentRuleSet.allowDiagonal = toggleAllowDiagonal.isOn;

        if (!string.IsNullOrEmpty(inputExecuteTime.text)) currentRuleSet.executeTime = float.Parse(inputExecuteTime.text);

        if (!string.IsNullOrEmpty(inputExecuteTimeMultiplier.text)) currentRuleSet.executeTimeMultiplier = float.Parse(inputExecuteTimeMultiplier.text);

        if (!string.IsNullOrEmpty(inputExecuteTimeMinimum.text)) currentRuleSet.executeTimeMinimum = float.Parse(inputExecuteTimeMinimum.text);

        if (!string.IsNullOrEmpty(inputItemDropDelay.text)) currentRuleSet.itemDropDelay = float.Parse(inputItemDropDelay.text);

        if (!string.IsNullOrEmpty(inputItemDropTime.text)) currentRuleSet.itemDropTime = float.Parse(inputItemDropTime.text);

        //if(inputPropellerAtLink) if (!string.IsNullOrEmpty(inputPropellerAtLink.text)) currentRuleSet.propellerAtLink = int.Parse(inputPropellerAtLink.text);
        //if(inputRocketAtLink) if (!string.IsNullOrEmpty(inputRocketAtLink.text)) currentRuleSet.rocketAtLink = int.Parse(inputRocketAtLink.text);
        if(inputBombAtLink) if (!string.IsNullOrEmpty(inputBombAtLink.text)) currentRuleSet.bombAtLink = int.Parse(inputBombAtLink.text);
        //if(inputDiscoAtLink) if (!string.IsNullOrEmpty(inputDiscoAtLink.text)) currentRuleSet.discoAtLink = int.Parse(inputDiscoAtLink.text);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Open()
    {
        canvasOpen.enabled = false;
        canvasPopup.enabled = true;

        UpdateSettings();
    }

    public void Close()
    {
        canvasOpen.enabled = true;
        canvasPopup.enabled = false;
    }
}
