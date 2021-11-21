using UnityEngine;
using UnityEditor;

namespace uREPL
{

[CustomEditor(typeof(Main))]
public class MainEditor : Editor
{
    private Main main
    {
        get { return target as Main; }
    }

    private Parameters parameters
    {
        get { return main.parameters; }
    }

    private EditorParameters editor
    {
        get { return main.editor; }
    }

    private bool completionsFoldOut 
    {
        get { return editor.completionFoldOut;  }
        set { editor.completionFoldOut = value; }
    }

    private void DrawCompletionPluginGUI()
    {
        editor.completionFoldOut = EditorGUILayout.Foldout(editor.completionFoldOut, "Completion Plugin");
        if (editor.completionFoldOut) {
            ++EditorGUI.indentLevel;
            Toggle("Mono", ref parameters.useMonoCompletion);
            Toggle("Command", ref parameters.useCommandCompletion);
            Toggle("Runtime Command", ref parameters.useRuntimeCommandCompletion);
            Toggle("GameObject Name", ref parameters.useGameObjectNameCompletion);
            Toggle("GameObject Path", ref parameters.useGameObjectPathCompletion);
            Toggle("Global Class", ref parameters.useGlobalClassCompletion);
            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }
    }

    private void DrawCompletoinParametersGUI()
    {
        editor.parametersFoldOut = EditorGUILayout.Foldout(editor.parametersFoldOut, "Parameters");
        if (editor.parametersFoldOut) {
            ++EditorGUI.indentLevel;
            Float("Completion Delay", ref parameters.completionDelay);
            Float("Annotation Delay", ref parameters.annotationDelay);
            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }
    }

    private void DrawPreLoadCommand()
    {
        EditorGUILayout.LabelField("Pre-Load Command");
        var code = EditorGUILayout.TextArea(main.preLoadCode, GUILayout.MinHeight(40f));
        if (code != main.preLoadCode) {
            main.preLoadCode = code;
            EditorUtility.SetDirty(main);
        }
    }

    public override void OnInspectorGUI()
    {
        DrawCompletionPluginGUI();
        DrawCompletoinParametersGUI();
        DrawPreLoadCommand();
    }

    public void Toggle(string name, ref bool param)
    {
        var result = EditorGUILayout.Toggle(name, param);
        if (result != param) {
            param = result;
            EditorUtility.SetDirty(main);
        }
    }

    public void Float(string name, ref float param)
    {
        var result = EditorGUILayout.FloatField(name, param);
        if (result != param) {
            param = result;
            EditorUtility.SetDirty(main);
        }
    }
}

}