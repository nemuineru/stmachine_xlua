using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class _placeHolderTimeCount : MonoBehaviour
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
        CountNow = Mathf.Min(CountNow, CountUpMaxNum);
        float AllCount = gameState.self.elapsedTime * Mathf.Pow(CountNow / CountUpMaxNum, 0.65f);

        int minute = Mathf.FloorToInt(AllCount / 60f);
        int sec = Mathf.FloorToInt((AllCount - minute * 60f));
        int decs = Mathf.FloorToInt((AllCount - minute * 60f - sec) * 10f);

        tms.text = minute.ToString("D2") + ":" + sec.ToString("D2") + "." + decs.ToString("D1");
        
    }
}
