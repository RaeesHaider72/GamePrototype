using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthBarUI healthBarUI;
    [SerializeField] private Canvas worldCanvas;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private bool faceCamera = true;

    private Transform cam;
    private HealthComponent healthComponent;

    private void Awake()
    {
        cam = Camera.main.transform;
        healthComponent = GetComponentInParent<HealthComponent>();
    }

    private void OnEnable()
    {
        if (healthComponent == null) return;

        healthComponent.events.OnHealthChanged.AddListener(OnHealthChanged);

        // Initialize at full health when spawned from pool
        healthBarUI.SetHealth(healthComponent.CurrentHealth, healthComponent.MaxHealth);

        if (hideWhenFull)
            healthBarUI.ForceHide();
    }

    private void OnDisable()
    {
        if (healthComponent == null) return;
        healthComponent.events.OnHealthChanged.RemoveListener(OnHealthChanged);
    }

    private void LateUpdate()
    {
        // Keep bar above enemy head
        transform.position = transform.parent.position + offset;

        // Always face camera
        if (faceCamera && cam != null)
            transform.rotation = Quaternion.LookRotation(
                transform.position - cam.position);
    }

    private void OnHealthChanged(float current, float max)
    {
        healthBarUI.SetHealth(current, max);
    }
}