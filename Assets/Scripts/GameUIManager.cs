using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public TextMeshProUGUI ScoreTextUI = null;
    public TextMeshProUGUI DistanceTextUI = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ScoreTextUI)
        {
            ScoreTextUI.text = GameManager.Instance.Score.ToString();
        }
        if (DistanceTextUI)
        {
            DistanceTextUI.text = $"{GameManager.Instance.Distance} m";
        }
    }



}
