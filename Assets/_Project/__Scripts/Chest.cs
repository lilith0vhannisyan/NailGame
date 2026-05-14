using UnityEngine;
using System.Collections.Generic;

public class Chest : MonoBehaviour
{
    [Header("Win Condition")]
    [Tooltip("If ANY ONE of these boards is removed → WIN")]
    public List<Board> requiredBoards = new List<Board>();


    [SerializeField] private Animator animator;
    void Start()
    {
        GameManager.Instance.RegisterChest(this);
    }

    // WIN if ANY ONE board is removed
    public bool IsUnlocked()
    {
        if (requiredBoards.Count == 0) return false;

        foreach (Board b in requiredBoards)
        {
            // null = destroyed = removed ✅
            if (b == null || b.IsRemoved())
            {
                animator.SetTrigger("Open");
                return true;    // ← ANY ONE is enough
            }

        }
        return false;
    }
}