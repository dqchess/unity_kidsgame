﻿using System.Collections;
using System.Collections.Generic;
using Tacticsoft;
using UnityEngine;
using UnityEngine.UI;

public class UITextView : UIView
{
    public ScrollRect scrollRect;
    public GameObject objScrollContent;
    public Text textContent;
    public string text
    {
        get
        {
            return textContent.text;
        }

        set
        {
            textContent.text = value;
            LayOut();
        }

    }


    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {

    }
    // Use this for initialization
    void Start()
    {

    }



    // Update is called once per frame
    void Update()
    {

    }
    public void SetContentHeight(float h)
    {
        RectTransform rctran = objScrollContent.GetComponent<RectTransform>();
        rctran.sizeDelta = new Vector2(rctran.sizeDelta.x, h);
    }

      public void SetFontSize(int sz)
    { 
        textContent.fontSize = sz;
    }

      public void SetTextColor(Color cr)
    { 
        textContent.color = cr;
    }
    public override void LayOut()
    {
        int fontsize = textContent.fontSize;
        float str_w = Common.GetStringLength(textContent.text, AppString.STR_FONT_NAME, fontsize);

        RectTransform rctran = this.GetComponent<RectTransform>();
        float str_h = (str_w / rctran.rect.size.x + 1) * fontsize*2;
        SetContentHeight(str_h);
        Debug.Log("textView str_w =" + str_w + " str_h=" + str_h + " rctran.rect.size.x=" + rctran.rect.size.x);
    }

}
