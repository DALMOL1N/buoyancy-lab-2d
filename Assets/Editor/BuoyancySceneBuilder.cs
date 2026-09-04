using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class BuoyancySceneBuilder
{
    const string ScenePath = "Assets/Scenes/BuoyancyLab.unity";
    const string GeneratedFolder = "Assets/Resources/GeneratedSprites";

    static BuoyancySceneBuilder()
    {
        EditorApplication.delayCall += EnsureEditableScene;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem("Buoyancy Lab/Reconstruir cena editável")]
    public static void RebuildFromMenu()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Pare o Play antes de reconstruir a cena.");
            return;
        }

        BuildAndSaveScene();
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += EnsureEditableScene;
    }

    static void EnsureEditableScene()
    {
        if (Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        SceneAsset existing = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (existing == null)
            BuildAndSaveScene();
    }

    static void BuildAndSaveScene()
    {
        EnsureFolder("Assets/Resources", "GeneratedSprites");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject("Buoyancy Lab - Editable");
        RuntimeGameBootstrap bootstrap = root.AddComponent<RuntimeGameBootstrap>();
        bootstrap.Build();

        PersistGeneratedSprites();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettingsScene[] buildScenes =
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };
        EditorBuildSettings.scenes = buildScenes;
        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);
        Debug.Log("Cena editável criada em " + ScenePath);
    }

    static void PersistGeneratedSprites()
    {
        Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Square.png");
        SpriteRenderer[] renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        var persisted = new Dictionary<int, Sprite>();
        int index = 0;

        foreach (SpriteRenderer renderer in renderers)
        {
            Sprite current = renderer.sprite;
            if (current == null) continue;

            if (current.name == "Runtime White Pixel")
            {
                renderer.sprite = square;
                continue;
            }

            if (EditorUtility.IsPersistent(current)) continue;
            int id = current.GetInstanceID();
            if (!persisted.TryGetValue(id, out Sprite saved))
            {
                string safeName = Sanitize(current.texture.name + "_" + index++);
                string assetPath = GeneratedFolder + "/" + safeName + ".asset";
                AssetDatabase.DeleteAsset(assetPath);
                saved = Object.Instantiate(current);
                saved.name = safeName;
                AssetDatabase.CreateAsset(saved, assetPath);
                persisted[id] = saved;
            }
            renderer.sprite = saved;
        }

        // Os frames que não estão ativos no SpriteRenderer também precisam ser persistidos.
        ExplorerController explorer = Object.FindFirstObjectByType<ExplorerController>();
        if (explorer != null)
        {
            SerializedObject serialized = new SerializedObject(explorer);
            SerializedProperty frames = serialized.FindProperty("frames");
            for (int i = 0; i < frames.arraySize; i++)
            {
                Sprite frame = frames.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
                if (frame == null || EditorUtility.IsPersistent(frame)) continue;
                string assetPath = GeneratedFolder + "/ExplorerFrame" + i + ".asset";
                AssetDatabase.DeleteAsset(assetPath);
                Sprite saved = Object.Instantiate(frame);
                saved.name = "ExplorerFrame" + i;
                AssetDatabase.CreateAsset(saved, assetPath);
                frames.GetArrayElementAtIndex(i).objectReferenceValue = saved;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        AssetDatabase.SaveAssets();
    }

    static string Sanitize(string value)
    {
        foreach (char invalid in Path.GetInvalidFileNameChars())
            value = value.Replace(invalid, '_');
        return value.Replace(' ', '_');
    }

    static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }
}
