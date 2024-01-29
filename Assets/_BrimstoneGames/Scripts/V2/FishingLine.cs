using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// script attached to the lure object that generates the fishing line
/// </summary>
public class FishingLine : MonoBehaviour
{
    public Vector2 destination; ///where the rope will go after cast
    public GameObject nodePrefab; //prefab of the node that represents a bending point in the fishing line
    public GameObject castSpot; //where the rope originates
    [HideInInspector]
    public Fish currentCaughtFish; //the current fish that is hooked
    public bool caughtFish = false;//if fish was caught


    private List<GameObject> nodes = new List<GameObject>(); //list of nodes in the line
    private AudioSource audioSource; //reference to audio Source    
    [SerializeField]
    private ThrowLure player; //reference to the player
    [SerializeField]
    private float speed = 5f; //speed of the line
    [SerializeField]
    private float nodeDistance = 0.5f; //distance between nodes
    private Rigidbody2D rb2D; //rigidbody reference
    private LineRenderer lineRenderer; //line renderer reference
    int vertexCount = 1; //an index representing the points in the line renderer
    private GameObject lastNode; //final node in the line
    private bool launched = false; //if the line was launched


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();  
        lineRenderer = GetComponent<LineRenderer>();    
        rb2D = GetComponent<Rigidbody2D>();
        lastNode = transform.gameObject; //adds the first node to the list (the lure)
        nodes.Add(lastNode);
    }


    /// <summary>
    /// spawns a node as the line moves forward
    /// </summary>
    void CreateNode()
    {
        Vector2 posToCreat = castSpot.transform.position - lastNode.transform.position; //determine the position where to create the new node
        posToCreat.Normalize();
        posToCreat *= nodeDistance;
        posToCreat += (Vector2)lastNode.transform.position;
        GameObject nodeGO = Instantiate (nodePrefab, posToCreat, Quaternion.identity);
        nodeGO.transform.SetParent(transform);
        //connect the hinge to the previous node
        lastNode.GetComponent<HingeJoint2D>().connectedBody = nodeGO.GetComponent<Rigidbody2D>();
        nodeGO.GetComponent<HingeJoint2D>().connectedBody = castSpot.GetComponent<Rigidbody2D>();
        castSpot.GetComponent<HingeJoint2D>().connectedBody = nodeGO.GetComponent<Rigidbody2D>();
        lastNode = nodeGO;
        nodes.Add(lastNode); //update node list
        vertexCount++;
    }


    /// <summary>
    /// displays the fishing line
    /// </summary>
    private void RenderFishingLine()
    {
        lineRenderer.positionCount = vertexCount;
        for ( int i = 0; i < nodes.Count; i++)
        {
            lineRenderer.SetPosition(i, nodes[i].transform.position);
        }
        lineRenderer.SetPosition(nodes.Count - 1, castSpot.transform.position);
    }

   

    /// <summary>
    /// enables the cast sequence
    /// </summary>
    public void StartCast()
    {
        launched = true;
    }

    /// <summary>
    /// starts the reeling sequence
    /// </summary>
    public void StartReel(Animator anim)
    {
        StartCoroutine(StartReelCoroutine(anim));
    }


    /// <summary>
    /// enables the reel sequence
    /// </summary>
    public IEnumerator StartReelCoroutine(Animator anim)
    {
        rb2D.isKinematic = false;
        for (int i = nodes.Count-1; i > 0; i--) //destroying nodes so itterating through the list backwards
        {
            yield return new WaitForEndOfFrame();
            castSpot.GetComponent<HingeJoint2D>().connectedBody = nodes[i-1].GetComponent<Rigidbody2D>();
            nodes[i - 1].GetComponent<HingeJoint2D>().connectedBody = castSpot.GetComponent<Rigidbody2D>();
            GameObject savedNode = nodes[i];
            nodes.Remove(nodes[i]);
            Destroy(savedNode);
            vertexCount--;
        }
        anim.SetBool("cast", false); //play reel animation
        lastNode = transform.gameObject;
        rb2D.isKinematic = true;
        rb2D.velocity = Vector3.zero;
        transform.position = castSpot.transform.position;
        launched = false;
        nodes.TrimExcess();
        player.isFishing = false;
        ProccessCaughtFish();
    }

    /// <summary>
    /// handles the changes for after catching a fish
    /// </summary>
    public void ProccessCaughtFish()
    {
        Debug.Log("Caught a fish");
        if (currentCaughtFish != null) //if we caught a fish and not just pulled the rod back with an empty hook
        {
            player.UpdateUI(currentCaughtFish.thisTypeOfFish); //update statistics
            Destroy(currentCaughtFish.gameObject); //destroys new fish
            if (PhotonNetwork.IsMasterClient) //tell master to spawn a new fish
            {
                GameController.instance.SpawnFish();
            }
          
            //Update UI Here
        }
        
        caughtFish = false;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Fish") && player.myView.IsMine)
        {
            if (!caughtFish)
            {
                Fish fish = collision.GetComponent<Fish>();
                //RTP logic: we devide the number of attempts by 10 and see if we have 3 more to complete a set of 10. If we do and the player didn't have 3 successful catches per 10 attempts, we force the catch to happen
                bool shouldCatch = player.GetStats();
                fish.CheckCatchFish(player.myView.ViewID, shouldCatch);
                
            }
          
        }
        else if (collision.tag.Equals("Water"))
        {
            audioSource.Play();
        }
    }

    private void Update()
    {
        if ((Vector2)transform.position != destination && launched)
        {
            
            if (Vector2.Distance(castSpot.transform.position, lastNode.transform.position) >nodeDistance)
            {
                CreateNode(); //generation of the fishing line while being cast
            }
        }
     
        else if ((Vector2)transform.position == destination && launched)
        {
            launched = false;
            player.isFishing = true;

        }

        RenderFishingLine();
    }



    private void FixedUpdate()
    {
        if (launched)
        { 
            //line movement when cast
            Vector2 newPosition = Vector2.MoveTowards(transform.position, destination, Time.deltaTime * speed);
            rb2D.MovePosition(newPosition);
        }


    }
}
