using DG.Tweening;
using UnityEngine;

public class Danda : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private bool isCritical = false;

    [Header("Swing Settings")]
    public float forwardDistance = 0.6f;
    public float height = 0.4f;
    public float duration = 0.15f;

    private Vector3 startPos;
    private Quaternion startRot;
    private bool isSwinging = false;

    [SerializeField]
    private Collider dandaCollider;

    public GameObject hitEffectPrefab; // optional hit effect

    void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;

        dandaCollider.enabled = false;
    }

    public void Hit()
    {
        if (isSwinging) return;
        isSwinging = true;

        Vector3 worldDir = transform.forward;
        Vector3 localDir = transform.parent.InverseTransformDirection(worldDir);

        Sequence seq = DOTween.Sequence();

        // Wind-up
        seq.Append(transform.DOLocalMove(startPos - localDir * 0.12f, 0.1f).SetEase(Ease.OutSine));
        seq.Join(transform.DOLocalRotateQuaternion(
            startRot * Quaternion.Euler(-25f, -15f, 0f), 0.1f).SetEase(Ease.OutSine));

        // Enable collider at strike start
        seq.AppendCallback(() => dandaCollider.enabled = true);

        // Strike
        seq.Append(transform.DOLocalMove(
            startPos + localDir * forwardDistance + Vector3.up * height * 0.3f,
            duration).SetEase(Ease.InQuad));
        seq.Join(transform.DOLocalRotateQuaternion(
            startRot * Quaternion.Euler(35f, 10f, 0f), duration).SetEase(Ease.InQuad));

        // Follow-through
        seq.Append(transform.DOLocalMove(
            startPos + localDir * (forwardDistance * 0.6f) + Vector3.up * height,
            0.08f).SetEase(Ease.OutQuad));
        seq.Join(transform.DOLocalRotateQuaternion(
            startRot * Quaternion.Euler(20f, 5f, 0f), 0.08f).SetEase(Ease.OutQuad));

        // Disable collider after follow-through
        seq.AppendCallback(() => dandaCollider.enabled = false);

        // Return
        seq.Append(transform.DOLocalMove(startPos, 0.2f).SetEase(Ease.InOutSine));
        seq.Join(transform.DOLocalRotateQuaternion(startRot, 0.2f).SetEase(Ease.InOutSine));

        seq.OnComplete(() => isSwinging = false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isSwinging) return;


        HealthComponent target = other.GetComponent<HealthComponent>();
        Debug.Log($"Danda hit: {other.name}, Target HealthComponent: {(target != null ? "Found" : "None")}");
        if (target == null) return;

        hitEffectPrefab.gameObject.SetActive(true);
       
        Vector3 knockbackDir = (other.transform.position - transform.position).normalized;
        DamageInfo info = new DamageInfo
        {
            amount = damage,
            type = DamageType.Physical,
            source = gameObject,
            hitPoint = other.ClosestPoint(transform.position),
            knockbackDir = knockbackDir,
            knockbackForce = knockbackForce,
            isCritical = isCritical
        };

        Debug.Log($"Damage Info - Amount: {info.amount}, Type: {info.type}, Source: {info.source.name}, HitPoint: {info.hitPoint}, KnockbackDir: {info.knockbackDir}, KnockbackForce: {info.knockbackForce}, IsCritical: {info.isCritical}");
        target.TakeDamage(info);
        dandaCollider.enabled = false; // one hit per swing


    }
}