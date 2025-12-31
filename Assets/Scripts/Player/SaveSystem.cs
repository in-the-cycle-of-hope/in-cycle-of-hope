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
        return GetCheckpointIndex() > 0;
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
