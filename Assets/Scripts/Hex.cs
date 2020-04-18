using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour
{
    public int row;
    public int col;
    public bool hasPeg;
    // Start is called before the first frame update
    void Start()
    {
        hasPeg = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setPeg()
    {
        hasPeg = true;
    }

    public void unsetPeg()
    {
        hasPeg = false;
    }
}
