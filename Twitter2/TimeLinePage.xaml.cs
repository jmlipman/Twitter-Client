using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Twitter2.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Networking.Connectivity;
using Twitter2.fDatabase;
using SQLite;
using Windows.UI.Xaml.Media.Imaging;

namespace Twitter2
{
    /**
     * This Page will display the Twitter account timeline stored in the database (or recently added).
     */ 
    public sealed partial class TimeLinePage : Page
    {
        private List<StoredStatus> statusSource = null; //Displayed status
        private List<Status> tempStatusList = null; //Temporal list to store status
        private TwitterContext twitterCtx = null; //Twitter context which we'll work.
        private string lastStatusId = null;
        private StoredAccount currentUser = null;
        private DispatcherTimer tweetsenttimer;
        private DispatcherTimer loadtweetstimer;
        private DispatcherTimer firstloadtimer;
        private Translator translator;
        private string replyid = "";
        private int replynamesize;
        private bool bool_multipletweets = false;
        private bool boolCheckNewMentions = false;
        private bool verticalOrientation = false;

        /**
         * Main method:
         * -Varaibles initialization.
         * -Context variables obtention.
         * -Establishment of some events.
         * -Get last tweets and mentions.
         * 
         * Every minute (timer) the program recovers last tweets and mentions. Throught the
         * "Connect" and "Mention" buttons it iterates between our TimeLine and Mention
         * tweets.
         */ 
        
        public TimeLinePage()
        {
            this.InitializeComponent();
            loadingring.IsActive = true;

            //Obtains the translator
            translator = SuspensionManager.SessionState["Translator"] as Translator;

            //Translates some texts
            //loadingtext.Text = translator.getCadena("loading...");
            //tweetsendingstatus.Text = translator.getCadena("sending...");
            whatshappening.Text = translator.getCadena("whatshappening");
            multipletweets.Content = translator.getCadena("multipletweets");
            orientationtext.Text = translator.getCadena("verticalorientation");
            

            //Timer to start the application
            firstloadtimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };

            //Timer to reload tweets
            loadtweetstimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1, 0) };
            
            //Timer to blink "tweet sent"
            tweetsenttimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 2) };

            //We must recive always the twitter context, and there should be impossible not to recive it
            if (SuspensionManager.SessionState.ContainsKey("TwitterContext"))
            {
                twitterCtx = SuspensionManager.SessionState["TwitterContext"] as TwitterContext;
            }
            else
            {  //Look for it in the database, but this shouldn't happen.
                //At this moment, this is not necessary
            }

            //It must recive always the user, and there should be impossible not to recive it
            if (SuspensionManager.SessionState.ContainsKey("MyUser"))
            {
                currentUser = SuspensionManager.SessionState["MyUser"] as StoredAccount;
            }
            else
            {  //Look for it in the database, but this shouldn't happen.
                //At this moment, this is not necessary
            }
            
            //Establishment of some events
            firstloadtimer.Tick += firstloadtimer_Tick;
            tweetsenttimer.Tick += tweetsenttimer_Tick;
            loadtweetstimer.Tick += loadtweetstimer_Tick;
            connectImage.PointerPressed += connect_Click;
            homeImage.PointerPressed += home_Click;

            firstloadtimer.Start();

            abouttext.Text = "JMTwitter has been developed by Juan Miguel Valverde Martinez (lipman). " +
            "I have many thoughts for the next version in mind, and I hope continue with this project.\n\n" +
            "Credits:\n-Joe Mayo and his team, who made this possible (LinqToTwitter)\n" +
            "-Frank Krueger (sqlite-net)\n-@rNiubo and #OlimpiadAPPs to encourage me to learn C# and about W8 development.\n\n"+
            "Contact: @jmlipman\nWebsite: delanover.com/ajsdbasdhas dhas dahs dajhs dha sdha s";

            /*var account = (from acct in twitterCtx.Account
                                       where acct.Type == AccountType.Settings
                                       select acct).SingleOrDefault();

                    
                    Debug.WriteLine("lalalal: "+account.Settings.Language);*/
            
        }

        void firstloadtimer_Tick(object sender, object e)
        {
            loadtweetstimer.Start();
            firstloadtimer.Stop();

            string woeid = (from acct in twitterCtx.Account
                           where acct.Type == AccountType.Settings
                           select acct).SingleOrDefault().Settings.TrendLocation.WoeID;
            
            if (twitterCtx != null)
            {
                getProfileInfo();
                getLastTweets(1);
                getTrends(woeid);
            }
            loadingring.IsActive = false;
            trendingtopicstext.Visibility = Windows.UI.Xaml.Visibility.Visible;
            paneltweets.Visibility = Windows.UI.Xaml.Visibility.Visible;
            panelfollowings.Visibility = Windows.UI.Xaml.Visibility.Visible;
            panelfollowers.Visibility = Windows.UI.Xaml.Visibility.Visible;
            loadinggrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        public void getTrends(string woeid){
            Debug.WriteLine("WOEID: "+woeid);
            
            List<Trend> trends =
                (from trnd in twitterCtx.Trends
                 where trnd.Type == TrendType.Place &&
                 trnd.WoeID == Convert.ToInt32(woeid) // something other than 1
                 select trnd)
                 .ToList();

            /*foreach (Trend item in trends)
            {
                Debug.WriteLine(item.Name);
            }*/

            //guardar los nombres. Al hacer click, que hagan una busqueda con el contenido y ""

            trend1.Text = trends[0].Name;
            trend2.Text = trends[1].Name;
            trend3.Text = trends[2].Name;
            trend4.Text = trends[3].Name;
            trend5.Text = trends[4].Name;
            trend6.Text = trends[5].Name;
            trend7.Text = trends[6].Name;
            trend8.Text = trends[7].Name;
            trend9.Text = trends[8].Name;
            trend10.Text = trends[9].Name;

            //trendsgridview.ItemsSource = trends;
        }

        /**
         * This method is launched when the user clicks on Connect botton.
         * We just change the icons (selected and unselected) and get the stored mentions.
         */ 
        void connect_Click(object sender, PointerRoutedEventArgs e)
        {
            homeImage.Source = new BitmapImage(new Uri("ms-appx:/Assets/home.png"));
            connectImage.Source = new BitmapImage(new Uri("ms-appx:/Assets/connectSelected.png"));

            //We obtain the tweet list
            /*tempStatusList = (from mention in twitterCtx.Status
                where mention.Type == StatusType.Mentions &&
                    mention.Count == 50 && mention.Page == 1
                select mention).ToList();*/

            //Retrieve of first 200 tweets and convert them into the appropriated format
            //statusSource = convertList(tempStatusList, "0");

            //saveMentions(statusSource);

            //timelinegridview.ItemsSource = statusSource;
            getStoredMentions();

        }

        /**
         * This method is launched when the user clicks on Mention botton.
         * We just change the icons (selected and unselected) and get the stored mentions.
         */ 
        void home_Click(object sender, PointerRoutedEventArgs e)
        {
            homeImage.Source = new BitmapImage(new Uri("ms-appx:/Assets/homeSelected.png"));
            connectImage.Source = new BitmapImage(new Uri("ms-appx:/Assets/connect.png"));
            getStoredTweets();
        }


        /**
         * This function is used to obtain user profle information such as avatar, tweets, friends,
         * screen name, followers, etc, and finally display it.
         */ 
        public void getProfileInfo()
        {

            myScreenName.Text = "@" + currentUser.TwUsername;
            myAvatar.Source = new BitmapImage(new Uri(currentUser.TwAvatarURL));
            myFollowers.Text = currentUser.TwFollowersCount.ToString();
            myFollowing.Text = currentUser.TwFollowingCount.ToString();
            myTweets.Text = currentUser.TwTweetCount.ToString();

        }

        /**
         * This functions targets to obtain the last tweets of the user timeline
         * and display them as source of the gridview XAML item.
         * We firstly check whether we have internet to show stored status or search them
         * in the Internet.
         * One of the keys in this function is the parameter 'param'. If this param es
         * a small number, this will represent the page of where tweets we want are. In other
         * case, this param represents the last status we have stored, and we can mark new statuses
         * as unread.
         * 
         * @param param is the page of the tweets or the last status we have.
         */ 
        public void getLastTweets(int param)
        {

            /*var trends =
    from trnd in twitterCtx.Trends
    where trnd.Type == TrendType.Available
    select trnd;

            List<Location> trend = trends.FirstOrDefault().Locations.ToList();

            foreach (Location item in trend)
            {
                Debug.WriteLine("Country: " + item.Country);
                Debug.WriteLine("Country Code: " + item.CountryCode);
                Debug.WriteLine("Name: " + item.Name);
                Debug.WriteLine("Parent ID: " + item.ParentID);
                Debug.WriteLine("Place Type Name: " + item.PlaceTypeName);
                Debug.WriteLine("Place Type Name Code: " + item.PlaceTypeNameCode);
                Debug.WriteLine("URL: " + item.Url);
                Debug.WriteLine("WoeID: " + item.WoeID);
            }*/

            

            /*trend.Locations.ToList().ForEach(
                loc => Console.WriteLine(
                    "Name: {0}, Country: {1}, WoeID: {2}",
                    loc.Name, loc.Country, loc.WoeID));*/




            //Firstly we check whether we have Internet access or not
            if (NetworkInformation.GetInternetConnectionProfile() == null)
            {   //No internet access
                getStoredTweets();
            }
            else
            {   //Internet Access
                
                if (param > 100)
                { //param will represent the last status we have

                    //Last 200 tweets
                    //var queryResponse =
                    //from tweet in twitterCtx.Status
                    //where tweet.Type == StatusType.Home &&
                    //tweet.Count == 200 && tweet.Page == 1
                    //select tweet;

                    //We obtain the tweet list
                    tempStatusList = (from tweet in twitterCtx.Status
                                      where tweet.Type == StatusType.Home &&
                                      tweet.Count == 200 && tweet.Page == 1
                                      select tweet).ToList();

                    //Retrieve of first 200 tweets and convert them into the appropriated format
                    statusSource = convertList(tempStatusList, false, lastStatusId);

                    //We obtain the first status ID
                    lastStatusId = statusSource.First().StatusID;

                    //Save tweet list in the database
                    saveTweets(statusSource);

                }
                else
                {   //param will represent the page, so, we will obtain its tweets
                    //var queryResponse =
                    //from tweet in twitterCtx.Status
                    //where tweet.Type == StatusType.Home &&
                    //tweet.Count == 200 && tweet.Page == param
                    //select tweet;

                    //We obtain the tweet list
                    //tempStatusList = queryResponse.ToList();
                    tempStatusList = (from tweet in twitterCtx.Status
                    where tweet.Type == StatusType.Home &&
                    tweet.Count == 200 && tweet.Page == param
                    select tweet).ToList();

                    //The first time we retrieve tweets, we want to store them
                    if (param == 1)
                    {
                        //Retrieve of first 200 tweets and convert them into the appropriated format
                        statusSource = convertList(tempStatusList, false);

                        //We obtain the first status ID
                        lastStatusId = statusSource.First().StatusID;

                        //Save tweet list in the database
                        saveTweets(statusSource);
                    }
                    else
                    {
                        //List = List + {200 newer tweets}
                        //METER UN TRY? DEBO PROBAR ESTA PARTE
                        //NullReferenceException
                        statusSource.AddRange(convertList(tempStatusList, false));
                    }

                    //We set the itemsource of the gridview XAML item as the status source we have
                    timelinegridview.ItemsSource = statusSource;
                }
            }

            //PARTE DE LAS MENCIONES
            //We obtain the tweet list
            tempStatusList = (from mention in twitterCtx.Status
                              where mention.Type == StatusType.Mentions &&
                                  mention.Count == 50 && mention.Page == 1
                              select mention).ToList();


            //Save tweet list in the database
            saveTweets(convertList(tempStatusList, true, "0"));
            
        }

        /**
         * Displays in the GUI the stored status we have. This is very useful in case we
         * don't have Internet access.
         * 
         */
        private async void getStoredMentions()
        {
            //Set the database path and initilize the database connection
            var path = Windows.Storage.ApplicationData.Current.LocalFolder.Path + @"\" + Constants.getDbName();
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(path);

            //We obtain the status
            //var tmpSource = await db.QueryAsync<StoredMention>("SELECT * FROM StoredMentions");
            var tmpSource = await db.QueryAsync<StoredStatus>("SELECT * FROM StoredStatuses where isMention = ?", true);

            Debug.WriteLine(tmpSource[0].isMention);

            timelinegridview.ItemsSource = tmpSource;

        }

        private async void getStoredTweets()
        {
            //Set the database path and initilize the database connection
            var path = Windows.Storage.ApplicationData.Current.LocalFolder.Path + @"\" + Constants.getDbName();
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(path);

            //We obtain the status
            statusSource = await db.QueryAsync<StoredStatus>("SELECT * FROM StoredStatuses where isMention = ?", false);
            Debug.WriteLine("Count: "+statusSource.Count);
            //We obtain the first status ID
            lastStatusId = statusSource.First().StatusID;
            //Debug.WriteLine(lastStatusId + "," + statusSource[0].viewed);

            //Display them
            timelinegridview.ItemsSource = statusSource;
        }

        /**
         * Convert an status list into storedstatus list.
         * 
         * @param list to be convert.
         * @return converted list.
         */
        public List<StoredStatus> convertList(List<Status> list, bool isMention, string statusId = "")
        {

            //Create the new list
            List<StoredStatus> newList = new List<StoredStatus>();

            //Walk through our list and add elements into the other list
            foreach (Status stat in list)
            {
                if (statusId == "")
                    newList.Add(convertStatus(stat, isMention, true));
                else
                {   //statusId > than current status ID
                    if(statusId.CompareTo(stat.StatusID.ToString())!=-1)
                        newList.Add(convertStatus(stat, isMention, true));
                    else
                        newList.Add(convertStatus(stat, isMention, false));
                }
                    
            }

            //----------------------------------------------------------------------->
            //AQUI DEBERIA COMPROBAR SI TENGO NUEVAS MENCIONES Y EMITIR UN SONIDO
            //Comparar los first
            //en loadtweetstimer_Tick se activa el boolean
            if (isMention && boolCheckNewMentions)
            {
                Debug.WriteLine("llego y no deberia llegar tan pronto");
                //checkNewMentions(newList[0]);
            }
            else
            {
                Debug.WriteLine("llego al otro sitio");
            }
            
            return newList;
        }

        private async void checkNewMentions(StoredStatus firstMention)
        {
            //Set the database path and initilize the database connection
            var path = Windows.Storage.ApplicationData.Current.LocalFolder.Path + @"\" + Constants.getDbName();
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(path);

            //We obtain the status
            List<StoredStatus> storedMentions = await db.QueryAsync<StoredStatus>("SELECT * FROM StoredStatuses where isMention = ?", true);

            //We obtain the first status ID

            Debug.WriteLine("First new mention: " + firstMention.StatusID.ToString());
            Debug.WriteLine("First stored mention: " + storedMentions.First().StatusID.ToString());
            if (firstMention.StatusID != storedMentions.First().StatusID)
            {
                Debug.WriteLine("Nueva mención!");
            }
            else
            {
                Debug.WriteLine("No hay nuevas menciones =(");
            }

        }


        public StoredStatus convertStatus(Status stat, bool isMention, bool viewed)
        {
            
            return new StoredStatus()
            {
                Id = Guid.NewGuid(),
                Latitude = stat.Coordinates.Latitude.ToString(),
                Longitude = stat.Coordinates.Longitude.ToString(),
                CreatedAt = stat.CreatedAt,
                RetweetCount = stat.RetweetCount,
                FavouriteCount = 0, //TO BE FIXED IN THE FUTURE
                ScreenName = stat.User.Identifier.ScreenName,
                StatusID = stat.StatusID,
                Text = stat.Text,
                UserID = stat.UserID,
                viewed = viewed,
                isMention = isMention,
                ProfileImageUrl = stat.User.ProfileImageUrl
            };
        }

        /**
        * Save the tweet list.
         * 
        * @param list of tweets to be saved.
        */
        public async void saveTweets(List<StoredStatus> list)
        {
            //Set the database path and initilize the database connection
            var path = Windows.Storage.ApplicationData.Current.LocalFolder.Path + @"\" + Constants.getDbName();
            SQLiteAsyncConnection db = new SQLiteAsyncConnection(path);

            

            //Everytime we want to save tweets, we want to stored them and remove the others we already have
            //So we delete our current table and create a new one
            await db.DropTableAsync<StoredStatus>();
            await db.CreateTableAsync<StoredStatus>();

            //Insert the list
            await db.InsertAllAsync(list);

            /*//Everytime we want to save tweets, we want to stored them and remove the others we already have
            //So we delete our current table and create a new one
            db.DropTableAsync<StoredStatus>();
            db.CreateTableAsync<StoredStatus>();

            //Insert the list
            db.InsertAllAsync(list);*/
        }

        /**
         *  This function calculates the chars left we can write in a tweet. This will take into account the
         *  usage of multiple tweets function.
         *  
         */
        private void updateCharactersLeft_TextChange(object sender, TextChangedEventArgs e)
        {
            updateCharactersLeft();
        }

        private void updateCharactersLeft()
        {
            int currentCharsLeft;
            bool bool_multipletweets = (bool)multipletweets.IsChecked;

            if (!bool_multipletweets)
            {
                currentCharsLeft = 140 - tweetbox.Text.Length;
                if (currentCharsLeft >= 20)
                    charactersleft.Foreground = new SolidColorBrush(Windows.UI.Colors.Green);
                else if (currentCharsLeft < 20 && currentCharsLeft > 0)
                    charactersleft.Foreground = new SolidColorBrush(Windows.UI.Colors.Orange);
                else
                    charactersleft.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);

            }
            else
            {
                currentCharsLeft = 660 - tweetbox.Text.Length;

                if (currentCharsLeft >= 20)
                    charactersleft.Foreground = new SolidColorBrush(Windows.UI.Colors.Green);
                else if (currentCharsLeft < 20 && currentCharsLeft > 0)
                    charactersleft.Foreground = new SolidColorBrush(Windows.UI.Colors.Orange);
                else
                    charactersleft.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);

            }

            charactersleft.Text = currentCharsLeft.ToString();
        }

        private void sendTweet_Click(object sender, PointerRoutedEventArgs e)
        {
            loadingbar.IsIndeterminate = true;
            //NOTA, EL COLOR ESTÁ TOCADO EN APP.XAML

            tweetsenttimer.Start();
            //tweetsendingstatus.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
        }


        void tweetsenttimer_Tick(object sender, object e)
        {

            List<String> tweets = new List<string>();
            int currentListIndex = 0;
            tweetsenttimer.Stop();

            //Single tweet
            if (!bool_multipletweets)
            {
                if ((tweetbox.Text.Length) <= 140)
                {
                    try
                    {
                        if (replyid != "")
                            twitterCtx.UpdateStatus(tweetbox.Text, replyid);
                        else
                            twitterCtx.UpdateStatus(tweetbox.Text);
                    }
                    catch (Exception)
                    {
                        //tweetstatus.Text = "Error. Try again later.";
                    }
                }
            }
            else //Multiples tweets
            {
                string[] words = tweetbox.Text.Split();
                tweets.Add("");

                //Generating of tweet list is going to be send
                for (int a = 0; a < words.Length; a++)
                {
                    if (tweets[currentListIndex].Length + words[a].Length > 135)
                    {
                        tweets[currentListIndex] += ">>";
                        if (replyid != "")
                        {
                            Debug.WriteLine("reply: " + replyid.Length);

                            tweets.Add("@" + replyscreenname.Text + " >> ");
                        }
                        else
                            tweets.Add(">> ");

                        currentListIndex++;
                    }

                    tweets[currentListIndex] += words[a] + " ";
                }

                //Sending tweet list
                foreach (string individualTweet in tweets)
                {
                    try
                    {
                        if (replyid != "")
                            twitterCtx.UpdateStatus(individualTweet, replyid);
                        else
                            twitterCtx.UpdateStatus(individualTweet);
                    }
                    catch (Exception)
                    {
                        tweetstatus.Text = "Error. Try again later.";
                    }
                }
            }

            loadingbar.IsIndeterminate = false;
            
            tweetstatus.Text = "";

            newtweetgrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            graygridoverlay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //tweetsendingstatus.Foreground = new SolidColorBrush(Windows.UI.Colors.Transparent);
        }


        void loadtweetstimer_Tick(object sender, object e)
        {
            
            boolCheckNewMentions = true;
            getLastTweets(1);
            homeImage.Source = new BitmapImage(new Uri("ms-appx:/Assets/homeSelected.png"));
            connectImage.Source = new BitmapImage(new Uri("ms-appx:/Assets/connect.png"));
            
        }

        /**
         * This method is launched when "new Tweet" button is pressed.
         * Write tweet window become visible.
         * In the textbox is automatically written the user it is wanted reply.
         */ 
        private void openWindowNewTweet(object sender, PointerRoutedEventArgs e)
        {

            StoredStatus selectedStatus = (StoredStatus)timelinegridview.SelectedItem;

            //Get replied user screenname
            if (selectedStatus != null)
            {
                replyid = selectedStatus.StatusID;
                replynamesize = selectedStatus.ScreenName.Length + 1;
                tweetbox.Text = "@" + selectedStatus.ScreenName + " ";
                replyicon.Visibility = Windows.UI.Xaml.Visibility.Visible;
                replyscreenname.Text = selectedStatus.ScreenName;
            }
            else
            {
                replyid = "";
                replynamesize = 0;
                tweetbox.Text = "";
                replyicon.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            newtweetgrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            graygridoverlay.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        /**
         * New tweet window closure.
         */ 
        private void closeNewTweetWindow(object sender, PointerRoutedEventArgs e)
        {
            newtweetgrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            graygridoverlay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            aboutpage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }


        private void multipletweets_click(object sender, RoutedEventArgs e)
        {
            bool_multipletweets = !bool_multipletweets;
            updateCharactersLeft();
        }

        private void searchTrend_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            List<Status> srch =
                (from search in twitterCtx.Search
                 where search.Type == SearchType.Search &&
                       search.Query == ((TextBlock)sender).Text &&
                       search.Count == 20
                 select search)
                .ToList().First().Statuses;

            timelinegridview.ItemsSource = convertList(srch,false);
            
        }


        private void aboutPage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            aboutpage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            graygridoverlay.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }


        private void changeOrientation_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            verticalOrientation = !verticalOrientation;

            if (verticalOrientation)
            {
                orientationtext.Text = translator.getCadena("horizontalorientation");
                trendsgridview.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                accountinfogridview.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                home_text.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                connect_text.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                timelinegridview.Margin = new Windows.UI.Xaml.Thickness(0,144,0,45);
                //"newtweetbuttongrid" Margin="1188,55,149,684"
                //newtweetbuttongrid.Margin = new Windows.UI.Xaml.Thickness(0, 0, 0, 0);
                newtweet_text.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                newtweetgrid.Width = 250;
                newtweetspace.Width = 40;
                multipletweets.Content = "";
                tweetbox.Width = 230;
                aboutpage.Width = 250;
                abouttext.Width = 230;
                tweetboxborder.Width = 230;
            }
            else
            {
                orientationtext.Text = translator.getCadena("verticalorientation");
                trendsgridview.Visibility = Windows.UI.Xaml.Visibility.Visible;
                accountinfogridview.Visibility = Windows.UI.Xaml.Visibility.Visible;
                home_text.Visibility = Windows.UI.Xaml.Visibility.Visible;
                connect_text.Visibility = Windows.UI.Xaml.Visibility.Visible;
                timelinegridview.Margin = new Windows.UI.Xaml.Thickness(234, 144, 0, 45);
                //newtweetbuttongrid.Margin = new Windows.UI.Xaml.Thickness(1188, 55, 149, 684);
                newtweet_text.Visibility = Windows.UI.Xaml.Visibility.Visible;
                newtweetgrid.Width = 500;
                newtweetspace.Width = 200;
                multipletweets.Content = translator.getCadena("multipletweets");
                tweetbox.Width = 470;
                aboutpage.Width = 500;
                abouttext.Width = 470;
                tweetboxborder.Width = 470;
            }
        }
    }
}
