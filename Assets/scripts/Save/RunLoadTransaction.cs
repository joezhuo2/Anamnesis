namespace CrystalFlux.SaveSystem
{
    public class RunLoadTransaction
    {
        private readonly SaveFile file;
        private bool done;

        private RunLoadTransaction(SaveFile f) => file = f;

        public static RunLoadTransaction Begin(SaveFile f)
        {
            if (f?.meta != null && f.meta.kind == SaveKind.Run) SaveStore.Delete(SaveKind.Run, f.meta.id);
            return new RunLoadTransaction(f);
        }

        public void Commit() => done = true;

        public void Rollback()
        {
            if (done || file?.meta == null || file.meta.kind != SaveKind.Run) return;

            SaveStore.Write(file);
            done = true;
        }
    }
}
