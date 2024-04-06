using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Scripting;

namespace TD.UServices.Lobbies.Models
{
    [Preserve]
    [DataContract(Name = "Lobby")]
    public class Lobby
    {
        /// <summary>
        /// Data about an individual lobby.
        /// </summary>
        /// <param name="id">id param</param>
        /// <param name="lobbyCode">A short code that can be used to join a lobby.  This is only visible to lobby members.  Typically this is displayed to the user so they can share it with other players out-of-game.  Users with the code can join a lobby even when it is private.</param>
        /// <param name="upid">The Unity project ID of the game.</param>
        /// <param name="environmentId">The ID of the environment this lobby exists in.</param>
        /// <param name="name">The name of the lobby.  Typically this is shown in game UI to represent the lobby.</param>
        /// <param name="maxPlayers">The maximum number of players that can be members of the lobby.</param>
        /// <param name="availableSlots">The number of remaining open slots for players before the lobby becomes full.</param>
        /// <param name="isPrivate">Whether or not the lobby is private.  Private lobbies do not appear in query results and cannot be fetched by non-members using the GetLobby API.  If the lobby is not publicly visible, the creator can share the &#x60;lobbyCode&#x60; with other users who can use it to join this lobby.</param>
        /// <param name="isLocked">Whether or not the lobby is locked.  If true, new players will not be able to join.</param>
        /// <param name="hasPassword">Indicates whether or not a password is required to join the lobby. Players wishing to join must provide the matching password or will be rejected.</param>
        /// <param name="players">The members of the lobby.</param>
        /// <param name="data">Properties of the lobby set by the host.</param>
        /// <param name="hostId">The ID of the player that is the lobby host.</param>
        /// <param name="created">When the lobby was created.  The timestamp is in UTC and conforms to ISO 8601.</param>
        /// <param name="lastUpdated">When the lobby was last updated.  The timestamp is in UTC and conforms to ISO 8601.</param>
        /// <param name="version">The current version of the lobby. Incremented when any non-private lobby data changes.</param>
        [Preserve]
        public Lobby(string id = default, string lobbyCode = default, string upid = default, string environmentId = default, string name = default, int maxPlayers = default, int availableSlots = default, bool isPrivate = default, bool isLocked = default, List<Player> players = default, Dictionary<string, DataObject> data = default, string hostId = default, DateTime created = default, DateTime lastUpdated = default, int version = default, bool hasPassword = default)
        {
            Id = id;
            LobbyCode = lobbyCode;
            Upid = upid;
            EnvironmentId = environmentId;
            Name = name;
            MaxPlayers = maxPlayers;
            AvailableSlots = availableSlots;
            IsPrivate = isPrivate;
            IsLocked = isLocked;
            HasPassword = hasPassword;
            Players = players;
            Data = data;
            HostId = hostId;
            Created = created;
            LastUpdated = lastUpdated;
            Version = version;
        }

        /// <summary>
        /// Parameter id of Lobby
        /// </summary>
        [Preserve]
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; internal set; }

        /// <summary>
        /// A short code that can be used to join a lobby.  This is only visible to lobby members.  Typically this is displayed to the user so they can share it with other players out-of-game.  Users with the code can join a lobby even when it is private.
        /// </summary>
        [Preserve]
        [DataMember(Name = "lobbyCode", EmitDefaultValue = false)]
        public string LobbyCode { get; internal set; }

        /// <summary>
        /// The Unity project ID of the game.
        /// </summary>
        [Preserve]
        [DataMember(Name = "upid", EmitDefaultValue = false)]
        public string Upid { get; internal set; }

        /// <summary>
        /// The ID of the environment this lobby exists in.
        /// </summary>
        [Preserve]
        [DataMember(Name = "environmentId", EmitDefaultValue = false)]
        public string EnvironmentId { get; internal set; }

        /// <summary>
        /// The name of the lobby.  Typically this is shown in game UI to represent the lobby.
        /// </summary>
        [Preserve]
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; internal set; }

        /// <summary>
        /// The maximum number of players that can be members of the lobby.
        /// </summary>
        [Preserve]
        [DataMember(Name = "maxPlayers", EmitDefaultValue = false)]
        public int MaxPlayers { get; internal set; }

        /// <summary>
        /// The number of remaining open slots for players before the lobby becomes full.
        /// </summary>
        [Preserve]
        [DataMember(Name = "availableSlots", EmitDefaultValue = false)]
        public int AvailableSlots { get; internal set; }

        /// <summary>
        /// Whether or not the lobby is private.  Private lobbies do not appear in query results and cannot be fetched by non-members using the GetLobby API.  If the lobby is not publicly visible, the creator can share the &#x60;lobbyCode&#x60; with other users who can use it to join this lobby.
        /// </summary>
        [Preserve]
        [DataMember(Name = "isPrivate", EmitDefaultValue = true)]
        public bool IsPrivate { get; internal set; }

        /// <summary>
        /// Whether or not the lobby is locked.  If true, new players will not be able to join.
        /// </summary>
        [Preserve]
        [DataMember(Name = "isLocked", EmitDefaultValue = true)]
        public bool IsLocked { get; internal set; }

        /// <summary>
        /// Indicates whether or not a password is required to join the lobby. Players wishing to join must provide the matching password or will be rejected.
        /// </summary>
        [Preserve]
        [DataMember(Name = "hasPassword", EmitDefaultValue = true)]
        public bool HasPassword { get; internal set; }

        /// <summary>
        /// The members of the lobby.
        /// </summary>
        [Preserve]
        [DataMember(Name = "players", EmitDefaultValue = false)]
        public List<Player> Players { get; internal set; }

        /// <summary>
        /// Properties of the lobby set by the host.
        /// </summary>
        [Preserve]
        [DataMember(Name = "data", EmitDefaultValue = false)]
        public Dictionary<string, DataObject> Data { get; internal set; }

        /// <summary>
        /// The ID of the player that is the lobby host.
        /// </summary>
        [Preserve]
        [DataMember(Name = "hostId", EmitDefaultValue = false)]
        public string HostId { get; internal set; }

        /// <summary>
        /// When the lobby was created.  The timestamp is in UTC and conforms to ISO 8601.
        /// </summary>
        [Preserve]
        [DataMember(Name = "created", EmitDefaultValue = false)]
        public DateTime Created { get; internal set; }

        /// <summary>
        /// When the lobby was last updated.  The timestamp is in UTC and conforms to ISO 8601.
        /// </summary>
        [Preserve]
        [DataMember(Name = "lastUpdated", EmitDefaultValue = false)]
        public DateTime LastUpdated { get; internal set; }

        /// <summary>
        /// The current version of the lobby. Incremented when any non-private lobby data changes.
        /// </summary>
        [Preserve]
        [DataMember(Name = "version", EmitDefaultValue = false)]
        public int Version { get; set; }

        /// <summary>
        /// Formats a Lobby into a string of key-value pairs for use as a path parameter.
        /// </summary>
        /// <returns>Returns a string representation of the key-value pairs.</returns>
        internal string SerializeAsPathParam()
        {
            var serializedModel = "";

            if (Id != null)
            {
                serializedModel += "id," + Id + ",";
            }
            if (LobbyCode != null)
            {
                serializedModel += "lobbyCode," + LobbyCode + ",";
            }
            if (Upid != null)
            {
                serializedModel += "upid," + Upid + ",";
            }
            if (EnvironmentId != null)
            {
                serializedModel += "environmentId," + EnvironmentId + ",";
            }
            if (Name != null)
            {
                serializedModel += "name," + Name + ",";
            }
            serializedModel += "maxPlayers," + MaxPlayers.ToString() + ",";
            serializedModel += "availableSlots," + AvailableSlots.ToString() + ",";
            serializedModel += "isPrivate," + IsPrivate.ToString() + ",";
            serializedModel += "isLocked," + IsLocked.ToString() + ",";
            serializedModel += "hasPassword," + HasPassword.ToString() + ",";
            if (Players != null)
            {
                serializedModel += "players," + Players.ToString() + ",";
            }
            if (Data != null)
            {
                serializedModel += "data," + Data.ToString() + ",";
            }
            if (HostId != null)
            {
                serializedModel += "hostId," + HostId + ",";
            }
            if (Created != null)
            {
                serializedModel += "created," + Created.ToString() + ",";
            }
            if (LastUpdated != null)
            {
                serializedModel += "lastUpdated," + LastUpdated.ToString() + ",";
            }
            serializedModel += "version," + Version.ToString();
            return serializedModel;
        }

        /// <summary>
        /// Returns a Lobby as a dictionary of key-value pairs for use as a query parameter.
        /// </summary>
        /// <returns>Returns a dictionary of string key-value pairs.</returns>
        internal Dictionary<string, string> GetAsQueryParam()
        {
            var dictionary = new Dictionary<string, string>();

            if (Id != null)
            {
                var idStringValue = Id.ToString();
                dictionary.Add("id", idStringValue);
            }

            if (LobbyCode != null)
            {
                var lobbyCodeStringValue = LobbyCode.ToString();
                dictionary.Add("lobbyCode", lobbyCodeStringValue);
            }

            if (Upid != null)
            {
                var upidStringValue = Upid.ToString();
                dictionary.Add("upid", upidStringValue);
            }

            if (EnvironmentId != null)
            {
                var environmentIdStringValue = EnvironmentId.ToString();
                dictionary.Add("environmentId", environmentIdStringValue);
            }

            if (Name != null)
            {
                var nameStringValue = Name.ToString();
                dictionary.Add("name", nameStringValue);
            }

            var maxPlayersStringValue = MaxPlayers.ToString();
            dictionary.Add("maxPlayers", maxPlayersStringValue);

            var availableSlotsStringValue = AvailableSlots.ToString();
            dictionary.Add("availableSlots", availableSlotsStringValue);

            var isPrivateStringValue = IsPrivate.ToString();
            dictionary.Add("isPrivate", isPrivateStringValue);

            var isLockedStringValue = IsLocked.ToString();
            dictionary.Add("isLocked", isLockedStringValue);

            var hasPasswordStringValue = HasPassword.ToString();
            dictionary.Add("hasPassword", hasPasswordStringValue);

            if (HostId != null)
            {
                var hostIdStringValue = HostId.ToString();
                dictionary.Add("hostId", hostIdStringValue);
            }

            if (Created != null)
            {
                var createdStringValue = Created.ToString();
                dictionary.Add("created", createdStringValue);
            }

            if (LastUpdated != null)
            {
                var lastUpdatedStringValue = LastUpdated.ToString();
                dictionary.Add("lastUpdated", lastUpdatedStringValue);
            }

            var versionStringValue = Version.ToString();
            dictionary.Add("version", versionStringValue);

            return dictionary;
        }
    }
}
