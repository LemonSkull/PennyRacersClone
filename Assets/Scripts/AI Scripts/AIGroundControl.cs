using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGroundControl : MonoBehaviour
{
    public string AIName;
    public GameObject GameController, Ground;
    GameObject LapController;
    public float DrunkLevel, TargetDistance;
    public int Lap, nextTarget, allTargets, MaxLaps;
    private int AINumber, AICount, EngineClass;
    [SerializeField] //DEBUGGING
    private float Mass, TurnSpeed, AISpeed, MaxSpeedHolder, MaxSpeed, LoseSpeed;
    private float AccLerp, AccNerfer, Steps, WaitSteps;

    Rigidbody rb;
    private Vector3 oldPosition;
    private Vector3 randomizeTargetPos;

    private GameObject AIController;
    //private GameObject Target;
    private static List<Vector3> targetPosList;
    public bool IsGrounded, ForceRespawn;
    [SerializeField]//DEBUGGING
    private bool GameStart = false, IsFinished=false, IsAI=true;
    public bool RaceIsOver;

    // Start is called before the first frame update
    void Awake()
    {
        RaceIsOver = false;
        GameStart = false;
        rb = GetComponent<Rigidbody>();
        Mass = rb.mass;
        Mass *=4f; //Original mass was 0.2f
        LoseSpeed = 1f;
        nextTarget = 0;
        //IsGrounded = false;
        ForceRespawn = false;

        if (AIController == null)
            AIController = GameObject.FindWithTag("aicontroller");

        if (GameController == null)
            GameController = GameObject.FindWithTag("GameController");

        LapController = GameObject.FindWithTag("LapController");
        /*
        if(gameObject.tag != "Player")
            Ground = this.gameObject.transform.GetChild(AINumber).gameObject;
        */
        Lap = 1;
        //MaxLaps = LapController.GetComponent<LapControl>().MaxLaps;
        
        if (IsAI) //Hide Unity's "VALUE NEVER USED" -message
            IsAI = true;
    }
    void OnEnable()
    {
        if(gameObject.tag == "Player")
        {
            IsAI = false;
            Ground = GameObject.FindWithTag("PlayerGround");
            AIName = "NotAItoday";
        }
        else
        {
            IsAI = true;
        }


    }


    public void AIControllerStartUp(int AInumber, int AIcount, int engineclass, float accLerp, float turnspeed, float maxspeed)
    {
        AINumber = AInumber;
        AICount = AIcount;
        EngineClass = engineclass;

        AccLerp = accLerp;
        TurnSpeed = turnspeed;
        //MaxSpeed = maxSpeedBaseStat - ((float)AINumber * 2f); //Makes number 0 the fastest and 1 slower and so on
        MaxSpeed = maxspeed;
        
        MaxSpeed -= (float)AINumber;
        MaxSpeedHolder = MaxSpeed;
        MaxSpeed -= (float)AINumber;
        //TurnSpeed = (float)EngineClass;

        //AIEngineClassCheck(EngineClass);
    }

    public void AllTargets(List<Vector3> targetposlist, int targetcount)
    {
        targetPosList = targetposlist;
        allTargets = targetcount;

        targetPosRandomizer(AINumber);

    }

    void FixedUpdate()
    {
        Steps += Time.deltaTime;
        AISpeed = Vector3.Distance(oldPosition, gameObject.transform.position) * 200f; // Original = * 100f
        oldPosition = gameObject.transform.position;

        float gravityBuff = 500f;
        rb.AddRelativeForce(Vector3.down * Time.deltaTime * gravityBuff);

        if (GameStart)
        {
            //if (IsGrounded)
            if(!ForceRespawn)
            {
                float spd = 0f;
                float maxspeed = MaxSpeed;
                float aispeed = AISpeed;
                float turnspeed = TurnSpeed;

                if (RaceIsOver)
                {
                    maxspeed *= 0.7f;
                }

                //float singleStep = AISpeed * Time.deltaTime;
                Vector3 targetpos = targetPosList[nextTarget] + randomizeTargetPos;
                //Vector3 newDirection = Vector3.RotateTowards(transform.position, targetpos, singleStep, 0.0f);

                //Vector3 targetDirection = targetpos - transform.position;
                Vector3 relativePos = targetpos - transform.position;

                Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);

                //Vector3 turningCloseTarget = targetpos + new Vector3(10f, 10f, 10f);

                if (Vector3.Distance(transform.position, targetpos) < 10.0f) //ai wont drive around the target
                {
                    turnspeed += 5f;

                }

                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * turnspeed);

                if (aispeed < (maxspeed/* / 2f*/)) //AI's speed 10f = 80km/h
                {
                    float acc = 0.001f; //This MUST BE 0.005f
                                        //acc = MaxSpeed * 0.00003f;
                    float speedvalue = aispeed * AccLerp - AccNerfer; //1f

                    float lerpTime = speedvalue * acc;

                    if (lerpTime < 0.2f)
                        lerpTime = 0.2f;


                    spd = MaxSpeed * 10f * 2f;


                    float CurrentAcceleration = Mathf.Lerp(0.1f, spd, lerpTime * Time.deltaTime);

                    if (IsGrounded)
                        LoseSpeed = 1f;
                    else if(LoseSpeed > 0.0f)
                        LoseSpeed -= 0.002f;

                    rb.AddRelativeForce(new Vector3(0, -2f, Mass * LoseSpeed * CurrentAcceleration));

                }
                TargetDistance = Vector3.Distance(transform.position, targetpos);

                if (TargetDistance < 5.0f) //5f normal
                {
                    // Swap the position of the cylinder.

                    if (nextTarget < allTargets)
                    {
                        nextTarget += 1;

                    }
                    else
                    {
                        nextTarget = 0;
                        Lap++;
                    }
                    targetPosRandomizer(AINumber);

                }

            }
            if (!IsGrounded || ForceRespawn) // BUGI PAIKKAAAA!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            {
                if (AISpeed < 30f)
                {
                    StartCoroutine(AIRespawn());
                    /*
                    float posZ = transform.rotation.z;
                    //transform.rotation = Quaternion.Euler(0, posZ + 0f, 0); //T??LT? L?YTYI K??NTYMIS BUGI

                    Vector3 relativePos = targetPosList[nextTarget] + transform.position;
                    Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
                    */
                }

            }
        }
        else if (!GameStart)
        {
            GameStart = GameController.GetComponent<RaceControl>().GameStart;

        }
        if(Lap>MaxLaps && !IsFinished)
        {
            if(!RaceIsOver)
                LapController.GetComponent<LapControl>().FinishedPlayers(AINumber, AIName);

            IsFinished = true;


        }
    }
    private void targetPosRandomizer(int AInumber)
    {
        float pos = DrunkLevel;


        for (int i = 0; i < AICount; i++)
        {
            if (i == 0)
            {
                randomizeTargetPos.x = Random.Range(-pos, pos);
                randomizeTargetPos.z = Random.Range(-pos, pos);

            }
        }

    }

    IEnumerator AIRespawn()
    {
        yield return new WaitForSeconds(2f);

        if(!IsGrounded || ForceRespawn)
        {
            int prevtarget = nextTarget - 1;
            int nexttarget = nextTarget;

            if (nextTarget <= 0)
            {
                prevtarget = 0;
                nexttarget = 1;

            }

            transform.rotation = Quaternion.Euler(0f, transform.rotation.y, 0f);
            transform.position = targetPosList[prevtarget];

            transform.LookAt(targetPosList[nexttarget]);
            ForceRespawn = false;
            IsGrounded = true;
        }

    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "ground" || other.gameObject.tag == "Asphalt" || other.gameObject.tag == "Grass")
        {
            IsGrounded = true;
        }
        /*
        else if (other.gameObject.tag == "wall")// WALLS
        {
            float maxspeed = MaxSpeed -= 20f;
            MaxSpeed = maxspeed;
        }
        else if(other.gameObject.tag == "ai")
        {
            string numb = " ";
            for (int i = 0; i < AINumber; i++)
            {
                numb = i.ToString();
                string aiNumb = "ai" + numb;

                if (other.gameObject.tag == aiNumb)
                {
                    if (i < AINumber)
                    {
                        float maxspd = MaxSpeed -= 20f;
                        MaxSpeed = maxspd;
                    }

                    break;
                }
            }

        }
        */
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "ground" || other.gameObject.tag == "Asphalt" || other.gameObject.tag == "Grass")
        {
            IsGrounded = true;
        }
        else if (other.gameObject.tag == "ForceRespawn")
        {
            ForceRespawn = true;

        }
        /*
        else if (other.gameObject.tag == "ai")
        {
            string numb = " ";
            for (int i = 0; i < AINumber; i++)
            {
                numb = i.ToString();
                string aiNumb = "ai" + numb;

                if (other.gameObject.tag == aiNumb)
                {
                    if (i < AINumber)
                    {
                        MaxSpeed -= 20f;
                    }

                    break;
                }
            }

        }
        */

        float speednerfer = MaxSpeedHolder / 1000f;

        if (EngineClass <= 2) //AI levels 0-2
        {
            if (other.tag == "slowtarget") //Red Target
            {
                MaxSpeed = MaxSpeedHolder * (0.95f - speednerfer); //0.95f
                //MaxSpeed -= DrunkLevel * 2f;
            }
            else if (other.tag == "normaltarget") //Grey Target
            {
                MaxSpeed = MaxSpeedHolder * (1f/* - speednerfer*/); //0.8f
                //MaxSpeed -= DrunkLevel * 2f;
            }
            else if (other.tag == "fasttarget") //Green Target
            {
                MaxSpeed = MaxSpeedHolder;
                //MaxSpeed -= DrunkLevel * 5f;
            }
        }
        else //AI levels 3-5
        {
            if (other.tag == "slowtarget") //Red Target
            {
                MaxSpeed = MaxSpeedHolder * (0.9f/* - speednerfer*/); //0.6f
                //MaxSpeed -= DrunkLevel * 2f;
            }
            else if (other.tag == "normaltarget") //Grey Target
            {
                MaxSpeed = MaxSpeedHolder * (1.0f/* - speednerfer*/); //0.8f
                //MaxSpeed -= DrunkLevel * 2f;
            }
            else if (other.tag == "fasttarget") //Green Target
            {
                MaxSpeed = MaxSpeedHolder;
                //MaxSpeed -= DrunkLevel * 5f;
            }
        }
    }
    /*
    void OnTriggerExit(Collider other)
    {
        
        if (other.gameObject.tag == "ground" || other.gameObject.tag == "Asphalt" || other.gameObject.tag == "Grass")
        {
            IsGrounded = true;
        }
    }
    */
    void OnCollisionExit(Collision other)
    {

        if (other.gameObject.tag == "ground" || other.gameObject.tag == "Asphalt" || other.gameObject.tag == "Grass")
        {
            IsGrounded = false;
        }
    }
}
