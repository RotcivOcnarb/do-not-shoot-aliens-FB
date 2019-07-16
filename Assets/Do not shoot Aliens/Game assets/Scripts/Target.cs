using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Analytics;

public class Target : MonoBehaviour {
	
	public GameObject carveNavmesh;
	public Animator anim;
	public float leaveDistance;
    public GameManager gameManager;
	
	public Animator tutorial;
	
	public Text scoreText;
	int score;
	
	MoveArea area;
	Transform player;

    private float timeOnCheckpoint;
	
	void Start(){
		area = GameObject.FindObjectOfType<MoveArea>();
		
		carveNavmesh.SetActive(false);
		scoreText.gameObject.SetActive(false);
	}
	
	void Update(){
		if(anim.GetBool("Open") && Vector3.Distance(transform.position, player.position) > leaveDistance){
			anim.SetBool("Open", false);
		
			StartCoroutine(LeftCircle());

            //Firebase Analytics
            FirebaseAnalytics.LogEvent("checkpoint_exit", new Parameter[] {
                new Parameter("game_id", gameManager.gameId),
                new Parameter("points", score),
                new Parameter("red_aliens", gameManager.totalRedAliens),
                new Parameter("time_on_checkpoint", timeOnCheckpoint)
            }) ;
		}
        else if (anim.GetBool("Open"))
        {
            timeOnCheckpoint += Time.deltaTime;
        }
	}
	
	public void OnTriggerEnter(Collider other){
		if(!other.gameObject.CompareTag("Player"))
			return;

        //Firebase Analytics
        timeOnCheckpoint = 0;
        FirebaseAnalytics.LogEvent("checkpoint_enter", new Parameter[] {
               new Parameter("gameId", gameManager.gameId),
               new Parameter("points", score),
               new Parameter("red_aliens", gameManager.totalRedAliens)
        }) ;

		carveNavmesh.SetActive(true);
		anim.SetBool("Open", true);
		
		player = other.gameObject.transform;
		player.GetComponent<PlayerController>().SwitchSafeState(true);
		
		AddPoints();
	}
	
	void AddPoints(){
		if(!scoreText.gameObject.activeSelf)
			scoreText.gameObject.SetActive(true);
		
		score++;
		scoreText.text = "" + score;
	}
	
	public int GetScore(){
		return score;
	}
	
	IEnumerator LeftCircle(){		
		if(!tutorial.GetBool("Invisible"))
			tutorial.SetBool("Invisible", true);
	
		yield return new WaitForSeconds(0.5f);
		
		carveNavmesh.SetActive(false);
		transform.position = area.RandomPosition();
		player.GetComponent<PlayerController>().SwitchSafeState(false);
	}
}
