namespace Editor.CoreAPI
{
    public class UserProfile
    {
        public string Region { get; set; }
        
        public string Name { get; set; }
        
        public string BootStrappedBucket { get; set; }
        
        public string FleetName { get; set; }
        
        public string FleetId { get; set; }
        
        public string CustomNameLocation { get; set; } //might not need as this is static
        
        public string ComputeName { get; set; }
        
        public string IpAddress { get; set; }
        
        public string WebSocketUrl { get; set; }

        public bool IsSelected { get; set; }
    }
}