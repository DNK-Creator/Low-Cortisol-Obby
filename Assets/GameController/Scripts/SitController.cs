using UnityEngine;
using UnityEngine.Events;

public class SitController : MonoBehaviour
{
    public UnityEvent OnSitDown;
    public UnityEvent OnStandUp;
    public KeyCode exitKey = KeyCode.Space; // Клавиша для выхода из состояния сидения
    public string sitAnimation = "Sitting"; // Название анимации сидения

    [SerializeField] private Transform sitPoint; // Точка, где персонаж будет сидеть

    public Rigidbody rb; // Главный Rigidbody
    public Animator animator; // Аниматор персонажа
    private RigidbodyConstraints originalConstraints; // Кэш изначальных ограничений Rigidbody
    private Transform originalParent;
    [SerializeField] private bool isSitting = false;

    private void Start()
    {

        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();

        if (rb != null)
            originalConstraints = rb.constraints; // Кэшируем изначальные ограничения
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, если объект имеет нужный тег перед получением компонента
        if (other.CompareTag("SitObject"))
        {
            SitTarget sitTarget = other.GetComponent<SitTarget>();
            if (sitTarget != null && !isSitting)
            {
                sitPoint = sitTarget.sitPoint;
                sitTarget.OnSeat.Invoke();
                SitDown();
            }
        }
    }

    private void SitDown()
    {
        if (sitPoint == null || animator == null) return;

        

        // Включаем анимацию сидения
        if (animator != null)
            animator.SetBool("isSitting", true);

        // Настраиваем Rigidbody
        if (rb != null)
        {
            rb.isKinematic = true; // Отключаем физику
        }

        OnSitDown.Invoke();

        // Привязываем персонажа к объекту
        originalParent = transform.parent;
        transform.parent = sitPoint;
        transform.position = sitPoint.position;
        transform.rotation = sitPoint.rotation;

        isSitting = true;
    }

    private void Update()
    {
        if (isSitting && Input.GetKeyDown(exitKey))
        {
            StandUp();
        }
    }

    public void StandUp()
    {
        if (!isSitting) return;

        // Выключаем анимацию сидения
        if (animator != null)
            animator.SetBool("isSitting", false);

        if (rb != null)
        {
            rb.isKinematic = false; // Включаем физику
        }
        // Отсоединяем персонажа от объекта
        transform.parent = originalParent;
        transform.localRotation = Quaternion.identity;

        // Выключаем состояние сидения
        isSitting = false;
        OnStandUp.Invoke();
    }
}