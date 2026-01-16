using UnityEngine;

public static class SaveSystem
{
    public static void Save(Vector3 playerPosition, string sceneName, int checkpointIndex)
    {
        PlayerPrefs.SetInt("CheckpointIndex", checkpointIndex);

        PlayerPrefs.SetFloat("PlayerX", playerPosition.x);
        PlayerPrefs.SetFloat("PlayerY", playerPosition.y);
        PlayerPrefs.SetFloat("PlayerZ", playerPosition.z);

        PlayerPrefs.SetString("SceneName", sceneName);
        PlayerPrefs.Save();
    }

    public static int GetCheckpointIndex()
    {
        return PlayerPrefs.GetInt("CheckpointIndex", -1);
    }

    public static bool CanContinue()
    {
        return PlayerPrefs.HasKey("SceneName");
    }
    public static void SaveAbilities(bool jump, bool dash, bool wall)
    {
        PlayerPrefs.SetInt("CanJump", jump ? 1 : 0);
        PlayerPrefs.SetInt("CanDash", dash ? 1 : 0);
        PlayerPrefs.SetInt("CanWallGrab", wall ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void LoadAbilities(PlayerMovement player)
    {
        player.canJump = PlayerPrefs.GetInt("CanJump", 0) == 1;
        player.canDash = PlayerPrefs.GetInt("CanDash", 0) == 1;
        player.canGrabWall = PlayerPrefs.GetInt("CanWallGrab", 0) == 1;

        player.isControlBlocked = false;
        player.isInDialogue = false;
        player.isIntroPlaying = false;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 5f;
        }
    }
    public static Vector3 LoadPosition()
    {
        return new Vector3(
            PlayerPrefs.GetFloat("PlayerX"),
            PlayerPrefs.GetFloat("PlayerY"),
            PlayerPrefs.GetFloat("PlayerZ")
        );
    }

    public static string LoadScene()
    {
        return PlayerPrefs.GetString("SceneName");
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
