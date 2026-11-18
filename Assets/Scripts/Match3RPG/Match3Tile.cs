using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CARE.Gameplay
{
    /// <summary>
    /// Interactive Candy Crush style tile on the Match-3 RPG Combat Board.
    /// Supports swipe gestures, tap-to-swap, smooth fall/swap animations,
    /// and vibrant candy/gem visuals with clear placeholder symbols.
    /// </summary>
    public class Match3Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerClickHandler
    {
        public int GridX { get; private set; }
        public int GridY { get; private set; }
        public int TypeId { get; private set; }
        public string ItemName { get; private set; }
        public string Symbol { get; private set; }
        public Color ThemeColor { get; private set; }
        public Sprite ItemSprite { get; private set; }

        public RectTransform RectTransform => rectTransform;

        private RectTransform rectTransform;
        private Image borderImage;
        private Image innerPlateImage;
        private Image glossImage;
        private Image iconImage;
        private Text symbolText;
        private Image highlightImage;
        private CanvasGroup canvasGroup;

        private Vector2 pointerDownPosition;
        private bool isPointerDown;
        private bool hasSwiped;
        private Coroutine activeMoveCoroutine;

        private static Font defaultFont;

        public static Match3Tile Spawn(Transform parent, int x, int y, int typeId, Sprite sprite, string itemName, string symbol, Color themeColor, Vector2 anchoredPos, Vector2 size)
        {
            GameObject tileObj = new GameObject($"Tile_{x}_{y}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            tileObj.transform.SetParent(parent, false);

            Match3Tile tile = tileObj.AddComponent<Match3Tile>();
            tile.Setup(x, y, typeId, sprite, itemName, symbol, themeColor, anchoredPos, size);
            return tile;
        }

        private void Setup(int x, int y, int typeId, Sprite sprite, string itemName, string symbol, Color themeColor, Vector2 anchoredPos, Vector2 size)
        {
            GridX = x;
            GridY = y;
            TypeId = typeId;
            ItemSprite = sprite;
            ItemName = itemName;
            Symbol = symbol;
            ThemeColor = themeColor;

            if (defaultFont == null)
            {
                defaultFont = Match3CombatManager.GetGameFont();
            }

            rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPos;
            rectTransform.localScale = Vector3.one;

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // 1. Outer Frame / Bevel Border
            borderImage = GetComponent<Image>();
            // Darker shade for crisp bevel border
            borderImage.color = new Color(themeColor.r * 0.45f, themeColor.g * 0.45f, themeColor.b * 0.45f, 0.98f);
            borderImage.raycastTarget = true;

            // 2. Selection Glow / Outline
            GameObject hlObj = new GameObject("Highlight", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            hlObj.transform.SetParent(transform, false);
            RectTransform hlRT = hlObj.GetComponent<RectTransform>();
            hlRT.anchorMin = Vector2.zero;
            hlRT.anchorMax = Vector2.one;
            hlRT.offsetMin = new Vector2(-6, -6);
            hlRT.offsetMax = new Vector2(6, 6);
            highlightImage = hlObj.GetComponent<Image>();
            highlightImage.color = new Color(1f, 0.92f, 0.3f, 0.95f); // Glowing gold
            highlightImage.raycastTarget = false;
            hlObj.SetActive(false);

            // 3. Inner Candy Plate (Vibrant Candy Gem Body)
            GameObject innerObj = new GameObject("InnerPlate", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            innerObj.transform.SetParent(transform, false);
            RectTransform innerRT = innerObj.GetComponent<RectTransform>();
            innerRT.anchorMin = new Vector2(0.06f, 0.06f);
            innerRT.anchorMax = new Vector2(0.94f, 0.94f);
            innerRT.offsetMin = Vector2.zero;
            innerRT.offsetMax = Vector2.zero;
            innerPlateImage = innerObj.GetComponent<Image>();
            innerPlateImage.color = themeColor;
            innerPlateImage.raycastTarget = false;

            // 4. Glossy Highlight Reflection (Top Half Shine)
            GameObject glossObj = new GameObject("Gloss", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glossObj.transform.SetParent(innerObj.transform, false);
            RectTransform glossRT = glossObj.GetComponent<RectTransform>();
            glossRT.anchorMin = new Vector2(0.08f, 0.52f);
            glossRT.anchorMax = new Vector2(0.92f, 0.92f);
            glossRT.offsetMin = Vector2.zero;
            glossRT.offsetMax = Vector2.zero;
            glossImage = glossObj.GetComponent<Image>();
            glossImage.color = new Color(1f, 1f, 1f, 0.32f);
            glossImage.raycastTarget = false;

            // 5. Item Sprite Icon (Criminal Case Asset if available)
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(innerObj.transform, false);
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.12f, 0.12f);
            iconRT.anchorMax = new Vector2(0.88f, 0.88f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;
            iconImage = iconObj.GetComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            if (sprite != null)
            {
                iconImage.sprite = sprite;
                iconImage.color = Color.white;
                iconObj.SetActive(true);
            }
            else
            {
                iconObj.SetActive(false);
            }

            // 6. Placeholder Candy Symbol (Bold Emoji / Gem Symbol)
            GameObject symObj = new GameObject("SymbolText", typeof(RectTransform));
            symObj.transform.SetParent(innerObj.transform, false);
            RectTransform symRT = symObj.GetComponent<RectTransform>();
            symRT.anchorMin = Vector2.zero;
            symRT.anchorMax = Vector2.one;
            symRT.offsetMin = Vector2.zero;
            symRT.offsetMax = Vector2.zero;

            symbolText = symObj.AddComponent<Text>();
            symbolText.font = defaultFont;
            symbolText.text = symbol;
            symbolText.fontSize = (sprite != null) ? 22 : 36;
            symbolText.fontStyle = FontStyle.Bold;
            symbolText.alignment = (sprite != null) ? TextAnchor.LowerRight : TextAnchor.MiddleCenter;
            symbolText.color = (sprite != null) ? new Color(1f, 1f, 1f, 0.9f) : Color.white;
            symbolText.raycastTarget = false;

            SetSelected(false);
        }

        public void SetGridPosition(int newX, int newY)
        {
            GridX = newX;
            GridY = newY;
        }

        public void SetSelected(bool selected)
        {
            if (highlightImage != null)
            {
                highlightImage.gameObject.SetActive(selected);
            }

            if (selected)
            {
                transform.localScale = new Vector3(1.15f, 1.15f, 1f);
                transform.SetAsLastSibling();
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }

        public void MoveTo(Vector2 targetPos, float duration, Action onComplete = null)
        {
            if (activeMoveCoroutine != null) StopCoroutine(activeMoveCoroutine);
            activeMoveCoroutine = StartCoroutine(AnimateMove(targetPos, duration, onComplete));
        }

        private IEnumerator AnimateMove(Vector2 targetPos, float duration, Action onComplete)
        {
            Vector2 startPos = rectTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease out cubic for snappy slide
                t = 1f - Mathf.Pow(1f - t, 3);
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            rectTransform.anchoredPosition = targetPos;
            activeMoveCoroutine = null;
            onComplete?.Invoke();
        }

        public void DropTo(Vector2 targetPos, float duration, Action onComplete = null)
        {
            if (activeMoveCoroutine != null) StopCoroutine(activeMoveCoroutine);
            activeMoveCoroutine = StartCoroutine(AnimateDrop(targetPos, duration, onComplete));
        }

        private IEnumerator AnimateDrop(Vector2 targetPos, float duration, Action onComplete)
        {
            Vector2 startPos = rectTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Bouncy gravity settle
                float ease = Mathf.Sin(t * Mathf.PI * 0.5f);
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
                yield return null;
            }

            rectTransform.anchoredPosition = targetPos;
            activeMoveCoroutine = null;
            onComplete?.Invoke();
        }

        public void AnimateMatchAndDestroy(float duration, Action onComplete = null)
        {
            StartCoroutine(MatchPopRoutine(duration, onComplete));
        }

        private IEnumerator MatchPopRoutine(float duration, Action onComplete)
        {
            float elapsed = 0f;
            Vector3 initialScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Quick pop scale up then shrink out
                float scale = (t < 0.35f) ? Mathf.Lerp(1f, 1.28f, t / 0.35f) : Mathf.Lerp(1.28f, 0.05f, (t - 0.35f) / 0.65f);
                transform.localScale = initialScale * scale;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - t;
                }
                yield return null;
            }

            onComplete?.Invoke();
            Destroy(gameObject);
        }

        #region Pointer & Swipe Event Handlers

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownPosition = eventData.position;
            isPointerDown = true;
            hasSwiped = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isPointerDown || hasSwiped || Match3CombatManager.Instance == null || !Match3CombatManager.Instance.CanPlayerMove)
                return;

            Vector2 dragDelta = eventData.position - pointerDownPosition;
            float dragThreshold = 18f; // Fast, responsive threshold

            if (dragDelta.magnitude >= dragThreshold)
            {
                hasSwiped = true;
                isPointerDown = false;

                int targetX = GridX;
                int targetY = GridY;

                if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
                {
                    // Horizontal swipe
                    if (dragDelta.x > 0) targetX++;
                    else targetX--;
                }
                else
                {
                    // Vertical swipe (Canvas Y: + is UP)
                    if (dragDelta.y > 0) targetY++;
                    else targetY--;
                }

                Match3CombatManager.Instance.TrySwapTiles(GridX, GridY, targetX, targetY);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerDown = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Ignore click if this was a drag/swipe gesture
            if (hasSwiped) return;

            if (Match3CombatManager.Instance == null || !Match3CombatManager.Instance.CanPlayerMove)
                return;

            Match3CombatManager.Instance.OnTileClicked(this);
        }

        #endregion
    }
}
