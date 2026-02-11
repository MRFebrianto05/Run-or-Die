using UnityEngine;
using UnityEngine.SceneManagement; // Wajib ada untuk pindah scene

public class autoScene : MonoBehaviour
{
    [Header("Pengaturan")]
    public string namaSceneTujuan = "Gameplay"; // Ganti dengan nama scene kamu
    public float durasiWaktu = 5f;              // Berapa detik sebelum pindah otomatis
    public KeyCode tombolSkip = KeyCode.Space;  // Tombol untuk skip

    private float timer;

    void Start()
    {
        timer = durasiWaktu;
    }

    void Update()
    {
        // 1. Kurangi waktu setiap detik
        timer -= Time.deltaTime;

        // 2. Cek apakah waktu habis ATAU tombol spasi ditekan
        if (timer <= 0 || Input.GetKeyDown(tombolSkip))
        {
            PindahScene();
        }
    }

    void PindahScene()
    {
        // Mencegah error jika nama scene salah/kosong
        if (Application.CanStreamedLevelBeLoaded(namaSceneTujuan))
        {
            SceneManager.LoadScene(namaSceneTujuan);
        }
        else
        {
            Debug.LogError("Scene '" + namaSceneTujuan + "' tidak ditemukan! Cek Build Profiles.");
        }
    }
}