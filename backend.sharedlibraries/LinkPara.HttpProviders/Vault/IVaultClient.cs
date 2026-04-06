namespace LinkPara.HttpProviders.Vault
{
    public interface IVaultClient
    {
        T GetSecretValue<T>(string engine, string path, string key = null);
        Task<T> GetSecretValueAsync<T>(string engine, string path, string key = null);
        Task PostSecretValueAsync<T>(string engine, string path, string secretKey = null);

    }
}