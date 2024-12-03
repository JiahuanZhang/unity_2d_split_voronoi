using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public class FpsUpdater : MonoBehaviour
{
    public Text fpsText;
    ProfilerRecorder drawCallsRecorder;

    private int count;
    private float deltaTime;

    public void Start()
    {
    }

    private void OnEnable()
    {
        drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
    }

    private void OnDisable()
    {
        drawCallsRecorder.Dispose();
    }

    void Update()
    {
        count++;
        deltaTime += Time.deltaTime;

        if (deltaTime >= 0.5f)
        {
            var fps = count / deltaTime;
            count = 0;
            deltaTime = 0;

            var sb = new StringBuilder();
            sb.AppendLine($"FPS: {Mathf.Ceil(fps)}");
            if (drawCallsRecorder.Valid)
                sb.AppendLine($"Draw Calls: {drawCallsRecorder.LastValue}");
            fpsText.text = sb.ToString();
        }
    }

}
