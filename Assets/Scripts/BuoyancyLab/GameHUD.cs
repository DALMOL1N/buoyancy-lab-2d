using System.Collections.Generic;
using UnityEngine;

public sealed class GameHUD : MonoBehaviour
{
    public struct ItemInfo
    {
        public Transform target;
        public string title;
        public string subtitle;
        public Color color;
    }

    readonly List<ItemInfo> items = new List<ItemInfo>();
    GUIStyle titleStyle;
    GUIStyle bodyStyle;
    GUIStyle itemStyle;

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
    }

    void OnGUI()
    {
        BuildStyles();
        GUI.color = new Color(0.02f, 0.06f, 0.16f, 0.9f);
        GUI.Box(new Rect(18, 18, 440, 100), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(34, 25, 410, 36), "LABORATÓRIO DE EMPUXO", titleStyle);
        GUI.Label(new Rect(34, 62, 410, 50), "A/D ou setas: mover  •  Espaço: pular/nadar  •  R: reiniciar", bodyStyle);

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
        GUI.Label(new Rect(Screen.width - 294, 28, 268, 56), "Empurre os três objetos na água.\nObserve quem afunda, equilibra e boia.", bodyStyle);
    }
}
