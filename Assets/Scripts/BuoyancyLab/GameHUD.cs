using System.Collections.Generic;
using UnityEngine;

public sealed class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    [System.Serializable]
    public struct ItemInfo
    {
        public Transform target;
        public string title;
        public string subtitle;
        public Color color;
    }

    [SerializeField] List<ItemInfo> items = new List<ItemInfo>();
    GUIStyle titleStyle;
    GUIStyle bodyStyle;
    GUIStyle itemStyle;
    GUIStyle centerStyle;
    string interactionText = string.Empty;
    int collected;
    int total = 3;
    bool phaseComplete;

    void Awake() => Instance = this;

    public void AddItem(Transform target, string title, string subtitle, Color color)
    {
        items.Add(new ItemInfo { target = target, title = title, subtitle = subtitle, color = color });
    }

    void BuildStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold };
        titleStyle.normal.textColor = new Color(0.55f, 0.95f, 1f);
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, richText = true, wordWrap = true };
        bodyStyle.normal.textColor = Color.white;
        itemStyle = new GUIStyle(GUI.skin.box) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, richText = true };
        itemStyle.normal.textColor = Color.white;
        centerStyle = new GUIStyle(titleStyle) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
    }

    void OnGUI()
    {
        BuildStyles();
        GUI.color = new Color(0.02f, 0.06f, 0.16f, 0.9f);
        GUI.Box(new Rect(18, 18, 440, 100), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(34, 25, 410, 36), "LABORATÓRIO DE EMPUXO", titleStyle);
        GUI.Label(new Rect(34, 62, 410, 50), "A/D: mover  •  W/S: nadar  •  Espaço: pular/subir  •  E: pegar  •  R: reiniciar", bodyStyle);

        Camera cam = Camera.main;
        if (cam == null) return;
        foreach (ItemInfo item in items)
        {
            if (item.target == null) continue;
            Vector3 p = cam.WorldToScreenPoint(item.target.position + Vector3.up * 1.35f);
            if (p.z < 0f) continue;
            Rect rect = new Rect(p.x - 78f, Screen.height - p.y - 22f, 156f, 44f);
            GUI.color = new Color(item.color.r, item.color.g, item.color.b, 0.94f);
            GUI.Box(rect, $"{item.title}\n<size=11>{item.subtitle}</size>", itemStyle);
        }

        GUI.color = new Color(0.04f, 0.12f, 0.22f, 0.9f);
        GUI.Box(new Rect(Screen.width - 308, 18, 290, 78), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width - 294, 28, 268, 56), $"Objetos recuperados: <b>{collected}/{total}</b>\nJogue na água, nade até eles e pressione E.", bodyStyle);

        if (!string.IsNullOrEmpty(interactionText))
        {
            GUI.color = new Color(0.02f, 0.08f, 0.16f, 0.94f);
            GUI.Box(new Rect(Screen.width * 0.5f - 265f, Screen.height - 92f, 530f, 58f), GUIContent.none);
            GUI.color = new Color(0.55f, 0.96f, 1f);
            GUI.Label(new Rect(Screen.width * 0.5f - 250f, Screen.height - 82f, 500f, 40f), interactionText, centerStyle);
        }

        if (phaseComplete)
        {
            GUI.color = new Color(0.01f, 0.04f, 0.11f, 0.96f);
            GUI.Box(new Rect(Screen.width * 0.5f - 310f, Screen.height * 0.5f - 105f, 620f, 210f), GUIContent.none);
            GUI.color = new Color(1f, 0.74f, 0.16f);
            GUI.Label(new Rect(Screen.width * 0.5f - 280f, Screen.height * 0.5f - 70f, 560f, 70f), "FASE CONCLUÍDA!", centerStyle);
            GUI.color = Color.white;
            GUI.Label(new Rect(Screen.width * 0.5f - 250f, Screen.height * 0.5f + 10f, 500f, 60f), "Você testou as três densidades e recuperou todos os objetos.\nPressione R para jogar novamente.", centerStyle);
        }
    }

    public void SetInteraction(string message) => interactionText = message;

    public void SetProgress(int value, int maximum)
    {
        collected = value;
        total = maximum;
    }

    public void SetPhaseComplete() => phaseComplete = true;
}
