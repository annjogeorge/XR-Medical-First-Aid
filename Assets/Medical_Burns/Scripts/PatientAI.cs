using UnityEngine;
using UnityEngine.AI;

public class  PatientAI : MonoBehaviour
{
    public Transform sinkTarget;
    private Animator anim;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if (agent != null)
        {
            agent.isStopped = true;
        }
    }

    public void GoToSink()
    {
        if (sinkTarget != null && agent!= null)
        {
            agent.isStopped = false;
            agent.SetDestination(sinkTarget.position);
        }

        if (anim!= null)
        {
            anim.SetBool("isWalking", true);
        }
    }

    public void Update()
    {
        if(agent!=null && !agent.pathPending)
        { 
            if(agent.remainingDistance<=agent.stoppingDistance)
            {
                if(anim!=null)
                {
                    anim.SetBool("isWalking", false);
                }
                agent.isStopped = true;
            }
        }
    }
}