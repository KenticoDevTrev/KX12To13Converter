namespace KX12To13Converter.Interfaces
{
    public interface IPreUpgrade5SiteAndDBPrep
    {
        void RunPreUpgradeFixes(ref string results);
        void SetCompatibilityLevelForDatabase(string databaseName, ref string results);
        void SetSitesToContentOnly();
    }
}
