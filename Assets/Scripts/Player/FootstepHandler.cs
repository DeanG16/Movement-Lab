using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepHandler : MonoBehaviour
{
    public AudioClip[] walkingFootStepFx;
    public AudioClip[] runningFootstepStepFx;

    public AudioClip[] jumpTakeOffFx;
    public AudioClip[] jumpLandingFx;

    public Transform footStepOrigin;

    StateManager stateManager;
    AudioSource audioSource;

    private int lastIndex = 0;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
        stateManager = GetComponent<StateManager>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    private void Update() {
    }

    AudioClip GetRandomSound(AudioClip[] source) {
        //AudioClip[] source = (stateManager.IsSprinting) ? runningFootstepStepFx : walkingFootStepFx;
        int randomIndex = Random.Range(0, source.Length - 1);
        if(randomIndex == lastIndex) {
            Debug.Log(randomIndex + ' ' + lastIndex);
            randomIndex = Random.Range(0, source.Length - 1);
        }
        lastIndex = randomIndex;
        return source[randomIndex];
    }

    public void PlayFootStep() {
        if(walkingFootStepFx.Length == 0) { return; }
        AudioClip sound = GetRandomSound(walkingFootStepFx);
        audioSource.clip = sound;
        audioSource.Play();
    }

    public void PlayJumpTakeOff() {
        if (jumpTakeOffFx.Length == 0) { return; }
        AudioClip sound = GetRandomSound(jumpTakeOffFx);
        audioSource.clip = sound;
        audioSource.Play();
    }
}
