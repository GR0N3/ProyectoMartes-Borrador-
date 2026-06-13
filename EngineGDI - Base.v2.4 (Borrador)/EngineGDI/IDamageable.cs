namespace EngineGDI
{
    public interface IDamageable
    {
        float Vida { get; }
        void TakeDamage(float amount);
    }
}
