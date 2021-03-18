using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public string type;
    Transform parent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        parent = transform.parent;
        transform.SetParent(transform.root.GetComponent<Canvas>().transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        transform.localScale = new Vector3(1.1f, 1.1f, 1);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localScale = new Vector3(0.8f, 0.8f, 1);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
