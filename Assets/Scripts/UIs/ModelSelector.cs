using NativeFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class ModelSelector : MonoBehaviour
{
    // ������
    public UIDocument navBarDocument;

    // �˵����VE
    private UIDocument document;
    private VisualElement rootVisualElement;
    private VisualElement veModelSelector;

    // ���������������
    private DropdownField dfTerrainRenderOptions;
    private VisualElement veRampOption;
    private VisualElement veTextureOption;
    private TextField tfRampOption;
    private TextField tfTextureOption;

    // �������Ͳ˵��������а���
    private Button btnSelectModel;
    private Button btnExportFiles;
    private Button btnCloseModel;
    private Button btnQuit;
    private Button btnConfirm;
    private Button btnClose;


    // ģ��ѡ��������˵��ͱ�ѡ�е�ģ������
    private DropdownField dfModel;
    private string activeModelName;

    // �˵��Ƿ�ɼ�
    private bool isVisible = false;

    // �˴�������Ż�
    private string MODEL_PARAMS_PATH = "Assets/ModelParams";

    private ModelController modelController;
    private CameraSwitcher cameraSwitcher;

    struct ParamElements
    {
        public VisualElement VE;
        public List<int> paramCount;
        public ModelParamJson paramJson;
    }

    private Dictionary<string, ParamElements> modelParamElementsDict = new();

    void Start()
    {
        modelController = GameObject.Find("Model Controller").GetComponent<ModelController>();
        cameraSwitcher = GameObject.Find("Virtual Cameras").GetComponent<CameraSwitcher>();
        
        // ��ȡUIԪ��
        document = GetComponent<UIDocument>();
        rootVisualElement = document.rootVisualElement;
        veModelSelector = rootVisualElement.Q<VisualElement>(UIConstants.MODEL_SELECTOR_VE_NAME);

        // ��ȡ�������İ���
        VisualElement navBarVE = navBarDocument.rootVisualElement;
        btnSelectModel = navBarVE.Q<Button>(UIConstants.SELECT_MODEL_BUTTON_NAME);
        btnExportFiles = navBarVE.Q<Button>(UIConstants.EXPORT_FILES_BUTTON_NAME);
        btnCloseModel = navBarVE.Q<Button>(UIConstants.CLOSE_MODEL_BUTTON_NAME);
        btnQuit = navBarVE.Q<Button>(UIConstants.QUIT_BUTTON_NAME);

        // ��ȡ�����еİ�����������
        btnConfirm = veModelSelector.Q<Button>(UIConstants.CONFIRM_BUTTON_NAME);
        btnClose = veModelSelector.Q<Button>(UIConstants.CLOSE_BUTTON_NAME);
        dfModel = veModelSelector.Q<DropdownField>(UIConstants.MODEL_DROPDOWNFIELD_NAME);

        // ���ص��λ��ƵĲ���
        VisualElement veTerrainTexture = rootVisualElement.Q<VisualElement>(UIConstants.TERRAIN_RENDER_OPTIONS_VE_NAME);
        dfTerrainRenderOptions = veTerrainTexture.Q<DropdownField>(UIConstants.TERRAIN_RENDER_OPTIONS_DROPDOWNFIELD_NAME);
        veRampOption = veTerrainTexture.Q<VisualElement>(UIConstants.RAMP_OPTION_VE_NAME);
        veTextureOption = veTerrainTexture.Q<VisualElement>(UIConstants.TEXTURE_OPTION_VE_NAME);
        dfTerrainRenderOptions.choices.Add("Raw");
        dfTerrainRenderOptions.choices.Add("Ramp");
        dfTerrainRenderOptions.choices.Add("Texture");
        dfTerrainRenderOptions.index = 0;

        tfRampOption = veRampOption.Q<TextField>(UIConstants.RAMP_OPTION_TEXTFIELD_NAME);
        tfTextureOption = veTextureOption.Q<TextField>(UIConstants.TEXTURE_OPTION_TEXTFIELD_NAME);
        Button btnRampOption = veRampOption.Q<Button>(UIConstants.RAMP_OPTION_BUTTON_NAME);
        Button btnTextureOption = veTextureOption.Q<Button>(UIConstants.TEXTURE_OPTION_BUTTON_NAME);
        btnRampOption.RegisterCallback<ClickEvent, TextField>(OnBtnLoadClick, tfRampOption);
        btnTextureOption.RegisterCallback<ClickEvent, TextField>(OnBtnLoadClick, tfTextureOption);

        // ����ģ���ļ����е�ģ��Json
        Dictionary<string, string> modelNameDict = GetModelNameDictionary(MODEL_PARAMS_PATH, "*.json");
        foreach (var activeModelName in modelNameDict)
        {
            dfModel.choices.Add(activeModelName.Key);
            ParamElements paramElements = new ParamElements();
            string jsonText = ReadData(activeModelName.Value);
            ModelParamJson modelJson = JsonUtility.FromJson<ModelParamJson>(jsonText);
            VisualElement modelVE = ModelWindowEditor.GetNewModelVisualElement(activeModelName.Key);
            List<int> paramCount = new List<int>();
            foreach (var param in modelJson.paramList)
            {
                FloatField paramFloatField = ModelWindowEditor.GetNewFloatParamFloatField(param.name, activeModelName.Key, param.defaultValue);
                modelVE.Add(paramFloatField);
            }
            paramCount.Add(modelJson.paramList.Count);
            foreach (var fileName in modelJson.fileList)
            {
                VisualElement fileVisualElement = ModelWindowEditor.GetNewFileParamTextField(fileName.name, activeModelName.Key);
                Button btnLoad = fileVisualElement.Q<Button>(activeModelName.Key + "_" + fileName.name + ModelWindowEditor.MODEL_BUTTON_SUFFIX);
                TextField fileTextField = fileVisualElement.Q<TextField>(activeModelName.Key + "_" + fileName.name + ModelWindowEditor.MODEL_TEXTFIELD_SUFFIX);
                btnLoad.RegisterCallback<ClickEvent, TextField>(OnBtnLoadClick, fileTextField);
                modelVE.Add(fileVisualElement);
            }
            paramCount.Add(modelJson.fileList.Count);
            modelVE.style.display = DisplayStyle.None;
            veModelSelector.contentContainer.Insert(3, modelVE);

            paramElements.VE = modelVE;
            paramElements.paramCount = paramCount;
            paramElements.paramJson = modelJson;

            modelParamElementsDict.Add(activeModelName.Key, paramElements);
        }

        // ��ʼ��ģ��Ϊindex = 0
        dfModel.index = 0;
        activeModelName = dfModel.text;
        modelParamElementsDict[activeModelName].VE.style.display = DisplayStyle.Flex;

        // �󶨵���¼�
        btnSelectModel.RegisterCallback<ClickEvent>(OnBtnSelectModelClick);
        btnExportFiles.RegisterCallback<ClickEvent>(OnBtnExportFilesClick);
        btnCloseModel.RegisterCallback<ClickEvent>(OnBtnCloseModelClick);
        btnQuit.RegisterCallback<ClickEvent>(OnBtnQuitClick);
        btnConfirm.RegisterCallback<ClickEvent>(OnBtnConfirmClick);
        btnClose.RegisterCallback<ClickEvent>(OnBtnCloseClick);
    }

    void Update()
    {
        // UI��ʾ
        veModelSelector.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        switch(dfTerrainRenderOptions.value)
        {
            case "Raw":
            default:
                veRampOption.style.display = DisplayStyle.None;
                veTextureOption.style.display = DisplayStyle.None;
                tfTextureOption.value = "";
                tfRampOption.value = "";
                break;
            case "Ramp":
                veRampOption.style.display = DisplayStyle.Flex;
                veTextureOption.style.display = DisplayStyle.None;
                tfTextureOption.value = "";
                break;
            case "Texture":
                veTextureOption.style.display = DisplayStyle.Flex;
                veRampOption.style.display = DisplayStyle.None;
                tfRampOption.value = "";
                break;
        }


        // ����ģ�Ͳ�����ʾ
        string curModelName = dfModel.text;
        if (curModelName != activeModelName)
        {
            activeModelName = curModelName;
            foreach (var model in modelParamElementsDict)
            {
                if (model.Key != activeModelName)
                    model.Value.VE.style.display = DisplayStyle.None;
                else
                    model.Value.VE.style.display = DisplayStyle.Flex;
            }
        }
    }

    private string ReadData(string path)
    {
        string data = string.Empty;
        using (StreamReader sr = new StreamReader(path))
        {
            data = sr.ReadToEnd();
            sr.Close();
        }
        return data;
    }

    private Dictionary<string, string> GetModelNameDictionary(string path, string pattern)
    {
        Dictionary<string, string> modelNameDict = new Dictionary<string, string>();

        string[] directoryEntries = Directory.GetFiles(path, pattern);
        for (int i = 0; i < directoryEntries.Length; i++)
        {
            string fileName = Path.GetFileName(directoryEntries[i]);
            fileName = fileName.Substring(0, fileName.Length - 5);
            modelNameDict[fileName] = directoryEntries[i];
        }
        return modelNameDict;
    }

    private void OnBtnConfirmClick(ClickEvent e)
    {
        List<float> paramList = new();
        List<string> fileList = new();

        // ����������ֵ�����
        string curModelName = dfModel.text;
        VisualElement curModelVE = modelParamElementsDict[curModelName].VE;
        List<int> paramCount = modelParamElementsDict[curModelName].paramCount;
        ModelParamJson paramJson = modelParamElementsDict[dfModel.text].paramJson;
        for (int i = 0; i < paramCount[0]; i++)
        {
            FloatField curElement = (FloatField)curModelVE.ElementAt(i);
            paramList.Add(curElement.value);
            curElement.value = paramJson.paramList[i].defaultValue;
        }
        for (int i = paramCount[0]; i < paramCount[0] + paramCount[1]; i++)
        {
            TextField curElement = (TextField)curModelVE.ElementAt(i).ElementAt(0);
            fileList.Add(curElement.value);
            curElement.value = string.Empty;
        }

        VisualElement drawingParamsVE = veModelSelector.Q<VisualElement>(UIConstants.DRAWING_PARAMS_VE_NAME);
        List<int> drawingParamsList = new();
        for (int i = 0; i < drawingParamsVE.childCount; i++)
        {
            FloatField curElement = (FloatField)drawingParamsVE.ElementAt(i);
            drawingParamsList.Add((int)curElement.value);
            curElement.value = UIConstants.DRAWING_PARAMS_STEPS_DEFAULT_VALUE[i];
        }


        // TODO: ��Ӽ���ȱ�Ĵ���

        // ������������
        cameraSwitcher.ActivateCameraList();
        modelController.StartGenerating(curModelName, paramList, fileList, drawingParamsList);

        string terrainRenderingType;
        string terrainRenderingFilePath = string.Empty;
        switch (dfTerrainRenderOptions.value)
        {
            case "Raw":
            default:
                terrainRenderingType = "Raw";
                break;
            case "Ramp":
                terrainRenderingType = "Ramp";
                terrainRenderingFilePath = tfRampOption.value;
                break;
            case "Texture":
                terrainRenderingType = "Texture";
                terrainRenderingFilePath = tfTextureOption.value;
                break;
        }
        modelController.SetTerrainRenderingParams(terrainRenderingType, terrainRenderingFilePath);

        isVisible = false;
    }

    // �����ļ�
    private void OnBtnLoadClick(ClickEvent e, TextField textField)
    {
        var title = "Open File";
        var path = StandaloneFileBrowser.OpenFilePanel(title, null, false);
        textField.value = path[0];
    }

    private void OnBtnCloseClick(ClickEvent e)
    {
        isVisible = false;
    }

    private void OnBtnSelectModelClick(ClickEvent e)
    {
        isVisible = !isVisible;
    }

    private void OnBtnExportFilesClick(ClickEvent e)
    {
        Debug.Log("Export Files!");
        modelController.ExportFiles();
    }

    private void OnBtnCloseModelClick(ClickEvent e)
    {
        Debug.Log("Finish Running!");
        modelController.FinishRunning();
        cameraSwitcher.ActivateInitialCamera();
    }

    private void OnBtnQuitClick(ClickEvent e)
    {
        Debug.Log("Quit Application!");
        Application.Quit();
    }
}
