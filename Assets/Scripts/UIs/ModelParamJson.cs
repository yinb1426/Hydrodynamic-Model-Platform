using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ModelParamJson
{
    public string modelName;
    public string IPAddr;
    public string IPPort;
    public List<ModelParam> paramList;
    public List<ModelFile> fileList;
}

[System.Serializable]
public class ModelParam
{
    public string name;
    public float defaultValue;
}

[System.Serializable]
public class ModelFile
{
    public string name;
    public string type;
}