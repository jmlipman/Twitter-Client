using SQLite;
using System;

namespace Twitter2.fDatabase
{
    [Table("StoredAccounts")]
    public class StoredAccount
    {
        [PrimaryKey, Unique]
        public Guid Id { get; set; }
        public string TwId { get; set; }
        public string TwUsername { get; set; }
        public string TwName { get; set; }
        public int TwTweetCount { get; set; }
        public int TwFollowingCount { get; set; }
        public int TwFollowersCount { get; set; }
        public string TwOAuthToken { get; set; }
        public string TwAvatarURL { get; set; }
        public string TwAccessToken { get; set; }
        public bool DefaultAccount { get; set; }

    }
}
