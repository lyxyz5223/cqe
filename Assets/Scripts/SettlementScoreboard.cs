using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettlementScoreboard : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI distanceText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBtnHomeClicked()
    {
        GameManager.Instance.LoadHomepageScene();
    }

    public void OnBtnReplayClicked()
    {
        GameManager.Instance.RestartGame();
    }

    public void SetScore(long score, long distance)
    {
        scoreText.text = $"{score}";
        distanceText.text = $"{distance} m";
    }

}
