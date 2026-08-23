using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class MultiEnemyAI : MonoBehaviour
{
    public NavMeshAgent 플레이어에이전트;
    public List<NavMeshAgent> 적군들;

    [Header("탐지 및 배회 설정")]
    public float 탐지범위 = 5f;
    public float 배회반경 = 2f;

    [Header("이동 시간 설정")]
    public float 랜덤이동최소시간 = 1f;
    public float 랜덤이동최대시간 = 10f;
    public float 직선이동최소시간 = 1f;
    public float 직선이동최대시간 = 10f;

    [Header("플레이어 속도 설정")]
    public float 플레이어추격속도 = 3f;
    public float 플레이어배회속도 = 1.5f;

    [Header("적군 속도 설정")]
    public float 적군추격속도 = 2.5f;
    public float 적군배회속도 = 1f;

    [Header("공격 설정")]
    public float 공격범위 = 1.5f;
    public GameObject[] 공격이펙트프리팹들;
    public float 이펙트높이 = 1.5f;
    public float 이펙트삭제시간 = 2f;
    public float 공격쿨다운 = 1f;
    public float 밀어내기힘 = 3f;                      // 적군을 밀어내는 힘

    // 오프셋을 Inspector에서 직접 입력 가능
    public Vector3 이펙트오프셋 = new Vector3(0f, 1.5f, -0.5f);

    private Dictionary<NavMeshAgent, float> 타이머 = new Dictionary<NavMeshAgent, float>();
    private Dictionary<NavMeshAgent, bool> 랜덤이동중 = new Dictionary<NavMeshAgent, bool>();
    private Dictionary<NavMeshAgent, float> 랜덤이동시간 = new Dictionary<NavMeshAgent, float>();
    private Dictionary<NavMeshAgent, float> 직선이동시간 = new Dictionary<NavMeshAgent, float>();

    // 적군별 마지막 공격 시간 기록
    private Dictionary<NavMeshAgent, float> 마지막공격시간 = new Dictionary<NavMeshAgent, float>();

    private Vector3 플레이어배회목표;
    private float 플레이어타이머 = 0f;
    private float 플레이어지속시간 = 0f;
    private bool 플레이어랜덤이동중 = true;

    void Start()
    {
        적군들 = new List<NavMeshAgent>(FindObjectsOfType<NavMeshAgent>());
        적군들.Remove(플레이어에이전트);

        플레이어에이전트.updateRotation = false;
        플레이어에이전트.updateUpAxis = false;

        foreach (NavMeshAgent agent in 적군들)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;

            타이머[agent] = 0f;
            랜덤이동중[agent] = true;
            랜덤이동시간[agent] = Random.Range(랜덤이동최소시간, 랜덤이동최대시간);
            직선이동시간[agent] = Random.Range(직선이동최소시간, 직선이동최대시간);

            마지막공격시간[agent] = -Mathf.Infinity; // 개별 초기화
        }

        플레이어배회목표 = 플레이어에이전트.transform.position;
        플레이어지속시간 = Random.Range(랜덤이동최소시간, 랜덤이동최대시간);
    }

    void Update()
    {
        // 플레이어 이동 로직
        float 가장가까운거리 = Mathf.Infinity;
        NavMeshAgent 가장가까운적 = null;
        foreach (NavMeshAgent enemy in 적군들)
        {
            float dist = Vector3.Distance(플레이어에이전트.transform.position, enemy.transform.position);
            if (dist < 가장가까운거리)
            {
                가장가까운거리 = dist;
                가장가까운적 = enemy;
            }
        }

        if (가장가까운적 != null && 가장가까운거리 < 탐지범위)
        {
            플레이어에이전트.speed = 플레이어추격속도;
            플레이어에이전트.SetDestination(가장가까운적.transform.position);
        }
        else
        {
            플레이어타이머 += Time.deltaTime;
            if (플레이어랜덤이동중)
            {
                if (플레이어타이머 >= 플레이어지속시간)
                {
                    플레이어타이머 = 0f;
                    플레이어랜덤이동중 = false;
                    플레이어지속시간 = Random.Range(직선이동최소시간, 직선이동최대시간);
                }
                else
                {
                    Vector3 randomPoint = 플레이어에이전트.transform.position + Random.insideUnitSphere * 배회반경;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomPoint, out hit, 배회반경, NavMesh.AllAreas))
                    {
                        플레이어에이전트.speed = 플레이어배회속도;
                        플레이어에이전트.SetDestination(hit.position);
                    }
                }
            }
            else
            {
                if (플레이어타이머 >= 플레이어지속시간)
                {
                    플레이어타이머 = 0f;
                    플레이어랜덤이동중 = true;
                    플레이어지속시간 = Random.Range(랜덤이동최소시간, 랜덤이동최대시간);
                }
                else
                {
                    플레이어에이전트.speed = 플레이어배회속도;
                    플레이어에이전트.SetDestination(플레이어배회목표);
                }
            }
        }

        // 적군 이동 및 공격 로직
        foreach (NavMeshAgent agent in 적군들)
        {
            float dist = Vector3.Distance(플레이어에이전트.transform.position, agent.transform.position);

            // 공격 범위 체크 + 개별 쿨다운
            if (dist <= 공격범위 && 공격이펙트프리팹들.Length > 0)
            {
                if (Time.time - 마지막공격시간[agent] >= 공격쿨다운)
                {
                    // 적 위치에서 이펙트 발동 (오프셋 적용)
                    Vector3 effectPos = agent.transform.position + 이펙트오프셋;
                    Quaternion lookRot = Quaternion.LookRotation(agent.transform.position - 플레이어에이전트.transform.position);

                    foreach (GameObject prefab in 공격이펙트프리팹들)
                    {
                        GameObject fx = Instantiate(prefab, effectPos, lookRot);
                        Destroy(fx, 이펙트삭제시간);
                    }


                    // 적군 밀어내기
                    Vector3 pushDir = (agent.transform.position - 플레이어에이전트.transform.position).normalized;
                    agent.Move(pushDir * 밀어내기힘);

                    // 마지막 공격 시각 갱신 (개별)
                    마지막공격시간[agent] = Time.time;
                }
            }

            // 이동 로직
            if (dist < 탐지범위)
            {
                agent.speed = 적군추격속도;
                agent.SetDestination(플레이어에이전트.transform.position);
                continue;
            }

            타이머[agent] += Time.deltaTime;

            if (랜덤이동중[agent])
            {
                if (타이머[agent] >= 랜덤이동시간[agent])
                {
                    타이머[agent] = 0f;
                    랜덤이동중[agent] = false;
                    직선이동시간[agent] = Random.Range(직선이동최소시간, 직선이동최대시간);
                }
                else
                {
                    Vector3 randomPoint = agent.transform.position + Random.insideUnitSphere * 배회반경;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomPoint, out hit, 배회반경, NavMesh.AllAreas))
                    {
                        agent.speed = 적군배회속도;
                        agent.SetDestination(hit.position);
                    }
                }
            }
            else
            {
                if (타이머[agent] >= 직선이동시간[agent])
                {
                    타이머[agent] = 0f;
                    랜덤이동중[agent] = true;
                    랜덤이동시간[agent] = Random.Range(랜덤이동최소시간, 랜덤이동최대시간);
                }
                else
                {
                    agent.speed = 적군추격속도;
                    agent.SetDestination(플레이어에이전트.transform.position);
                }
            }
        }
    }
}
