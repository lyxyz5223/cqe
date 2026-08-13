using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FpsController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsTextUI = null;

    private Coroutine fpsUpdater = null;
    private YieldInstruction waitForSeconds = new WaitForSeconds(1f);

    public float fps = 0f;
    private float deltaTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;

#if UNITY_ANDROID || UNITY_IOS
        // 移动平台，必须指定具体数值
        SetMobileFrameRate();
#else
        // 桌面平台，-1表示不限制
        Application.targetFrameRate = -1;
#endif
    }

    void SetMobileFrameRate()
    {
        int targetFPS = 60; // 默认

#if UNITY_ANDROID
        // Android获取屏幕刷新率
        int refreshRate = (int)Screen.currentResolution.refreshRateRatio.value;

        if (refreshRate >= 120)
        {
            targetFPS = 120; // 高刷手机
        }
        else if (refreshRate >= 90)
        {
            targetFPS = 90;  // 中高刷手机
        }
        else
        {
            targetFPS = 60;  // 普通手机
        }
#endif

        Application.targetFrameRate = targetFPS;
        Debug.Log($"Set target frame rate to: {targetFPS} FPS");
    }

    private void OnEnable()
    {
        fpsUpdater = StartCoroutine(UpdateFps());
    }

    private void OnDisable()
    {
        if (fpsUpdater != null)
            StopCoroutine(fpsUpdater);
    }

    // Update is called once per frame
    void Update()
    {
        // 计算帧率
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    IEnumerator UpdateFps()
    {
        while (true)
        {
            if (fpsTextUI)
            {
                if (deltaTime > 0)
                    fpsTextUI.SetText($"{1 / deltaTime:F1}");
            }
            yield return waitForSeconds;
        }
    }
}
