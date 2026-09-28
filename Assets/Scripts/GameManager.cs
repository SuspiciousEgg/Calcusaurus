using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum GamePhase
    {
        Init,
        CoreLoop,
        Combat,
        EndCheck,
        GameOver
    }

    public enum Difficulty { Easy, Normal, Hard }

    public GamePhase currentPhase;
    public Difficulty currentDifficulty = Difficulty.Normal;

    [Header("Stats")]
    public int playerHealth;
    public int enemyHealth;
    public int maxPlayerHealth = 100;
    public int maxEnemyHealth = 100;
    public int roundCount = 0;

    [Header("UI References")]
    public TMP_Text problemText;
    public TMP_Text timerText;
    public TMP_InputField answerInput;
    public Button submitButton;
    public TMP_Text playerHPText;
    public TMP_Text enemyHPText;
    public TMP_Text resultText;
    public Button restartButton;
    public Slider playerHPSlider;
    public Slider enemyHPSlider;
    public Image playerSpriteImage;
    public Image enemySpriteImage;
    public TMP_Text titleText;
    public TMP_Text instructionsText;
    public Button startButton;
    public TMP_Text selectDifficultyText;
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;
    public Button backButton;
    public TMP_Text roundText;
    public RectTransform canvasRect;

    [Header("Audio Panel")]
    public Button audioButton;
    public GameObject audioPanel;
    public Slider volumeSlider;
    public Toggle muteToggle;
    public Button closeAudioButton;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource musicSource;
    public AudioClip hitSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;
    public AudioClip backgroundMusic;

    private float lastVolumeBeforeMute = 0.5f;
    private int correctAnswer;
    private int timeLeft;
    private Coroutine timerCoroutine;

    void Start()
    {
        startButton.onClick.AddListener(OnStartButtonPressed);
        easyButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Easy));
        normalButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Normal));
        hardButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Hard));
        backButton.onClick.AddListener(OnBackButtonPressed);
        submitButton.onClick.AddListener(OnSubmitPressed);
        restartButton.onClick.AddListener(RestartGame);

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = 0.5f;
        musicSource.Play();

        // Set initial values WITHOUT firing listeners (avoids startup chain-reaction bugs)
        volumeSlider.SetValueWithoutNotify(musicSource.volume);
        muteToggle.SetIsOnWithoutNotify(false);

        // Attach these listeners AFTER initial values are set safely
        audioButton.onClick.AddListener(OnAudioButtonPressed);
        closeAudioButton.onClick.AddListener(OnCloseAudioPressed);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        muteToggle.onValueChanged.AddListener(OnMuteToggled);

        audioPanel.SetActive(false);
        ShowTitleScreen();
    }

    void OnAudioButtonPressed()
    {
        titleText.gameObject.SetActive(false);
        instructionsText.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        audioButton.gameObject.SetActive(false);

        audioPanel.SetActive(true);
    }

    void OnCloseAudioPressed()
    {
        audioPanel.SetActive(false);

        titleText.gameObject.SetActive(true);
        instructionsText.gameObject.SetActive(true);
        startButton.gameObject.SetActive(true);
        audioButton.gameObject.SetActive(true);
    }

    void OnVolumeChanged(float value)
    {
        musicSource.volume = value;
        sfxSource.volume = value;

        if (value > 0f)
        {
            lastVolumeBeforeMute = value;
            muteToggle.SetIsOnWithoutNotify(false);
        }
    }

    void OnMuteToggled(bool isMuted)
    {
        if (isMuted)
        {
            lastVolumeBeforeMute = musicSource.volume > 0f ? musicSource.volume : lastVolumeBeforeMute;
            musicSource.volume = 0f;
            sfxSource.volume = 0f;
            volumeSlider.SetValueWithoutNotify(0f);
        }
        else
        {
            musicSource.volume = lastVolumeBeforeMute;
            sfxSource.volume = lastVolumeBeforeMute;
            volumeSlider.SetValueWithoutNotify(lastVolumeBeforeMute);
        }
    }

    void ShowTitleScreen()
    {
        titleText.gameObject.SetActive(true);
        instructionsText.gameObject.SetActive(true);
        startButton.gameObject.SetActive(true);
        audioButton.gameObject.SetActive(true);

        selectDifficultyText.gameObject.SetActive(false);
        easyButton.gameObject.SetActive(false);
        normalButton.gameObject.SetActive(false);
        hardButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        problemText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        answerInput.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);
        playerHPText.gameObject.SetActive(false);
        enemyHPText.gameObject.SetActive(false);
        playerHPSlider.gameObject.SetActive(false);
        enemyHPSlider.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);

        playerSpriteImage.color = Color.white;
        enemySpriteImage.color = Color.white;
        playerSpriteImage.gameObject.SetActive(false);
        enemySpriteImage.gameObject.SetActive(false);

        roundText.gameObject.SetActive(false);
        resultText.text = "";
    }

    void OnStartButtonPressed()
    {
        titleText.gameObject.SetActive(false);
        instructionsText.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        audioButton.gameObject.SetActive(false);

        selectDifficultyText.gameObject.SetActive(true);
        easyButton.gameObject.SetActive(true);
        normalButton.gameObject.SetActive(true);
        hardButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);
    }

    void OnBackButtonPressed()
    {
        selectDifficultyText.gameObject.SetActive(false);
        easyButton.gameObject.SetActive(false);
        normalButton.gameObject.SetActive(false);
        hardButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        ShowTitleScreen();
    }

    void OnDifficultySelected(Difficulty chosenDifficulty)
    {
        currentDifficulty = chosenDifficulty;

        selectDifficultyText.gameObject.SetActive(false);
        easyButton.gameObject.SetActive(false);
        normalButton.gameObject.SetActive(false);
        hardButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        problemText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);
        answerInput.gameObject.SetActive(true);
        submitButton.gameObject.SetActive(true);
        playerHPText.gameObject.SetActive(true);
        enemyHPText.gameObject.SetActive(true);
        playerHPSlider.gameObject.SetActive(true);
        enemyHPSlider.gameObject.SetActive(true);
        playerSpriteImage.gameObject.SetActive(true);
        enemySpriteImage.gameObject.SetActive(true);
        roundText.gameObject.SetActive(true);

        currentPhase = GamePhase.Init;
        InitGame();
    }

    void InitGame()
    {
        playerHealth = maxPlayerHealth;
        enemyHealth = maxEnemyHealth;
        roundCount = 0;

        playerSpriteImage.color = Color.white;
        enemySpriteImage.color = Color.white;

        roundText.text = "Round: " + roundCount;
        resultText.text = "";
        UpdateHPDisplay();

        currentPhase = GamePhase.CoreLoop;
        Debug.Log("Game Initialized. Dino HP: " + playerHealth + " Bossauros HP: " + enemyHealth);

        GenerateMathProblem();
    }

    void UpdateHPDisplay()
    {
        playerHPText.text = "Dino HP: " + playerHealth;
        enemyHPText.text = "Bossauros HP: " + enemyHealth;

        playerHPSlider.value = playerHealth;
        enemyHPSlider.value = enemyHealth;

        UpdateHPBarColor(playerHPSlider, playerHealth, maxPlayerHealth);
        UpdateHPBarColor(enemyHPSlider, enemyHealth, maxEnemyHealth);
    }

    void UpdateHPBarColor(Slider slider, int currentHP, int maxHP)
    {
        Image fillImage = slider.fillRect.GetComponent<Image>();
        float percent = (float)currentHP / maxHP;

        if (percent <= 0.25f)
            fillImage.color = Color.red;
        else if (percent <= 0.5f)
            fillImage.color = Color.yellow;
        else
            fillImage.color = Color.green;
    }

    void GenerateMathProblem()
    {
        int a, b;
        string operation;
        int result;

        if (currentDifficulty == Difficulty.Easy)
        {
            a = Random.Range(1, 20);
            b = Random.Range(1, 20);
            result = a + b;
            operation = "+";
        }
        else if (currentDifficulty == Difficulty.Normal)
        {
            if (roundCount < 3)
            {
                a = Random.Range(1, 20);
                b = Random.Range(1, 20);
                result = a + b;
                operation = "+";
            }
            else if (roundCount < 6)
            {
                a = Random.Range(10, 50);
                b = Random.Range(10, 50);
                result = a + b;
                operation = "+";
            }
            else
            {
                a = Random.Range(2, 12);
                b = Random.Range(2, 12);
                result = a * b;
                operation = "x";
            }
        }
        else
        {
            if (roundCount < 1)
            {
                a = Random.Range(10, 50);
                b = Random.Range(10, 50);
                result = a + b;
                operation = "+";
            }
            else
            {
                a = Random.Range(2, 12);
                b = Random.Range(2, 12);
                result = a * b;
                operation = "x";
            }
        }

        correctAnswer = result;
        problemText.text = a + " " + operation + " " + b + " = ?";
        answerInput.text = "";

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(CountdownTimer());
    }

    System.Collections.IEnumerator CountdownTimer()
    {
        int baseTime = currentDifficulty == Difficulty.Easy ? 15 :
                       currentDifficulty == Difficulty.Hard ? 7 : 10;
        int minTime = currentDifficulty == Difficulty.Easy ? 8 :
                      currentDifficulty == Difficulty.Hard ? 4 : 5;

        timeLeft = Mathf.Max(minTime, baseTime - roundCount);
        while (timeLeft > 0)
        {
            timerText.text = "Time: " + timeLeft;
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }
        timerText.text = "Time: 0";
        EvaluateAnswer(false, true);
    }

    void OnSubmitPressed()
    {
        int playerAnswer;
        bool validNumber = int.TryParse(answerInput.text, out playerAnswer);
        bool isCorrect = validNumber && playerAnswer == correctAnswer;

        EvaluateAnswer(isCorrect, false);
    }

    void EvaluateAnswer(bool isCorrect, bool timedOut)
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        currentPhase = GamePhase.Combat;

        if (isCorrect)
        {
            enemyHealth -= 10;
            roundCount++;
            roundText.text = "Round: " + roundCount;
            StartCoroutine(FlashDamage(enemySpriteImage));
            StartCoroutine(ScreenShake(canvasRect));
            sfxSource.PlayOneShot(correctSound);
            sfxSource.PlayOneShot(hitSound);
            Debug.Log("Correct! Bossauros HP: " + enemyHealth);
        }
        else
        {
            playerHealth -= 10;
            StartCoroutine(FlashDamage(playerSpriteImage));
            StartCoroutine(ScreenShake(canvasRect));
            sfxSource.PlayOneShot(wrongSound);
            sfxSource.PlayOneShot(hitSound);
            Debug.Log((timedOut ? "Timed out! " : "Wrong! ") + "Dino HP: " + playerHealth);
        }

        UpdateHPDisplay();

        currentPhase = GamePhase.EndCheck;
        CheckHealthPools();
    }

    System.Collections.IEnumerator FlashDamage(Image sprite)
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sprite.color = Color.white;
    }

    System.Collections.IEnumerator ScreenShake(RectTransform target, float duration = 0.2f, float magnitude = 15f)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            target.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPos;
    }

    void CheckHealthPools()
    {
        if (enemyHealth <= 0)
        {
            currentPhase = GamePhase.GameOver;
            EndGame(true);
        }
        else if (playerHealth <= 0)
        {
            currentPhase = GamePhase.GameOver;
            EndGame(false);
        }
        else
        {
            currentPhase = GamePhase.CoreLoop;
            GenerateMathProblem();
        }
    }

    void EndGame(bool playerWon)
    {
        resultText.text = playerWon ? "VICTORY!" : "DEFEAT!";

        problemText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        answerInput.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);

        restartButton.gameObject.SetActive(true);

        if (playerWon)
            sfxSource.PlayOneShot(victorySound);
        else
            sfxSource.PlayOneShot(defeatSound);

        Debug.Log(playerWon ? "Dino wins!" : "Bossauros wins!");
    }

    void RestartGame()
    {
        ShowTitleScreen();
    }
}