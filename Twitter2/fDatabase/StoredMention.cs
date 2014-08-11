using LinqToTwitter;
using SQLite;
using System;
using System.Collections.Generic;

namespace Twitter2.fDatabase
{
    [Table("StoredMentions")]
    public class StoredMention
    {
        [PrimaryKey, Unique]
        public Guid Id { get; set; }
        //public Annotation Annotation { get; set; }
        //public List<Contributor> Contributors { get; set; }
        //public Coordinate Coordinates { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        //public int Count { get; set; }
        public DateTime CreatedAt { get; set; }
        //public Entities Entities { get; set; }
        //public bool ExcludeReplies { get; set; }
        //public bool Favorited { get; set; }
        //public Geo geo{ get; set; }
        //public string ID { get; set; }
        //public bool IncludeContributorDetails { get; set; }
        //public bool IncludeEntities { get; set; }
        //public bool IncludeRetweets { get; set; }
        //public string InReplyToScreenName { get; set; }
        //public string InReplyToStatusID { get; set; }
        //public string InReplyToUserID { get; set; }
        //public ulong MaxID { get; set; }
        //public string MaxID { get; set; }
        //public int Page { get; set; }
        //public Place Place { get; set; }
        //public bool PossiblySensitive { get; set; }
        //public Retweet retweet{ get; set; }
        public int RetweetCount { get; set; }
        public int FavouriteCount { get; set; }
        //public bool Retweeted { get; set; }
        //?????????????public Status RetweetedStatus { get; set; }
        public string ScreenName { get; set; } //USER.ALGO
        //public ulong SinceID { get; set; }
        //public string SinceID { get; set; }
        //public string Source { get; set; }
        public string StatusID { get; set; }
        public string Text { get; set; }
        //public bool TrimUser { get; set; } I DON'T THINK SO TIM
        //public bool Truncated { get; set; } I DON'T THINK SO TIM
        //?????????????public StatusType Type { get; set; }
        //public User User { get; set; }
        public string UserID { get; set; }
        //public List<User> Users { get; set; }
        public bool viewed { get; set; }
        public string ProfileImageUrl { get; set; }
    }
}
