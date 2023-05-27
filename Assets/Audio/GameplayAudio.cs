using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayAudio : MonoBehaviour
{
    public List<AudioClip> orderedNotesToPlay;
    private int currentIdx = 0;
    private AudioSource audioSource;
    private bool menuMusic = false;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayMenuMusic();
    }

    public void PlayNextNote()
    {
        audioSource.PlayOneShot(orderedNotesToPlay[currentIdx]);
        currentIdx = (currentIdx + 1) % orderedNotesToPlay.Count;
    }

    public void PlayMenuMusic()
    {
        StartCoroutine(MenuMusic());
    }

    public void StopMenuMusic()
    {
        menuMusic = false;
    }

    private IEnumerator MenuMusic()
    {
        menuMusic = true;
        while (menuMusic)
        {
            audioSource.PlayOneShot(orderedNotesToPlay[Random.Range(0,orderedNotesToPlay.Count)]);
            yield return new WaitForSeconds(Random.Range(0f, 1f));
        }
    }

    public void PlayRandomChord(int numNotes)
    {
        for (int i = 0; i < numNotes; i ++)
        {
            audioSource.PlayOneShot(orderedNotesToPlay[Random.Range(0,orderedNotesToPlay.Count)]);
            audioSource.PlayOneShot(orderedNotesToPlay[Random.Range(0,orderedNotesToPlay.Count)]);
            audioSource.PlayOneShot(orderedNotesToPlay[Random.Range(0,orderedNotesToPlay.Count)]);
        }
    }
}
