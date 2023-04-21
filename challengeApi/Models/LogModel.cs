namespace ChallengeApi.Models
{
    public class LogModel
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;//:yyyy-MM-dd HH:mm:ss.fff zzz
        public int Direction { get; set; }  // 0-Input , 1-Output
        public int Level { get; set; } = 0;  //0-normal 1- medium 2-hard 3-hazard
        public string HttpVerb { get; set; }
        public string Rout { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
    }
}
