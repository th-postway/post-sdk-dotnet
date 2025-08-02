namespace Postway
{
    public class Post : IDisposable
    {
        private readonly string _clientId;
        private readonly string _clientSecret;

        public Post(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }   
        
        public void Dispose()
        {
        }
    }
}