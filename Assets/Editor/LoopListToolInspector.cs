using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;

[CustomEditor(typeof(LoopListTool))]
public class LoopListToolInspector : Editor
{
    LoopListTool mLoopListTool;
    bool dataIsChange = false;
    public override void OnInspectorGUI()
    {
        dataIsChange = false;
        mLoopListTool = target as LoopListTool;



        GUILayout.BeginHorizontal();
        bool hType = EditorGUILayout.ToggleLeft("horizontal Type （水平滑动）", mLoopListTool.horizontalType);

        bool vType = EditorGUILayout.ToggleLeft("vertical Type（垂直滑动）", mLoopListTool.verticalType);

        if (hType && hType != mLoopListTool.horizontalType)
        {
            dataIsChange = true;
            mLoopListTool.horizontalType = hType;
            if (Application.isPlaying)
                mLoopListTool.ResetLayout();
        }
        else if (vType && vType != mLoopListTool.verticalType)
        {
            dataIsChange = true;
            mLoopListTool.verticalType = vType;
            if (Application.isPlaying)
                mLoopListTool.ResetLayout();
        }
        GUILayout.EndHorizontal();

        if (hType)
        {
            mLoopListTool.padding_left = EditorGUILayout.FloatField("padding left （左内边距）", mLoopListTool.padding_left);
            mLoopListTool.padding_right = EditorGUILayout.FloatField("padding right （右内边距）", mLoopListTool.padding_right);
        }
        else if(vType)
        {
            mLoopListTool.padding_top = EditorGUILayout.FloatField("padding top （上内边距）", mLoopListTool.padding_top);
            mLoopListTool.padding_bottom = EditorGUILayout.FloatField("padding bottom （下内边距）", mLoopListTool.padding_bottom);
        }

        bool isItemLoop = EditorGUILayout.ToggleLeft("is Item Loop (是否循环模式，和回弹冲突)", mLoopListTool.isItemLoop);
        if (isItemLoop != mLoopListTool.isItemLoop)
        {
            dataIsChange = true;
            mLoopListTool.isItemLoop = isItemLoop;
            if (mLoopListTool.isElastic)
                mLoopListTool.isElastic = !isItemLoop;
        }

        bool isElastic = EditorGUILayout.ToggleLeft("isElastic（是否有回弹，和循环模式冲突）", mLoopListTool.isElastic);
        if (isElastic != mLoopListTool.isElastic)
        {
            dataIsChange = true;
            mLoopListTool.isElastic = isElastic;
            if (isElastic)
                mLoopListTool.isItemLoop = !isElastic;
        }

        bool inertia = EditorGUILayout.ToggleLeft("inertia （惯性运动）", mLoopListTool.inertia);
        if (inertia != mLoopListTool.inertia)
        {
            dataIsChange = true;
            mLoopListTool.inertia = inertia;
        }

        mLoopListTool.space = EditorGUILayout.FloatField("space （item间距）", mLoopListTool.space);

        //base.OnInspectorGUI();
        if (dataIsChange)
        {
            EditorUtility.SetDirty(mLoopListTool);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
