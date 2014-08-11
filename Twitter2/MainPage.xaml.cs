using LinqToTwitter;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Twitter2.Common;
using Twitter2.fDatabase;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// La plantilla de elemento Página en blanco está documentada en http://go.microsoft.com/fwlink/?LinkId=234238

namespace Twitter2
{
    /**
    * Main timeline page. Here we have the main page.
    */ 
    public sealed partial class MainPage : Page
    {
        User currentUser;
        SQLiteAsyncConnection db;
        StoredAccount myDefaultAccount;
        private PinAuthorizer auth; //Authentication pin
        Translator translator;

        public MainPage()
        {
            this.InitializeComponent();
            checkLanguage();
            checkDatabaseExists();
            
            //This function is triggered when the page is loaded
            this.Loaded += OAuthPage_Loaded;
        }
        
        /**
         * In this function we firstly load the auth pin authorizer with the application credentials.
         * We also load the login twitter page in order to obtain the pin.
         */
        void OAuthPage_Loaded(object sender, RoutedEventArgs e)
        {
            //Auth definition with our application credentials
            auth = new PinAuthorizer
            {
                Credentials = new InMemoryCredentials
                {
                    ConsumerKey = Constants.getConsumerKey(),
                    ConsumerSecret = Constants.getConsumerSecret()
                },
                UseCompression = true,
                //Browser authentication webpage
                GoToTwitterAuthorization = pageLink =>
                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => OAuthWebBrowser.Navigate(new Uri(pageLink, UriKind.Absolute)))
            };

            auth.BeginAuthorize(resp =>
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    switch (resp.Status)
                    {
                        case TwitterErrorStatus.Success:
                            break;
                        case TwitterErrorStatus.RequestProcessingException:
                        case TwitterErrorStatus.TwitterApiError:
                            //new MessageDialog(resp.Error.ToString(), resp.Message).ShowAsync();
                            break;
                    }
                }));
        }

        /**
         * This function is triggered when the user clicks on the authentication button.
         * Here we release the "success" function (async) in order to check if the retrieved data is okey.
         * We also are careful with many twitter status errors.
         */
        void AuthenticatePinButton_Click(object sender, RoutedEventArgs e)
        {

            loadingring.IsActive = true;
            //SuspensionManager.SessionState["test"] = "asdasdasd";
            Debug.WriteLine("lleeeego");
            auth.CompleteAuthorize(
                PinTextBox.Text,
                completeResp => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Debug.WriteLine("status: "+completeResp.Status);
                    switch (completeResp.Status)
                    {
                        case TwitterErrorStatus.Success:
                            //SuspensionManager.SessionState["Authorizer"] = auth;
                            success(auth);
                            break;
                        case TwitterErrorStatus.RequestProcessingException:
                        case TwitterErrorStatus.TwitterApiError:
                            Debug.WriteLine(completeResp.Message.ToString());
                            Debug.WriteLine(completeResp.Exception.ToString());
                            //new MessageDialog(completeResp.Error.ToString(), completeResp.Message).ShowAsync();
                            break;
                    }
                }));

        }

        /**
         * This asynchronous function check our credentials and store them in our database.
         * We also store some user information such as tweets numbers.
         */ 
        public async void success(PinAuthorizer auth)
        {
            //We use the data, check that is correct and store it. Finally, we navigate to other frame.
            //We create a twitter account handler
            var twitterCtx = new TwitterContext(auth);

            //Selection of our account
            var accounts =
                from acct in twitterCtx.Account
                where acct.Type == AccountType.VerifyCredentials
                select acct;

            //Current user
            User currentUser = accounts.SingleOrDefault().User;

            //Open the database and store this data
            var path = Windows.Storage.ApplicationData.Current.LocalFolder.Path + @"\" + Constants.getDbName();
            db = new SQLiteAsyncConnection(path);

            //We store all the data in the new account
            var newUser = new StoredAccount()
            {
                TwOAuthToken = auth.OAuthTwitter.OAuthToken,
                TwAccessToken = auth.OAuthTwitter.OAuthTokenSecret,
                TwId = currentUser.Identifier.UserID,
                TwUsername = currentUser.Identifier.ScreenName,
                TwName = currentUser.Name,
                TwFollowingCount = currentUser.FriendsCount,
                TwFollowersCount = currentUser.FollowersCount,
                TwTweetCount = currentUser.StatusesCount,
                TwAvatarURL = currentUser.ProfileImageUrl,
                DefaultAccount = true
            };

            //Finally, we store the data in the DB
            await db.InsertAsync(newUser);

            //And we change the page to the timeline page
            SuspensionManager.SessionState["MyUser"] = newUser;
            SuspensionManager.SessionState["TwitterContext"] = twitterCtx;
            Frame.Navigate(typeof(TimeLinePage));
        }


        //---------------------------------------------------------------------------------

        public void checkLanguage()
        {
            Translator translator = new Translator("en");
            SuspensionManager.SessionState["Translator"] = translator;
        }

        public async void createTables()
        {
            await db.CreateTableAsync<StoredAccount>();
            await db.CreateTableAsync<StoredStatus>();
        }


        /**
         * Check if the database exists. In that case, we need to know if there is some content there or not.
         * If there is no content, we must create the tables and look forward to request tokens to the user (loggin).
         * If there is content, we must retrieve and use it to loggin.
         */
        public async void checkDatabaseExists()
        {

            var path = Windows.Storage.ApplicationData.Current.LocalFolder.Path + @"\" + Constants.getDbName();
            db = new SQLiteAsyncConnection(path);
            List<StoredAccount> allAccounts = null;
            //await db.DropTableAsync<StoredAccount>();
            //await db.DropTableAsync<StoredStatus>();

            //QUITAR ESTA MIERDA
            await db.CreateTableAsync<StoredAccount>();
            await db.CreateTableAsync<StoredStatus>();
            //await db.CreateTableAsync<StoredMention>();
            
            //Look if there is something on storedaccounts
           
            allAccounts = await db.QueryAsync<StoredAccount>("SELECT * FROM StoredAccounts");
            
            if (allAccounts == null || allAccounts.ToArray().Length == 0)
            {
                
                //Obtains the translator
                translator = SuspensionManager.SessionState["Translator"] as Translator;

                //Translates some texts
                steps.Text = translator.getCadena("steps:");
                instructions.Text = translator.getCadena("instructions");
                notstoringcredentials.Text = translator.getCadena("notstoringcredentials");
                AuthenticatePinButton.Content = translator.getCadena("authenticate");
            } 
            else
            {
                //Get the default user
                var defaultAccount = allAccounts.FindAll(findDefaultUser);

                //The first situation should be the correct one
                switch (defaultAccount.ToArray().Length)
                {
                    case 1: //There is one default user, so I'll take it
                        myDefaultAccount = defaultAccount.ToArray()[0];
                        break;

                    case 0: //There is no default user, so I'll set up the first userAccount as default
                        var singleAccount = allAccounts.First();
                        singleAccount.DefaultAccount = true;
                        await db.UpdateAsync(singleAccount);

                        myDefaultAccount = singleAccount;
                        break;

                    default: //Too many defaults users, so I'll set up the first user Account as default
                        //Firstly, I'll set up all default detected accounts as false
                        foreach (StoredAccount singleTemporalAccount in defaultAccount)
                        {
                            var tmp = singleTemporalAccount;
                            tmp.DefaultAccount = false;
                            await db.UpdateAsync(tmp);
                        }

                        var singleAccount2 = allAccounts.First();
                        singleAccount2.DefaultAccount = true;
                        await db.UpdateAsync(singleAccount2);

                        myDefaultAccount = singleAccount2;
                        break;
                } //switch

                //After that, we must check the credentials from myDefaultAccount variable
                SingleUserAuthorizer auth = new SingleUserAuthorizer
                {
                    Credentials = new InMemoryCredentials
                    {
                        ConsumerKey = Constants.getConsumerKey(),
                        ConsumerSecret = Constants.getConsumerSecret(),
                        OAuthToken = myDefaultAccount.TwOAuthToken,
                        AccessToken = myDefaultAccount.TwAccessToken
                    }
                };

                var twitterCtx = new TwitterContext(auth);

                if (NetworkInformation.GetInternetConnectionProfile() == null)
                {
                    SuspensionManager.SessionState["MyUser"] = myDefaultAccount;
                }
                else
                {
                    
                    //Selection of our account
                    //var accounts =
                    //    from acct in twitterCtx.Account
                    //    where acct.Type == AccountType.VerifyCredentials
                    //    select acct;

                    //Current user
                    //currentUser = accounts.SingleOrDefault().User;
                    currentUser = (from acct in twitterCtx.Account
                                   where acct.Type == AccountType.VerifyCredentials
                                   select acct).SingleOrDefault().User;

                    
                    
                    var newUser = new StoredAccount()
                    {
                        TwOAuthToken = auth.OAuthTwitter.OAuthToken,
                        TwAccessToken = auth.OAuthTwitter.OAuthTokenSecret,
                        TwId = currentUser.Identifier.UserID,
                        TwUsername = currentUser.Identifier.ScreenName,
                        TwName = currentUser.Name,
                        TwFollowingCount = currentUser.FriendsCount,
                        TwFollowersCount = currentUser.FollowersCount,
                        TwTweetCount = currentUser.StatusesCount,
                        TwAvatarURL = currentUser.ProfileImageUrl,
                        DefaultAccount = true
                    };

                    SuspensionManager.SessionState["MyUser"] = newUser;
                }


                //Finally, we transfer the credentials
                
                SuspensionManager.SessionState["TwitterContext"] = twitterCtx;
                Frame.Navigate(typeof(TimeLinePage));
            }
        }

        public bool findDefaultUser(StoredAccount storedAccount)
        {
            return storedAccount.DefaultAccount;
        }
    }
}
