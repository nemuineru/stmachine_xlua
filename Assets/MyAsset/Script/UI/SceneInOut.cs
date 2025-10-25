using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInOut : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ExitAnimSet()
    {
        animator.SetBool("IsSceneLoadNew",true);
    }
}
