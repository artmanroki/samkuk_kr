using UnityEngine;
using UnityEngine.UI;

public class Status_Module : MonoBehaviour
{
    [Header("상태값 입력")]
    public int HP = 100;   // 체력
    public int maxHP = 100;

    public int ARM = 50;   // 방어막
    public int maxARM = 50;

    public int MP = 30;    // 마나
    public int maxMP = 30;

    public int EXP = 0;    // 경험치
    public int maxEXP = 100;

    [Header("UI 설정")]
    public Vector3 barOffset = new Vector3(0, 2f, 0); // 머리 위 위치
    public Image hpBar;
    public Image armBar;
    public Image mpBar;
    public Image expBar;

    private Camera cam;
    private bool isDead = false;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (isDead) return;

        // 체력/방어막/마나/경험치 비율 갱신
        if (hpBar != null) hpBar.fillAmount = Mathf.Clamp01((float)HP / maxHP);
        if (armBar != null) armBar.fillAmount = Mathf.Clamp01((float)ARM / maxARM);
        if (mpBar != null) mpBar.fillAmount = Mathf.Clamp01((float)MP / maxMP);

        if (expBar != null && gameObject.layer != LayerMask.NameToLayer("Mob"))
            expBar.fillAmount = Mathf.Clamp01((float)EXP / maxEXP);

        // UI 위치를 캐릭터 머리 위로 이동
        if (hpBar != null)
        {
            Transform canvasTransform = hpBar.transform.parent; // Canvas 기준
            canvasTransform.position = transform.position + barOffset;

            // 항상 카메라를 바라보게
            canvasTransform.LookAt(canvasTransform.position + cam.transform.rotation * Vector3.forward,
                                   cam.transform.rotation * Vector3.up);
        }
    }

    // 데미지 처리
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (ARM > 0)
        {
            int absorbed = Mathf.Min(ARM, damage);
            ARM -= absorbed;
            damage -= absorbed;
        }

        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            OnDeath();
        }
    }

    private void OnDeath()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} 사망");
        // 여기서 Sprite 교체, 오브젝트 비활성화 등 추가 가능
    }

    public void ResetStatus()
    {
        HP = maxHP;
        ARM = maxARM;
        MP = maxMP;
        EXP = 0;
        isDead = false;
    }
}
