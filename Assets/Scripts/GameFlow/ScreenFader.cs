using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CARE.UI
{
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image fadeImage;

        private Coroutine activeFadeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureComponents();
        }

        private void EnsureComponents()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (fadeImage == null)
            {
                fadeImage = GetComponent<Image>();
                if (fadeImage == null) fadeImage = gameObject.AddComponent<Image>();
                fadeImage.color = Color.black;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        /// <summary>
        /// Melakukan efek fade-out (layar meredup ke hitam), mengeksekusi onBlack saat gelap total,
        /// lalu fade-in (kembali terang) ke gameplay atau percakapan baru.
        /// </summary>
        public static void Do(Action onBlack, Action onFinish = null, float duration = 0.4f, float hold = 0.15f)
        {
            if (Instance != null)
            {
                Instance.FadeTransition(onBlack, onFinish, duration, hold);
            }
            else
            {
                onBlack?.Invoke();
                onFinish?.Invoke();
            }
        }

        public void FadeTransition(Action onBlack, Action onFinish = null, float duration = 0.4f, float hold = 0.15f)
        {
            if (activeFadeRoutine != null) StopCoroutine(activeFadeRoutine);
            activeFadeRoutine = StartCoroutine(FadeRoutine(onBlack, onFinish, duration, hold));
        }

        private IEnumerator FadeRoutine(Action onBlack, Action onFinish, float duration, float hold)
        {
            EnsureComponents();
            transform.SetAsLastSibling(); // Pastikan selalu berada paling atas di layer UI Canvas
            canvasGroup.blocksRaycasts = true;

            // Fade Out (Menuju Hitam Pekat)
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / duration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            // Jalankan perpindahan adegan / logika saat layar gelap
            try
            {
                onBlack?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScreenFader] Error in onBlack callback: {ex}");
            }

            // Tahan sejenak dalam keadaan gelap
            if (hold > 0f)
            {
                yield return new WaitForSecondsRealtime(hold);
            }

            // Fade In (Kembali Terang)
            t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(1f - (t / duration));
                yield return null;
            }
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;

            activeFadeRoutine = null;
            try
            {
                onFinish?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScreenFader] Error in onFinish callback: {ex}");
            }
        }
    }
}
