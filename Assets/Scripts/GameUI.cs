using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image FadePlaneImage;
    public GameObject GameOverUi;

    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }

    private void Start()
    {
        FindObjectOfType<Player>().OnDeath += OnGameOver;
    }

    private void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, Color.black, 1));
        GameOverUi.SetActive(true);
    }

    private IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            FadePlaneImage.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }
}