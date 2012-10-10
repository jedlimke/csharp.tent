using System.Runtime.Serialization;
using System.ServiceModel.Web;
using System.ServiceModel;
using System;
namespace TentLibrary
{

    #region Server
    [DataContract]
    public class ServerResponse
    {
        [DataMember(Name = "https://tent.io/types/info/core/v0.1.0")]
        public ServerData ServerData { get; set; }
    }

    [DataContract]
    public class ServerData
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
    #endregion

    #region Registration
    //[WebInvoke(UriTemplate = "/cart", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    //Ship GetShipInfo( RegistrationResponse registrationResponse, string Website);
    //}


    [DataContractAttribute]
    [Serializable]
    public class RegistrationResponse
    {
        [DataMemberAttribute(Name = "https://tent.io/types/info/core/v0.1.0")]
        public RegistrationData RegistrationData { get; set; }
    }

    //[ServiceContractAttribute()]
    //public interface IService
    //{
    //    [WebInvokeAttribute(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    //    [OperationContractAttribute()]
    //    RegistrationData ProcessData();
    //}

    [DataContractAttribute]
    [Serializable]
    public class RegistrationData
    {
        [DataMemberAttribute(Name = "name")]
        public string Name { get; set; }

        [DataMemberAttribute(Name = "description")]
        public string Description { get; set; }

        [DataMemberAttribute(Name = "url")]
        public string Url { get; set; }

        [DataMemberAttribute(Name = "icon")]
        public string Icon { get; set; }

        [DataMemberAttribute(Name = "redirect_uris")]
        public string[] RedirectUris { get; set; }

        [DataMemberAttribute(Name = "scopes")]
        public Scopes Scopes { get; set; }

        [DataMemberAttribute(Name = "id", IsRequired = false)]
        public string Id { get; set; }

        [DataMemberAttribute(Name = "mac_key_id", IsRequired = false)]
        public string MacKeyId { get; set; }

        [DataMemberAttribute(Name = "mac_key", IsRequired = false)]
        public string MacKey { get; set; }

        [DataMemberAttribute(Name = "mac_algorithm", IsRequired = false)]
        public string MacAlgorithm { get; set; }

        [DataMemberAttribute(Name = "authorizations", IsRequired = false)]
        public string[] Authorizations { get; set; }
    }

    [DataContractAttribute]
    public class Scopes
    {
        [DataMemberAttribute(Name = "write_profile")]
        public string WriteProfile { get; set; }

        [DataMemberAttribute(Name = "read_followings")]
        public string ReadFollowings { get; set; }
    }
    #endregion
}
