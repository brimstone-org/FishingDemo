using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;


/// <summary>
/// main game logic controller
/// </summary>
public class GameController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject tutorialText;
    [SerializeField]
    private Transform playerOneSpawnPoint; //where player 1 spawns
    [SerializeField]
    private Transform playerTwoSpawnPoint; //where player 2 spawns
    private PhotonView myView; //photonView reference
    [SerializeField]
    private GameObject goldFishPrefab;
    [SerializeField]
    private GameObject yellowFishPrefab;
    [SerializeField]
    private GameObject greenFishPrefab;
    [SerializeField]
    private int maxFishNumber = 7; //how many fish on screen at the same time
    private List<Transform> wayPoints = new List<Transform>(); //waypoints between which the fish can patrol



    public static bool canStartGame; //a bool to determine if all players are connected so we can start spawning
    public static GameController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        myView = GetComponent<PhotonView>();
        CreatePlayer(); //spawns the player
      
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Waypoint");  // Search for all waypoints.
        for (int i = 0; i < gameObjects.Length; i++) //store the waypoints
        {
            wayPoints.Add(gameObjects[i].transform);
        }

        if (canStartGame) //if both players are spawned, start creating fish
        {
            for (int i = 0; i < maxFishNumber; i++)
            {
                SpawnFish();

            }
            myView.RPC("RPC_StartGame", RpcTarget.All);
        }
      

    }

  

    /// <summary>
    /// spawns player via Photon
    /// </summary>
    private void CreatePlayer()
    {
        Transform spawnPos = playerOneSpawnPoint;

        if (!PhotonNetwork.IsMasterClient)
        {
            spawnPos = playerTwoSpawnPoint;
        }
        var player = PhotonNetwork.Instantiate("Player", spawnPos.position, Quaternion.identity,0, new object[] { "AnyData" });
        player.transform.rotation = spawnPos.rotation ;
       
    }


    /// <summary>
    /// spawns an individual fish via photon for syncing
    /// </summary>
    public  void SpawnFish()
    {
        int fishToSpawn = Random.Range(0, 3);
        int randomPointToSpawn = Random.Range(0, wayPoints.Count);
        switch (fishToSpawn)
        {
            case 0:
                PhotonNetwork.Instantiate("GoldFish", wayPoints[randomPointToSpawn].position, Quaternion.identity);
                break;
            case 1:
                PhotonNetwork.Instantiate("YellowFish", wayPoints[randomPointToSpawn].position, Quaternion.identity);
                break;
            case 2:
                PhotonNetwork.Instantiate("GreenFish", wayPoints[randomPointToSpawn].position, Quaternion.identity);
                break;

        }
        
    }


    /// <summary>
    /// disables the tutorial text after the first cast
    /// </summary>
    public void DisableTutorial()
    {
        if (tutorialText.activeInHierarchy)
        {
            tutorialText.SetActive(false);
        }
    }

    #region RPCS

    /// <summary>
    /// RPC to let both clients know the game can start
    /// </summary>
    [PunRPC]
    public void RPC_StartGame()
    {
        canStartGame = true;

    }

    #endregion

}
