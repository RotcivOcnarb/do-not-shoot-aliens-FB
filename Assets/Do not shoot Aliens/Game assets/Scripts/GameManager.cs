using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Analytics;
using Firebase.Messaging;
using Firebase.InstanceId;

public class GameManager : MonoBehaviour {
	
	public Animator startPanel;
	public Animator gamePanel;
	public Animator gameOverPanel;
	public Animator startOverlay;
	public Animator tutorial;
	public Animator moveCamera;
	public Animator transition;
	
	public Text bestText;
	public Text scoreText;
	
	[HideInInspector]
	public bool gameStarted;
    public string gameId;
    public int totalRedAliens = 0;
    public int aliensKilled = 0;
    public float timePlayed = 0;
	
	Spawner spawner;
	PlayerController player;
	
	void Start(){
        Debug.Log("Start Game Manager");
        spawner = GameObject.FindObjectOfType<Spawner>();
		player = GameObject.FindObjectOfType<PlayerController>();

        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;

        FirebaseMessaging.TokenRegistrationOnInitEnabled = true;

        Debug.Log("Begin searching token");
        new System.Threading.Tasks.Task( async () => {
            string token = await FirebaseInstanceId.DefaultInstance.GetTokenAsync();
            Debug.Log(token);
        }).Start();
        
    }

    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    }

    void Update(){
        if (gameStarted)
        {
            timePlayed += Time.deltaTime;
        }

		if(Input.GetMouseButtonDown(0)){
			if(!gameStarted){
				StartGame();
			}
			else if(!gamePanel.gameObject.activeSelf){
				StartCoroutine(RestartGame());
			}
		}
	}

    string GenerateRandomString(int charAmount)
    {
        const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789"; //add the characters you want
        string ret = "";
        for (int i = 0; i < charAmount; i++)
        {
            ret += glyphs[Random.Range(0, glyphs.Length)];
        }
        return ret;
    }
	
	void StartGame(){
		gameStarted = true;
        totalRedAliens = 0;
        aliensKilled = 0;
        timePlayed = 0;

        moveCamera.SetTrigger("MoveCamera");
		
		startPanel.SetTrigger("Fade");
		startOverlay.SetTrigger("Fade");
		
		tutorial.SetBool("Invisible", false);
		
		gamePanel.SetBool("Visible", true);
		
		spawner.StartSpawning();

        //Firebase analytics
        gameId = GenerateRandomString(10);
        FirebaseAnalytics.LogEvent("game_start", new Parameter[] {
               new Parameter("gameId", gameId)
        });
	}
    
	public void GameOver(){
		if(!gamePanel.gameObject.activeSelf)
			return;
		
		Target target = GameObject.FindObjectOfType<Target>();
		int score = target.GetScore();
		
		if(score > PlayerPrefs.GetInt("Best"))
			PlayerPrefs.SetInt("Best", score);
		
		bestText.text = PlayerPrefs.GetInt("Best") + "";
		scoreText.text = score + "";
		
		gamePanel.gameObject.SetActive(false);
		gameOverPanel.SetTrigger("Game over");
		
		player.Die();
	}
	
	IEnumerator RestartGame(){
		transition.SetTrigger("Transition");
		
		yield return new WaitForSeconds(0.5f);
		
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
