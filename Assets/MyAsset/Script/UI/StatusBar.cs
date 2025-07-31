using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;
public class StatusBar : MonoBehaviour
{
    [SerializeField]
    Shapes.Polyline lifeBar, energyBar;


    [SerializeField]
    [Range(0f, 1.0f)]
    float Health = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //lifeBar.
    }
}
