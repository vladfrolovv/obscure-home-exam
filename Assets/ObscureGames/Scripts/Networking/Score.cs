using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Score : MonoBehaviourPunCallbacks, IPunObservable
{
    public int score = 0;
    public TextMeshProUGUI scoreText;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }

    public void GiveScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.SetText(score.ToString());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
