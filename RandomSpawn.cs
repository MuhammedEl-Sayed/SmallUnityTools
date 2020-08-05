using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawn : MonoBehaviour
{
    //Obj you want to spawn
    public GameObject spawnObj;
    
    //Size of the area you want the Obj to spawn in
    Vector3 size = new Vector3(10, 10, 0);
    
    //timer that counts seconds
    private float counter = 0;
    
    //float that sets the max amount the counter reaches before spawning
    private float counterLimit = 2f;
    //Referenced in another script (ClickAndDrag), used to signal when to update the list of the Obj
    public static bool addedObj = false;
    //Counts number of objs spawned since start
    private int numberofObj = 0;
    
    //Referenced in another script (ClickAndDrag), used to indicated to spawn another when one is picked up
    private bool incrementCount = false;


    void Update()
    {
        /*
        Referenced script. pickedup increments each time Obj is picked up by player, and this statements detects the change and tells
        script to spawn a new Obj
        */
        if (ClickAndDrag.oldpickedUp != ClickAndDrag.pickedUp)
        {
            incrementCount = true;
            ClickAndDrag.oldpickedUp = ClickAndDrag.pickedUp;
        }
        //incrementing counter using deltaTime
        counter += Time.deltaTime;
        
        /*
        Checks if counter has reached time limit, and if it is the first or second Obj, since those should spawn without the player picking one up, and then
        adds more if player picks one up.
        */
        if (counter > counterLimit && (incrementCount || numberofObj == 0|| numberofObj == 1))
        {
            counterLimit = Random.Range(2f, 3f);
            SpawnObj();
            counter = 0;
            addedObj = true;
            numberofObj++;
            incrementCount = false;
        }
        //Resets the counter whenever it can't spawn an Obj so that it doesn't increment when it can't add one.
        if (!(incrementCount || numberofObj == 0|| numberofObj == 1))
        {
            counter = 0;
        }
        
    }
    //Handles the spawning. pos is center of the square along with a random range somewhere within the square, and is then instantiated
    private void SpawnObj()
    {
        Vector3 pos = gameObject.transform.position + new Vector3(Random.Range(-size.x / 3, size.x / 3), Random.Range(-size.y / 3, size.y / 3), 0);

        Instantiate(spawnObj, pos, Quaternion.identity);
    }
    
    //drawing the square
    void OnDrawGizmosSelected()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(gameObject.transform.position, size);
    }
}
