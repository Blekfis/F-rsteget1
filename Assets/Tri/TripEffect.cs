using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TripEffect : MonoBehaviour
{
    public Volume volume;
    
    private ColorAdjustments colorAdjustments;
    private LensDistortion lensDistortion;
    private ChromaticAberration chromatic;

    private float tripDuration = 0f;
    private float tripTimer = 0f;
    private bool isTripping = false;

    void Start()
    {
        // Hämtar effekterna från din Global Volume
        volume.profile.TryGet(out colorAdjustments);
        volume.profile.TryGet(out lensDistortion);
        volume.profile.TryGet(out chromatic);

        // Säkerställ att effekterna är avstängade från start
        ResetEffects();
    }

    // Anropa den här metoden för att starta effekten och bestämma hur länge den ska hålla i sig (i sekunder)
    public void StartTrip(float duration)
    {
        tripDuration = duration;
        tripTimer = 0f;
        isTripping = true;
    }

    void Update()
    {
        // Om effekten inte är igång gör vi ingenting
        if (!isTripping) return;

        // Räkna upp timglaset
        tripTimer += Time.deltaTime;

        // Beräkna hur långt gången effekten är (0.0 till 1.0)
        float progress = tripTimer / tripDuration;

        // Om tiden har runnit ut
        if (progress >= 1f)
        {
            ResetEffects();
            isTripping = false;
            return;
        }

        // Skapa en "fade"-faktor som dämpar effekten. 
        // Här låter vi den vara full styrka till 75% av tiden, och sedan fada ut linjärt under sista 25%.
        float fadeMultiplier = 1f;
        if (progress > 0.75f)
        {
            // Mappar om progress från (0.75 - 1.0) till (1.0 - 0.0)
            fadeMultiplier = Mathf.InverseLerp(1f, 0.75f, progress);
        }

        // --- Själva effekterna (multiplicerade med fadeMultiplier) ---

        if (colorAdjustments != null)
        {
            float hue = Mathf.PingPong(Time.time * 40f, 360f) - 180f;
            colorAdjustments.hueShift.value = hue * fadeMultiplier;
        }

        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = Mathf.Sin(Time.time * 2f) * 0.45f * fadeMultiplier;
        }

        if (chromatic != null)
        {
            chromatic.intensity.value = ((Mathf.Sin(Time.time * 3f) + 1f) * 0.5f) * fadeMultiplier;
        }
    }

    // Nollställer alla effekter till "normalt" läge
    void ResetEffects()
    {
        if (colorAdjustments != null) colorAdjustments.hueShift.value = 0f;
        if (lensDistortion != null) lensDistortion.intensity.value = 0f;
        if (chromatic != null) chromatic.intensity.value = 0f;
    }
}