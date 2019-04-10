﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
宝宝巴士 宝宝涂色 
http://as.baidu.com/software/23393827.html
http://app.mi.com/details?id=com.sinyee.babybus.paintingIII&ref=search
*/

public class ColorItemInfo : ItemInfo
{
    public List<Color> listColor;
    public string name;
    public string picmask;
    public string colorJson;
    public Vector2 pt;
    public Color colorOrigin;//填充前原来颜色
    public Color colorFill;//当前填充颜色
    public Color colorMask;
    public Color32 color32Fill;
    public string fileSave;
    public string fileSaveWord;
    public string addtime;
    public string date;
    public Rect rectFill;
}
public class UIGamePaint : UIGameBase
{
    public const string STR_KEYNAME_VIEWALERT_SAVE_FINISH = "STR_KEYNAME_VIEWALERT_SAVE_FINISH";
    public const string STR_KEYNAME_VIEWALERT_SAVE = "STR_KEYNAME_VIEWALERT_SAVE";
    public const string STR_KEYNAME_VIEWALERT_SAVE_TIPS = "STR_KEYNAME_VIEWALERT_SAVE_TIPS";
    public const string STR_KEYNAME_VIEWALERT_FIRST_USE_FUNCTION = "keyname_viewalert_first_use_function";
    public const string KEY_STR_FIRST_USE_STRAW = "KEY_STR_FIRST_USE_STRAW";
    public const string KEY_STR_FIRST_USE_COLOR_INPUT = "KEY_STR_FIRST_USE_COLOR_INPUT";

    public const string STR_KEYNAME_VIEWALERT_DELETE_ALL = "keyname_viewalert_delete_all_paint";
    public const int GAME_MODE_NORMAL = 0;
    public const int GAME_MODE_FREE_DRAW = 1;
    public const string KEY_STR_COLOR_PEN = "KEY_STR_COLOR_PEN";
    public const string KEY_STR_COLOR_FILL = "KEY_STR_COLOR_FILL";
    public const string KEY_STR_COLOR_SIGN = "KEY_STR_COLOR_SIGN";

    public const int BTN_CLICK_MODE_STRAW = 0;
    public const int BTN_CLICK_MODE_COLOR_INPUT = 1;

    public MeshTexture meshTex;
    public Image imagePenBarBg;

    UIColorBoard uiColorBoardPrefab;
    public UIColorBoard uiColorBoard;
    UIColorInput uiColorInputPrefab;
    public UIColorInput uiColorInput;
    UILineSetting uiLineSettingPrefab;

    GamePaint gamePaintPrefab;
    GamePaint gamePaint;


    public UILineSetting uiLineSetting;

    public GameObject objLayoutBtn;
    public GameObject objLayoutBtnPen;
    public GameObject objLayoutTopBar;
    public Button btnColor;//任意颜色
    public Button btnStraw;//颜色吸管
    public Button btnFill;
    public Button btnPen;
    public Button btnSign;
    public Button btnErase;
    public Button btnMagic;
    public Image imagePenSel_H;
    public Image imagePenSel_V;
    Image imagePenSel;
    int btnClickMode;
    //multi-touch
    bool isMultiTouchDownPic;
    float touchDistance;//两个触摸点之间的距离 
    float touchDeltaX;    //目标x轴的改变值
    float touchDeltaY;    //目标y轴的改变值 
    bool isFirstMultiMove;
    bool isHaveTouchMove;
    bool isHaveMultiTouch;
    Vector2 ptDown;
    Vector2 ptDownWorld;
    //color select
    List<Color> listColorSelect;
    Color colorTouch;
    Vector2 ptColorTouch;
    List<ColorItemInfo> listColorFill;
    List<ColorItemInfo> listColorJson;

    Texture2D texPic;
    Texture2D texPicFromFile;
    Texture2D texPicOrign;//原始图片
    Texture2D texPicMask;
    Texture2D texBrush;

    bool isGameSelectorClose;
    int gamePicOffsetHeight;
    long tickDraw;
    long tick1, tick2;
    int indexSprite;


    Material matPenColor;
    bool isHaveInitShader;
    bool isNeedUpdateSpriteFill;
    float colorBoardOffsetYNormal;


    bool isFirstUseStraw
    {
        get
        {
            if (Common.noad)
            {
                return false;
            }
            return Common.Int2Bool(PlayerPrefs.GetInt(KEY_STR_FIRST_USE_STRAW, Common.Bool2Int(true)));
        }
        set
        {

            PlayerPrefs.SetInt(KEY_STR_FIRST_USE_STRAW, Common.Bool2Int(value));
        }
    }

    bool isFirstUseColorInput
    {
        get
        {
            if (Common.noad)
            {
                return false;
            }
            return Common.Int2Bool(PlayerPrefs.GetInt(KEY_STR_FIRST_USE_COLOR_INPUT, Common.Bool2Int(true)));
        }
        set
        {

            PlayerPrefs.SetInt(KEY_STR_FIRST_USE_COLOR_INPUT, Common.Bool2Int(value));
        }
    }

    Color colorPen
    {
        get
        {
            return Common.RGBString2Color(PlayerPrefs.GetString(KEY_STR_COLOR_PEN, "255,0,0"));
        }
        set
        {

            PlayerPrefs.SetString(KEY_STR_COLOR_PEN, Common.Color2RGBString(value));
        }
    }
    Color colorSign
    {
        get
        {
            return Common.RGBString2Color(PlayerPrefs.GetString(KEY_STR_COLOR_SIGN, "0,255,0"));
        }
        set
        {

            PlayerPrefs.SetString(KEY_STR_COLOR_SIGN, Common.Color2RGBString(value));
        }
    }
    Color colorFill
    {
        get
        {
            return Common.RGBString2Color(PlayerPrefs.GetString(KEY_STR_COLOR_FILL, "0,0,255"));
        }
        set
        {

            PlayerPrefs.SetString(KEY_STR_COLOR_FILL, Common.Color2RGBString(value));
        }
    }
    void Awake()
    {
        AppSceneBase.main.UpdateWorldBg(AppRes.IMAGE_GAME_BG);//  
        LoadPrefab();
        matPenColor = new Material(Shader.Find("Custom/PenColor"));

        isGameSelectorClose = false;
        isHaveInitShader = false;

        // tickUpdateCur = Common.GetCurrentTimeMs();
        // tickUpdatePre = Common.GetCurrentTimeMs();


        if (Device.isLandscape)
        {
            imagePenSel_H.gameObject.SetActive(true);
            imagePenSel_V.gameObject.SetActive(false);
            imagePenSel = imagePenSel_H;
        }
        else
        {
            imagePenSel_H.gameObject.SetActive(false);
            imagePenSel_V.gameObject.SetActive(true);
            imagePenSel = imagePenSel_V;
        }
        ParseGuanka();


        LoadGameTexture(true);

        gamePaint = (GamePaint)GameObject.Instantiate(gamePaintPrefab);
        AppSceneBase.main.AddObjToMainWorld(gamePaint.gameObject);
        gamePaint.transform.localPosition = new Vector3(0f, 0f, -1f);



        indexSprite = 0;

        UpdateBtnMusic();
        //ShowFPS();
    }
    // Use this for initialization
    void Start()
    {

        {
            RectTransform rctranPrefab = uiColorBoardPrefab.transform as RectTransform;
            this.controller.AddView(uiColorBoard);

            RectTransform rctran = uiColorBoard.transform as RectTransform;
            // 初始化rect
            rctran.offsetMin = rctranPrefab.offsetMin;
            rctran.offsetMax = rctranPrefab.offsetMax;
            uiColorBoard.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        {
            RectTransform rctranPrefab = uiColorInputPrefab.transform as RectTransform;
            // Debug.Log("uiColorInputPrefab :offsetMin=" + rctranPrefab.offsetMin + " offsetMax=" + rctranPrefab.offsetMax);
            this.controller.AddView(uiColorInput);

            RectTransform rctran = uiColorInput.transform as RectTransform;
            //  Debug.Log("uiColorInput 1:offsetMin=" + rctran.offsetMin + " offsetMax=" + rctran.offsetMax);
            // 初始化rect
            rctran.offsetMin = rctranPrefab.offsetMin;
            rctran.offsetMax = rctranPrefab.offsetMax;
            uiColorInput.transform.localScale = new Vector3(1f, 1f, 1f);
            // Debug.Log("uiColorInput 2:offsetMin=" + rctran.offsetMin + " offsetMax=" + rctran.offsetMax);
        }
        {
            RectTransform rctranPrefab = uiLineSettingPrefab.transform as RectTransform;

            this.controller.AddView(uiLineSetting);
            RectTransform rctran = uiLineSetting.transform as RectTransform;
            // 初始化rect
            rctran.offsetMin = rctranPrefab.offsetMin;
            rctran.offsetMax = rctranPrefab.offsetMax;
            uiLineSetting.transform.localScale = new Vector3(1f, 1f, 1f);
        }



        InitPenColor();
        isNeedUpdateSpriteFill = true;
        UpdateGuankaLevel(GameManager.gameLevel);

    }
    // Update is called once per frame
    void Update()
    {

    }
    void OnGUI()
    {
        GUIStyle bb = new GUIStyle();
        bb.normal.background = null;    //这是设置背景填充的
        bb.normal.textColor = new Color(1f, 0f, 0f);   //设置字体颜色的
        bb.fontSize = 20;       //当然，这是字体大小
        if (Common.isiOS || Common.isAndroid)
        {
            bb.fontSize = bb.fontSize * 2;
        }
        //居中显示FPS
        if (colorTouch != null)
        {
            int r = (int)(colorTouch.r * 255);
            int g = (int)(colorTouch.g * 255);
            int b = (int)(colorTouch.b * 255);
            int a = (int)(colorTouch.a * 255);
            // GUI.Label(new Rect(0, 100, 400, 200), "tickUpdateStep=" + tickUpdateStep + " tickPaint=" + tickPaint + "ms", bb);
        }

    }

    void LoadPrefab()
    {

        {
            GameObject obj = (GameObject)Resources.Load("App/Prefab/Game/UIColorBoard");
            if (obj != null)
            {
                uiColorBoardPrefab = obj.GetComponent<UIColorBoard>();
                uiColorBoard = (UIColorBoard)GameObject.Instantiate(uiColorBoardPrefab);
                uiColorBoard.gameObject.SetActive(false);
                // RectTransform rctranPrefab = uiColorBoardPrefab.transform as RectTransform;
                // //AppSceneBase.main.AddObjToMainCanvas(uiColorBoard.gameObject);
                uiColorBoard.transform.SetParent(this.transform);
                // RectTransform rctran = uiColorBoard.transform as RectTransform;
                // // 初始化rect
                // rctran.offsetMin = rctranPrefab.offsetMin;
                // rctran.offsetMax = rctranPrefab.offsetMax;

                uiColorBoard.callBackClick = OnUIColorBoardDidClick;
                uiColorBoard.transform.localScale = new Vector3(1f, 1f, 1f);
                UIViewController.ClonePrefabRectTransform(uiColorBoardPrefab.gameObject, uiColorBoard.gameObject);
            }
        }
        {
            GameObject obj = (GameObject)Resources.Load("App/Prefab/Game/UIColorInput");
            if (obj != null)
            {
                uiColorInputPrefab = obj.GetComponent<UIColorInput>();
                uiColorInput = (UIColorInput)GameObject.Instantiate(uiColorInputPrefab);
                uiColorInput.gameObject.SetActive(false);

                uiColorInput.transform.SetParent(this.transform);
                uiColorInput.callBackUpdateColor = OnUIColorInputUpdateColor;
                uiColorInput.transform.localScale = new Vector3(1f, 1f, 1f);
                UIViewController.ClonePrefabRectTransform(uiColorInputPrefab.gameObject, uiColorInput.gameObject);
            }
        }

        {
            GameObject obj = (GameObject)Resources.Load("App/Prefab/Game/UILineSetting");
            if (obj != null)
            {
                uiLineSettingPrefab = obj.GetComponent<UILineSetting>();
                uiLineSetting = (UILineSetting)GameObject.Instantiate(uiLineSettingPrefab);
                uiLineSetting.gameObject.SetActive(false);
                uiLineSetting.transform.SetParent(this.transform);

                uiLineSetting.callBackSettingLineWidth = OnUILineSettingLineWidth;
                uiLineSetting.transform.localScale = new Vector3(1f, 1f, 1f);
                UIViewController.ClonePrefabRectTransform(uiLineSettingPrefab.gameObject, uiLineSetting.gameObject);
            }
        }




        {
            GameObject obj = PrefabCache.main.Load("App/Prefab/Game/GamePaint");
            gamePaintPrefab = obj.GetComponent<GamePaint>();
        }


    }

    public override void UpdateGuankaLevel(int level)
    {
        InitUI();
    }

    void InitUI()
    {
        ColorItemInfo info = GetItemInfo();
        if (gameMode == GAME_MODE_FREE_DRAW)
        {
            gamePaint.isFreeDraw = true;
            btnFill.gameObject.SetActive(false);
        }
        else
        {
            gamePaint.isFreeDraw = false;
            btnFill.gameObject.SetActive(true);
        }
        if (gamePaint == null)
        {
            return;
        }
        // //init paint color
        gamePaint.colorInfo = GetItemInfo();
        gamePaint.texPic = texPic;
        gamePaint.texPicMask = texPicMask;
        gamePaint.texBrush = texBrush;
        gamePaint.texPicOrign = texPicOrign;
        gamePaint.colorPaint = colorPen;
        gamePaint.UpdateMode(GamePaint.MODE_PAINT);

        // SpriteRenderer spRender = objSpriteStraw.GetComponent<SpriteRenderer>();
        // paintColor.objSpriteStraw = objSpriteStraw;
        // paintColor.callBackClickStraw = OnPaintColorClickStraw;
        // paintColor.callBackErase = OnPaintColorErase;

        InitPaintRect();//必须在paintColor.Init前面 

        UpdateColorCur();

        uiColorInput.UpdateInitColor(gamePaint.colorPaint);

        uiLineSetting.lineWidthPixsel = gamePaint.lineWidthPixsel;

        InitPenColor();

        UpdateImagePenSelPosition();
        gamePaint.UpdateBg(AppRes.IMAGE_PAINT_BG);


        gamePaint.UpdateGamePic(info.pic);
        LayOut();

        Invoke("OnUIDidFinish", 0.2f);
    }

    void InitPaintRect()
    {
        Vector2 sizeCanvas = AppSceneBase.main.sizeCanvas;
        float x = 0, y = 0, z = 0, w = 0, h = 0;
        float scalex = 0, scaley = 0, scale = 0;
        //   if (texPic)
        {
            float topbar_h_world = Common.CanvasToWorldHeight(mainCam, sizeCanvas, 160);
            float adbar_h_world = topbar_h_world;

            Vector2 world_size = AppSceneBase.main.GetRectMainWorld().rect.size;
            float ratio = 0.95f;
            float oft_y = topbar_h_world;
            if (!Device.isLandscape)
            {
                //topbar + 底部的工具条
                oft_y = topbar_h_world * 2;
            }
            w = world_size.x;
            h = (world_size.y - oft_y);

            float w_pic = texPic.width / 100f;
            float h_pic = texPic.height / 100f;
            scale = Common.GetBestFitScale(w_pic, h_pic, w, h) * ratio;

            float w_disp = w_pic * scale;
            float h_disp = h_pic * scale;
            x = -w_disp / 2;
            y = -h_disp / 2 - oft_y / 2;
            if (!Device.isLandscape)
            {
                y = -h_disp / 2;
            }
            gamePaint.rectMain = new Rect(x, y, w_disp, h_disp);

        }

    }

    public override void LayOut()
    {
        float x = 0, y = 0, z = 0, w = 0, h = 0;
        float scalex = 0, scaley = 0, scale = 0;
        Vector2 sizeCanvas = AppSceneBase.main.sizeCanvas;
        //objSpriteStraw

        // //paint board bg
        // {
        //     SpriteRenderer render = objSpritePaintBoardMid.GetComponent<SpriteRenderer>();
        //     render.size = paintColor.rectMain.size;
        //     render.drawMode = SpriteDrawMode.Sliced;
        //     z = objSpritePaintBoardMid.transform.position.z;
        //     objSpritePaintBoardMid.transform.position = new Vector3(paintColor.rectMain.center.x, paintColor.rectMain.center.y, z);
        //     objSpritePaintBoardMid.transform.localScale = new Vector3(1f, 1f, 1f);
        // }
        // {
        //     SpriteRenderer render = objSpritePaintBoardTop.GetComponent<SpriteRenderer>();
        //     scale = (paintColor.rectMain.size.x / 3) / render.size.x;
        //     objSpritePaintBoardTop.transform.localScale = new Vector3(scale, scale, 1f);

        //     x = 0;
        //     y = paintColor.rectMain.center.y + paintColor.rectMain.size.y / 2 + render.bounds.size.y / 2;
        //     objSpritePaintBoardTop.transform.position = new Vector3(x, y, z);
        // }
        // {
        //     SpriteRenderer render = objSpritePaintBoardBottom.GetComponent<SpriteRenderer>();
        //     scale = (paintColor.rectMain.size.x) / render.size.x;
        //     Debug.Log("paintColor.rectMain.size=" + paintColor.rectMain.size + "render.size=" + render.size);
        //     objSpritePaintBoardBottom.transform.localScale = new Vector3(scale, scale, 1f);

        //     x = 0;
        //     y = paintColor.rectMain.center.y - paintColor.rectMain.size.y / 2 - render.bounds.size.y / 2;
        //     objSpritePaintBoardBottom.transform.position = new Vector3(x, y, z);
        // }

        //pen button
        {
            GridLayoutGroup gridLayout = objLayoutBtnPen.GetComponent<GridLayoutGroup>();
            Vector2 cellSize = gridLayout.cellSize;
            //  SpriteRenderer render = objSpritePaintBoardMid.GetComponent<SpriteRenderer>();
            float w_canvas = 160f;// Common.WorldToCanvasWidth(mainCam, sizeCanvas, render.bounds.size.x);
            float h_canvas = 512f;// Common.WorldToCanvasHeight(mainCam, sizeCanvas, render.bounds.size.y);
            RectTransform rctran = objLayoutBtnPen.transform as RectTransform;
            float oft = 10;

            Vector2 posScreen = Vector2.zero;// mainCam.WorldToScreenPoint(objSpritePaintBoardMid.transform.position);
            int total_btn = 4;
            if (Device.isLandscape)
            {
                // h = rctran.sizeDelta.y;
                h = (cellSize.y + 8) * total_btn;
                rctran.sizeDelta = new Vector2(cellSize.x, h);
                // x = w_canvas / 2 + cellSize.x / 2 + oft;
                x = sizeCanvas.x / 2 - cellSize.x / 2;
                y = 0;
            }
            else
            {
                w = (cellSize.x + 8) * total_btn;
                rctran.sizeDelta = new Vector2(w, cellSize.y);
                x = 0;
                Vector3 ptlocal = new Vector3(0, gamePaint.rectMain.y, 0);
                // Vector3 ptworld = mainCam.ScreenToWorldPoint(this.transform.TransformPoint(ptlocal));
                Vector3 ptworld = gamePaint.transform.TransformPoint(ptlocal);
                Vector2 ptcanvas = Common.WorldToCanvasPoint(mainCam, sizeCanvas, ptworld);
                Debug.Log("ptcanvas=" + ptcanvas + " ptworld=" + ptworld + " ptlocal=" + ptlocal);
                oft = cellSize.y + 8f;
                y = -(sizeCanvas.y / 2 - ptcanvas.y) - oft;
            }

            rctran.anchoredPosition = new Vector2(x, y);

            if (Device.isLandscape)
            {
                // Vector2 pos = objLayoutBtnPen.transform.position;
                // objLayoutBtnPen.transform.position = new Vector2(pos.x, posScreen.y);
            }


        }





    }



    //将背景填充成白色
    void FillWhiteBg(Texture2D tex)
    {
        ColorImage colorImageTmp = new ColorImage();
        colorImageTmp.Init(tex);
        for (int j = 0; j < tex.height; j++)
        {
            for (int i = 0; i < tex.width; i++)
            {
                Vector2 pttmp = new Vector2(i, j);

                Color colorpic = colorImageTmp.GetImageColorOrigin(pttmp);

                {
                    //统一为纯白色
                    colorpic.r = 1f;
                    colorpic.g = 1f;
                    colorpic.b = 1f;
                    colorpic.a = 1f;
                    colorImageTmp.SetImageColor(pttmp, colorpic);
                }


            }
        }

        colorImageTmp.UpdateTexture();
    }

    void LoadGameTexture(bool isNew)
    {
        Debug.Log("LoadGameTexture: gameMode=" + gameMode);
        ColorItemInfo info = GetItemInfo();
        texBrush = TextureCache.main.Load("App/UI/Brush/brush_dot");

        string picfile = info.pic;
        if (gameMode == GAME_MODE_FREE_DRAW)
        {
            //PaintBlank
            //texPicFromFile = LoadTexture.LoadFromResource();
            texPicFromFile = LoadTexture.LoadFromAsset(info.pic);
            FillWhiteBg(texPicFromFile);
            texPic = texPicFromFile;
            texPicOrign = texPicFromFile;
            texPicMask = texPicFromFile;
            //texPicMask = LoadTexture.LoadFromResource("APP/UI/Game/PaintBlank");
            return;
        }
        if (!isNew)
        {
            Debug.Log("fileSave=" + info.fileSave);
            if (FileUtil.FileIsExist(info.fileSave))
            {
                picfile = info.fileSave;
                texPicFromFile = LoadTexture.LoadFromFile(picfile);
            }
            else
            {
                texPicFromFile = LoadTexture.LoadFromAsset(picfile);
            }

        }
        else
        {
            texPicFromFile = LoadTexture.LoadFromAsset(picfile);
        }

        if (info.pic == picfile)
        {
            texPicOrign = texPicFromFile;
        }
        else
        {
            texPicOrign = LoadTexture.LoadFromAsset(info.pic);
        }

        texPic = texPicFromFile;

        //@moon解决Texture.Apply()在像素比较多的时候无法更新sptire的各个像素点alpha值的bug。先给sprie设置一个空的单色的或空的texture(所有像素的alpha值为1f)
        //然后再把实际的图片texture拷贝到texPic更新sprite显示
        // texPic = CreateTexTureBg(texPicFromFile.width, texPicFromFile.height);

        // SpriteRenderer spRender = objSpritePaintPic.GetComponent<SpriteRenderer>();
        // Sprite sp = Sprite.Create(texPic, new Rect(0, 0, texPic.width, texPic.height), new Vector2(0.5f, 0.5f));
        // spRender.sprite = sp;



        // //copy texture 
        // texPic.LoadRawTextureData(texPicFromFile.GetRawTextureData());
        // texPic.Apply();

        //最后初始化更新
        // colorImage.Init(texPic);


        texPicMask = LoadTexture.LoadFromAsset(info.picmask);
    }
    Texture2D UpdateTextureColor(Texture2D tex, Texture2D texMask, Color colorFill, Color colorMask)
    {
        Material mat = matPenColor;//new Material(Shader.Find("Custom/PenColor"));
        mat.SetTexture("_MainTex", tex);
        mat.SetTexture("_TexMask", texMask);
        mat.SetColor("_ColorMask", colorMask);
        mat.SetColor("_ColorFill", colorFill);

        RenderTexture rtTmp = new RenderTexture(tex.width, tex.height, 0);
        //var rtTmp = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        Graphics.Blit(tex, rtTmp, mat);
        Texture2D texOut = TextureUtil.RenderTexture2Texture2D(rtTmp);
        //RenderTexture.ReleaseTemporary(rtTmp);
        return texOut;
    }
    void UpdateColorCur()
    {
        btnColor.GetComponent<Image>().color = gamePaint.colorPaint;
    }

    void InitPenColorFill()
    {
        Texture2D tex = LoadTexture.LoadFromResource("App/UI/Game/btn_fill_pen");
        Texture2D texMask = LoadTexture.LoadFromResource("APP/UI/Game/btn_fill_pen_mask");
        Texture2D texNew = UpdateTextureColor(tex, texMask, colorFill, Color.white);
        btnFill.GetComponent<Image>().sprite = LoadTexture.CreateSprieFromTex(texNew);
    }
    void InitPenColorSign()
    {
        Texture2D tex = LoadTexture.LoadFromResource("APP/UI/Game/btn_sign_pen");
        Texture2D texMask = LoadTexture.LoadFromResource("APP/UI/Game/btn_sign_pen_mask");
        Texture2D texNew = UpdateTextureColor(tex, texMask, colorSign, Color.white);
        btnSign.GetComponent<Image>().sprite = LoadTexture.CreateSprieFromTex(texNew);
    }
    void InitPenColorPen()
    {
        Texture2D tex = LoadTexture.LoadFromResource("APP/UI/Game/btn_color_pen");
        Texture2D texMask = LoadTexture.LoadFromResource("APP/UI/Game/btn_color_pen_mask");
        Texture2D texNew = UpdateTextureColor(tex, texMask, colorPen, Color.white);
        btnPen.GetComponent<Image>().sprite = LoadTexture.CreateSprieFromTex(texNew);
    }
    void InitPenColor()
    {
        InitPenColorPen();
        // Invoke("InitPenColorFill",0.1f);
        InitPenColorFill();
        //
        InitPenColorSign();
        //InitPenColorSign();
        //Invoke("InitPenColorSign",1f);
    }

    void UpdatePenColor(Color color)
    {
        Texture2D texNew = null;
        Texture2D tex = null;
        Texture2D texMask = null;
        switch (gamePaint.mode)
        {
            case GamePaint.MODE_PAINT:
                {
                    tex = LoadTexture.LoadFromResource("APP/UI/Game/btn_color_pen");
                    texMask = LoadTexture.LoadFromResource("APP/UI/Game/btn_color_pen_mask");
                    texNew = UpdateTextureColor(tex, texMask, color, Color.white);
                    btnPen.GetComponent<Image>().sprite = LoadTexture.CreateSprieFromTex(texNew);
                }
                break;
            case GamePaint.MODE_FILLCOLR:
                {
                    tex = LoadTexture.LoadFromResource("APP/UI/Game/btn_fill_pen");
                    texMask = LoadTexture.LoadFromResource("APP/UI/Game/btn_fill_pen_mask");
                    texNew = UpdateTextureColor(tex, texMask, color, Color.white);
                    btnFill.GetComponent<Image>().sprite = LoadTexture.CreateSprieFromTex(texNew);
                }
                break;
            case GamePaint.MODE_SIGN:
                {
                    tex = LoadTexture.LoadFromResource("APP/UI/Game/btn_sign_pen");
                    texMask = LoadTexture.LoadFromResource("APP/UI/Game/btn_sign_pen_mask");
                    texNew = UpdateTextureColor(tex, texMask, color, Color.white);
                    btnSign.GetComponent<Image>().sprite = LoadTexture.CreateSprieFromTex(texNew);
                }
                break;
        }
    }

    public void OnUILineSettingLineWidth(int width)
    {
        gamePaint.lineWidthPixsel = width;
        gamePaint.UpdateLineWidth();

    }
    public void OnPaintColorErase()
    {
        // Vector2 inputPos = Common.GetInputPosition();
        // Vector3 posTouchWorld = mainCam.ScreenToWorldPoint(inputPos);
        // posTouchWorld.z = objSpriteErase.transform.position.z;
        // objSpriteErase.transform.position = posTouchWorld;
        // if (!objSpriteErase.activeSelf)
        // {
        //     objSpriteErase.SetActive(true);
        // }
    }
    public void OnUIColorInputUpdateColor(Color color)
    {

        switch (gamePaint.mode)
        {
            case GamePaint.MODE_PAINT:
                {
                    colorPen = color;
                }
                break;
            case GamePaint.MODE_FILLCOLR:
                {
                    colorFill = color;
                }
                break;
            case GamePaint.MODE_SIGN:
                {
                    colorSign = color;
                }
                break;
        }
        gamePaint.colorPaint = color;
        UpdateColorCur();
        UpdatePenColor(color);
    }
    public void OnUIColorBoardDidClick(UIColorBoard ui, UIColorBoardCellItem item, bool isOutSide)
    {
        if (isOutSide)
        {
            uiColorBoard.gameObject.SetActive(false);
        }
        else
        {
            switch (gamePaint.mode)
            {
                case GamePaint.MODE_PAINT:
                    {
                        colorPen = item.color;
                    }
                    break;
                case GamePaint.MODE_FILLCOLR:
                    {
                        colorFill = item.color;
                    }
                    break;
                case GamePaint.MODE_SIGN:
                    {
                        colorSign = item.color;
                    }
                    break;
            }
            gamePaint.colorPaint = item.color;
            uiColorBoard.gameObject.SetActive(false);
            UpdateColorCur();
            UpdatePenColor(item.color);
        }
    }
    ColorItemInfo GetItemInfo()
    {
        int idx = GameManager.gameLevel;
        if (listGuanka == null)
        {
            return null;
        }
        if (idx >= listGuanka.Count)
        {
            return null;
        }
        ColorItemInfo info = listGuanka[idx] as ColorItemInfo;
        return info;
    }

    public override int GetGuankaTotal()
    {
        ParseGuanka();
        if (listGuanka != null)
        {
            return listGuanka.Count;
        }
        return 0;
    }

    public override void CleanGuankaList()
    {
        if (listGuanka != null)
        {
            listGuanka.Clear();
        }
    }

    public override int ParseGuanka()
    {
        int count = 0;

        if ((listGuanka != null) && (listGuanka.Count != 0))
        {
            return listGuanka.Count;
        }

        listGuanka = new List<object>();
        int idx = GameManager.placeLevel;
        string fileName = Common.GAME_RES_DIR + "/guanka/guanka_list" + idx + ".json";
        //FILE_PATH
        string json = FileUtil.ReadStringAsset(fileName);//((TextAsset)Resources.Load(fileName, typeof(TextAsset))).text;
        // Debug.Log("json::"+json);
        JsonData root = JsonMapper.ToObject(json);
        string strPlace = (string)root["place"];
        JsonData items = root["items"];
        for (int i = 0; i < items.Count; i++)
        {
            JsonData item = items[i];
            ColorItemInfo info = new ColorItemInfo();
            string strdir = Common.GAME_RES_DIR + "/image/" + strPlace;

            info.id = (string)item["id"];
            info.pic = strdir + "/draw/" + info.id + ".png";

            info.picmask = strdir + "/mask/" + info.id + ".png";
            info.colorJson = strdir + "/json/" + info.id + ".json";
            info.icon = strdir + "/thumb/" + info.id + ".png";

            //info.pic = info.picmask;

            string filepath = GetFileSave(info);
            info.fileSave = filepath;

            // string picname = (i + 1).ToString("d3");
            // info.pic = Common.GAME_RES_DIR + "/animal/draw/" + picname + ".png";
            // info.picmask = Common.GAME_RES_DIR + "/animal/mask/" + picname + ".png";
            // info.colorJson = Common.GAME_RES_DIR + "/animal/draw/" + picname + ".json";
            // info.icon = Common.GAME_RES_DIR + "/animal/thumb/" + picname + ".png";

            listGuanka.Add(info);
        }

        count = listGuanka.Count;

        // Debug.Log("ParseGame::count=" + count);
        return count;
    }


    Rect RectString2Rect(string strrect)
    {
        float x, y, w, h;
        x = 0;
        y = 0;
        w = 0;
        h = 0;
        string[] sArray = strrect.Split(',');
        int idx = 0;
        foreach (string str in sArray)
        {
            if (idx == 0)
            {
                x = Common.String2Int(str);
            }
            if (idx == 1)
            {
                y = Common.String2Int(str);
            }
            if (idx == 2)
            {
                w = Common.String2Int(str);
            }
            if (idx == 3)
            {
                h = Common.String2Int(str);
            }

            idx++;
        }
        Rect rc = new Rect(x, y, w, h);
        return rc;
    }
    void ParseColorJson()
    {
        listColorJson = new List<ColorItemInfo>();
        ColorItemInfo info = GetItemInfo();
        string json = FileUtil.ReadStringAsset(info.colorJson);//((TextAsset)Resources.Load(fileName, typeof(TextAsset))).text;
        string filePath = Application.streamingAssetsPath + "/" + info.colorJson;
        //  System.IO.StreamReader file = new System.IO.StreamReader(filePath);//读取文件中的数据  
        // json = file.ReadToEnd(); 

        Debug.Log("json:" + json + " size = " + json.Length);
        JsonData root = JsonMapper.ToObject(json);
        //strPlace = (string)root["place"];
        JsonData items = root["items"];
        Debug.Log("items count::" + items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            JsonData item = items[i];
            string strcolor = (string)item["color"];
            string strrect = (string)item["rect"];
            ColorItemInfo infocolor = new ColorItemInfo();
            infocolor.colorFill = Common.RGBString2Color(strcolor);
            infocolor.rectFill = RectString2Rect(strrect);
            listColorJson.Add(infocolor);
        }
    }

    void UpdateImagePenSelPosition()
    {
        RectTransform rctran = imagePenSel.GetComponent<RectTransform>();
        Vector2 offsetMax = rctran.offsetMax;
        Vector2 offsetMin = rctran.offsetMin;
        if (Device.isLandscape)
        {
            offsetMax.y = -4;
            offsetMin.y = 4;
        }
        else
        {
            offsetMin.x = 4;
            offsetMax.x = -4;

            offsetMax.y = -4;
        }
        rctran.offsetMax = offsetMax;
        rctran.offsetMin = offsetMin;
        if (!Device.isLandscape)
        {
            Vector2 selsize = rctran.sizeDelta;
            selsize.y = 16;
            //offsetMax 修改之后sizeDelta也会跟着变化，需要还原
            rctran.sizeDelta = selsize;
        }

    }
    public void ShowFirstUseAlert()
    {

        string title = Language.main.GetString("STR_UIVIEWALERT_TITLE_FIRST_USE_FUNCTION");
        string msg = Language.main.GetString("STR_UIVIEWALERT_MSG_FIRST_USE_FUNCTION");
        string yes = Language.main.GetString("STR_UIVIEWALERT_YES_FIRST_USE_FUNCTION");
        string no = "no";
        ViewAlertManager.main.ShowFull(title, msg, yes, no, false, STR_KEYNAME_VIEWALERT_FIRST_USE_FUNCTION, OnUIViewAlertFinished);

    }
    //返回保存提示
    public void ShowSaveTipsAlert()
    {

        string title = Language.main.GetString("STR_UIVIEWALERT_TITLE_SAVE");
        string msg = Language.main.GetString("STR_UIVIEWALERT_MSG_SAVE");
        string yes = Language.main.GetString("STR_UIVIEWALERT_YES_SAVE");
        string no = Language.main.GetString("STR_UIVIEWALERT_NO_SAVE");
        ViewAlertManager.main.ShowFull(title, msg, yes, no, true, STR_KEYNAME_VIEWALERT_SAVE_TIPS, OnUIViewAlertFinished);
    }

    public void ShowSaveAlert()
    {
        string title = Language.main.GetString("STR_UIVIEWALERT_TITLE_SAVE");
        string msg = Language.main.GetString("STR_UIVIEWALERT_MSG_SAVE");
        string yes = Language.main.GetString("STR_UIVIEWALERT_YES_SAVE");
        string no = Language.main.GetString("STR_UIVIEWALERT_NO_SAVE");
        ViewAlertManager.main.ShowFull(title, msg, yes, no, true, STR_KEYNAME_VIEWALERT_SAVE, OnUIViewAlertFinished);
    }

    public void ShowSaveFinishAlert()
    {

        string title = Language.main.GetString("STR_UIVIEWALERT_TITLE_SAVE_FINISH");
        string msg = Language.main.GetString("STR_UIVIEWALERT_MSG_SAVE_FINISH");
        string yes = Language.main.GetString("STR_UIVIEWALERT_YES_SAVE_FINISH");
        string no = "no";
        //  viewAlert.HideDelay(2f);
        ViewAlertManager.main.ShowFull(title, msg, yes, no, false, STR_KEYNAME_VIEWALERT_SAVE_FINISH, OnUIViewAlertFinished);
    }
    void DoBtnBack()
    {
        base.OnClickBtnBack();
    }

    public override void OnClickBtnBack()
    {

        if ((!gamePaint.isHasSave) && (gamePaint.isHasPaint))
        {
            ShowSaveTipsAlert();
            return;
        }
        DoBtnBack();
    }

    string GetFileSave(ColorItemInfo info)
    {
        string filedir = DBColor.strSaveColorDir;
        //创建文件夹
        if (!Directory.Exists(filedir))
        {
            Directory.CreateDirectory(filedir);
        }
        string strid = info.id;
        if (gameMode == GAME_MODE_FREE_DRAW)
        {
            strid = "id_freedraw_" + Common.GetCurrentTimeMs();
            info.id = strid;
        }
        string filepath = filedir + "/" + strid + ".png";
        return filepath;
    }
    public void OnClickBtnSave()
    {
        if (gamePaint.isHasPaint)
        {
            ShowSaveAlert();
        }
    }
    void DoBtnSave()
    {

        if (!gamePaint.isHasPaint)
        {
            //没有作画
            return;
        }

        ColorItemInfo info = GetItemInfo();
        string filepath = GetFileSave(info);
        info.fileSave = filepath;
        gamePaint.SaveImage(filepath);


        bool isexist = DBColor.main.IsItemExist(info);
        Debug.Log("IsItemExist:" + isexist);
        if (isexist)
        {
            DBColor.main.UpdateItemTime(info);
        }
        else
        {
            DBColor.main.AddItem(info);
        }

        ShowSaveFinishAlert();
    }

    public void OnPaintColorClickStraw(Color color)
    {
        if (color.a == 0)
        {
            return;
        }
        gamePaint.colorPaint = color;
        UpdateColorCur();
    }

    public void OnClickBtnLineSetting()
    {
        uiLineSetting.gameObject.SetActive(true);
    }
    public void OnClickBtnColorBoard()
    {
        Debug.Log("OnClickBtnColorBoard");
        ResetPaintModeBeforeColorStraw();
        //colorImage.ApplyTexture();
        uiColorBoard.gameObject.SetActive(!uiColorBoard.gameObject.activeSelf);
    }

    void OnUIViewAlertFinished(UIViewAlert alert, bool isYes)

    {
        if (STR_KEYNAME_VIEWALERT_DELETE_ALL == alert.keyName)
        {
            if (isYes)
            {
                DoDeleteAll();
            }
            else
            {

            }
        }

        if (STR_KEYNAME_VIEWALERT_FIRST_USE_FUNCTION == alert.keyName)
        {
            if (isYes)
            {
                if (btnClickMode == BTN_CLICK_MODE_STRAW)
                {
                    DoClickBtnStrawAlert();
                }
                if (btnClickMode == BTN_CLICK_MODE_COLOR_INPUT)
                {
                    DoClickBtnColorInputAlert();
                }
            }
            else
            {

            }
        }

        if (STR_KEYNAME_VIEWALERT_SAVE_TIPS == alert.keyName)
        {
            if (isYes)
            {
                DoBtnSave();
            }

            DoBtnBack();
        }

        if (STR_KEYNAME_VIEWALERT_SAVE == alert.keyName)
        {
            if (isYes)
            {
                DoBtnSave();
            }
        }

    }

    //恢复颜色吸管之前的模式
    void ResetPaintModeBeforeColorStraw()
    {
        // if (objSpriteStraw.activeSelf)
        // {
        //     objSpriteStraw.SetActive(false);
        //     paintColor.mode = paintColor.modePre;
        // }
    }

    //吸管

    public void DoClickBtnStraw()
    {
        isFirstUseStraw = false;
        // objSpriteStraw.SetActive(!objSpriteStraw.activeSelf);

        // if (objSpriteStraw.activeSelf)
        // {
        //     objSpriteErase.SetActive(false);
        //     paintColor.modePre = paintColor.mode;
        //     paintColor.mode = PaintColor.MODE_STRAW;
        // }
        // else
        // {
        //     //恢复之前的模式
        //     paintColor.mode = paintColor.modePre;
        //     if (paintColor.mode == PaintColor.MODE_ERASE)
        //     {
        //         objSpriteErase.SetActive(true);
        //     }
        // }
    }

    void DoClickBtnStrawAlert()
    {
        if (AppVersion.appCheckHasFinished && !Application.isEditor)
        {
            if (isFirstUseStraw)
            {
                //show ad video
                AdKitCommon.main.ShowAdVideo();
            }
            else
            {
                DoClickBtnStraw();
            }
        }
        else
        {
            DoClickBtnStraw();
        }
    }

    public void OnClickBtnStraw()
    {
        Debug.Log("OnClickBtnStraw");
        btnClickMode = BTN_CLICK_MODE_STRAW;
        if (AppVersion.appCheckHasFinished && isFirstUseStraw)
        {
            ShowFirstUseAlert();
        }
        else
        {
            DoClickBtnStraw();
        }


    }


    void DoClickBtnColorInput()
    {
        isFirstUseColorInput = false;
        uiColorInput.UpdateInitColor(gamePaint.colorPaint);
        uiColorInput.gameObject.SetActive(!uiColorInput.gameObject.activeSelf);
        Color color = colorPen;
        switch (gamePaint.mode)
        {
            case GamePaint.MODE_PAINT:
                {
                    color = colorPen;
                }
                break;
            case GamePaint.MODE_FILLCOLR:
                {
                    color = colorFill;
                }
                break;
            case GamePaint.MODE_SIGN:
                {
                    color = colorSign;
                }
                break;
        }
        uiColorInput.ColorNow = color;
        uiColorInput.UpdateColorNow();
    }
    void DoClickBtnColorInputAlert()
    {
        if (AppVersion.appCheckHasFinished && !Application.isEditor)
        {
            if (isFirstUseColorInput)
            {
                //show ad video
                AdKitCommon.main.ShowAdVideo();
            }
            else
            {
                DoClickBtnColorInput();
            }
        }
        else
        {
            DoClickBtnColorInput();
        }
    }

    public void OnClickBtnColorInput()
    {
        btnClickMode = BTN_CLICK_MODE_COLOR_INPUT;
        ResetPaintModeBeforeColorStraw();
        if (AppVersion.appCheckHasFinished && isFirstUseColorInput)
        {
            ShowFirstUseAlert();
        }
        else
        {
            DoClickBtnColorInput();
        }
    }

    void DoDeleteAll()
    {
        if (listColorFill != null)
        {
            listColorFill.Clear();
        }

        gamePaint.EraseAll();
    }
    public void OnClickBtnDelAll()
    {
        tickDraw = 0;

        {
            string title = Language.main.GetString("STR_UIVIEWALERT_TITLE_DELETE_ALL_PAINT_POINT");
            string msg = Language.main.GetString("STR_UIVIEWALERT_MSG_DELETE_ALL_PAINT_POINT");
            string yes = Language.main.GetString("STR_UIVIEWALERT_YES");
            string no = Language.main.GetString("STR_UIVIEWALERT_NO");
            ViewAlertManager.main.ShowFull(title, msg, yes, no, true, STR_KEYNAME_VIEWALERT_DELETE_ALL, OnUIViewAlertFinished);
        }


    }
    //彩笔
    public void OnClickBtnColorPen()
    {
        imagePenSel.transform.parent = btnPen.transform;
        UpdateImagePenSelPosition();
        // objSpriteErase.SetActive(false);
        //uiColorBoard.gameObject.SetActive(true);
        gamePaint.colorPaint = colorPen;
        gamePaint.UpdateMode(GamePaint.MODE_PAINT);
        gamePaint.UpdateLineWidth();
        UpdateColorCur();
        TTS.main.Speak(Language.main.GetString("STR_BTN_COLOR_PEN"));
    }
    //魔术笔
    public void OnClickBtnMagicPen()
    {
        imagePenSel.transform.parent = btnMagic.transform;
        UpdateImagePenSelPosition();
        // objSpriteErase.SetActive(false);
        // //uiColorBoard.gameObject.SetActive(false); 
        gamePaint.UpdateMode(GamePaint.MODE_MAGIC);
        gamePaint.UpdateLineWidth();
        TTS.main.Speak(Language.main.GetString("STR_BTN_MAGIC_PEN"));
    }
    //油漆桶
    public void OnClickBtnFillPen()
    {
        imagePenSel.transform.parent = btnFill.transform;
        UpdateImagePenSelPosition();
        //  objSpriteErase.SetActive(false);
        //uiColorBoard.gameObject.SetActive(true);
        gamePaint.colorPaint = colorFill;
        gamePaint.UpdateMode(GamePaint.MODE_FILLCOLR);
        gamePaint.UpdateLineWidth();
        UpdateColorCur();
        TTS.main.Speak(Language.main.GetString("STR_BTN_FILLCOLOR"));
    }
    //印章
    public void OnClickBtnSignPen()
    {
        imagePenSel.transform.parent = btnSign.transform;
        UpdateImagePenSelPosition();
        // objSpriteErase.SetActive(false);
        // //uiColorBoard.gameObject.SetActive(true);
        gamePaint.colorPaint = colorSign;
        gamePaint.UpdateMode(GamePaint.MODE_SIGN);
        UpdateColorCur();
        TTS.main.Speak(Language.main.GetString("STR_BTN_SIGN"));
    }
    //橡皮擦
    public void OnClickBtnErasePen()
    {
        imagePenSel.transform.parent = btnErase.transform;
        UpdateImagePenSelPosition();
        // //uiColorBoard.gameObject.SetActive(false);
        gamePaint.UpdateMode(GamePaint.MODE_ERASE);
        gamePaint.UpdateEraseLineWidth();
        TTS.main.Speak(Language.main.GetString("STR_BTN_ERASE"));
    }

    public void OnClickBtnDelLast()
    {

    }

    public void OnClickMainPaint()
    {
        Debug.Log("OnClickMainPaint");
        if (uiColorBoard.gameObject.activeSelf)
        {
            uiColorBoard.gameObject.SetActive(false);
        }
    }



    public override void AdVideoDidFail(string str)
    {
        ShowAdVideoFailAlert();
    }

    public override void AdVideoDidStart(string str)
    {

    }
    public override void AdVideoDidFinish(string str)
    {
        if (btnClickMode == BTN_CLICK_MODE_STRAW)
        {
            DoClickBtnStraw();
        }
        if (btnClickMode == BTN_CLICK_MODE_COLOR_INPUT)
        {
            DoClickBtnColorInput();
        }
    }
}

