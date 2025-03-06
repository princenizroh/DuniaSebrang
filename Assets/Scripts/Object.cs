using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip audioClip;

    private void OnCollisionEnter () {
        FindObjectOfType<AI>().HearingSound(transform.position);
        audioSource.PlayOneShot(audioClip);
        Debug.Log("object hit ground");
    }
}
