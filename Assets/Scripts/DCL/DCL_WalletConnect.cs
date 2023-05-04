using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WalletConnectSharp.Unity;

public class DCL_WalletConnect : MonoBehaviour
{

    public UnityEvent OnConnect;
    public TMPro.TMP_InputField accountText;
    // Start is called before the first frame update
   

    public void Conectado()
    {
        if (WalletConnect.ActiveSession.Accounts == null)
            return;

        accountText.text = WalletConnect.ActiveSession.Accounts[0];
        OnConnect.Invoke();
    }
}



