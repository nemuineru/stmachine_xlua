using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;
public class StatusBar : MonoBehaviour
{
    [SerializeField]
    Shapes.Polyline lifeBar, lifeBar_Fill , energyBar, energyBar_Fill;


    [SerializeField]
    [Range(0f, 1.0f)]
    float health = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float Length = 0f;
        List<float> sgLength = new List<float>();
        for (int t = 0; t < lifeBar.points.Count - 1; t++)
        {
            float f = (lifeBar.points[t].point - lifeBar.points[t + 1].point).sqrMagnitude;
            sgLength.Add(f);
            Length += f;
        }
        float curLength = health * Length;

    }
}
