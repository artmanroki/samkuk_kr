using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceHPBar : MonoBehaviour
{
    [Header("연결할 상태 모듈")]
    public Status_Module status;   // HP, ARM 등 상태값을 가진 컴포넌트

    [Header("UI 설정")]
    public Image hpBar;            // 체력바 이미지 (Image Type = Filled)
    public Vector3 offset = new Vector3(0, 2f, 0); // 머리 위 위치 오프셋

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (status == null || cam == null) return;

        // HP 비율 계산 (0 ~ 1)
        float ratio = Mathf.Clamp01(status.HP / 100f);
        hpBar.fillAmount = ratio;

        // 체력바 위치를 캐릭터 머리 위로 이동
        transform.position = status.transform.position + offset;

        // 체력바가 항상 카메라를 바라보도록 설정
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                         cam.transform.rotation * Vector3.up);
    }
}
