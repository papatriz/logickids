using UnityEngine.Analytics;
using UnityEngine;
 
public class UnityAnalyticDisabler : MonoBehaviour
{
    // Disables analytics before any code runs that sends analytic events
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void OnRuntimeMethodBeforeSceneLoad()
    {
        Analytics.enabled = false;
        Analytics.deviceStatsEnabled = false;
        Analytics.initializeOnStartup = false;
        Analytics.limitUserTracking = true;
        PerformanceReporting.enabled = false;
    }
}
