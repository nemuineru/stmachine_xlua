using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class _placeHolderKOCountUP : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text tms;
    // Start is called before the first frame update
    void Start()
    {
        tms = GetComponent<TMP_Text>();
    }

    float CountUpMaxNum = 2.0f;
    float CountNow = 0;

    // Update is called once per frame
    void Update()
    {
        CountNow += Time.fixedDeltaTime;
        CountNow = Mathf.Min(CountNow,CountUpMaxNum);
        tms.text = Mathf.CeilToInt(gameState.self.KillValue * Mathf.Pow(CountNow/CountUpMaxNum,0.65f)).ToString();
    }
}
