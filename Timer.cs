using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Timer : MonoBehaviour
{
    //Initial value for timer
    public float timerValue = 15;
    
    //Increments with time
    private float counter = 0;
    
    //Decides when to increase
    private bool decrease = true;
    
    //Takes in a TextMeshPro Obj
    public TMP_Text timerText;

    void Update()
    {
        //Waits to receive a bool from another script, could be when the player first moves or when crossing a certain part of the level
        if (ClickAndDrag.startTimer)
        {
            //increment counter
            counter += Time.deltaTime;
            
            //automatically begins as decrease was intialized as true
            if (decrease)
            {
                //Decreases Timer
                timerValue -= Time.deltaTime;
                //TIME FOR SOME JANK TIME!!! WHO WANTS TO MAKE SOME DUMBASS CODE!!! MEEEE!!!!
                /*
                Problem is, by incrementing timer by Time.deltaTime, it increments the value every frame rather than every second
                which makes the visual unappealing, ex. 15 will increment to 14.98883 or something. So this will wait until the counter
                has reached a full second. I attempted limiting the max number of characters but that causes the Timer to spaz back and forth as 
                the numbers are still there, just hidden. Other ways to do this exist, keep that in mind, this is what came to me at the time.
                */
                if (counter >= 1)
                {
                   
                    timerValue = Mathf.RoundToInt(timerValue);
                    timerText.text = timerValue.ToString();
                    counter = 0;
                }

            }
            //stops decreasing if time has reached zero, and reloads scene.
            if (timerValue == 0)
            {
                decrease = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
