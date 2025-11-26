using UnityEditor;
using UnityEngine;

public class HollowMaskShaderGUI : ShaderGUI
{
    private enum ShapeType { Rectangle, Circle }
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // 查找所有属性
        MaterialProperty shapeProp = FindProperty("_SHAPE", properties);
        MaterialProperty mainTex = FindProperty("_MainTex", properties);
        MaterialProperty color = FindProperty("_Color", properties);
        
        // 矩形参数
        MaterialProperty rectCenter = FindProperty("_RectCenter", properties);
        MaterialProperty rectSize = FindProperty("_RectSize", properties);
        MaterialProperty cornerRadius = FindProperty("_CornerRadius", properties);
        MaterialProperty rectFeather = FindProperty("_RectFeather", properties);
        
        // 圆形参数
        MaterialProperty circleCenter = FindProperty("_CircleCenter", properties);
        MaterialProperty circleRadius = FindProperty("_CircleRadius", properties);
        MaterialProperty aspectRatio = FindProperty("_AspectRatio", properties);
        MaterialProperty circleFeather = FindProperty("_CircleFeather", properties);
        
        // 当前材质
        Material material = materialEditor.target as Material;
        
        // 绘制基础属性
        materialEditor.ShaderProperty(mainTex, "Main Texture");
        materialEditor.ShaderProperty(color, "Color Tint");
        
        // 形状选择
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("挖孔形状", EditorStyles.boldLabel);
        
        ShapeType currentShape = (ShapeType)shapeProp.floatValue;
        ShapeType newShape = (ShapeType)EditorGUILayout.EnumPopup("Hole Shape", currentShape);
        
        if (newShape != currentShape)
        {
            shapeProp.floatValue = (float)newShape;
            material.EnableKeyword(newShape == ShapeType.Rectangle ? "_SHAPE_RECTANGLE" : "_SHAPE_CIRCLE");
            material.DisableKeyword(newShape == ShapeType.Rectangle ? "_SHAPE_CIRCLE" : "_SHAPE_RECTANGLE");
        }
        
        // 根据形状显示不同参数
        if (newShape == ShapeType.Rectangle)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("矩形设置", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(rectCenter, "Center Position");
            materialEditor.ShaderProperty(rectSize, "Size (Normalized)");
            materialEditor.ShaderProperty(cornerRadius, "Corner Radius");
            materialEditor.ShaderProperty(rectFeather, "Feather Amount");
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("圆形设置", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(circleCenter, "Center Position");
            materialEditor.ShaderProperty(circleRadius, "Radius");
            materialEditor.ShaderProperty(aspectRatio, "AspectRatio");
            materialEditor.ShaderProperty(circleFeather, "Feather Amount");
        }
        
        // 应用修改
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(material);
        }
    }
}