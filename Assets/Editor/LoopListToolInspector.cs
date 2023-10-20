using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LoopListTool))]
public class LoopListToolInspector : Editor
{
    LoopListTool mLoopListTool;
    public override void OnInspectorGUI()
    {
        mLoopListTool = target as LoopListTool;
        bool isItemLoop = EditorGUILayout.Toggle("is Item Loop", mLoopListTool.isItemLoop);
        if (isItemLoop != mLoopListTool.isItemLoop)
        {
            mLoopListTool.isItemLoop = isItemLoop;
        }


        bool hType = EditorGUILayout.Toggle("horizontal Type", mLoopListTool.horizontalType);
        bool vType = EditorGUILayout.Toggle("vertical Type", mLoopListTool.verticalType);
        //Debug.LogError("hType:" + hType + " : " + "vType: " + vType);
        if (hType && hType != mLoopListTool.horizontalType)
        {
            //Debug.LogError("1111111111111");
            mLoopListTool.horizontalType = hType;
        }
        else if(vType && vType != mLoopListTool.verticalType)
        {
            //Debug.LogError("22222222222222");
            mLoopListTool.verticalType = vType;
        }

        bool inertia = EditorGUILayout.Toggle("inertia", mLoopListTool.inertia);
        if (inertia != mLoopListTool.inertia)
        {
            //Debug.LogError("3333333333333");
            mLoopListTool.inertia = inertia;
        }

        bool isElastic = EditorGUILayout.Toggle("isElastic", mLoopListTool.isElastic);
        if (isElastic != mLoopListTool.isElastic)
        {
            mLoopListTool.isElastic = isElastic;
        }
        base.OnInspectorGUI();
    }
}
