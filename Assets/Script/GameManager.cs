using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives a single level's 10-question round, matching the "Tahap 2 / 02:35 / 120 / hearts" UI shown in the mockup.
/// The timer is ONE continuous countdown for the whole level (not per-question), driven by a coroutine
/// that writes to a TMP_Text every frame using Time.deltaTime.
/// Attach to an empty GameObject in the scene and wire up the fields in the Inspector.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private MathOperation operationType = MathOperation.Add;
    [SerializeField] private int level = 1;               // "Tahap 1"
    [SerializeField] private int questionsPerLevel = 10;
    [SerializeField] private int startingHearts = 3;
    [SerializeField] private int pointsPerCorrectAnswer = 10;

    [Header("Timer - total seconds for the WHOLE level, index 0 = Level 1")]
    [Tooltip("Level 1 gets the MOST time, Level 10 gets the LEAST - harder levels are more time-pressured.")]
    [SerializeField]
    private float[] levelTimeSeconds = new float[10]
    {
        150f, 140f, 130f, 120f, 110f, 100f, 90f, 80f, 70f, 60f
    };

    [Header("UI - Header")]
    [SerializeField] private TMP_Text levelLabel;          // "Tahap 1"
    [SerializeField] private TMP_Text timerLabel;           // "00:00"
    [SerializeField] private TMP_Text scoreLabel;            // "0"
    [SerializeField] private Image[] heartIcons;             // 3 heart images
    [SerializeField] private Sprite heartFullSprite;          // red filled heart
    [SerializeField] private Sprite heartEmptySprite;         // gray/outline heart

    [Header("UI - Question Card")]
    [SerializeField] private TMP_Text progressLabel;         // "1 / 10"
    [SerializeField] private TMP_Text questionLabel;          // "456 + 378 = ?"
    [SerializeField] private Button[] answerButtons;           // 4 buttons: A B C D
    [SerializeField] private TMP_Text[] answerLabels;           // text inside each button

    private QuestionGenerator _generator;
    private MathQuestion _currentQuestion;
    private int _questionIndex;      // 0-based, shown as +1
    private int _score;
    private int _correctCount;
    private int _heartsRemaining;
    private float _timeRemaining;
    private Coroutine _timerRoutine;
    private bool _roundEnded;

    private void Awake()
    {
        _generator = new QuestionGenerator();
    }

    private void Start()
    {
        _score = 0;
        _correctCount = 0;
        _heartsRemaining = startingHearts;
        _questionIndex = 0;
        _roundEnded = false;

        UpdateHearts();
        UpdateScore();
        levelLabel.text = $"Tahap {level}";

        // Start the ONE level-wide countdown here, not per question
        _timeRemaining = GetTimeForLevel(level);
        _timerRoutine = StartCoroutine(LevelTimerTick());

        LoadNextQuestion();
    }

    private float GetTimeForLevel(int lvl)
    {
        int idx = Mathf.Clamp(lvl - 1, 0, levelTimeSeconds.Length - 1);
        return levelTimeSeconds[idx];
    }

    private void LoadNextQuestion()
    {
        if (_roundEnded) return;

        if (_questionIndex >= questionsPerLevel)
        {
            EndRound(finishedInTime: true);
            return;
        }

        _currentQuestion = _generator.Generate(operationType, level);
        _questionIndex++;

        progressLabel.text = $"{_questionIndex} / {questionsPerLevel}";
        questionLabel.text = _currentQuestion.QuestionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int optionValue = _currentQuestion.Options[i];
            answerLabels[i].text = optionValue.ToString();

            Button btn = answerButtons[i];
            btn.onClick.RemoveAllListeners();
            btn.interactable = true;
            btn.onClick.AddListener(() => OnAnswerSelected(optionValue));
        }
    }

    private void OnAnswerSelected(int selectedValue)
    {
        if (_roundEnded) return;

        foreach (var btn in answerButtons) btn.interactable = false; // lock while resolving

        bool isCorrect = selectedValue == _currentQuestion.CorrectAnswer;
        if (isCorrect)
        {
            _correctCount++;
            _score += pointsPerCorrectAnswer;
            UpdateScore();
        }
        else
        {
            _heartsRemaining = Mathf.Max(0, _heartsRemaining - 1);
            UpdateHearts();
        }

        // TODO: play your green-check / red-X feedback animation here
        // e.g. FeedbackAnimator.Show(isCorrect);

        StartCoroutine(NextQuestionAfterDelay(0.8f));
    }

    private IEnumerator NextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_roundEnded) yield break;

        if (_heartsRemaining <= 0)
        {
            EndRound(finishedInTime: false); // ran out of hearts, not time - still "didn't finish cleanly"
            yield break;
        }

        LoadNextQuestion();
    }

    /// <summary>
    /// The single continuous countdown for the entire level.
    /// Runs every frame via Time.deltaTime and writes straight into the TMP_Text field -
    /// this is what makes it a real-time, live-updating timer instead of a static label.
    /// </summary>
    private IEnumerator LevelTimerTick()
    {
        while (_timeRemaining > 0f && !_roundEnded)
        {
            _timeRemaining -= Time.deltaTime;
            int mins = Mathf.FloorToInt(Mathf.Max(0, _timeRemaining) / 60f);
            int secs = Mathf.FloorToInt(Mathf.Max(0, _timeRemaining) % 60f);
            timerLabel.text = $"{mins:00}:{secs:00}";
            yield return null;
        }

        if (!_roundEnded)
        {
            // Time fully ran out before finishing all questions
            timerLabel.text = "00:00";
            EndRound(finishedInTime: false);
        }
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            bool isFilled = i < _heartsRemaining;

            if (heartFullSprite != null && heartEmptySprite != null)
            {
                // Preferred: swap to a grayed-out/empty heart sprite (keeps layout stable)
                heartIcons[i].sprite = isFilled ? heartFullSprite : heartEmptySprite;
                heartIcons[i].enabled = true;
            }
            else
            {
                // Fallback if no empty-heart sprite assigned: just hide it
                heartIcons[i].enabled = isFilled;
            }
        }
    }

    private void UpdateScore() => scoreLabel.text = _score.ToString();

    /// <summary>
    /// Ends the round - either because all 10 questions were answered in time,
    /// hearts ran out, or the level-wide timer hit zero.
    /// Computes final star rating: 3 stars only possible if the player actually
    /// finished all questions before time ran out.
    /// </summary>
    private void EndRound(bool finishedInTime)
    {
        if (_roundEnded) return;
        _roundEnded = true;

        if (_timerRoutine != null) StopCoroutine(_timerRoutine);
        foreach (var btn in answerButtons) btn.interactable = false;

        int starRating = ComputeStarRating(_correctCount, questionsPerLevel, finishedInTime);

        Debug.Log($"Round ended. Correct: {_correctCount}/{questionsPerLevel}, " +
                  $"Answered: {_questionIndex}/{questionsPerLevel}, " +
                  $"FinishedInTime: {finishedInTime}, Score: {_score}, Stars: {starRating}/3");

        if (_heartsRemaining <= 0)
        {
            OnGameOver();
        }

        // TODO: show your results screen here, e.g.:
        // ResultsScreen.Show(_score, starRating, _correctCount, questionsPerLevel);
    }

    private void OnGameOver()
    {
        Debug.Log("Out of hearts - game over.");
        // TODO: show game-over screen, offer retry
    }

    /// <summary>
    /// Star rating based on accuracy across the FULL question set (unanswered = wrong),
    /// capped at 2 stars if the player didn't finish all questions within the time limit.
    /// </summary>
    private int ComputeStarRating(int correctAnswers, int totalQuestions, bool finishedInTime)
    {
        float accuracy = totalQuestions > 0 ? (float)correctAnswers / totalQuestions : 0f;

        int stars;
        if (accuracy >= 0.9f) stars = 3;
        else if (accuracy >= 0.6f) stars = 2;
        else if (accuracy > 0f) stars = 1;
        else stars = 0;

        // Can't earn a perfect 3 stars unless the player actually completed
        // the level within the time limit (i.e. didn't time out or run out of hearts).
        if (!finishedInTime && stars > 2)
        {
            stars = 2;
        }

        return stars;
    }
}