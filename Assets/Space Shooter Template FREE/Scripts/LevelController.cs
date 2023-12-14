using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine;

#region Serializable classes
[System.Serializable]
public class EnemyWaves 
{
    [Tooltip("time for wave generation from the moment the game started")]
    public float timeToStart;

    [Tooltip("Enemy wave's prefab")]
    public GameObject wave;
}

[System.Serializable]
public class Levels{
    public int totalWave;
    public List<GameObject> unlockedWaves;
    public float shortestDelay, longestDelay;
    public GameObject boss;
}

#endregion

public class LevelController : MonoBehaviour {

    //Serializable classes implements
    public Levels[] levels;
    public TextMeshProUGUI levelUI;
    public CanvasGroup dangerCanvas;
    public GameManager gameManager;

    public GameObject powerUp;
    public float timeForNewPowerup;
    public GameObject[] planets;
    public float timeBetweenPlanets;
    public float planetsSpeed;
    List<GameObject> planetsList = new List<GameObject>();

    Camera mainCamera;   
    private int currentLevel = 0;
    private List<GameObject> availableWaves = new List<GameObject>();

    private void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(CreateEnemyWave(levels[currentLevel]));
        StartCoroutine(PowerupBonusCreation());
        StartCoroutine(PlanetsCreation());
    }

    private void Awake(){
        gameManager.onBossKilled += ()=>{
            Invoke("StartNextLevel", 0f);
        };
    }

    private void OnDestroy(){
        gameManager.onBossKilled -= ()=>{
            Invoke("StartNextLevel", 0f);
        };
    }
    
    //Create a new wave after a delay
    IEnumerator CreateEnemyWave(Levels level) 
    {
        if(level.unlockedWaves.Count > 0) availableWaves.AddRange(level.unlockedWaves);
        for(int i = 0; i<level.totalWave; i++){
            yield return new WaitForSeconds(Random.Range(level.shortestDelay,level.longestDelay));
            if(Player.instance != null) Instantiate(availableWaves[Random.Range(0, availableWaves.Count)]);
        }
        if(level.boss == null){
            yield return new WaitForSeconds(8f);
            StartNextLevel();
        }else{
            LeanTween.alphaCanvas(dangerCanvas, 1f, 0.5f).setOnComplete(()=>{
                LeanTween.alphaCanvas(dangerCanvas, 0f, 0.5f).setDelay(2f);
            });
            yield return new WaitForSeconds(3f);
            if(Player.instance != null) Instantiate(level.boss);
        }
    }

    void StartNextLevel(){
        currentLevel++;
        levelUI.text = "Wave "+(currentLevel+1);
        LeanTween.value(gameObject, Color.white, Color.green, 0.5f).setLoopPingPong(1).setEaseOutCubic().setOnUpdateColor((value) => {
            levelUI.color = value;
        });
        LeanTween.value(gameObject, 1f, 1.3f, 0.5f).setLoopPingPong(1).setEaseOutCubic().setOnUpdate((value) => {
            levelUI.transform.localScale = Vector2.one * value;
        });
        StartCoroutine(CreateEnemyWave(levels[currentLevel]));
    }

    //endless coroutine generating 'levelUp' bonuses. 
    IEnumerator PowerupBonusCreation() 
    {
        while (true) 
        {
            yield return new WaitForSeconds(timeForNewPowerup);
            Instantiate(
                powerUp,
                //Set the position for the new bonus: for X-axis - random position between the borders of 'Player's' movement; for Y-axis - right above the upper screen border 
                new Vector2(
                    Random.Range(PlayerMoving.instance.borders.minX, PlayerMoving.instance.borders.maxX), 
                    mainCamera.ViewportToWorldPoint(Vector2.up).y + powerUp.GetComponent<Renderer>().bounds.size.y / 2), 
                Quaternion.identity
                );
        }
    }

    IEnumerator PlanetsCreation()
    {
        //Create a new list copying the arrey
        for (int i = 0; i < planets.Length; i++)
        {
            planetsList.Add(planets[i]);
        }
        yield return new WaitForSeconds(10);
        while (true)
        {
            ////choose random object from the list, generate and delete it
            int randomIndex = Random.Range(0, planetsList.Count);
            GameObject newPlanet = Instantiate(planetsList[randomIndex]);
            planetsList.RemoveAt(randomIndex);
            //if the list decreased to zero, reinstall it
            if (planetsList.Count == 0)
            {
                for (int i = 0; i < planets.Length; i++)
                {
                    planetsList.Add(planets[i]);
                }
            }
            newPlanet.GetComponent<DirectMoving>().speed = planetsSpeed;

            yield return new WaitForSeconds(timeBetweenPlanets);
        }
    }

    public void PlayAgain(){
        SceneLoader.Instance.Load(SceneManager.GetActiveScene().name);
    }
    public void BackToMain(){
        SceneLoader.Instance.Load("MainMenu");
    }
}
