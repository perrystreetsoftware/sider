using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Sider.WP8.Tests.Resources;
using System.Diagnostics;
using Sider;

namespace Sider.WP8.Tests
{
    public class RedisSubscriptionReporter : IObserver<Message<string>>
    {
        private IDisposable unsubscriber;
        private string instName;

        public RedisSubscriptionReporter(string name)
        {
            this.instName = name;
        }

        public string Name
        { get { return this.instName; } }

        public virtual void Subscribe(IObservable<Message<string>> provider)
        {
            if (provider != null)
                unsubscriber = provider.Subscribe(this);
        }

        public virtual void OnCompleted()
        {
            Debug.WriteLine("The Location Tracker has completed transmitting data to {0}.", this.Name);
            this.Unsubscribe();
        }

        public virtual void OnError(Exception e)
        {
            Debug.WriteLine("{0}: The location cannot be determined.", this.Name);
        }

        public virtual void OnNext(Message<string> value)
        {
            Debug.WriteLine("A message is {0} / {1}", value.SourceChannel, value.Body);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }
    }

    public partial class MainPage : PhoneApplicationPage
    {

        RedisSubscriptionReporter mSubscription;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            RedisClient client = new RedisClient();
            if (client.Ping())
            {
                Debug.WriteLine("Got a pong");
            }

            Debug.WriteLine("Setting foo now");
            if (client.Set("foo", "bar"))
            {
                Debug.WriteLine("Set foo to bar");
            }

            mSubscription = new RedisSubscriptionReporter("Um test");
            mSubscription.Subscribe(client.Subscribe("foo"));

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}