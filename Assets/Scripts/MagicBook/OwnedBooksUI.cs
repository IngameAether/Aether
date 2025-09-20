using System.Collections.Generic;
using UnityEngine;

public class OwnedBooksUI : MonoBehaviour
{
    // 인스펙터 창에 BookInfoItemUI가 붙어있는 프리팹을 연결
    [SerializeField] private GameObject _bookInfoItemPrefab;
    [SerializeField] private Transform _contentParent;

    void OnEnable()
    {
        // 기존 UI 삭제
        foreach (Transform child in _contentParent)
        {
            Destroy(child.gameObject);
        }

        // 보유한 책 목록 가져오기
        List<MagicBookManager.OwnedBookInfo> ownedBooks = MagicBookManager.Instance.GetOwnedBookInfos();

        // [핵심] 받아온 목록이 비어있는지 확인
        if (ownedBooks == null || ownedBooks.Count == 0)
        {
            Debug.LogWarning("OwnedBooksUI: 표시할 보유 도서가 없습니다. MagicBookManager가 빈 목록을 반환했습니다.");
            return; // 함수를 여기서 종료
        }

        Debug.Log($"OwnedBooksUI: {ownedBooks.Count}개의 보유 도서를 표시합니다.");

        // 가져온 책 목록을 순회하며 UI 생성
        foreach (var bookInfo in ownedBooks)
        {
            // bookData가 null인지 다시 한번 확인
            if (bookInfo.BookData == null)
            {
                Debug.LogError("OwnedBooksUI: BookData가 null인 책 정보가 있습니다!");
                continue; // 이 책은 건너뛰기
            }

            // [핵심] 실제 데이터 내용 로그로 출력
            Debug.Log($"- 책 이름: {bookInfo.BookData.Name}, 등급: {bookInfo.BookData.Rank}, 설명: {bookInfo.BookData.Description}");

            GameObject itemGO = Instantiate(_bookInfoItemPrefab, _contentParent);
            var itemUI = itemGO.GetComponent<BookDisplayUI>(); // 스크립트 이름 확인
            if (itemUI != null)
            {
                itemUI.Setup(bookInfo.BookData, bookInfo.CurrentStack);
            }
        }
    }
}
