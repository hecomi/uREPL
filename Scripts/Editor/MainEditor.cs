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

	public void DrawCompletionGUI()
	{
		editor.completionFoldOut = EditorGUILayout.Foldout(editor.completionFoldOut, "Completion");

		if (editor.completionFoldOut) {
			++EditorGUI.indentLevel;
			Toggle("Mono", ref parameters.useMonoCompletion);
			Toggle("Command", ref parameters.useCommandCompletion);
			Toggle("GameObject Name", ref parameters.useGameObjectNameCompletion);
			Toggle("GameObject Path", ref parameters.useGameObjectPathCompletion);
			EditorGUILayout.Separator();
			--EditorGUI.indentLevel;
		}
	}

	public override void OnInspectorGUI()
	{
		DrawCompletionGUI();
	}

	public void Toggle(string name, ref bool param)
	{
		var result = EditorGUILayout.Toggle(name, param);
		if (result != param) param = result;
	}
}

}