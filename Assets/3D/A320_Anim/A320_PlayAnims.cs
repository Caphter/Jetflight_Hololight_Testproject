using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A320_PlayAnims : MonoBehaviour
{
    [Header("Animation IDs\n0 = Nothing\n1 = PushBack\n2 = TOTaxi\n3 = TakeOff\n8 = Touchdown\n9 = LATaxi\n10 = TODitching - Niz\n11 = TOGround\n12 = TOStopped\n13 = LADitching - Niz\n14 = LAGround\n15 = LAStopped\nAchtung: ID 4, 5, 6 und 7 ist nicht vergeben!")]
    public int animId = 0;
    private int animIdOld = 0;
    public GameObject A320_Gangway;
    public GameObject A320_Model;
    public GameObject A320_AnimStages;

    private Animation A320_GangwayAnim;
    private Animation A320_ModelAnim;
    private Vector3 A320_PosMuc = new Vector3(36304.41f, 56.30532f, 53680.73f);
    private Vector3 A320_PosNiz = new Vector3(1325.87f, 12.2f, 1435.25f);


    // Start is called before the first frame update
    void Start()
    {
        A320_GangwayAnim = A320_Gangway.GetComponent<Animation>();
        A320_ModelAnim = A320_Model.GetComponent<Animation>();

        A320_AnimStages.transform.localPosition = A320_PosMuc;
        A320_ModelAnim.Play("A320_Stay", PlayMode.StopAll);
        A320_GangwayAnim.Play("A320_GangWay_Stay", PlayMode.StopAll);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (animIdOld != animId)
            PlayAnim();

        animIdOld = animId;
    }

    private void PlayAnim()
    {
        switch (animId)
        {
            case 1: //MUC
                A320_AnimStages.transform.localPosition = A320_PosMuc;
                A320_ModelAnim.Play("A320_PushBack", PlayMode.StopAll);
                A320_GangwayAnim.Play("A320_GangWay_Open", PlayMode.StopAll);
                A320_Gangway.SetActive(true);
                animId = 0;
                break;

            case 2: //MUC
                A320_AnimStages.transform.localPosition = A320_PosMuc;
                A320_ModelAnim.Play("A320_TOTaxi", PlayMode.StopAll);
                A320_GangwayAnim.Play("A320_GangWay_End", PlayMode.StopAll);
                A320_Gangway.SetActive(true);
                animId = 0;
                break;

            case 3: //MUC
                A320_AnimStages.transform.localPosition = A320_PosMuc;
                A320_ModelAnim.Play("A320_TakeOff", PlayMode.StopAll);
                A320_GangwayAnim.Play("A320_GangWay_End", PlayMode.StopAll);
                A320_Gangway.SetActive(true);
                animId = 0;
                break;

            case 8: //MUC
                A320_AnimStages.transform.localPosition = A320_PosMuc;
                A320_ModelAnim.Play("A320_Touchdown", PlayMode.StopAll);
                A320_GangwayAnim.Play("A320_GangWay_End", PlayMode.StopAll);
                A320_Gangway.SetActive(true);
                animId = 0;
                break;

            case 9: //MUC
                A320_AnimStages.transform.localPosition = A320_PosMuc;
                A320_ModelAnim.Play("A320_LATaxi", PlayMode.StopAll);
                A320_GangwayAnim.Play("A320_GangWay_End", PlayMode.StopAll);
                A320_Gangway.SetActive(true);
                animId = 0;
                break;

            case 10: //NIZ
                A320_AnimStages.transform.localPosition = A320_PosNiz;
                A320_ModelAnim.Play("A320_TODitching", PlayMode.StopAll);
                A320_GangwayAnim.Play("A320_GangWay_End", PlayMode.StopAll);
                A320_Gangway.SetActive(false);
                animId = 0;
                break;

            case 11: //MUC
                A320_AnimStages.transform.localPosition = A320_PosMuc;
                A320_ModelAnim.Play("A320_TOGround", PlayMode.StopAll);
                A320_GangwayAnim.Play("A320_GangWay_End", PlayMode.StopAll);
                A320_Gangway.SetActive(true);
                animId = 0;
                break;

            case 12: //MUC
                A320_AnimStages.transform.localPosition = A320_PosMuc;
                A320_ModelAnim.Play("A320_TOStopped", PlayMode.StopAll);
                A320_GangwayAnim.Play("A320_GangWay_End", PlayMode.StopAll);
                A320_Gangway.SetActive(true);
                animId = 0;
                break;

            case 13: //NIZ
                A320_AnimStages.transform.localPosition = A320_PosNiz;
                A320_ModelAnim.Play("A320_LADitching", PlayMode.StopAll);
                A320_GangwayAnim.Play("A320_GangWay_End", PlayMode.StopAll);
                A320_Gangway.SetActive(false);
                animId = 0;
                break;

            case 14: //MUC
                A320_AnimStages.transform.localPosition = A320_PosMuc;
                A320_ModelAnim.Play("A320_LAGround", PlayMode.StopAll);
                A320_GangwayAnim.Play("A320_GangWay_End", PlayMode.StopAll);
                A320_Gangway.SetActive(true);
                animId = 0;
                break;

            case 15: //MUC
                A320_AnimStages.transform.localPosition = A320_PosMuc;
                A320_ModelAnim.Play("A320_LAStopped", PlayMode.StopAll);
                A320_GangwayAnim.Play("A320_GangWay_End", PlayMode.StopAll);
                A320_Gangway.SetActive(true);
                animId = 0;
                break;

            default:
                A320_AnimStages.transform.localPosition = A320_PosMuc;
                A320_GangwayAnim.Play("A320_GangWay_Stay", PlayMode.StopAll);
                A320_ModelAnim.Play("A320_Stay", PlayMode.StopAll);
                A320_Gangway.SetActive(true);
                animId = 0;
                break;
        }
    }
}
