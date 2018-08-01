using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonFinder : MonoBehaviour {
    
    private GameManager gm;
    private Button b;

    void Start()
    {
        gm = GameObject.FindWithTag("GameManagerObject").GetComponent<GameManager>();
        b = GetComponent<Button>();
        if (gameObject.name == "BuyButton")
        {
            b.onClick.AddListener(gm.buyStock);
        }
        else
        {
            b.onClick.AddListener(gm.sellStock);
        }
    }
}
