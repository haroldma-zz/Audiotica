namespace Audiotica.Data.Model.EchoNest
{
    public class EchoRoot<T>
    {
        public T response { get; set; }
    }

    public class EchoResponse
    {
        public EchoStatus status { get; set; }
    }
    
    public class EchoListResponse : EchoResponse
    {
        public int start { get; set; }
        public int total { get; set; }
    }

    public class EchoStatus
    {
        public string version { get; set; }
        public int code { get; set; }
        public string message { get; set; }
    }
}