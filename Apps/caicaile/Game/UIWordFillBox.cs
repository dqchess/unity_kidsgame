﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//成语接龙
/*
成语小秀才
https://www.taptap.com/app/167939
 */

public interface IUIWordFillBoxDelegate
{
    //回退word
    void UIWordFillBoxDidBackWord(UIWordFillBox ui, string word);

}
public class UIWordFillBox : UIView, IUILetterItemDelegate
{
    public Image imageBg;
    public Text textTitle;
    public UILetterItem uiLetterItemPrefab;
    public int row = 7;
    public int col = 7;
    public List<UILetterItem> listItem;
    LayOutGrid lygrid;
    int indexFillWord;
    int indexAnswer;
    private IUIWordFillBoxDelegate _delegate;

    public IUIWordFillBoxDelegate iDelegate
    {
        get { return _delegate; }
        set { _delegate = value; }
    }

    void Awake()
    {
        lygrid = this.GetComponent<LayOutGrid>();
        listItem = new List<UILetterItem>();
        row = 7;
        col = 7;
        lygrid.row = row;
        lygrid.col = col;
        lygrid.enableLayout = false;
        lygrid.dispLayVertical = LayOutBase.DispLayVertical.TOP_TO_BOTTOM;
    }

    // Use this for initialization
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {

    }

    public override void LayOut()
    {
        float x, y, w, h;
        RectTransform rctranRoot = this.GetComponent<RectTransform>();
        if (lygrid != null)
        {
            lygrid.LayOut();
            foreach (UILetterItem item in listItem)
            {
                Vector2 pos = lygrid.GetItemPostion(item.indexRow, item.indexCol);
                RectTransform rctran = item.GetComponent<RectTransform>();
                w = (rctranRoot.rect.width - (lygrid.space.x) * (col - 1)) / col;
                h = w;
                rctran.sizeDelta = new Vector2(w, h);
                rctran.anchoredPosition = pos;
                item.LayOut();
            }
        }


    }
    public void UpdateGuankaLevel(int level)
    {
        UpdateItem();
    }
    public void UpdateItem()
    {
        CaiCaiLeItemInfo info = GameGuankaParse.main.GetItemInfo();

        for (int i = 0; i < info.listPosX.Count; i++)
        {
            if (col < (info.listPosX[i] + 1))
            {
                col = info.listPosX[i] + 1;
            }
        }

        for (int i = 0; i < info.listPosY.Count; i++)
        {
            if (row < (info.listPosY[i] + 1))
            {
                row = info.listPosY[i] + 1;
            }
        }
        Debug.Log("UpdateItem row = " + row + " col=" + col);
        row = Mathf.Max(row, col);
        col = row;
        lygrid.row = row;
        lygrid.col = col;

        for (int i = 0; i < info.listWord.Count; i++)
        {
            string strword = info.listWord[i];
            UILetterItem ui = (UILetterItem)GameObject.Instantiate(uiLetterItemPrefab);
            ui.transform.SetParent(this.transform);
            ui.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            UIViewController.ClonePrefabRectTransform(uiLetterItemPrefab.gameObject, ui.gameObject);
            ui.iDelegate = this;
            ui.indexRow = info.listPosY[i];
            ui.indexCol = info.listPosX[i];
            ui.index = i;
            ui.SetStatus(UILetterItem.Status.UNLOCK);
            ui.wordAnswer = strword;
            ui.UpdateItem(strword);
            listItem.Add(ui);
        }
        indexAnswer = 0;
        indexFillWord = info.listWordAnswer[indexAnswer];
        for (int i = 0; i < info.listWordAnswer.Count; i++)
        {
            int idx = info.listWordAnswer[i];
            UILetterItem ui = listItem[idx];
            if (idx == indexFillWord)
            {
                ui.SetStatus(UILetterItem.Status.LOCK_SEL);
            }
            else
            {
                ui.SetStatus(UILetterItem.Status.LOCK_UNSEL);
            }

        }

        LayOut();

    }


    public UILetterItem GetItem(int idxRow, int idxCol)
    {
        foreach (UILetterItem item in listItem)
        {
            if ((idxRow == item.indexRow) && (idxCol == item.indexCol))
            {
                return item;
            }
        }
        return null;
    }

    public void ScanItem(int idxRow, int idxCol)
    {
        bool isAllRight = true;

        //scan row
        foreach (UILetterItem item in listItem)
        {
            if (idxRow == item.indexRow)
            {
                if (!((item.GetStatus() == UILetterItem.Status.RIGHT_ANSWER) || ((item.GetStatus() == UILetterItem.Status.UNLOCK))))
                {
                    isAllRight = false;
                }
            }
        }
        if (isAllRight)
        {
            foreach (UILetterItem item in listItem)
            {
                if (idxRow == item.indexRow)
                {
                    item.SetStatus(UILetterItem.Status.RIGHT_ANSWER);
                }
            }
        }

        //scan col
        isAllRight = true;
        foreach (UILetterItem item in listItem)
        {
            if (idxCol == item.indexCol)
            {
                if (!((item.GetStatus() == UILetterItem.Status.RIGHT_ANSWER) || ((item.GetStatus() == UILetterItem.Status.UNLOCK))))
                {
                    isAllRight = false;
                }
            }
        }
        if (isAllRight)
        {
            foreach (UILetterItem item in listItem)
            {
                if (idxCol == item.indexCol)
                {
                    item.SetStatus(UILetterItem.Status.RIGHT_ANSWER);
                }
            }
        }

    }

    //判断答案是否正确
    public bool CheckAllAnswer()
    {
        bool isAllAnswer = true;
        CaiCaiLeItemInfo info = GameGuankaParse.main.GetItemInfo();
        for (int i = 0; i < info.listWordAnswer.Count; i++)
        {
            int idx = info.listWordAnswer[i];
            UILetterItem ui = listItem[idx];
            // Debug.Log("CheckAllAnswer Status=" + ui.GetStatus() + " i=" + i);
            if (ui.GetStatus() != UILetterItem.Status.RIGHT_ANSWER)
            {
                Debug.Log("CheckAllAnswer Status=" + ui.GetStatus() + " i=" + i);
                isAllAnswer = false;
                break;
            }
        }

        if (isAllAnswer)
        {
            //全部猜对 game win
            // OnGameWin();

        }
        else
        {
            //游戏失败
            //  OnGameFail();
        }
        return isAllAnswer;
    }

    int GetNextFillWord(CaiCaiLeItemInfo info)
    {
        int ret = -1;
        for (int i = 0; i < info.listWordAnswer.Count; i++)
        {
            int idx = info.listWordAnswer[i];
            UILetterItem ui = listItem[idx];
            if ((ui.GetStatus() == UILetterItem.Status.LOCK_UNSEL) || (ui.GetStatus() == UILetterItem.Status.ERROR_ANSWER))
            {
                ret = i;
                break;
            }

        }
        return ret;
    }


    int GetIndexAnswer(UILetterItem uiSel)
    {
        int ret = -1;
        CaiCaiLeItemInfo info = GameGuankaParse.main.GetItemInfo();
        for (int i = 0; i < info.listWordAnswer.Count; i++)
        {
            int idx = info.listWordAnswer[i];
            UILetterItem ui = listItem[idx];
            if (ui.index == uiSel.index)
            {
                ret = i;
                break;
            }

        }
        return ret;
    }
    public void OnAddWord(string word)
    {
        UILetterItem ui = listItem[indexFillWord];
        CaiCaiLeItemInfo info = GameGuankaParse.main.GetItemInfo();
        if (UILetterItem.Status.ERROR_ANSWER == ui.GetStatus())
        {
            //先字符退回
            if (iDelegate != null)
            {
                iDelegate.UIWordFillBoxDidBackWord(this, ui.wordDisplay);
            }
        }
        ui.UpdateItem(word);
        if (ui.wordAnswer == word)
        {
            ui.SetStatus(UILetterItem.Status.RIGHT_ANSWER);
            ScanItem(ui.indexRow, ui.indexCol);
            //显示下一个
            indexAnswer = GetNextFillWord(info);
            if ((indexAnswer < info.listWordAnswer.Count) && (indexAnswer >= 0))
            {
                indexFillWord = info.listWordAnswer[indexAnswer];
                UILetterItem uiNext = listItem[indexFillWord];
                uiNext.SetStatus(UILetterItem.Status.LOCK_SEL);
            }

        }
        else
        {
            ui.SetStatus(UILetterItem.Status.ERROR_ANSWER);
        }
        // foreach (UIWordItem item in listItem)
        // {
        //     if (Common.BlankString(item.wordDisplay))
        //     {
        //         item.UpdateTitle(word);
        //         wordNumCur++;
        //         break;
        //     }
        // }

        // if (IsWordFull())
        // {
        //     CheckAnswer();
        // }
    }
    void UpdateSelItem(UILetterItem ui, CaiCaiLeItemInfo info)
    {
        //更新选中项目
        foreach (UILetterItem item in listItem)
        {
            if (item.GetStatus() == UILetterItem.Status.LOCK_SEL)
            {
                Debug.Log("OnUILetterItemDidClick unsel word=" + item.wordAnswer);
                item.SetStatus(UILetterItem.Status.LOCK_UNSEL);
            }
        }
        ui.SetStatus(UILetterItem.Status.LOCK_SEL);
        int idx_answer = GetIndexAnswer(ui);
        Debug.Log("OnUILetterItemDidClick idx_answer=" + idx_answer);
        if (idx_answer >= 0)
        {
            indexAnswer = idx_answer;
            indexFillWord = info.listWordAnswer[indexAnswer];
        }
    }

    public void OnUILetterItemDidClick(UILetterItem ui)
    {
        Debug.Log("OnUILetterItemDidClick");
        CaiCaiLeItemInfo info = GameGuankaParse.main.GetItemInfo();
        if (ui.GetStatus() == UILetterItem.Status.LOCK_UNSEL)
        {
            //更新选中项目
            UpdateSelItem(ui, info);
        }

        if (ui.GetStatus() == UILetterItem.Status.ERROR_ANSWER)
        {
            //回退并且选中 
            UpdateSelItem(ui, info);
            if (iDelegate != null)
            {
                iDelegate.UIWordFillBoxDidBackWord(this, ui.wordDisplay);
            }
        }

    }
    public void OnClickItem()
    {
    }
}
