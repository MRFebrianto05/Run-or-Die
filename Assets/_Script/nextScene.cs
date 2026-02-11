using UnityEngine;
using UnityEngine.SceneManagement; // WAJIB ADA: Library untuk urus Scene

public class nextScene : MonoBehaviour
{
    // Fungsi ini HARUS 'public' agar bisa dilihat oleh Tombol di Inspector
    public void PindahKeScene(string namaScene)
    {
        Debug.Log("Sedang pindah ke: " + namaScene);
        SceneManager.LoadScene(namaScene);
    }

    // Bonus: Fungsi untuk Restart Level saat ini
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}