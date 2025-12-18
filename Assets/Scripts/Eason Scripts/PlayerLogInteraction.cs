using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerLogInteraction : MonoBehaviour
{
    [Header("�����]�w")]
    public float interactDistance = 3.0f; // ���ʶZ��
    public LayerMask interactLayer;       // ��ĳ�]�w Layer (�Ҧp Default) �קK�~�P

    [Header("UI ����")]
    public TMP_Text hintText; // �ù��Ǥߪ��񪺴��ܤ�r (�Ҧp: �� E �\Ū)

    private Camera playerCam;
    private LogUIManager uiManager;

    void Start()
    {
        playerCam = Camera.main; // ����D��v��
        uiManager = FindObjectOfType<LogUIManager>(); // �۰ʴM��������� UI �޲z��
    }

    void Update()
    {
        // �p�G���bŪ��x�A�N�����氻���A�òM�Ŵ���
        if (uiManager != null && uiManager.IsReading())
        {
            if (hintText != null) hintText.text = "";
            return;
        }

        DetectLog();
    }

    void DetectLog()
    {
        // �q��v����m�V�e��o�g�g�u
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        RaycastHit hit;

        // �o�g�g�u
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            // �ˬd���쪺����O�_�� DeveloperLog �}��
            DeveloperLog1 log = hit.collider.GetComponent<DeveloperLog1>();
            if (log != null)
            {
                // 1. ��ܴ���
                if (hintText != null) hintText.text = "press [E] to read diary,[Esc] to close";

                // 2. ������J
                if (Input.GetKeyDown(KeyCode.E))
                {
                    uiManager.ShowLog(log.logContent);
                }
                return; // ���ؼЫ�N�����A�קK����U�誺�M�ŵ{���X
            }
        }

        // �p�G�S�������F��A�Υ��쪺���O��x�A�M�Ŵ���
        if (hintText != null) hintText.text = "";
    }
}
