using System.Runtime.Serialization;

namespace TentLibrary
{
    

    [DataContract]
    // https://tent.io/docs/info-types
    public class CoreResponse
    {
        [DataMember(Name = "https://tent.io/types/info/core/v0.1.0")]
        public Profile Profile { get; set; }
    }

    [DataContract]
    public class Profile
    {
        [DataMember(Name = "entity")]
        public string Entity { get; set; }
        [DataMember(Name = "licenses")]
        public string[] Licenses { get; set; }
        [DataMember(Name = "servers")]
        public string[] Servers { get; set; }
        [DataMember(Name = "permissions")]
        public Permission Permission { get; set; }
    }


    [DataContract]
    public class Permission
    {
        [DataMember(Name = "public")]
        public bool Public { get; set; }
    }
}
