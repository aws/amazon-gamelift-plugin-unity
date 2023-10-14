namespace Editor.CoreAPI
{
    public class UserProfile
    {
        public string Region { get; set; }

        public string Name { get; set; }

        public string BucketName { get; set; }

        #region Anywhere Fleet Settings

        public string AnywhereFleetName { get; set; }

        public string AnywhereFleetId { get; set; }

        public string AnywhereFleetLocation { get; set; }
        
        public string ComputeName { get; set; }

        public string IpAddress { get; set; }

        public string WebSocketUrl { get; set; }

        #endregion

        #region Managed EC2 Settings

        public DeploymentScenarios DeploymentScenario { get; set; }
        
        public string DeploymentGameName { get; set; }
        
        public string ManagedEC2FleetName { get; set; }
        
        public string BuildName { get; set; }
        
        public string LaunchParameters { get; set; }
        
        public string BuildOperatingSystem { get; set; }
        
        public string DeploymentBuildFilePath { get; set; }
        
        public string DeploymentBuildFolderPath { get; set; }

        #endregion
    }
}