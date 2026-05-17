using System.Collections;
using UnityEngine;

public class Rift_Controller : MonoBehaviour
{
    [Header("Applied to the effects at start")]
    [SerializeField] private Color effectsColor;
    [Header("Changing these might break the effects")]
    [Space(20)]
    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private ParticleSystem[] effectsParticles;
    [SerializeField] private Light riftLight;
    [SerializeField] private AudioSource[] effectsAudio;

    private float maxIntLight = 4;
    private float transitionSpeed = 0.8f;
    private bool inTransition, activated;
    private Material matInstance;
    private float fadeFloat;
    private Coroutine transitionCor, runeBlastCor;

    private void Awake()
    {
        matInstance = meshRenderer.material;
        matInstance.SetColor("_EmissionColor", effectsColor);
        matInstance.SetFloat("_EmissionStrength", 0);
        maxIntLight = riftLight.intensity;
        riftLight.intensity = 0f;
        riftLight.color = effectsColor;

        foreach (ParticleSystem part in effectsParticles)
        {
            ParticleSystem.MainModule mod = part.main;
            mod.startColor = effectsColor;
        }
    }

    public void F_ToggleRift(bool _activate)
    {
        if (inTransition || _activate == activated)
            return;

        activated = _activate;

        if (_activate)
        {
            for (int i = 0; i <= 3; i++)
                effectsParticles[i].Play();

            effectsAudio[0].Play();

            if (transitionCor != null) StopCoroutine(transitionCor);
            transitionCor = StartCoroutine(TransitionSequence());

            runeBlastCor = StartCoroutine(RuneBlasts());
        }
        else
        {
            if (runeBlastCor != null) StopCoroutine(runeBlastCor);

            for (int i = 0; i <= 3; i++)
                effectsParticles[i].Stop();

            if (transitionCor != null) StopCoroutine(transitionCor);
            transitionCor = StartCoroutine(TransitionSequence());
        }
    }

    private IEnumerator TransitionSequence()
    {
        inTransition = true;
        float target = activated ? 1f : 0f;

        while (fadeFloat != target)
        {
            fadeFloat = Mathf.MoveTowards(fadeFloat, target, Time.deltaTime * transitionSpeed);

            effectsAudio[0].volume = fadeFloat * 0.8f;
            matInstance.SetFloat("_EmissionStrength", fadeFloat);
            riftLight.intensity = maxIntLight * fadeFloat;

            yield return null;
        }

        if (!activated)
            effectsAudio[0].Stop();

        inTransition = false;
    }

    private IEnumerator RuneBlasts()
    {
        ParticleSystem.MainModule partMain = effectsParticles[4].main;

        while (true)
        {
            effectsParticles[4].Stop();
            partMain.duration = Random.Range(0.8f, 1f);
            effectsParticles[4].Play();
            effectsAudio[1].pitch = Random.Range(0.85f, 0.9f);
            effectsAudio[1].Play();
            yield return new WaitForSeconds(Random.Range(2f, 6f));
        }
    }
}