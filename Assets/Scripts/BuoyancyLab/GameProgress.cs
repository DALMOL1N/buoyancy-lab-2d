using UnityEngine;

public sealed class GameProgress : MonoBehaviour
{
    int collected;
    int total;
    GameHUD hud;

    public void Configure(GameHUD gameHud, int itemCount)
    {
        hud = gameHud;
        total = itemCount;
        hud.SetProgress(0, total);
    }

    public void RegisterCollection(string itemName)
    {
        collected++;
        hud.SetProgress(collected, total);
        if (collected >= total)
            hud.SetPhaseComplete();
    }
}
