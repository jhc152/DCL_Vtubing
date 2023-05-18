using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class Codigo : MonoBehaviour
{

    


    public UIDocument panel;


    public VisualElement btnWalletConnect;


    // Start is called before the first frame update
    void Start()
    {

        var root = panel.rootVisualElement;

        btnWalletConnect = root.Q<VisualElement>("CTABoton");

        // btnWalletConnect.clicked += BtnWalletPresionada;
        btnWalletConnect.AddManipulator(new Clickable(evt => BtnWalletPresionada()));

    }

    void BtnWalletPresionada()
    {
        Debug.Log("le picaste aqui");
    }
}
