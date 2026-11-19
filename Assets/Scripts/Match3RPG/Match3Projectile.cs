using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CARE.Gameplay
{
    /// <summary>
    /// Rocket Projectile spawned when tiles are matched in Match-3 RPG Combat.
    /// Launches vertically straight UPWARDS from the matched tile's column.
    /// If it collides with the boss's moving hitbox, it deals damage.
    /// If the boss has moved away, the rocket flies past into the sky and misses!
    /// </summary>
    public class Match3Projectile : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Image flameImage;
        private RectTransform flameRT;
        private Image glowImage;
        private Image iconImage;
        private Text symbolText;

        private float speed = 1200f;
        private int damage;
        private int combo;
        private Color themeColor;
        private Action<int, int, Vector2, Color> onHitCallback;
        private Action<Vector2> onMissCallback;

        private bool hasResolved = false;
        private float flightTime = 0f;

        /// <summary>
        /// Spawns a vertical attack rocket from the destroyed tile's column straight upwards.
        /// </summary>
        public static void SpawnRocket(
            Transform parent,
            Vector2 startPos,
            Sprite sprite,
            string symbol,
            Color color,
            int damage,
            int combo,
            Action<int, int, Vector2, Color> onHit,
            Action<Vector2> onMiss)
        {
            GameObject obj = new GameObject("Match3Rocket", typeof(RectTransform));
            obj.transform.SetParent(parent, false);

            Match3Projectile proj = obj.AddComponent<Match3Projectile>();
            proj.LaunchRocket(startPos, sprite, symbol, color, damage, combo, onHit, onMiss);
        }

        // Backward compatibility overload
        public static void Spawn(
            Transform parent,
            Vector2 startPos,
            Vector2 targetPos,
            Sprite sprite,
            string symbol,
            Color color,
            int damage,
            int combo,
            Action<int, int, Vector2, Color> onHit)
        {
            SpawnRocket(parent, startPos, sprite, symbol, color, damage, combo, onHit, null);
        }

        public void LaunchRocket(
            Vector2 startPos,
            Sprite sprite,
            string symbol,
            Color color,
            int damage,
            int combo,
            Action<int, int, Vector2, Color> onHit,
            Action<Vector2> onMiss)
        {
            this.damage = damage;
            this.combo = combo;
            this.themeColor = color;
            this.onHitCallback = onHit;
            this.onMissCallback = onMiss;
            this.hasResolved = false;
            this.flightTime = 0f;

            // Root RectTransform (Rocket Container)
            rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(48, 86);
            rectTransform.anchoredPosition = startPos;
            rectTransform.localEulerAngles = Vector3.zero; // Points straight up!

            Sprite white = Match3CombatManager.GetWhiteSprite();

            // 1. Outer Glowing Aura
            GameObject glowObj = new GameObject("GlowAura", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glowObj.transform.SetParent(transform, false);
            RectTransform glowRT = glowObj.GetComponent<RectTransform>();
            glowRT.sizeDelta = new Vector2(68, 100);
            glowRT.anchoredPosition = Vector2.zero;
            glowImage = glowObj.GetComponent<Image>();
            glowImage.sprite = white;
            glowImage.color = new Color(color.r, color.g, color.b, 0.45f);
            glowImage.raycastTarget = false;

            // 2. Rocket Booster Flame (At bottom tail)
            GameObject flameObj = new GameObject("BoosterFlame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            flameObj.transform.SetParent(transform, false);
            flameRT = flameObj.GetComponent<RectTransform>();
            flameRT.sizeDelta = new Vector2(26, 42);
            flameRT.anchoredPosition = new Vector2(0, -42);
            flameImage = flameObj.GetComponent<Image>();
            flameImage.sprite = white;
            flameImage.color = new Color(1f, 0.65f, 0.15f, 0.95f); // Bright fiery orange
            flameImage.raycastTarget = false;

            // Inner flame core
            GameObject coreFlameObj = new GameObject("CoreFlame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            coreFlameObj.transform.SetParent(flameObj.transform, false);
            RectTransform coreRT = coreFlameObj.GetComponent<RectTransform>();
            coreRT.sizeDelta = new Vector2(14, 26);
            coreRT.anchoredPosition = new Vector2(0, 4);
            Image coreImg = coreFlameObj.GetComponent<Image>();
            coreImg.sprite = white;
            coreImg.color = new Color(1f, 0.95f, 0.4f, 0.95f); // Bright white-yellow flame core
            coreImg.raycastTarget = false;

            // 3. Left & Right Stabilization Fins
            GameObject finLeft = new GameObject("FinLeft", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            finLeft.transform.SetParent(transform, false);
            RectTransform finLRT = finLeft.GetComponent<RectTransform>();
            finLRT.sizeDelta = new Vector2(14, 26);
            finLRT.anchoredPosition = new Vector2(-18, -22);
            finLRT.localEulerAngles = new Vector3(0, 0, 24f);
            Image finLImg = finLeft.GetComponent<Image>();
            finLImg.sprite = white;
            finLImg.color = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f);
            finLImg.raycastTarget = false;

            GameObject finRight = new GameObject("FinRight", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            finRight.transform.SetParent(transform, false);
            RectTransform finRRT = finRight.GetComponent<RectTransform>();
            finRRT.sizeDelta = new Vector2(14, 26);
            finRRT.anchoredPosition = new Vector2(18, -22);
            finRRT.localEulerAngles = new Vector3(0, 0, -24f);
            Image finRImg = finRight.GetComponent<Image>();
            finRImg.sprite = white;
            finRImg.color = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f);
            finRImg.raycastTarget = false;

            // 4. Main Rocket Fuselage Body
            GameObject bodyObj = new GameObject("RocketBody", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bodyObj.transform.SetParent(transform, false);
            RectTransform bodyRT = bodyObj.GetComponent<RectTransform>();
            bodyRT.sizeDelta = new Vector2(36, 52);
            bodyRT.anchoredPosition = new Vector2(0, -6);
            Image bodyImg = bodyObj.GetComponent<Image>();
            bodyImg.sprite = white;
            bodyImg.color = color;
            bodyImg.raycastTarget = false;

            // Dark inner fuselage accent
            GameObject innerBodyObj = new GameObject("BodyPlate", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            innerBodyObj.transform.SetParent(bodyObj.transform, false);
            RectTransform ibRT = innerBodyObj.GetComponent<RectTransform>();
            ibRT.sizeDelta = new Vector2(28, 44);
            ibRT.anchoredPosition = Vector2.zero;
            Image ibImg = innerBodyObj.GetComponent<Image>();
            ibImg.sprite = white;
            ibImg.color = new Color(color.r * 0.85f, color.g * 0.85f, color.b * 0.85f, 1f);
            ibImg.raycastTarget = false;

            // 5. Nose Cone (Sharp aerodynamic warhead)
            GameObject noseObj = new GameObject("NoseCone", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            noseObj.transform.SetParent(transform, false);
            RectTransform noseRT = noseObj.GetComponent<RectTransform>();
            noseRT.sizeDelta = new Vector2(24, 24);
            noseRT.anchoredPosition = new Vector2(0, 24);
            noseRT.localEulerAngles = new Vector3(0, 0, 45f); // 45 deg diamond creates pointed tip!
            Image noseImg = noseObj.GetComponent<Image>();
            noseImg.sprite = white;
            noseImg.color = Color.Lerp(color, Color.white, 0.75f);
            noseImg.raycastTarget = false;

            // 6. Center Insignia (Icon or Symbol)
            if (sprite != null)
            {
                GameObject iconObj = new GameObject("Insignia", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconObj.transform.SetParent(transform, false);
                RectTransform iconRT = iconObj.GetComponent<RectTransform>();
                iconRT.sizeDelta = new Vector2(26, 26);
                iconRT.anchoredPosition = new Vector2(0, -6);
                iconImage = iconObj.GetComponent<Image>();
                iconImage.sprite = sprite;
                iconImage.color = Color.white;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
            }
            else if (!string.IsNullOrEmpty(symbol))
            {
                GameObject symObj = new GameObject("SymbolText", typeof(RectTransform));
                symObj.transform.SetParent(transform, false);
                RectTransform symRT = symObj.GetComponent<RectTransform>();
                symRT.sizeDelta = new Vector2(30, 30);
                symRT.anchoredPosition = new Vector2(0, -6);

                symbolText = symObj.AddComponent<Text>();
                symbolText.font = Match3CombatManager.GetGameFont();
                symbolText.text = symbol;
                symbolText.fontSize = 20;
                symbolText.alignment = TextAnchor.MiddleCenter;
                symbolText.color = Color.white;
                symbolText.raycastTarget = false;
            }
        }

        private void Update()
        {
            if (hasResolved) return;

            flightTime += Time.deltaTime;

            // 1. Rocket moves straight UPWARDS!
            Vector2 pos = rectTransform.anchoredPosition;
            pos.y += speed * Time.deltaTime;
            rectTransform.anchoredPosition = pos;

            // 2. Animate booster exhaust flame flicker & size pulse
            if (flameRT != null)
            {
                float flameScaleY = 0.8f + Mathf.PingPong(flightTime * 28f, 0.5f);
                flameRT.localScale = new Vector3(1f, flameScaleY, 1f);
            }

            // 3. Collision Check against Boss Hitbox
            if (Match3CombatManager.Instance != null)
            {
                // A) Check Hit
                if (Match3CombatManager.Instance.CheckEnemyHit(pos, 28f))
                {
                    hasResolved = true;
                    onHitCallback?.Invoke(damage, combo, pos, themeColor);
                    StartCoroutine(ImpactBurstAndDestroy());
                    return;
                }

                // B) Check Miss (flew past top boundary of boss area)
                if (pos.y >= Match3CombatManager.Instance.BossHitboxTop + 65f)
                {
                    hasResolved = true;
                    onMissCallback?.Invoke(pos);
                    StartCoroutine(MissDissolveAndDestroy());
                    return;
                }
            }
            else
            {
                // Fallback top limit if manager null
                if (pos.y > 550f)
                {
                    Destroy(gameObject);
                }
            }
        }

        private IEnumerator ImpactBurstAndDestroy()
        {
            // Pop & scale up into explosion flash
            float elapsed = 0f;
            float duration = 0.12f;
            Vector3 baseScale = rectTransform.localScale;

            if (flameImage != null) flameImage.enabled = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.localScale = baseScale * (1f + t * 0.9f);
                if (glowImage != null)
                {
                    Color c = glowImage.color;
                    c.a = Mathf.Lerp(0.85f, 0f, t);
                    glowImage.color = c;
                }
                yield return null;
            }

            Destroy(gameObject);
        }

        private IEnumerator MissDissolveAndDestroy()
        {
            // Fades into thin air as it flies into the clouds
            float elapsed = 0f;
            float duration = 0.16f;
            Vector3 baseScale = rectTransform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                Vector2 pos = rectTransform.anchoredPosition;
                pos.y += (speed * 0.5f) * Time.deltaTime;
                rectTransform.anchoredPosition = pos;

                rectTransform.localScale = Vector3.Lerp(baseScale, Vector3.zero, t);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
