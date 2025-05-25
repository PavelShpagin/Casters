using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FixSSAOError : MonoBehaviour
{
    [Header("SSAO Error Fix")]
    [SerializeField] private bool fixOnStart = true;
    
    void Start()
    {
        if (fixOnStart)
        {
            FixSSAOIssues();
        }
    }
    
    [ContextMenu("Fix SSAO Errors")]
    public void FixSSAOIssues()
    {
        // Get the current URP asset
        var urpAsset = UniversalRenderPipeline.asset;
        if (urpAsset == null)
        {
            Debug.LogError("[FixSSAOError] No URP Asset found!");
            return;
        }
        
        Debug.Log("[FixSSAOError] Attempting to fix SSAO renderer feature issues...");
        
        // Force Unity to refresh the renderer features
        // This often resolves null reference issues with SSAO
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(urpAsset);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        #endif
        
        Debug.Log("[FixSSAOError] SSAO fix completed. Restart Unity Editor if issues persist.");
    }
} 