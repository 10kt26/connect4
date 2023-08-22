using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public void setColor(int poa){
        if(poa==1){
            gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
        if(poa==-1){
            gameObject.GetComponent<Renderer>().material.color = Color.blue;
        }
    }
}
