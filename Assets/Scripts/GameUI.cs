using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image FadePlaneImage;
    public GameObject GameOverUi;
    public RectTransform NewWaveBanner;
    public Text NewWaveTitle;
    public Text NewWaveEnemyCount;

    private Spawner _spawner;

    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }

    private void Start()
    {
        FindObjectOfType<Player>().OnDeath += OnGameOver;
    }

    private void Awake()
    {
        _spawner = FindObjectOfType<Spawner>();
        _spawner.OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        string[] numbers = { "One", "Two", "Three", "Four", "Five" };
        NewWaveTitle.text = $"- Wave {numbers[waveNumber - 1]} -";
        string enemyCountString = _spawner.Waves[waveNumber - 1].Infinite ? "Infinite" : $"{_spawner.Waves[waveNumber - 1].EnemyCount}";
        NewWaveEnemyCount.text = $"Enemies: {enemyCountString}";

        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    private IEnumerator AnimateNewWaveBanner()
    {
        float animationPercent = 0;
        float speed = 3f;
        float delayTime = 1.5f;
        int dir = 1;

        float endDelayTime = Time.time + 1 / speed + delayTime;

        while (animationPercent >= 0)
        {
            animationPercent += Time.deltaTime * speed * dir;

            if (animationPercent >= 1)
            {
                animationPercent = 1;
                if (Time.time > endDelayTime)
                {
                    dir = -1;
                }
            }

            NewWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(70, 400, animationPercent);
            yield return null;
        }
    }


    private void OnGameOver()
    {
        Cursor.visible = true;
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