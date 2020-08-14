﻿using System;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTubeClient;
using Console = System.Console;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using Toolbar = Android.Support.V7.Widget.Toolbar;
 

namespace PlayTube.Activities.Upload
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class VideoUploadActivity : AppCompatActivity
    {
        #region Variables Basic

        private static SwipeRefreshLayout SwipeRefreshLayout;
        private static WebView HybridView;
        private string Url = "";
        private AdView MAdView; 
        private static IValueCallback MUm;
        private static IValueCallback MUma;
        private static readonly int Fcr = 1;
         
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.LocalWebView_layout);

                if (AppSettings.SetTabDarkTheme)
                {
                    Url = Client.WebsiteUrl + "/upload-api?cookie=" + UserDetails.AccessToken + "&user_id=" + UserDetails.UserId + "&mode=night";
                }
                else
                {
                    Url = Client.WebsiteUrl + "/upload-api?cookie=" + UserDetails.AccessToken + "&user_id=" + UserDetails.UserId + "&mode=day";
                }

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetWebView(); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);

                MAdView?.Resume();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);

                MAdView?.Pause();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                MAdView?.Destroy();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                HybridView = FindViewById<WebView>(Resource.Id.LocalWebView);
                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, null);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Btn_Upload);
                    toolbar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetWebView()
        {
            try
            {
                //Set WebView and Load url to be rendered on WebView
                if (!Methods.CheckConnectivity())
                {
                    SwipeRefreshLayout.Refreshing = false; 
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                }
                else
                { 
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        LoadWebView();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted 
                            && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.AccessMediaLocation) == Permission.Granted)
                        {
                            LoadWebView();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    } 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void LoadWebView()
        {
            try
            {
                WebSettings webSettings = HybridView.Settings;
                HybridView.SetWebViewClient(new MyWebViewClient(this));
                HybridView.SetWebChromeClient(new MyWebChromeClient(this));

                webSettings.JavaScriptEnabled = true;
                webSettings.AllowFileAccess = true;
                webSettings.LoadsImagesAutomatically = true;
                webSettings.JavaScriptCanOpenWindowsAutomatically = true;
                webSettings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
                webSettings.DomStorageEnabled = true;
                HybridView.ClearCache(true); HybridView.Settings.LoadWithOverviewMode = true;

                HybridView.CopyBackForwardList();
                HybridView.CanGoBackOrForward(0);

                //Load url to be rendered on WebView
                HybridView.LoadUrl(Url); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
 
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events
         
        //Event Refresh Data Page
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                HybridView.Reload();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Result & Permissions

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if ((int)Build.VERSION.SdkInt >= 21)
                {
                    Android.Net.Uri[] results = null;
                    //Check if response is positive
                    if (resultCode == Result.Ok)
                    {
                        if (requestCode == Fcr)
                        {
                            if (MUma == null)
                                return;

                            var dataString = data?.Data?.ToString();
                            if (dataString != null)
                            {
                                results = new[]
                                {
                                    Android.Net.Uri.Parse(dataString)
                                };
                            }
                        }
                    }
                    MUma.OnReceiveValue(results);
                    MUma = null;
                }
                else
                {
                    if (requestCode == Fcr)
                    {
                        if (null == MUm)
                            return;

                        Android.Net.Uri result = data == null || resultCode != Result.Ok ? null : data.Data;
                        MUm.OnReceiveValue(result);
                        MUm = null;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        LoadWebView();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long).Show();
                        Finish();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                if (e.Action == KeyEventActions.Down)
                {
                    if (keyCode == Keycode.Back)
                    {
                        if (HybridView.CanGoBack())
                        {
                            HybridView.GoBack();
                        }
                        else
                        {
                            Finish();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            return base.OnKeyDown(keyCode, e);
        }

        private class MyWebViewClient : WebViewClient, IValueCallback
        {
            private readonly VideoUploadActivity MActivity;
            public MyWebViewClient(VideoUploadActivity mActivity)
            {
                MActivity = mActivity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                try
                {
                    if (request.Url.ToString() == MActivity.Url)
                    {
                        view.LoadUrl(request.Url.ToString());
                    }
                    else if (request.Url.ToString().Contains("watch"))
                    {
                        var con = TabbedMainActivity.GetInstance()?.MyChannelFragment?.VideosFragment;
                        if (con != null)
                        {
                            con.SwipeRefreshLayout.Refreshing = true;

                            con.MAdapter.VideoList.Clear();
                            con.MAdapter.NotifyDataSetChanged();

                            con.MainScrollEvent.IsLoading = false;
                             
                            con.StartApiService(); 
                        }
                        MActivity.Finish();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                return false; 
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                try
                {
                    base.OnPageStarted(view, url, favicon);

                    view.Settings.JavaScriptEnabled = true;
                    view.Settings.DomStorageEnabled = true;
                    view.Settings.AllowFileAccess = true;
                    view.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public override void OnPageFinished(WebView view, string url)
            {
                try
                {
                    base.OnPageFinished(view, url);
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;
                     
                    const string js = "javascript:" +
                                      "var elements = document.getElementsByClassName('navbar navbar-findcond navbar-fixed-top header-layout');" +
                                      " elements[0].style.display = 'none'; " +
                                      "$('.header').hide();" +
                                      "$('.pt_footer').hide();" +
                                      "$('.btn-fab-floating').hide();" +
                                      "$('.content-container').css('margin-top', '0');" +
                                      "$('.wo_about_wrapper_parent').css('top', '0');";

                    if (Build.VERSION.SdkInt >= (BuildVersionCodes)19)
                    {
                        view.EvaluateJavascript(js, this);
                    }
                    else
                    {
                        view.LoadUrl(js);
                    }
                }
                catch (Exception e)
                {
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;

                    Console.WriteLine(e);
                }
            }

            public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
            {
                try
                {
                    base.OnReceivedError(view, request, error);
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;

                    const string js = "javascript:" +
                                      "var elements = document.getElementsByClassName('navbar navbar-findcond navbar-fixed-top header-layout');" +
                                      " elements[0].style.display = 'none'; " +
                                      "$('.header').hide();" +
                                      "$('.pt_footer').hide();" +
                                      "$('.btn-fab-floating').hide();" +
                                      "$('.content-container').css('margin-top', '0');" +
                                      "$('.wo_about_wrapper_parent').css('top', '0');";

                    if (Build.VERSION.SdkInt >= (BuildVersionCodes)19)
                    {
                        view.EvaluateJavascript(js, this);
                    }
                    else
                    {
                        view.LoadUrl(js);
                    }
                }
                catch (Exception e)
                {
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;

                    Console.WriteLine(e);
                }
            }

            public void OnReceiveValue(Object value)
            {
                try
                {

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
         
        private class MyWebChromeClient : WebChromeClient
        {
            private readonly Activity MActivity;

            public MyWebChromeClient(Activity mActivity)
            {
                MActivity = mActivity;
            }


            
            [Android.Runtime.Register("onShowFileChooser", "(Landroid/webkit/WebView;Landroid/webkit/ValueCallback;Landroid/webkit/WebChromeClient$FileChooserParams;)Z", "GetOnShowFileChooser_Landroid_webkit_WebView_Landroid_webkit_ValueCallback_Landroid_webkit_WebChromeClient_FileChooserParams_Handler", ApiSince = 21)]
            public override bool OnShowFileChooser(WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
            {
                try
                {
                    MUma?.OnReceiveValue(null);

                    MUma = filePathCallback;
                      
                    Intent contentSelectionIntent = Android.OS.Environment.GetExternalStorageState(null).Equals(Android.OS.Environment.MediaMounted) ? new Intent(Intent.ActionPick, MediaStore.Video.Media.ExternalContentUri) : new Intent(Intent.ActionPick, MediaStore.Video.Media.InternalContentUri);
                    contentSelectionIntent.SetType("video/*");
                    contentSelectionIntent.PutExtra("return-data", true); //added snippet
                    contentSelectionIntent.AddFlags(ActivityFlags.GrantReadUriPermission); 
                    MActivity.StartActivityForResult(Intent.CreateChooser(contentSelectionIntent, "video Chooser")  , Fcr); 

                    return true;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    return false;
                }
            } 
        } 
    }
} 