using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PredatoryBehaviour : MonoBehaviour {
    public float foodAmmount;
    public float foodDecay;
    public float foodLevelToHunt;
    public float foodPerBird;

    private BirdAgent target = null;
    private BirdAgent agent;

    private bool waitingToCharge = false;
    private int chargeCount = 0;
    void Awake() {
        agent = GetComponent<BirdAgent>();
    }
    private float timeInRange = 0;
    void Update() {
        if (!agent.alive) return;
        foodAmmount -= foodDecay * Time.deltaTime;
        if(foodAmmount < 0) {
            World.instance.KillBird(agent, false);
            return;
        }
        if(foodAmmount < foodLevelToHunt && target == null) {
            FindNextBird();
        }else if(target != null) {
            if (waitingToCharge) return;

            float dist = Vector3.Distance(target.transform.position, transform.position);
            if (dist < 0.5f) {
                World.instance.KillBird(target, true);
                target = null;
                agent.SetWatchedBirds(new List<BirdAgent>());
                foodAmmount += foodPerBird;
            }else if(dist < 5) {
                timeInRange += Time.deltaTime;
                if(timeInRange > 4) {
                    if (chargeCount < 4) {
                        StartCoroutine(DoWaitAndCharge());
                        chargeCount++;
                    } else {
                        FindNextBird();
                    }
                }
            } else {
                timeInRange = 0;
            }
        }
    }
    void FindNextBird() {
        target = World.instance.birds[BirdType.prey].GetRandom();
        if (target != null) {
            List<BirdAgent> l = new List<BirdAgent>();
            l.Add(target);
            chargeCount = 0;
            timeInRange = 0;
            agent.SetWatchedBirds(l);
        }
    }
    IEnumerator DoWaitAndCharge() {
        waitingToCharge = true;
        agent.SetWatchedBirds(new List<BirdAgent>());
        yield return new WaitForSeconds(3);
        var l = new List<BirdAgent>();
        l.Add(target);
        agent.SetWatchedBirds(l);
        waitingToCharge = false;
    }
}
