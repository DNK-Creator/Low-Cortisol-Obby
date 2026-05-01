using UnityEngine;
using UnityEngine.Events;

public class SimpleRagdoll : MonoBehaviour
{
    public UnityEvent OnEnterRagdoll;
    public UnityEvent OnExitRagdoll;

    public Rigidbody rb; // ������� Rigidbody
    public CapsuleCollider mainCollider; // ������� ���������
    public Animator animator; // �������� ���������

    public float ragdollColliderRadius = 0.5f; // ������ ���������� � ������ �������
    public float ragdollColliderHeigh = 0.5f; // ������ ���������� � ������ �������
    public Vector3 fetalPosition = new Vector3(0, 1, 0); // �������� � ���� ��������


    private RigidbodyConstraints originalConstraints; // ��� ����������� ����������� Rigidbody
    private float originalColliderRadius; // ������������ ������ ����������
    private float originalColliderHeigh; // ������������ ������ ����������
    private bool isRagdoll = false; // ������ ��������

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (mainCollider == null) mainCollider = GetComponent<CapsuleCollider>();
        if (animator == null) animator = GetComponent<Animator>();

        if (rb != null)
            originalConstraints = rb.constraints; // �������� ����������� �����������

        if (mainCollider != null)
        {
            originalColliderHeigh = mainCollider.height; // ��������� ������������ ������ ����������
            originalColliderRadius = mainCollider.radius; // ��������� ������������ ������ ����������            
        }
    }

    private void OnEnable()
    {
        if (isRagdoll)
            ExitRagdoll();
    }

    public void EnterRagdoll()
    {
        if (isRagdoll) return;


        // ��������� ��������
        if (animator != null) 
            animator.SetBool("isRagdoll", true);

        // ����������� Rigidbody
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None; // ������������ ��� ���
            rb.useGravity = true; // �������� ����������
        }

        OnEnterRagdoll.Invoke();
        // �������� ���������
        if (mainCollider is CapsuleCollider capsuleCollider)
        {
            capsuleCollider.radius = ragdollColliderRadius;
            capsuleCollider.height = ragdollColliderHeigh;
        }

        // ������� ��������� � ���� ��������
        transform.localRotation = Quaternion.identity; // ���������� ��������
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // ���������� ��������
            rb.angularVelocity = Vector3.zero; // ���������� ������� ��������
        }

        transform.localPosition += fetalPosition;


        isRagdoll = true;
    }

    public void ExitRagdoll()
    {
        if (!isRagdoll) return;


        // �������� ��������
        if (animator != null)
            animator.SetBool("isRagdoll", false);

        // ��������������� Rigidbody
        if (rb != null)
        {
            rb.constraints = originalConstraints; // ��������������� �����������
            rb.useGravity = false; // ��������� ����������
        }

        // ��������������� ���������
        if (mainCollider is CapsuleCollider capsuleCollider)
        {
            capsuleCollider.radius = originalColliderRadius; // ���������� �������� ������
            capsuleCollider.height = originalColliderHeigh; // ���������� �������� ������
        }

        transform.localRotation = Quaternion.identity; // ���������� ��������

        OnExitRagdoll.Invoke();

        isRagdoll = false;
    }

    void Update()
    {
        // �������� ������� ��� ��������� � ���������� ��������
        if (Input.GetKeyDown(KeyCode.R))
            EnterRagdoll();
        if (Input.GetKeyDown(KeyCode.T))
            ExitRagdoll();
    }
}
