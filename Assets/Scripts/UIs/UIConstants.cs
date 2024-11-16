using System.Collections.Generic;

public class UIConstants
{
    // 模型选择器VE
    public static string MODEL_SELECTOR_VE_NAME = "Model_Selector_VisualElement";

    // 导航栏VE中的所有元素
    public static string SELECT_MODEL_BUTTON_NAME = "Select_Model_Button";
    public static string EXPORT_FILES_BUTTON_NAME = "Export_Files_Button";
    public static string CLOSE_MODEL_BUTTON_NAME = "Close_Model_Button";
    public static string QUIT_BUTTON_NAME = "Quit_Button";

    // 模型选择器VE中的所有元素
    public static string CONFIRM_BUTTON_NAME = "Confirm_Button";
    public static string CLOSE_BUTTON_NAME = "Close_Button";
    public static string MODEL_DROPDOWNFIELD_NAME = "Model_DropdownField";
    public static string DRAWING_PARAMS_VE_NAME = "DrawingParameters_VisualElement";
    public static List<int> DRAWING_PARAMS_STEPS_DEFAULT_VALUE = new() { 200, 3000, 300 };

    // 地形纹理VE中的所有元素
    public static string TERRAIN_RENDER_OPTIONS_VE_NAME = "TerrainRenderOptions_VisualElement";
    public static string TERRAIN_RENDER_OPTIONS_DROPDOWNFIELD_NAME = "TerrainRenderOptions_DropdownField";
    public static string RAMP_OPTION_VE_NAME = "RampOption_VisualElement";
    public static string RAMP_OPTION_TEXTFIELD_NAME = "RampOption_TextField";
    public static string RAMP_OPTION_BUTTON_NAME = "RampOption_Button";
    public static string TEXTURE_OPTION_VE_NAME = "TextureOption_VisualElement";
    public static string TEXTURE_OPTION_TEXTFIELD_NAME = "TextureOption_TextField";
    public static string TEXTURE_OPTION_BUTTON_NAME = "TextureOption_Button";

    // 底栏VE中的所有元素
    public static string STEP_LABEL_NAME = "Step_Label";
}
