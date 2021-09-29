namespace CollectStats_Functions.Models
{

    public class RepoDependenciesResponse
    {
        public Repository repository { get; set; }
    }


    public class Repository
    {
        public Dependencygraphmanifests dependencyGraphManifests { get; set; }
    }

    public class Dependencygraphmanifests
    {
        public Node[] nodes { get; set; }
    }

    public class Node
    {
        public string blobPath { get; set; }
        public Dependencies dependencies { get; set; }
    }

    public class Dependencies
    {
        public Dependency[] nodes { get; set; }
    }

    public class Dependency
    {
        public string packageName { get; set; }
        public string requirements { get; set; }
        public bool hasDependencies { get; set; }
        public DependentRepo repository { get; set; }
        public string packageManager { get; set; }
    }

    public class DependentRepo
    {
        public string nameWithOwner { get; set; }
    }

}
