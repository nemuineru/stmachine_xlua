using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;
public class StatusBar : MonoBehaviour
{
    [SerializeField]
    Shapes.Polyline lifeBar, lifeBar_Fill, energyBar, energyBar_Fill;


    [SerializeField]
    [Range(0f, 1.0f)]
    float health = 0;
    [SerializeField]
    [Range(0f, 1.0f)]
    float energy = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ChangeFill(lifeBar, ref lifeBar_Fill, health);
        ChangeFill(energyBar, ref energyBar_Fill, energy);
    }

    void ChangeFill(Polyline BaseBar, ref Polyline FillBar, float ref_values)
    { 
        float Length = 0f;
        float calcHealth = ref_values;
        List<float> sgLength = new List<float>();
        for (int t = 0; t < BaseBar.points.Count - 1; t++)
        {
            float f = (BaseBar.points[t].point - BaseBar.points[t + 1].point).sqrMagnitude;
            sgLength.Add(f);
            Length += f;
        }
        //セグメントごとの距離に変更.
        sgLength = sgLength.Select(f => f = f / Length).ToList();
        int index = 0;
        float nx = 1.0f;
        foreach (float f in sgLength)
        {
            nx = f;
            if (calcHealth <= f + 0.001f)
            {
                break;
            }
            index++;
            calcHealth -= f;
        }
        float X = calcHealth / nx;
        Vector3 pos = BaseBar.points[index].point * (1 - X) + BaseBar.points[index + 1].point * (X);
        FillBar.SetPointPosition(0, pos);
        FillBar.meshOutOfDate = true;
        Debug.Log(index + " " + pos);
    }
}


