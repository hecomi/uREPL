using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

namespace uREPL
{

public class CompletionView : MonoBehaviour
{
    public Window parentWindow { get; set; }

    private AnnotationView annotation_;
    private float elapsedTimeFromLastSelect_ = 0f;

    private const string contentGameObjectName = "Content";
    private const float scrollDamping = 0.3f;

    public GameObject itemPrefab;
    public Vector3 offset = new Vector3(0, 10, 0);
    private Transform content;
    private ScrollRect scroll;
    private RectTransform rect;
    private RectTransform contentRect;

    private int currentIndex_ = 0;
    private float scrollPos_ = 0;

    public int itemCount
    {
        get { return content.childCount; }
    }

    private int itemIndex
    {
        get { return itemCount - 1 - currentIndex_; }
    }

    public bool hasItem
    {
        get { return itemCount > 0; }
    }

    public Vector3 position
    {
        get { return rect.localPosition; }
        set { rect.localPosition = value + offset; }
    }

    public CompletionItem selectedItem
    {
        get {
            return itemCount > 0 ?
                content.GetChild(itemIndex).GetComponent<CompletionItem>() :
                null;
        }
    }

    public string selectedCompletion
    {
        get {
            return itemCount > 0 ?
                selectedItem.completion :
                "";
        }
    }

    public Vector3 selectedPosition
    {
        get {
            return itemCount > 0 ?
                content.GetChild(itemIndex).position :
                Vector3.zero;
        }
    }

    public float width
    {
        get { return rect.rect.width; }
    }

    public void Initialize(Window window)
    {
        parentWindow = window;
    }

    void Awake()
    {
        rect    = GetComponent<RectTransform>();
        scroll  = GetComponent<ScrollRect>();
        content = transform.Find(contentGameObjectName);
        contentRect = content.GetComponent<RectTransform>();
        annotation_ = transform.parent.Find("Annotation View").GetComponent<AnnotationView>();
    }

    void Update()
    {
        for (int i = 0; i < itemCount; ++i) {
            content.GetChild(i).GetComponent<CompletionItem>().SetHighlight(i == itemIndex);
        }

        UpdateAnnotation();
    }

    void LateUpdate()
    {
        var targetScrollPos = (itemCount <= 1) ? 0f : 1f * currentIndex_ / (itemCount - 1);
        scrollPos_ += (targetScrollPos - scrollPos_) * scrollDamping;
        if (scroll != null) {
            scroll.verticalNormalizedPosition = scrollPos_;
            // TODO: make the completion view flexible with uGUI instead of this code.
            if (contentRect.rect.width != rect.rect.width) {
                rect.sizeDelta = new Vector2(contentRect.rect.width, rect.rect.height);
            }
        }
    }

    private void UpdateAnnotation()
    {
        elapsedTimeFromLastSelect_ += Time.deltaTime;

        var hasDescription = (selectedItem != null) && selectedItem.hasDescription;
        if (hasDescription) annotation_.text = selectedItem.description;

        var isAnnotationVisible = elapsedTimeFromLastSelect_ >= parentWindow.parameters.annotationDelay;
        annotation_.gameObject.SetActive(hasDescription && isAnnotationVisible);

        annotation_.transform.position = selectedPosition + Vector3.right * (width + 4f);
    }

    private void ResetAnnotation()
    {
        elapsedTimeFromLastSelect_ = 0f;
    }

    public void SetCompletions(CompletionInfo[] completions)
    {
        Reset();
        if (completions == null || completions.Length == 0) return;

        foreach (var info in completions.Reverse()) {
            var itemObject = Instantiate(
                itemPrefab,
                content.position,
                content.rotation) as GameObject;
            itemObject.transform.SetParent(content);
            itemObject.transform.localScale = Vector3.one;
            var item = itemObject.GetComponent<CompletionItem>();
            item.description = info.description;
            item.SetMark(info.mark, info.color);
            item.SetCompletion(info.code, info.prefix);
        }

#if UNITY_5_2 || UNITY_5_3_OR_NEWER
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
#endif
    }

    private void Clear()
    {
        for (int i = 0; i < itemCount; ++i) {
            var item = content.GetChild(i).gameObject;
            Destroy(item);
        }
        ResetAnnotation();
    }

    public void Next()
    {
        if (itemCount == 0) {
            currentIndex_ = 0;
        } else {
            currentIndex_ = (currentIndex_ + 1) % itemCount;
        }
        ResetAnnotation();
    }

    public void Prev()
    {
        if (itemCount == 0) {
            currentIndex_ = 0;
        } else {
            --currentIndex_;
            if (currentIndex_ < 0) {
                currentIndex_ += itemCount;
            }
        }
        ResetAnnotation();
    }

    public void Reset()
    {
        Clear();
        currentIndex_ = 0;
        scrollPos_ = 0;
        if (scroll != null) {
            scroll.verticalNormalizedPosition = scrollPos_;
        }
        ResetAnnotation();
    }
}

}
