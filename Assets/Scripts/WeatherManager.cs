using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class WeatherManager : MonoBehaviour
{
    Material sbMaterial;
    private PostProcessVolume ppVolume;
    private Vignette vignette;

    public Vector2 tbGrade = Vector2.zero;

    private Color sbCurrent;
    private Color sbTarget;

    private Color abCurrent;
    private Color abTarget;

    private Color vgCurrent;
    private Color vgTarget;

    private bool isToUpdate = false;

    private float updatingTime = 2.0f;
    private float updatingTimer = 0.0f;

    private Color t0b1SB = new Color(0.261f, 0.542f, 0.547f, 1.0f);
    private Color t0b1AB = new Color(0.363f, 0.617f, 1.0f, 1.0f);
    private Color t0b1VG = new Color(0.259f, 0.756f, 1.0f, 1.0f);

    private Color t1b1SB = new Color(0.651f, 0.453f, 0.144f, 1.0f);
    private Color t1b1AB = new Color(1.0f, 0.375f, 0.000f, 1.0f);
    private Color t1b1VG = new Color(1.0f, 0.346f, 0.000f, 1.0f);

    private Color t0b0SB = new Color(0.067f, 0.059f, 0.142f, 1.0f);
    private Color t0b0AB = new Color(0.000f, 0.116f, 0.292f, 1.0f);
    private Color t0b0VG = new Color(0.000f, 0.165f, 0.245f, 1.0f);

    private Color t1b0SB = new Color(0.085f, 0.057f, 0.014f, 1.0f);
    private Color t1b0AB = new Color(0.028f, 0.012f, 0.000f, 1.0f);
    private Color t1b0VG = new Color(0.113f, 0.039f, 0.000f, 1.0f);

    private void Start()
    {
        sbMaterial = RenderSettings.skybox;

        ppVolume = GetComponent<PostProcessVolume>();

    }

    private void Update()
    {
        if(isToUpdate)
        {
            if(updatingTimer < updatingTime)
            {
                sbMaterial.SetColor("_Tint", Color.Lerp(sbCurrent, sbTarget, updatingTimer / updatingTime));
                RenderSettings.ambientLight = Color.Lerp(abCurrent, abTarget, updatingTimer / updatingTime);
                vignette.color.Override(Color.Lerp(vgCurrent, vgTarget, updatingTimer / updatingTime));

                updatingTimer += Time.deltaTime;
            }
            else
            {
                updatingTimer = 0.0f;
                isToUpdate = false;
            }
        }
    }

    private void UpdateTempBri(Vector2 tbVector)
    {
        ppVolume.profile.TryGetSettings(out vignette);

        sbCurrent = sbMaterial.GetColor("_Tint");
        abCurrent = RenderSettings.ambientLight;
        vgCurrent = vignette.color.value;


        if (tbVector.y > 0.5)
        {
            sbTarget = Color.Lerp(t0b1SB, t1b1SB, tbVector.x);
            abTarget = Color.Lerp(t0b1AB, t1b1AB, tbVector.x);
            vgTarget = Color.Lerp(t0b1VG, t1b1VG, tbVector.x);
        }
        else
        {
            sbTarget = Color.Lerp(t0b0SB, t1b0SB, tbVector.x);
            abTarget = Color.Lerp(t0b0AB, t1b0AB, tbVector.x);
            vgTarget = Color.Lerp(t0b0VG, t1b0VG, tbVector.x);
        }
        
        isToUpdate = true;
    }

    [ContextMenu("Testing Grade")]
    public void Test()
    {
        UpdateTempBri(tbGrade);
    }

}
