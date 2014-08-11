using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LinqToTwitter;
using Twitter2.Common;
using Windows.UI.Popups;
using System.Diagnostics;
using SQLite;
using Twitter2.fDatabase;

// La plantilla de elemento Página en blanco está documentada en http://go.microsoft.com/fwlink/?LinkId=234238

namespace Twitter2
{
    /**
     * This page is used for store the authentication. We also check if the authentication works.
     */ 
    public sealed partial class OAuthPage : Page
    {
        private PinAuthorizer auth; //Authentication pin
        SQLiteAsyncConnection db;
        Translator translator;

        public OAuthPage()
        {
            this.InitializeComponent();
            //This function is triggered when the page is load
            this.Loaded += OAuthPage_Loaded;
            //Obtains the translator
            translator = SuspensionManager.SessionState["Translator"] as Translator;

            //Translates some texts
            steps.Text = translator.getCadena("steps:");
            instructions.Text = translator.getCadena("instructions");
            notstoringcredentials.Text = translator.getCadena("notstoringcredentials");
            AuthenticatePinButton.Content = translator.getCadena("authenticate");
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

    }
}
