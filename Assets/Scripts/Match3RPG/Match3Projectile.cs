using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CARE.Gameplay
{
    /// <summary>
    /// Projectile spawned when tiles are matched in Match-3 RPG Combat.
    /// Rockets upwards toward the target/enemy above, damaging the target on impact.
    /// </summary>
    public class Match3Projectile : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Image iconImage;
        private Image glowImage;
        private Text symbolText;

        public static void Spawn(Transform parent, Vector2 startPos, Vector2 targetPos, Sprite sprite, string symbol, Color color, int damage, int combo, Action<int, int, Vector2, Color> onHit)
        {
            GameObject obj = new GameObject("Match3Projectile", typeof(RectTransform));
            obj.transform.SetParent(parent, false);

            Match3Projectile proj = obj.AddComponent<Match3Projectile>();
            proj.Launch(startPos, targetPos, sprite, symbol, color, damage, combo, onHit);
        }

        public void Launch(Vector2 startPos, Vector2 targetPos, Sprite sprite, string symbol, Color color, int damage, int combo, Action<int, int, Vector2, Color> onHit)
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(56, 56);
            rectTransform.anchoredPosition = startPos;
            rectTransform.localScale = Vector3.one * 0.85f;

            // 1. Outer Glowing Aura
            GameObject glowObj = new GameObject("Glow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glowObj.transform.SetParent(transform, false);
            RectTransform glowRT = glowObj.GetComponent<RectTransform>();
            glowRT.sizeDelta = new Vector2(72, 72);
            glowRT.anchoredPosition = Vector2.zero;
            glowImage = glowObj.GetComponent<Image>();
            glowImage.sprite = Match3CombatManager.GetWhiteSprite();
            glowImage.color = new Color(color.r, color.g, color.b, 0.85f);
            glowImage.raycastTarget = false;

            // 2. Center Item Sprite or Icon
            if (sprite != null)
            {
                GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconObj.transform.SetParent(transform, false);
                RectTransform iconRT = iconObj.GetComponent<RectTransform>();
                iconRT.sizeDelta = new Vector2(44, 44);
                iconRT.anchoredPosition = Vector2.zero;
                iconImage = iconObj.GetComponent<Image>();
                iconImage.sprite = sprite;
                iconImage.color = Color.white;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
            }

            // 3. Center Symbol
            if (!string.IsNullOrEmpty(symbol))
            {
                GameObject symObj = new GameObject("Symbol", typeof(RectTransform));
                symObj.transform.SetParent(transform, false);
                RectTransform symRT = symObj.GetComponent<RectTransform>();
                symRT.sizeDelta = new Vector2(50, 50);
                symRT.anchoredPosition = Vector2.zero;

                symbolText = symObj.AddComponent<Text>();
                symbolText.font = Match3CombatManager.GetGameFont();
                symbolText.text = symbol;
                symbolText.fontSize = 28;
                symbolText.alignment = TextAnchor.MiddleCenter;
                symbolText.color = Color.white;
                symbolText.raycastTarget = false;
            }

            StartCoroutine(FlightRoutine(startPos, targetPos, color, damage, combo, onHit));
        }

        private IEnumerator FlightRoutine(Vector2 startPos, Vector2 targetPos, Color color, int damage, int combo, Action<int, int, Vector2, Color> onHit)
        {
            float duration = 0.36f;
            float elapsed = 0f;

            // Control point for an inward homing trajectory curve
            Vector2 midControl = new Vector2(
                Mathf.Lerp(startPos.x, targetPos.x, 0.4f),
                Mathf.Lerp(startPos.y, targetPos.y, 0.5f) - 30f
            );

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Accelerating upward ease
                float easeT = t * t;

                Vector2 pos = (1f - easeT) * (1f - easeT) * startPos + 2f * (1f - easeT) * easeT * midControl + easeT * easeT * targetPos;
                rectTransform.anchoredPosition = pos;

                // Rotation and scale effect
                rectTransform.localEulerAngles = new Vector3(0, 0, elapsed * 720f);
                float scale = Mathf.Lerp(0.85f, 1.4f, t);
                rectTransform.localScale = new Vector3(scale, scale, 1f);

                yield return null;
            }

            rectTransform.anchoredPosition = targetPos;

            // Trigger hit callback with color for explosion VFX
            onHit?.Invoke(damage, combo, targetPos, color);

            // Pop burst effect
            yield return ImpactBurst();

            Destroy(gameObject);
        }

        private IEnumerator ImpactBurst()
        {
            float elapsed = 0f;
            float duration = 0.12f;
            Vector3 baseScale = rectTransform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.localScale = baseScale * (1f + t * 0.8f);
                if (glowImage != null)
                {
                    Color c = glowImage.color;
                    c.a = 1f - t;
                    glowImage.color = c;
                }
                yield return null;
            }
        }
    }
}
