using System.Collections.Generic;
using UnityEngine;

public class OwnedBooksUI : MonoBehaviour
{
    [SerializeField] private GameObject _bookInfoItemPrefab;
    [SerializeField] private Transform _contentParent;

    void OnEnable()
    {
        foreach (Transform child in _contentParent) Destroy(child.gameObject);

        List<MagicBookManager.OwnedBookInfo> ownedBooks = MagicBookManager.Instance.GetOwnedBookInfos();

        foreach (var bookInfo in ownedBooks)
        {
            GameObject itemGO = Instantiate(_bookInfoItemPrefab, _contentParent);
            itemGO.GetComponent<BookInfoItemUI>().Setup(bookInfo.BookData, bookInfo.CurrentStack);
        }
    }
}
