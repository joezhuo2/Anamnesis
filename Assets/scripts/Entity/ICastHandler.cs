namespace CrystalFlux.EntitySystem
{
    public interface ICastHandler
    {
        bool IsCasting { get; }
        void CancelCast();
    }
}
