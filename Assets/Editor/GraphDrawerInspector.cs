using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

[CustomEditor(typeof(GraphDrawer))]
public class GraphDrawerInspector : Editor
{
    VisualElement root;

    GraphDrawer graph;

    SerializedProperty functionType2dProp;
    SerializedProperty functionType3dProp;
    SerializedProperty figureType2dProp;
    SerializedProperty figureType3dProp;

    GraphDrawer.Type type;
    EnumField functionField;
    EnumField figureField;

    Button turn3D;
    Button turn2D;

    void Init()
    {
        InitVariables();
        FindProperties();

        graph = (GraphDrawer)target;

        graph.OnCurrentViewChangedCallback += RedrawInspector;
        functionField.RegisterCallback<ChangeEvent<Enum>>(evt => graph.Interpolate());
        figureField.RegisterCallback<ChangeEvent<Enum>>(evt => graph.Interpolate());
    }

    private void InitVariables()
    {
        root = new();
        functionField = new();
        figureField = new();
        turn3D = new();
        turn2D = new();
    }

    private void FindProperties()
    {
        functionType2dProp = serializedObject.FindProperty("functionType2d");
        functionType3dProp = serializedObject.FindProperty("functionType3d");
        figureType2dProp = serializedObject.FindProperty("figureType2d");
        figureType3dProp = serializedObject.FindProperty("figureType3d");
    }

    void RedrawInspector(GraphDrawer.Type newType)
    {
        type = newType;

        UpdateType();
    }

    private void UpdateType()
    {
        FindProperties();
        switch (type)
        {
            case GraphDrawer.Type._2D:
                functionField.label = "2D Function";
                functionField.Unbind();
                functionField.BindProperty(functionType2dProp);
                figureField.label = "2D Figure";
                figureField.Unbind();
                figureField.BindProperty(figureType2dProp);
                break;
            case GraphDrawer.Type._3D:
                functionField.label = "3D Function";
                functionField.Unbind();
                functionField.BindProperty(functionType3dProp);
                figureField.label = "3D Figure";
                figureField.Unbind();
                figureField.BindProperty(figureType3dProp);
                break;
        }
    }

    void TurnOnButtons()
    {
        if(graph.CurrentView == GraphDrawer.Type._3D)
        {
            turn3D.SetEnabled(false);
            turn2D.SetEnabled(true);
        }

        if(graph.CurrentView == GraphDrawer.Type._2D)
        {
            turn3D.SetEnabled(true);
            turn2D.SetEnabled(false);
        }
    }

    public override VisualElement CreateInspectorGUI()
    {
        Init();
        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        ConfigureButtons();
        UpdateType();

        return root;
    }

    private void ConfigureButtons()
    {
        turn3D.text = "3D";
        turn3D.clicked += () => { graph.ChangeView(GraphDrawer.Type._3D); TurnOnButtons(); graph.BuildGraph(); };
        turn2D.text = "2D";
        turn2D.clicked += () => { graph.ChangeView(GraphDrawer.Type._2D); TurnOnButtons(); graph.BuildGraph(); };

        TurnOnButtons();

        Button updatePosition = new();
        updatePosition.text = "Build Graph";
        updatePosition.clicked += () => { graph.BuildGraph(); };
        root.Add(updatePosition);

        root.Insert(1, turn3D);
        root.Insert(2, turn2D);
        root.Insert(3, functionField);
        root.Insert(4, figureField);
    }
}