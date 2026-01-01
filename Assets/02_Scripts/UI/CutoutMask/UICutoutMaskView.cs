using UnityEngine;

public class UICutoutMaskView : MonoBehaviour
{
    [SerializeField] private Transform _cutoutMask;
    [SerializeField] private GameObject _highlight;
    [Header("Configurations")]
    [SerializeField] private bool _hideOnAwake = true;

    private void Awake()
    {
        if (_hideOnAwake)
        {
            Hide();
        }
    }

    public void Show(Vector3 cutoutPosition)
    {
        _cutoutMask.gameObject.SetActive(true);
        _cutoutMask.position = cutoutPosition;
    }

    public void Hide()
    {
        _cutoutMask.gameObject.SetActive(false);
    }

    public void ShowHighlight()
    {
        if (_highlight != null)
        {
            _highlight.gameObject.SetActive(true);
        }
    }

    public void HideHighlight()
    {
        if (_highlight != null)
        {
            _highlight.gameObject.SetActive(false);
        }
    }
}
