using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Chest : MonoBehaviour
{
    [Header("Win Condition")]
    [Tooltip("If ANY ONE of these boards is removed → WIN")]
    public List<Board> requiredBoards = new List<Board>();

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem coinParticle;
    //[SerializeField] private AudioClip chestOpenningSound;
    // Remove keyAnimator SerializeField — find it automatically instead

    private KeyAnimator keyAnimator;

    void Start()
    {
        GameManager.Instance.RegisterChest(this);
        // Find Key in scene automatically
        keyAnimator = FindAnyObjectByType<KeyAnimator>();
    }

    public bool IsUnlocked()
    {
        if (requiredBoards.Count == 0) return false;
        foreach (Board b in requiredBoards)
        {
            if (b == null || b.IsRemoved())
                return true;
        }
        return false;
    }

    public void PlayOpenSequence()
    {
        if (keyAnimator != null)
            keyAnimator.EnterChestAndOpen(this);
        else
            OpenChest();
    }

    public void OpenChest()
    {
        if (animator != null)
        {
            animator.SetTrigger("Open");
            audioSource.Play();
            coinParticle.Play();
        }

        GameManager.Instance.OnChestOpened();
    }

    IEnumerator CoinParticles()
    {
        yield return new WaitForSeconds(0.1f);
        coinParticle.Play();
    }
    
}