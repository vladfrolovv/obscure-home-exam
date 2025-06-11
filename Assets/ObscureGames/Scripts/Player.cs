using System.Collections;
using System.Collections.Generic;
using ObscureGames.Gameplay.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    public ScriptablePlayerProfile playerProfile;

    public string playerName = "Player 1";
    public Color playerColor = Color.blue;

    public int playerIndex;

    internal int score = 0;
    internal int bonus = 0;
    private float bonusDelay = 0;

    internal int moves = 0;

    public TextMeshProUGUI nameText;
    public Image avatarImage;

    public TextMeshProUGUI scoreText;

    public TextMeshProUGUI bonusText;
    public Animator bonusAnimator;

    public TextMeshProUGUI movesText;
    public Animator movesAnimator;
    public ProgressBarView MovesBarView;

    // Start is called before the first frame update
    void Awake()
    {
        Setup();
    }

    private void Update()
    {
        if (bonusDelay > 0) bonusDelay -= Time.deltaTime;
        else
        {
            if ( bonus > 0 )
            {
                RemoveBonus(1);
                ChangeScore(1);
            }
        }
    }

    public void Setup()
    {
        nameText.SetText(playerName);

        if ( playerProfile )
        {
            avatarImage.sprite = playerProfile.avatarIcon;
        }

        SetScore(score);
        SetMoves(0);
        SetBonus(0);
    }

    public void AddBonus(int addBonus, float setDelay)
    {
        bonus += addBonus;
        UpdateBonus();

        bonusAnimator.Play("Bounce");
        bonusAnimator.Play("Bounce2");

        bonusDelay = setDelay;
    }

    public void RemoveBonus(int removeBonus)
    {
        bonus -= removeBonus;
        UpdateBonus();
    }

    public void ChangeScore(int changeValue)
    {
        score += changeValue;

        UpdateScore();
    }

    public void SetScore(int setValue)
    {
        score = setValue;

        UpdateScore();
    }

    public void UpdateScore()
    {
        scoreText.SetText(score.ToString("000"));
    }

    public void SetBonus(int setValue)
    {
        bonus = setValue;

        UpdateBonus();
    }

    public void UpdateBonus()
    {
        bonusText.gameObject.SetActive(bonus > 0);

        bonusText.SetText("+" + bonus.ToString());
    }

    public void SetMoves(int setValue)
    {
        moves = setValue;
        UpdateMoves();
    }

    public void ChangeMoves(int changeValue)
    {
        moves += changeValue;

        if (MovesBarView) MovesBarView.ChangeProgress(changeValue);

        UpdateMoves();
    }

    public void UpdateMoves()
    {
        movesText.SetText(moves.ToString());

        movesAnimator.Play("Bounce");
        movesAnimator.Play("Bounce2");
    }
}
