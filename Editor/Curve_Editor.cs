using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Curve))]
public class Curve_Editor : Editor
{
    Curve curve;

    SerializedProperty sp_points;

    int selectedPoint = -1;
    
    void OnEnable()
    {
        curve = serializedObject.targetObject as Curve;
        sp_points = serializedObject.FindProperty("points");
    }

    KeyCode keyTracker;

    Vector3 _hPos = Vector3.zero;

    Vector3 hPos
    {
        get { return _hPos; }
        set { _hPos = value;
            UpdateHandleSize();
        }
    }
    float hSize = 1f;

    const float handleSizeScale = 0.03f;

    void OnSceneGUI()
    {
        switch (Event.current.type)
        {
            case EventType.KeyDown:
                keyTracker = Event.current.keyCode;
                break;
            case EventType.KeyUp:
                if (keyTracker == Event.current.keyCode) keyTracker = KeyCode.None;
                break;
        }
        
        Handles.matrix = curve.useLocalSpace? curve.transform.localToWorldMatrix : Matrix4x4.identity ;
        
        for (int i = 0; i < sp_points.arraySize; ++i)
        {
            DrawPoint(i);

            if (i > 0)
            {
                DrawSegment(i - 1, i);
            }
            else if (curve.loop)
            {
                DrawSegment(sp_points.arraySize-1, 0);
            }
        }

        if (serializedObject.ApplyModifiedProperties())
        {
            
        }
    }

    void DrawPoint(int index)
    {
        var s_point = sp_points.GetArrayElementAtIndex(index);
    
        var s_pos = s_point.FindPropertyRelative("position");
        var s_tangentIn = s_point.FindPropertyRelative("tangentIn");
        var s_tangentOut = s_point.FindPropertyRelative("tangentOut");
        
        if (index>0 && !curve.loop) Handles.DrawLine( s_pos.vector3Value, s_pos.vector3Value + s_tangentIn.vector3Value );
        if (index<(sp_points.arraySize-1) && !curve.loop) Handles.DrawLine( s_pos.vector3Value, s_pos.vector3Value + s_tangentOut.vector3Value );

        hPos = s_pos.vector3Value;
        
        if (index == selectedPoint)
        {
            s_pos.vector3Value = Handles.PositionHandle(s_pos.vector3Value, Quaternion.identity);
        }
        else if (
            Handles.Button(
                hPos,
                Quaternion.identity,
                hSize,
                hSize,
                Handles.DotHandleCap))
        {
            selectedPoint = index;
        }

        if (!curve.loop)
        {
            if (index == 0)
            {
                hPos = s_pos.vector3Value - s_tangentOut.vector3Value;
                if (Handles.Button(hPos, Quaternion.identity, hSize, hSize, Handles.DotHandleCap))
                {
                    InsertPoint(
                        0,
                        hPos,
                        s_tangentIn.vector3Value,
                        s_tangentOut.vector3Value
                    );

                    sp_points.MoveArrayElement(0, 1);
                    selectedPoint = 0;
                }
            }

            if (index == (sp_points.arraySize - 1))
            {
                hPos = s_pos.vector3Value - s_tangentIn.vector3Value;
                if (Handles.Button(hPos, Quaternion.identity, hSize, hSize, Handles.DotHandleCap))
                {
                    InsertPoint(
                        sp_points.arraySize - 1,
                        hPos,
                        s_tangentIn.vector3Value,
                        s_tangentOut.vector3Value
                    );
                }
            }
        }
    }

    void DrawSegment(int startIndex, int endIndex)
    {
        var s_point1 = sp_points.GetArrayElementAtIndex(startIndex); 
        var s_pos1 = s_point1.FindPropertyRelative("position");
        var s_tangentOut1 = s_point1.FindPropertyRelative("tangentOut");
        var s_point2 = sp_points.GetArrayElementAtIndex(endIndex); 
        var s_pos2 = s_point2.FindPropertyRelative("position");
        var s_tangentIn2 = s_point2.FindPropertyRelative("tangentIn");
        
        Handles.DrawBezier(
            s_pos1.vector3Value,
            s_pos2.vector3Value,
            s_pos1.vector3Value + s_tangentOut1.vector3Value,
            s_pos2.vector3Value + s_tangentIn2.vector3Value, 
            Handles.color, 
            Texture2D.whiteTexture, 
            1f
            );

        hPos = curve.InterpolatePoints(startIndex, endIndex, .5f);
        
        if (Handles.Button(hPos, Quaternion.identity, hSize, hSize, Handles.DotHandleCap))
        {
            InsertPoint(
                startIndex,
                hPos,
                -s_tangentOut1.vector3Value,
                -s_tangentIn2.vector3Value
            );
        }
    }

    void UpdateHandleSize()
    {
        hSize = HandleUtility.GetHandleSize(_hPos) * handleSizeScale;
    }

    void DeletePoint(int index)
    {
        if (sp_points.arraySize < 3) return;
        
        sp_points.DeleteArrayElementAtIndex(index);

        if (sp_points.arraySize >= selectedPoint) selectedPoint = sp_points.arraySize - 1;
    }

    void InsertPoint(int index, Vector3 position, Vector3 tangentIn, Vector3 tangentOut)
    {
        sp_points.InsertArrayElementAtIndex(index);
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        
        var newPoint = sp_points.GetArrayElementAtIndex(index+1);

        newPoint.FindPropertyRelative("position").vector3Value = position;
        newPoint.FindPropertyRelative("tangentIn").vector3Value = tangentIn;
        newPoint.FindPropertyRelative("tangentOut").vector3Value = tangentOut;
        
        selectedPoint = index+1;
    }
}
