using UnityEngine;

public class Target : MonoBehaviour, ITarget
{
    public void GetHit() 
    { 
         Debug.Log("I got hit"); 
    }
}
