using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HalloweenThemeController : GameThemeBaseController
{
    public GameObject decorButtonPrefab;
    public List<Button> pumpkinDecorButtons;
    public List<Button> batDecorButtons;
    public List<Button> twoBallDecorButton;
    // Start is called before the first frame update
    void Start()
    {
    }
    public void Init()
    {
        //for (int i = 0; i < pumpkinDecorButtons.Count; i++)
        //{
        //    ThemeDecorButton decor = Instantiate(decorButtonPrefab).GetComponent<ThemeDecorButton>();
        //    decor.SetDecor(DecorButtonType.Pumpkin, pumpkinDecorButtons[i].transform);
        //}
        //for (int i = 0; i < batDecorButtons.Count; i++)
        //{
        //    ThemeDecorButton decor = Instantiate(decorButtonPrefab).GetComponent<ThemeDecorButton>();
        //    decor.SetDecor(DecorButtonType.Bat, batDecorButtons[i].transform);
        //}
        //for (int i = 0; i < twoBallDecorButton.Count; i++)
        //{
        //    ThemeDecorButton decor = Instantiate(decorButtonPrefab).GetComponent<ThemeDecorButton>();
        //    decor.SetDecor(DecorButtonType.TwoBat, twoBallDecorButton[i].transform);
        //}
    }
    public void DeactivateTheme()
    {

    }
    public 
    // Update is called once per frame
    void Update()
    {
        
    }
}
