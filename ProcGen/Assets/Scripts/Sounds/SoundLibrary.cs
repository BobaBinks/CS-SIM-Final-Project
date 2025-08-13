using UnityEngine;
using System.Collections.Generic;

public class SoundLibrary: MonoBehaviour
{
    public static SoundLibrary Instance { get; private set; }

    public enum Player
    {
        PLAYER_HIT,
        SWORD_SFX,
        CROSSBOW_SFX,
        HEAL,
        XP_GAIN,
        FOOTSTEP_1,
        FOOTSTEP_2,
        FOOTSTEP_3,
        FOOTSTEP_4,
        FOOTSTEP_5
    }

    public enum Enemy
    {
        BITE,
        PHYSICAL_ATTACK,
        HIT,
        FOOTSTEP_1,
        FOOTSTEP_2,
        FOOTSTEP_3,
        FOOTSTEP_4,
        FOOTSTEP_5
    }

    public enum Spells
    {
        FIREBALL_1,
        TELEPORT
    }

    public enum Music
    {
        MENU,
        DUNGEON_1,
        DUNGEON_2,
        BOSS
    }

    [Header("Player Clips")]
    [SerializeField] List<AudioClip> playerClips;

    [Header("Enemy Clips")]
    [SerializeField] List<AudioClip> enemyClips;

    [Header("Spells Clips")]
    [SerializeField] List<AudioClip> spellsClips;

    [Header("Music")]
    [SerializeField] List<AudioClip> musicClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(this);
    }

    public AudioClip GetAudioClip(Player sound)
    {
        int index = (int)sound;
        if (index >= 0 && index < playerClips.Count)
            return playerClips[index];

        Debug.LogWarning($"Player sound '{sound}' not assigned.");
        return null;
    }

    public AudioClip GetAudioClip(Enemy sound)
    {
        int index = (int)sound;
        if (index >= 0 && index < enemyClips.Count)
            return enemyClips[index];

        Debug.LogWarning($"Enemy sound '{sound}' not assigned.");
        return null;
    }

    public AudioClip GetAudioClip(Spells sound)
    {
        int index = (int)sound;
        if (index >= 0 && index < spellsClips.Count)
            return spellsClips[index];

        Debug.LogWarning($"Spell sound '{sound}' not assigned.");
        return null;
    }

    public AudioClip GetAudioClip(Music sound)
    {
        int index = (int)sound;
        if (index >= 0 && index < musicClips.Count)
            return musicClips[index];

        Debug.LogWarning($"Music sound '{sound}' not assigned.");
        return null;
    }
}