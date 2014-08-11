using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitter2.Common
{
    public class Constants
    {
        private static string dbName = "twitter.db";
        private static string consumerKey = "PAgpS5aX1yduUkvPwwsA";
        private static string consumerSecret = "E7YvwTyIBnhQsWctyNN6pXeGqGj19fKtqRPcgpk";

        public static string getDbName() { return dbName; }
        public static string getConsumerKey() { return consumerKey; }
        public static string getConsumerSecret() { return consumerSecret; }
    }
}