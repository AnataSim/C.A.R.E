using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class PanelAnimationSetup : EditorWindow
{
    [MenuItem("Tools/Setup Animasi & Suara Panel")]
    public static void SetupPanelAnimation()
    {
        // 1. Buat folder Assets/Animations jika belum ada
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
        {
            AssetDatabase.CreateFolder("Assets", "Animations");
        }

        string animPath = "Assets/Animations/Panel_PopUp.anim";
        string controllerPath = "Assets/Animations/Panel_Controller.controller";

        // 2. Buat AnimationClip dengan kurva skala (efek Pop-Up bounce)
        AnimationClip clip = new AnimationClip();
        clip.name = "Panel_PopUp";

        // Kurva Skala X dan Y: dari 0 -> 1.1 (sedikit membal) -> 1.0
        Keyframe[] keysX = new Keyframe[]
        {
            new Keyframe(0f, 0f, 0f, 5f),
            new Keyframe(0.2f, 1.1f, 0f, 0f),
            new Keyframe(0.35f, 1f, 0f, 0f)
        };
        Keyframe[] keysY = new Keyframe[]
        {
            new Keyframe(0f, 0f, 0f, 5f),
            new Keyframe(0.2f, 1.1f, 0f, 0f),
            new Keyframe(0.35f, 1f, 0f, 0f)
        };
        Keyframe[] keysZ = new Keyframe[]
        {
            new Keyframe(0f, 1f),
            new Keyframe(0.35f, 1f)
        };

        clip.SetCurve("", typeof(Transform), "m_LocalScale.x", new AnimationCurve(keysX));
        clip.SetCurve("", typeof(Transform), "m_LocalScale.y", new AnimationCurve(keysY));
        clip.SetCurve("", typeof(Transform), "m_LocalScale.z", new AnimationCurve(keysZ));

        // 3. Tambahkan efek suara ke Frame 0 animasi menggunakan Animation Event
        AudioClip whooshSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_ui/soundreality-whoosh-bamboo-389752.mp3");

        AnimationEvent soundEvent = new AnimationEvent();
        soundEvent.time = 0f; // Frame pertama
        soundEvent.functionName = "PlaySound";
        soundEvent.objectReferenceParameter = whooshSound;

        AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[] { soundEvent });

        // Simpan asset AnimationClip
        AssetDatabase.CreateAsset(clip, animPath);

        // 4. Buat Animator Controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddMotion(clip);

        // 5. Cari GameObject Panel di Scene
        GameObject targetPanel = Selection.activeGameObject;
        if (targetPanel == null || !targetPanel.name.Contains("Panel"))
        {
            targetPanel = GameObject.Find("Panel");
        }

        if (targetPanel != null)
        {
            // Pasang komponen Animator
            Animator animator = targetPanel.GetComponent<Animator>();
            if (animator == null)
            {
                animator = targetPanel.AddComponent<Animator>();
            }
            animator.runtimeAnimatorController = controller;

            // Pasang komponen AudioSource
            AudioSource audioSource = targetPanel.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = targetPanel.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D Sound

            // Pasang script AnimationAudioPlayer
            AnimationAudioPlayer audioPlayer = targetPanel.GetComponent<AnimationAudioPlayer>();
            if (audioPlayer == null)
            {
                audioPlayer = targetPanel.AddComponent<AnimationAudioPlayer>();
            }

            // Tandai scene telah dimodifikasi
            EditorUtility.SetDirty(targetPanel);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(targetPanel.scene);

            Selection.activeGameObject = targetPanel;

            EditorUtility.DisplayDialog(
                "Berhasil!",
                $"Animasi Pop-Up dan Efek Suara Whoosh berhasil dibuat dan dipasang ke GameObject '{targetPanel.name}'!\n\n" +
                $"File yang dibuat:\n- {animPath}\n- {controllerPath}",
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Aset Dibuat!",
                $"Animasi Pop-Up dan Animator Controller berhasil dibuat di folder:\n- {animPath}\n- {controllerPath}\n\nSilakan seret file controller tersebut ke objek Panel yang Anda inginkan.",
                "OK"
            );
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

