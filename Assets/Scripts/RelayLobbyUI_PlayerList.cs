using UnityEngine;
using TMPro;
using UnityEngine.UI;

using System.Collections;

public class RelayLobbyUI_PlayerList : MonoBehaviour
{
    [Header("References")]
    public GameObject playerListContent;

    // Delegate to get the current player names
    public System.Func<string[]> GetPlayerNames;

    private string[] lastPlayerNames = null;

    private void Start()
    {
        // Start polling for player list changes
        StartCoroutine(PlayerListPollCoroutine());
    }

    private IEnumerator PlayerListPollCoroutine()
    {
        while (true)
        {
            string[] names = GetPlayerNames != null ? GetPlayerNames() : null;
            if (names != null && (lastPlayerNames == null || !AreArraysEqual(names, lastPlayerNames)))
            {
                UpdatePlayerList(names);
                lastPlayerNames = (string[])names.Clone();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private bool AreArraysEqual(string[] a, string[] b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }

    public void UpdatePlayerList(string[] playerNames)
    {
    // Debug log for playerNames removed to avoid ambiguous string.Join error
        StartCoroutine(UpdatePlayerListWithRetry(playerNames));
    }

    private void LogHierarchyDiagnostics()
    {
        if (playerListContent == null)
        {
            Debug.LogError("[RelayLobbyUI_PlayerList] Diagnostics: playerListContent is null");
            return;
        }
        Transform t = playerListContent.transform;
        int depth = 0;
        while (t != null)
        {
            var rect = t.GetComponent<RectTransform>();
            string rectInfo = rect != null ? $"RectTransform: anchorMin={rect.anchorMin}, anchorMax={rect.anchorMax}, offsetMin={rect.offsetMin}, offsetMax={rect.offsetMax}, sizeDelta={rect.sizeDelta}" : "No RectTransform";
            var compArr = t.GetComponents<Component>();
            string comps = string.Join(", ", System.Linq.Enumerable.Select(compArr, c => c.GetType().Name));
            Debug.Log($"[RelayLobbyUI_PlayerList] Hierarchy depth {depth}: {t.name} | {rectInfo} | Components: {comps}");
            t = t.parent;
            depth++;
        }
    }

    private IEnumerator UpdatePlayerListWithRetry(string[] playerNames)
    {
        int retries = 10;
        while (playerListContent == null && retries-- > 0)
        {
            // retry silently; avoid spamming the console
            yield return new WaitForSeconds(0.1f);
        }
        if (playerListContent == null)
        {
            // Try to find by name as fallback
            var go = GameObject.Find("PlayerListContent");
                if (go != null)
                {
                    playerListContent = go;
                }
        }
        if (playerListContent == null)
        {
            Debug.LogError("[RelayLobbyUI_PlayerList] playerListContent is STILL null after retries and fallback!");
            RelayLobbyUI_Utils.DumpUIHierarchy();
            yield break;
        }
    // (moved LogHierarchyDiagnostics to after RectTransform fix)

        // Ensure layout group and content size fitter are present
        var layout = playerListContent.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = playerListContent.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childAlignment = TextAnchor.UpperLeft;
        }
        var fitter = playerListContent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = playerListContent.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
        }
        // Get the content rect. Don't overwrite its anchors/pivot if it was created by the UI bootstrap
        // (the bootstrap intentionally sets top-stretch anchors for proper ScrollRect behaviour).
        var plcRect = playerListContent.GetComponent<RectTransform>();
        // Ensure size/position are sane but preserve anchors/pivot set by the scene/bootstrap
        plcRect.anchoredPosition = Vector2.zero;
        plcRect.sizeDelta = Vector2.zero;
    // Ensure the content transform has a sane scale (some editor/layout setups can leave scale at 0)
    plcRect.localScale = Vector3.one;
    // Defensive: check all ancestors and fix any accidental zero scale that would make children invisible
    var fixedAncestors = new System.Collections.Generic.List<string>();
    Transform at = playerListContent.transform;
    while (at != null)
    {
        var ls = at.localScale;
        if (Mathf.Approximately(ls.x, 0f) || Mathf.Approximately(ls.y, 0f) || Mathf.Approximately(ls.z, 0f))
        {
            at.localScale = Vector3.one;
            fixedAncestors.Add(at.name);
        }
        at = at.parent;
    }
    if (fixedAncestors.Count > 0)
    {
        Debug.LogWarning($"[RelayLobbyUI_PlayerList] Fixed zero-scale ancestors: {string.Join(", ", fixedAncestors.ToArray())}");
    }

        // Ensure ScrollRect (if present) references the content and viewport correctly
        var scrollRect = playerListContent.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        if (scrollRect != null)
        {
            if (scrollRect.content == null)
                scrollRect.content = plcRect;
            if (scrollRect.viewport == null)
            {
                var viewport = scrollRect.transform.Find("Viewport");
                if (viewport != null)
                    scrollRect.viewport = viewport.GetComponent<RectTransform>();
            }
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
        }

    // Diagnostics suppressed to keep console clean

        foreach (Transform child in playerListContent.transform)
            Destroy(child.gameObject);
        for (int i = 0; i < playerNames.Length; i++)
        {
            var name = playerNames[i];
            var entryGO = new GameObject("PlayerEntry");
            entryGO.transform.SetParent(playerListContent.transform, false);
            var entryRect = entryGO.AddComponent<RectTransform>();
            // make sure the newly created entry has a normal scale so it's visible
            entryRect.localScale = Vector3.one;
            // Use top-stretch anchors so VerticalLayoutGroup positions entries predictably under a top-anchored content
            entryRect.anchorMin = new Vector2(0, 1);
            entryRect.anchorMax = new Vector2(1, 1);
            entryRect.pivot = new Vector2(0.5f, 1f);
            var entryText = entryGO.AddComponent<TextMeshProUGUI>();
            entryText.text = name;
            entryText.fontSize = 24;
            entryText.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            entryText.alignment = TMPro.TextAlignmentOptions.Left;
            entryText.color = Color.white;
            var textRect = entryText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 0);
            textRect.offsetMax = new Vector2(-8, 0);
            textRect.pivot = new Vector2(0, 0.5f);
            var layoutElem = entryGO.AddComponent<UnityEngine.UI.LayoutElement>();
            layoutElem.minHeight = 32;
            layoutElem.preferredHeight = 36;
            // created
        }
        // Force a layout rebuild so entries appear immediately
        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(playerListContent.GetComponent<RectTransform>());
        // Conditional visibility diagnostics: only log when a viewport exists and none of the children are inside it
        var scrollRectDiag = playerListContent.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        RectTransform viewportRect = null;
        if (scrollRectDiag != null)
            viewportRect = scrollRectDiag.viewport ?? scrollRectDiag.transform.Find("Viewport") as RectTransform;

        bool anyInside = true; // assume visible if no viewport to check against
        if (viewportRect != null)
        {
            anyInside = false;
            Vector3[] vcorners = new Vector3[4];
            viewportRect.GetWorldCorners(vcorners);
            float vx0 = vcorners[0].x, vy0 = vcorners[0].y, vx2 = vcorners[2].x, vy2 = vcorners[2].y;
            for (int i = 0; i < playerListContent.transform.childCount; i++)
            {
                var child = (RectTransform)playerListContent.transform.GetChild(i);
                Vector3[] corners = new Vector3[4];
                child.GetWorldCorners(corners);
                foreach (var c in corners)
                    if (c.x >= vx0 && c.x <= vx2 && c.y >= vy0 && c.y <= vy2) { anyInside = true; break; }
                if (anyInside) break;
            }
        }

        if (!anyInside)
        {
            Debug.LogWarning($"[RelayLobbyUI_PlayerList] Visibility: No entries intersect the viewport. Children: {playerListContent.transform.childCount}");
            if (viewportRect != null)
            {
                Vector3[] vc = new Vector3[4];
                viewportRect.GetWorldCorners(vc);
                string vstr = string.Join(", ", System.Array.ConvertAll(vc, v => $"({v.x:0},{v.y:0})"));
                Debug.LogWarning($"[RelayLobbyUI_PlayerList] Viewport '{viewportRect.name}' world corners: {vstr}");
            }
            for (int i = 0; i < playerListContent.transform.childCount; i++)
            {
                var child = (RectTransform)playerListContent.transform.GetChild(i);
                Vector3[] corners = new Vector3[4];
                child.GetWorldCorners(corners);
                string cornersStr = string.Join(", ", System.Array.ConvertAll(corners, v => $"({v.x:0},{v.y:0})"));
                Debug.LogWarning($"[RelayLobbyUI_PlayerList] Child {i} '{child.name}' world corners: {cornersStr} | sizeDelta={child.sizeDelta}");
            }
            // also dump a compact list of components on the content parent for clue about masks/canvas
            var compArr = playerListContent.GetComponents<Component>();
            var names = System.Array.ConvertAll(compArr, c => c.GetType().Name);
            Debug.LogWarning($"[RelayLobbyUI_PlayerList] PlayerListContent components: {string.Join(", ", names)}");
        }

        Debug.Log($"[RelayLobbyUI_PlayerList] Player list update complete. Children count: {playerListContent.transform.childCount}");
    }
}
