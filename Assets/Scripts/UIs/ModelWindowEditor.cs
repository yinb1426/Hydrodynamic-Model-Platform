using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class ModelWindowEditor
{
    public static string MODEL_FLOATFIELD_SUFFIX = "_Field";
    public static string MODEL_VISUALELEMENT_SUFFIX = "_VisualElement";
    public static string MODEL_TEXTFIELD_SUFFIX = "_TextField";
    public static string MODEL_BUTTON_SUFFIX = "_Button";

    public static FloatField GetNewFloatParamFloatField(string paramName, string modelName, float defaultValue)
    {
        FloatField paramField = new FloatField();
        paramField.name = modelName + "_" + paramName + MODEL_FLOATFIELD_SUFFIX;
        var styleSheet = Resources.Load<StyleSheet>("StyleSheet/FloatFieldStyleSheet");
        paramField.styleSheets.Add(styleSheet);
        paramField.label = paramName;
        paramField.value = defaultValue;
        return paramField;
    }

    public static VisualElement GetNewFileParamTextField(string fileName, string modelName)
    {
        // 加载文件框体的基础VE
        VisualElement fileVE = new VisualElement();
        fileVE.name = modelName + "_" + fileName + MODEL_VISUALELEMENT_SUFFIX;
        var styleSheet = Resources.Load<StyleSheet>("StyleSheet/FileLoaderStyleSheet");
        fileVE.styleSheets.Clear();
        fileVE.styleSheets.Add(styleSheet);

        // TextField
        TextField textField = new TextField();
        textField.name = modelName + "_" + fileName + MODEL_TEXTFIELD_SUFFIX;
        textField.label = fileName;
        textField.value = string.Empty;
        textField.isReadOnly = true;
        var textFieldStyleSheet = Resources.Load<StyleSheet>("StyleSheet/TextFieldStyleSheet");
        textField.styleSheets.Add(textFieldStyleSheet);
        fileVE.Add(textField);

        // Load按钮
        Button btnLoad = new Button();
        btnLoad.name = modelName + "_" + fileName + MODEL_BUTTON_SUFFIX;
        btnLoad.text = "Load";
        var btnStyleSheet = Resources.Load<StyleSheet>("StyleSheet/LoadFileButtonStyleSheet");
        btnLoad.styleSheets.Add(btnStyleSheet);
        fileVE.Add(btnLoad);

        return fileVE;
    }

    public static VisualElement GetNewModelVisualElement(string modelName)
    {
        VisualElement modelVE = new VisualElement();
        modelVE.name = modelName + MODEL_VISUALELEMENT_SUFFIX;
        var styleSheet = Resources.Load<StyleSheet>("StyleSheet/VisualElementStyleSheet");
        modelVE.styleSheets.Add(styleSheet);
        return modelVE;
    }
}
