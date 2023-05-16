using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class DCL_UIManager : MonoBehaviour
{


    public Animator anima;
    void Start()
    {
        
    }

    
   

    public void SettingsHide()
    {
        anima.SetBool("show", false);
    }

    public void SettingsShow()
    {
        anima.SetBool("show", true);
    }
}
